using System;
using System.Threading.Tasks;

namespace WebApplication3
{
    public class WsCommand
    {
        private readonly string _id;
        private readonly WebsocketEx _socket;
        private readonly TaskCompletionSource _tcs = new();

        public WsCommand(string id, WebsocketEx socket)
        {
            _id = id;
            _socket = socket;
        }
        public async Task StartAsync()
        {
            Console.WriteLine($"Command {_id} started");
            var timeout = Task.Delay(10000);

            await Task.WhenAny(timeout, _tcs.Task);

            if (_tcs.Task.IsCompleted)
            {
                //done
                //done
                //do stuff
                Console.WriteLine($"Command {_id} completed");
            }
            else
            {
                //timeout
                //do stuff
                Console.WriteLine($"Command {_id} timed out");
            }
            
            _socket.UnregisterCommand(_id);
        }

        public void Complete()
        {
            _tcs.TrySetResult();
        }
    }
}