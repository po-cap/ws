using System.Text.Encodings.Web;
using System.Text.Json;
using StackExchange.Redis;
using WS.Domain.Entities;


namespace WS.Infrastructure.Utilities;

public static class SubscriberExtension
{
    public static async Task SendAsync(this ISubscriber subscriber, Message message)
    {
        var payload = JsonSerializer.Serialize(
            message, 
            new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        
        await subscriber.PublishAsync($"chat:{message.ReceiverId}", payload);
    }
}