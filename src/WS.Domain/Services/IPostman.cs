using WS.Domain.Entities;

namespace WS.Domain.Services;

public interface IPostman
{
    /// <summary>
    /// 登記練線用戶
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task RegisterAsync(User user);
    
    /// <summary>
    /// 請 Postman 送信
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task DeliverAsync(Message message);
}