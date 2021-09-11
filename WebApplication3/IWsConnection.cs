using System;
using System.Threading.Tasks;

namespace WebApplication3
{
    public record ConnectionId(string ChargePointId, string UniqueId);
    
    public interface IWsConnection
    {
        ConnectionId ConnectionId { get; }
        Task ListeningAsync();
        // event EventHandler<CloseEventArgs> OnClose;
        // event EventHandler<MessageEventArgs> OnMessage;
        Task SendAsync(string message);
        Task<bool> OpenHandshakeAsync();
        WsContext GetWsContext();
        void Cancel();
    }

    public class WsContext
    {
    }
}