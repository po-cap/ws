using System.Text.Encodings.Web;
using System.Text.Json;
using StackExchange.Redis;
using WS.Domain.Entities;
using WS.Domain.Repositories;
using WS.Domain.Services;
using WS.Infrastructure.Utilities;

namespace WS.Infrastructure.Services;

public class Postman : IPostman
{
    private readonly ISubscriber _subscriber;
    private readonly IMessageRepository _msgRepository;
    
    public Postman(
        ISubscriber subscriber,
        IMessageRepository msgRepository)
    {
        _subscriber = subscriber;
        _msgRepository = msgRepository;
    }


    public async Task RegisterAsync(User user)
    {
        var messages = await _msgRepository.GetAsync(userId: user.UserId);
    
        foreach (var msg in messages)
        {
            await user.ReadAsync(msg);
        }
    
        await _msgRepository.DeleteAsync(messages);    
        
        
        await _subscriber.SubscribeAsync($"chat:{user.UserId}",async (_, payload) =>
        {
            // process - 反序列化訊息
            var message = JsonSerializer.Deserialize<Message>(
                payload.ToString(), 
                new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            
            // process - 收訊者讀取訊息
            await user.ReadAsync(message:message);
            
            // process - 講訊息從資料庫中刪除
            await _msgRepository.DeleteAsync(message);  
        });
    }

    public async Task DeliverAsync(Message message)
    {
        // process - 把訊息加入資料庫
        await _msgRepository.AddAsync(message);
        // process - 發送訓訊息
        await _subscriber.SendAsync(message);
    }
}