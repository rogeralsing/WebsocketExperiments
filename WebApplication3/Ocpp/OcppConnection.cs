using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication3
{
    public record OcppConnectionId(string ChargePointId, string UniqueId);
    public class OcppConnection
    {
        private readonly ConcurrentDictionary<string, OcppCommand> _commands = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly WsConnection _wsConnection;
        public OcppConnectionId Id { get; }
        public CancellationToken CancellationToken => _wsConnection.CancellationToken;

        public OcppConnection(OcppConnectionId id, WsConnection wsConnection)
        {
            Id = id;
            _wsConnection = wsConnection;
            _wsConnection.RegisterOnMessage(OnMessageAsync);
        }
        
        public Task CreateCommandAsync(string commandId, string call)
        {
            var command = new OcppCommand(commandId, this);
            _commands.TryAdd(commandId, command);
            
            //fire and forget
            _ = SendCommand(call, command);
            return Task.CompletedTask;
        }
        
        private async Task SendCommand(string call, OcppCommand command)
        {
            await _semaphore.WaitAsync(_wsConnection.CancellationToken);
            //this task owns the lock now
            //it is only released when we get a response or command times out
            _ = command.StartAsync();
            await _wsConnection.SendUtf8StringAsync(call);
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

        public async Task StartAsync()
        {
            await _wsConnection.StartAsync();
        }

        public async Task StopAsync(WebSocketCloseStatus closeStatus, string statusDescription)
        {
            await _wsConnection.StopAsync(closeStatus, statusDescription);
        }
        
        private async Task OnMessageAsync(string message)
        {
            Console.WriteLine("Got message " + message);
            //this would be the using the message processor
            await _wsConnection.SendUtf8StringAsync($"Server: Hello. You said: {message}");
        }
    }
}