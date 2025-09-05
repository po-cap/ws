using WS.Domain.Entities;

namespace WS.Domain.Repositories;

public interface IMessageRepository
{
    /// <summary>
    /// 獲取訊息
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<IEnumerable<Message>> GetAsync(long userId);

    /// <summary>
    /// 插入訊息
    /// </summary>
    /// <param name="message"></param>
    Task AddAsync(Message message);

    /// <summary>
    /// 刪除訊息
    /// </summary>
    /// <param name="message"></param>
    Task DeleteAsync(Message message);

    /// <summary>
    /// 刪除訊息
    /// </summary>
    /// <param name="messages"></param>
    Task DeleteAsync(IEnumerable<Message> messages);
}