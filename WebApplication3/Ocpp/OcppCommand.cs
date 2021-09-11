using System;
using System.Threading.Tasks;

namespace WebApplication3
{
    public class OcppCommand
    {
        private readonly string _id;
        private readonly OcppConnection _connection;
        private readonly TaskCompletionSource _tcs = new();

        public OcppCommand(string id, OcppConnection connection)
        {
            _id = id;
            _connection = connection;
        }
        public async Task StartAsync()
        {
            Console.WriteLine($"Command {_id} started");
            var timeout = Task.Delay(10000, _connection.CancellationToken);
            await using (_connection.CancellationToken.Register(() => _tcs.SetCanceled()))
            {
                try
                {
                    await Task.WhenAny(timeout, _tcs.Task);

                    if (_tcs.Task.IsCompleted)
                    {
                        if (_tcs.Task.IsCompletedSuccessfully)
                        {
                            await OnCompletedAsync();
                        }
                        else
                        {
                            await OnConnectionClosed();
                        }
                    }
                    else
                    {
                        await OnTimeoutAsync();
                    }
                }
                catch(Exception x)
                {
                    Console.WriteLine("Command failed " + x);
                }
                finally
                {
                    _connection.UnregisterCommand(_id);
                }
            }
        }

        private async Task OnConnectionClosed()
        {
            //cancellation token 
            Console.WriteLine($"Command failed {_id} due to connection close");  
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