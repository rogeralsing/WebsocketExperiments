using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public static async Task<string> ReceiveUtf8StringAsync(this WebSocket webSocket)
        {
            var byteBuffer = new List<byte>();
            var buffer = Pool.Rent(1024);
            try
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    //discard binary messages
                    if (result.MessageType == WebSocketMessageType.Binary) continue;

                    //append bytes
                    //this can be optimized, e.g. just use the buffer if payload is endmessage and fits in one buffer
                    byteBuffer.AddRange(buffer[..result.Count]);
                    if (!result.EndOfMessage) continue;

                    var bytes = byteBuffer.ToArray();

                    var stringData = Encoding.UTF8.GetString(bytes);
                    byteBuffer.Clear();

                    return stringData;

                } while (!result.CloseStatus.HasValue);

                return null;
            }
            catch(Exception x)
            {
                //Console.WriteLine("Error " + x);
                return null;
            }
            finally
            {
                //we could probably return the buffer between await ReceiveAsync, to limit memory pressure
                Pool.Return(buffer);
            }
        }
    }
}