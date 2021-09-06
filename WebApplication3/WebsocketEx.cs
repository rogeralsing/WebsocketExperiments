using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication3
{
    public class WebsocketEx
    {
        private readonly WebSocket _socket;
        private readonly Func<WebSocket, string, Task> _callback;

        public WebsocketEx(WebSocket socket,Func<WebSocket, string, Task> callback )
        {
            _socket = socket;
            _callback = callback;
        }
        
        public async Task RunClientAsync()
        {
            while (true)
            {
                var str = await _socket.ReceiveUtf8StringAsync();
                if (str == null) break;

                await _callback(_socket, str);
            }

            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }
    }
}