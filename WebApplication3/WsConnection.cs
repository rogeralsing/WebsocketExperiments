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

        private readonly ConcurrentDictionary<string, WsCommand> _commands = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        public CancellationToken CancellationToken => _cts.Token;

        public WsConnection(WebSocket socket)
        {
            Socket = socket;
        }
        
        public async Task RunClientAsync()
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

        public async Task CreateCommandAsync(string commandId, string call)
        {
            
            var command = new WsCommand(commandId, this);
            _commands.TryAdd(commandId, command);
            
            _ = SendCommand(call, command);
        }

        private async Task SendCommand(string call, WsCommand command)
        {
            await _semaphore.WaitAsync(CancellationToken);
            //this task owns the lock now
            //it is only released when we get a response
            _ = command.StartAsync();
            await Socket.SendUtf8StringAsync(call);
        }

        public void UnregisterCommand(string commandId)
        {
            _commands.TryRemove(commandId, out _);
            _semaphore.Release();
        }

        private void TryCompleteCommand(string commandId)
        {
            if (_commands.TryGetValue(commandId, out var command))
            {
                command.Complete();
            }
        }
        
        private async Task OnMessageAsync(string message)
        {
            Console.WriteLine("Got message " + message);

            TryCompleteCommand(message);
            
            //this would be the using the message processor
            await Socket.SendUtf8StringAsync($"Server: Hello. You said: {message}");
        }

        public async Task StopAsync(WebSocketCloseStatus closeStatus, string statusDescription)
        {
            await Socket.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
            Socket.Dispose();
            _cts.Cancel();
        }
    }
}