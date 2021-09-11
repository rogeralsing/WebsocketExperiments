using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication3
{
    public class WsConnection
    {
        private WebSocket Socket { get; }
        
        private readonly CancellationTokenSource _cts = new();
        private Func<string, Task>? _onMessageAsync;
        public CancellationToken CancellationToken => _cts.Token;

        public WsConnection(WebSocket socket)
        {
            Socket = socket;
        }
        
        public async Task StartAsync()
        {
            WebSocketReceiveResult? result = null;
            // ReSharper disable once TooWideLocalVariableScope
            string? str = null;
            while (!CancellationToken.IsCancellationRequested)
            {
                (str, result) = await Socket.ReceiveUtf8StringAsync(CancellationToken);

                if (result?.CloseStatus != null || str == null)
                {
                    break;
                }

                await OnMessageAsync(str);
            }

            //connection is stopping
            _cts.Cancel();
            
            await Socket.CloseAsync(
                result?.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                result?.CloseStatusDescription ?? "Unknown status", 
                CancellationToken.None);
        }

        private async Task OnMessageAsync(string message)
        {
            if (_onMessageAsync == null)
                return;
            
            await _onMessageAsync(message);
        }

        public async Task StopAsync(WebSocketCloseStatus closeStatus, string statusDescription)
        {
            await Socket.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
            Socket.Dispose();
            _cts.Cancel();
        }

        public async Task SendUtf8StringAsync(string payload)
        {
            await Socket.SendUtf8StringAsync(payload);
        }

        public void RegisterOnMessage(Func<string, Task> onMessageAsync)
        {
            _onMessageAsync = onMessageAsync;
        }
    }
}