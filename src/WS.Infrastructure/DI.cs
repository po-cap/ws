using System.Reflection.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using WS.Domain.Repositories;
using WS.Domain.Services;
using WS.Infrastructure.Reposotories;
using WS.Infrastructure.Services;

namespace WS.Infrastructure;

public static class DI
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // description - 注入郵差
        services.AddSingleton<IPostman, Postman>();
        
        // description - repositories
        services.AddSingleton<IMessageRepository, MessageRepository>();
        
        // processing - Redis 配置
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(
            config.GetConnectionString("Redis") 
            ?? throw new NullReferenceException("Please add connection string for redis"))
        );

        services.AddSingleton<ISubscriber>(sp => sp
            .GetRequiredService<IConnectionMultiplexer>()
            .GetSubscriber());
        
        services.AddSingleton<IDatabase>(sp => sp
            .GetRequiredService<IConnectionMultiplexer>()
            .GetDatabase(Constant.RedisDbNumber));
        
        // 目前沒有 redis 集群，只有一台而已
        services.AddSingleton<IServer>(sp =>
        {
            var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            var endPoints = multiplexer.GetEndPoints();

            return multiplexer.GetServer(endPoints[0]);
        });
        
        return services;
    }
}