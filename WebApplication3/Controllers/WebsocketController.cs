using System;
using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<string, OcppConnection> Connections = new();

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
                  var ws = new WsConnection(webSocket);
                  var ocpp = new OcppConnection(new OcppConnectionId(connectionId, Guid.NewGuid().ToString("N")), ws);
                  Connections.TryAdd(connectionId, ocpp);
                  await ocpp.StartAsync();
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