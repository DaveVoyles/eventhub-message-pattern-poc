using Microsoft.Extensions.Configuration;

namespace OneWeek_Eventing.Common.Providers.Redis
{
    public static class RedisConfiguration
    {
        public static string GetConnectionString()
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets(typeof(RedisConfiguration).Assembly);

            // Configure SQL server.
            var configuration = builder.Build();
            return configuration.GetSection("Redis:ConnectionString").Value;
        }
    }
}
