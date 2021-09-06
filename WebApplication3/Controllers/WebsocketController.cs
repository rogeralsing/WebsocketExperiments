using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApplication3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketsController : ControllerBase
    {
        private readonly ILogger<WebSocketsController> _logger;

        public WebSocketsController(ILogger<WebSocketsController> logger)
        {
            _logger = logger;
        }

        [HttpGet("/ws")]
        public async Task Get()
        {
          if (HttpContext.WebSockets.IsWebSocketRequest)
          {
              using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
              _logger.Log(LogLevel.Information, "WebSocket connection established");
              await Echo(webSocket, Callback);
          }
          else
          {
              HttpContext.Response.StatusCode = 400;
          }
        }

        private async Task Callback(WebSocket webSocket, string stringData)
        {
            Console.WriteLine("Got message " + stringData);
            await webSocket.SendUtf8StringAsync($"Server: Hello. You said: {stringData}");
        }

        private async Task Echo(WebSocket webSocket, Func<WebSocket, string, Task> callback)
        {
            while (true)
            {
                var str = await webSocket.ReceiveUtf8StringAsync();
                if (str == null) break;

                await callback(webSocket, str);
            }

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }
    }
}