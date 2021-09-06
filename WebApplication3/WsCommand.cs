using System.Threading.Tasks;

namespace WebApplication3
{
    public class WsCommand
    {
        private readonly string _id;
        private readonly WebsocketEx _socket;
        private readonly TaskCompletionSource _tsc = new();

        public WsCommand(string id, WebsocketEx socket)
        {
            _id = id;
            _socket = socket;
        }
        public async Task StartAsync()
        {
            var timeout = Task.Delay(10000);

            await Task.WhenAny(timeout, _tsc.Task);

            if (_tsc.Task.IsCompleted)
            {
                //done
                //do stuff
            }
            else
            {
                //timeout
                //do stuff
            }
            
            _socket.UnregisterCommand(_id);
        }
    }
}