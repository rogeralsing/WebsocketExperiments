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
              var ex = new WebsocketEx(webSocket, Callback);
              await ex.RunClientAsync();
          }
          else
          {
              HttpContext.Response.StatusCode = 400;
          }
        }

        private async Task Callback(WebsocketEx socket, string stringData)
        {
            Console.WriteLine("Got message " + stringData);

            socket.TryCompleteCommand(stringData);

            if (stringData == "hej")
            {
                var id = Guid.NewGuid().ToString();
                await socket.CreateCommand(id, id);
            }
            
            await socket.Socket.SendUtf8StringAsync($"Server: Hello. You said: {stringData}");
        }
    }
}