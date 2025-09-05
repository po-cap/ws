using System.Net.WebSockets;
using Po.Api.Response;
using WS.Domain.Utilities;

namespace WS.Domain.Entities;

public class User
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public required long UserId { get; set; }
    
    /// <summary>
    /// 長鏈結
    /// </summary>
    public required WebSocket WebSocket { get; set; }

    /// <summary>
    /// 對象 ID
    /// </summary>
    public long? ReceiverId { get; set; }
    
    /// <summary>
    /// 商品鏈結
    /// </summary>
    public string? Uri { get; set; }
    
    /// <summary>
    /// 賣家 ID
    /// </summary>
    public long? buyerId {
        get
        {
            if (Uri == null) return null; 
            
            if(!long.TryParse(Uri.Split("/")[0], out var id))
                throw Failure.BadRequest();
            
            return id;
        } 
    }
    
    /// <summary>
    /// 商品鏈結 ID
    /// </summary>
    public long? itemId {
        get
        {
            if (Uri == null) return null; 
            
            if(!long.TryParse(Uri.Split("/")[1], out var id))
                throw Failure.BadRequest();
            
            return id;
        } 
    }


    public async Task ReadAsync(Message message)
    {
        await WebSocket.ReadAsync(message);
    }
}