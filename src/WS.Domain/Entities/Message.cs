using System.Text.Json.Serialization;

namespace WS.Domain.Entities;

public struct Message: IComparable<Message>
{
    /// <summary>
    /// 聊天室 URI
    /// </summary>
    [JsonPropertyName("uri")]
    public required string? Uri { get; set; }
    
    ///// <summary>
    ///// 收訊者
    ///// </summary>
    [JsonPropertyName("senderId")]
    public required long SenderId { get; set; }
    
    ///// <summary>
    ///// 收訊者
    ///// </summary>
    [JsonPropertyName("receiverId")]
    public required long? ReceiverId { get; set; }

    /// <summary>
    /// 訊息內容
    /// </summary>
    [JsonPropertyName("content")]
    public required string Content { get; set; }
    
    /// <summary>
    /// 訊息類型
    /// </summary>
    [JsonPropertyName("type")]
    public required MType MType { get; set; }

    /// <summary>
    /// 時間搓
    /// </summary>
    [JsonPropertyName("timestamp")]
    public required long Timestamp { get; set; }
    
    
    public int CompareTo(Message other)
    {
        return other.Timestamp.CompareTo(Timestamp);
    }
}