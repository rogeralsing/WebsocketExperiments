using System;
using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<string, WsConnection> Connections = new();

        public WebSocketsController(ILogger<WebSocketsController> logger)
        {
            _logger = logger;
        }

        [HttpGet("/ws")]
        public async Task Get()
        {
          if (HttpContext.WebSockets.IsWebSocketRequest)
          {
              var connectionId = Guid.NewGuid().ToString();
              using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
              
              Console.WriteLine("starting connection " + connectionId);
              try
              {
                  var ex = new WsConnection(webSocket);
                  Connections.TryAdd(connectionId, ex);
                  await ex.RunClientAsync();
              }
              finally
              {
                  Connections.TryRemove(connectionId, out _);
                  Console.WriteLine("Removing connection " + connectionId);
              }
          }
          else
          {
              HttpContext.Response.StatusCode = 400;
          }
        }

        [HttpGet("/commands/{connection}")]
        public async Task<string> RunCommand(string connection)
        {
            if (Connections.TryGetValue(connection, out var wsConnection))
            {
                var commandId = Guid.NewGuid().ToString();
                await wsConnection.CreateCommandAsync(commandId,commandId);
                return commandId;
            }

            return "Connection not found " + connection;
        }
    }
}