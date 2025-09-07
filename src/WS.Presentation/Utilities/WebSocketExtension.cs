using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Po.Api.Response;
using WS.Domain.Entities;
using WS.Domain.Services;

namespace WS.Presentation.Utilities;

public static class WebSocketExtension
{
    public static async Task EstablishAConnectionAsync(this HttpContext context)
    {
        // process - 取得 WebSocket
        using var socket = await context.WebSockets.AcceptWebSocketAsync();
        
        // process - 獲取連線使用者的 ID
        var userId = context.UserID();
     
        // process - 先獲取 mediator (這裏稱它叫郵差)
        var postman = context.RequestServices.GetService<IPostman>();
        if(postman == null) throw Failure.BadRequest();
        
        // process - 建立用戶資料
        var user = new User()
        {
            UserId = userId,
            WebSocket = socket
        };
        
        // process - 通知郵差有人加入了
        await postman.RegisterAsync(user);

        // process - Web Socket 開使跑業務邏輯
        await socket.RunAsync(user: user, postman: postman);
    }

    public static async Task RunAsync(
        this WebSocket socket,
        User user,
        IPostman postman)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(1024 * 4);
        
        try
        {
            while (socket.State == WebSocketState.Open)
            {
                // process - 解收 socket 傳來的訊息
                var request = await socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);

                // process - 如果是文字訊息，就處理，並發送給其他使用者
                if (request.MessageType == WebSocketMessageType.Text)
                {
                    var content = Encoding.UTF8.GetString(buffer, 0, request.Count);
                    var message = JsonSerializer.Deserialize<Message>(content);
                    await postman.DeliverAsync(message);
                }
                // process - 如果是 close 幀，就關閉 web socket
                else if (request.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(
                        request.CloseStatus!.Value,
                        request.CloseStatusDescription,
                        CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            socket.Dispose();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }  
    }
}