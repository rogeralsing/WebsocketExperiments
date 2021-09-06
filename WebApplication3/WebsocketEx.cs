using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication3
{
    public class WebsocketEx
    {
        public WebSocket Socket { get; }
        private readonly Func<WebsocketEx, string, Task> _callback;

        private readonly ConcurrentDictionary<string, WsCommand> _commands = new();

        public WebsocketEx(WebSocket socket,Func<WebsocketEx, string, Task> callback )
        {
            Socket = socket;
            _callback = callback;
        }
        
        public async Task RunClientAsync()
        {
            while (true)
            {
                //todo: we could return status codes here too
                var str = await Socket.ReceiveUtf8StringAsync();
                if (str == null) break;

                await _callback(this, str);
            }

            await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }

        public async Task CreateCommand(string id, string call)
        {
            var command = new WsCommand(id, this);
            _commands.TryAdd(id, command);
            _ = command.StartAsync();
            await Socket.SendUtf8StringAsync(call);
        }

        public void UnregisterCommand(string id)
        {
            _commands.TryRemove(id, out _);
        }
    }
}