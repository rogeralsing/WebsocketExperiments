#nullable enable
using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;

namespace WebApplication3
{
    public static class Extensions
    {
        private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Create();
        
        
        public static async Task SendUtf8StringAsync(this WebSocket webSocket, string payload)
        {
            var serverMsg = Encoding.UTF8.GetBytes(payload);
            await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), 
                WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task<(string? Content, WebSocketReceiveResult? Result)> ReceiveUtf8StringAsync(this WebSocket webSocket,CancellationToken ct = default)
        {
            var bytes = Pool.Rent(1024);
            WebSocketReceiveResult? result = null;
            try
            {
                var buffer = new ArraySegment<byte>(bytes);
                await using var ms = new MemoryStream();
                do
                {
                    ct.ThrowIfCancellationRequested();

                    result = await webSocket.ReceiveAsync(buffer, ct);

                    
                    
                    ms.Write(buffer.Array!, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text || result.Count.Equals(0))
                {
                    throw new Exception("Unexpected message");
                }

                using var reader = new StreamReader(ms, Encoding.UTF8);
                var content = await reader.ReadToEndAsync();
                return (content, result);
            }
            catch
            {
                return (null, result);
            }
            finally
            {
                Pool.Return(bytes);
            }
        }
    }
}