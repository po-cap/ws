using StackExchange.Redis;
using WS.Domain.Repositories;
using WS.Domain.Entities;

namespace WS.Infrastructure.Reposotories;

public class MessageRepository : IMessageRepository
{
    private readonly IDatabase _db;

    public MessageRepository(IDatabase db)
    {
        _db = db;
    }
    
    /// <summary>
    /// 獲取訊息
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Message>> GetAsync(long userId)
    {
        var messages = new List<Message>();
        var pattern = $"message:{userId}:*";
        
        // 使用 Scan 方法
        var keys = new List<string>();
        var cursor = 0;
        do
        {
            var result = _db.Execute("SCAN", cursor.ToString(), "MATCH", pattern, "COUNT", 100);
            var response = (RedisResult[])result!;
            
            cursor = int.Parse((string)response[0]!);
            var scanKeys = (RedisKey[])response[1]!;
            
            foreach (var key in scanKeys)
            {
                keys.Add(key!);
            }
        } while (cursor != 0);

        
        foreach (var key in keys)
        {
            var hashes = await _db.HashGetAllAsync(key);
            var message = _buildMsg(hashes);
            if(message!= null) messages.Add(message.Value);
        }
        
        messages.Sort();

        return messages;
    }
    
    /// <summary>
    /// 插入訊息
    /// </summary>
    /// <param name="message"></param>
    public async Task AddAsync(Message message)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var key = $"message:{message.ReceiverId}:{timestamp}";
        
        await _db.HashSetAsync(key, new HashEntry[]
        {
            new ("uri",       message.Uri),
            new ("sender",    message.SenderId),
            new ("receiver",  message.ReceiverId),
            new ("content",   message.Content),
            new ("type",      (int)message.MType),
            new ("timestamp", timestamp)
        });

        _db.KeyExpire(key, TimeSpan.FromDays(30));
    }

    /// <summary>
    /// 刪除訊息
    /// </summary>
    /// <param name="message"></param>
    public async Task DeleteAsync(Message message)
    {
        await _db.KeyDeleteAsync($"message:{message.ReceiverId}:{message.Timestamp}");
    }
    
    /// <summary>
    /// 刪除訊息
    /// </summary>
    /// <param name="messages"></param>
    public async Task DeleteAsync(IEnumerable<Message> messages)
    { 
        var batch = _db.CreateBatch();
        var deleteTasks = new List<Task<bool>>();

        foreach (var message in messages)
        {
            deleteTasks.Add(batch.KeyDeleteAsync($"message:{message.ReceiverId}:{message.Timestamp}"));
            
            batch.Execute();
        
            // 等待所有删除操作完成
            await Task.WhenAll(deleteTasks);
        }
    }
    
    /// <summary>
    /// 把 redis hash set 從建成 messages
    /// </summary>
    /// <param name="entries"></param>
    /// <returns></returns>
    private Message? _buildMsg(HashEntry[] entries)
    {
        if (entries.Length == 0)
            return null;
        
        // process - 取出需要的 sets
        if (!long.TryParse(entries.FirstOrDefault(x => x.Name == "sender").Value, out var senderId))
            return null;
        if (!long.TryParse(entries.FirstOrDefault(x => x.Name == "receiver").Value, out var receiverId))
            return null;
        if (!int.TryParse(entries.FirstOrDefault(x => x.Name == "type").Value, out var typeInt))
            return null;
        if (!long.TryParse(entries.FirstOrDefault(x => x.Name == "timestamp").Value, out var timestamp))
            return null;
                
        var uri     = entries.FirstOrDefault(x => x.Name == "uri").Value.ToString();
        var content = entries.FirstOrDefault(x => x.Name == "content").Value.ToString();
        var type    = (MType)typeInt;
                
        if(uri == null || content == null)
            return null;
                
        // process - 利用 set 重現 message
        var message = new Message()
        {
            Uri = uri,
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            MType = type,
            Timestamp = timestamp
        };

        return message;
    }
}