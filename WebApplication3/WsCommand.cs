using System;
using System.Threading.Tasks;

namespace WebApplication3
{
    public class WsCommand
    {
        private readonly string _id;
        private readonly WsConnection _connection;
        private readonly TaskCompletionSource _tcs = new();

        public WsCommand(string id, WsConnection connection)
        {
            _id = id;
            _connection = connection;
        }
        public async Task StartAsync()
        {
            Console.WriteLine($"Command {_id} started");
            var timeout = Task.Delay(10000);

            await Task.WhenAny(timeout, _tcs.Task);

            if (_tcs.Task.IsCompleted)
            {
                await OnCompletedAsync();
            }
            else
            {
                await OnTimeoutAsync();
            }
            
            _connection.UnregisterCommand(_id);
        }

        private async Task OnTimeoutAsync()
        {
            //timeout
            Console.WriteLine($"Command {_id} timed out");
        }

        private async Task OnCompletedAsync()
        {
            //completed
            Console.WriteLine($"Command {_id} completed");
        }

        public void Complete()
        {
            _tcs.TrySetResult();
        }
    }
}