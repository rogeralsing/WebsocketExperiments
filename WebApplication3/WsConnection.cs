using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication3
{
    public class WsConnection
    {
        public WebSocket Socket { get; }

        private readonly ConcurrentDictionary<string, WsCommand> _commands = new();

        public WsConnection(WebSocket socket)
        {
            Socket = socket;
        }
        
        public async Task RunClientAsync()
        {
            while (true)
            {
                //todo: we could return status codes here too
                var str = await Socket.ReceiveUtf8StringAsync();
                if (str == null) break;

                await OnMessageAsync(str);
            }

            await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "connection closed", CancellationToken.None);
        }

        public async Task CreateCommandAsync(string commandId, string call)
        {
            var command = new WsCommand(commandId, this);
            _commands.TryAdd(commandId, command);
            _ = command.StartAsync();
            await Socket.SendUtf8StringAsync(call);
        }

        public void UnregisterCommand(string commandId)
        {
            _commands.TryRemove(commandId, out _);
        }

        public void TryCompleteCommand(string commandId)
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

            //hack, when we get this. start a new command
            if (message == "command")
            {
                var id = Guid.NewGuid().ToString();
                await CreateCommandAsync(id, id);
            }
            
            //this would be the using the message processor
            await Socket.SendUtf8StringAsync($"Server: Hello. You said: {message}");
        }
    }
}