using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace WS.Domain.Utilities;

public static class WebSocketExtension
{
    public static async Task ReadAsync<T>(this WebSocket socket, T message)
    {
        var content = JsonSerializer.Serialize(message, new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        var body    = Encoding.UTF8.GetBytes(content);
        
        var buffer  = ArrayPool<byte>.Shared.Rent(body.Length);
        try
        {
            // 將訊息放入剛剛租用的記憶體空間內
            body.CopyTo(buffer, 0);
            var segment = new ArraySegment<byte>(buffer, 0, body.Length);

            // 傳送
            await socket.SendAsync(
                segment,
                WebSocketMessageType.Text,
                endOfMessage: true,
                CancellationToken.None);
        }
        finally
        {
            // 向系統歸還一塊記憶體空間
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    
}