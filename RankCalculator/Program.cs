using Microsoft.Extensions.Logging;
using Storage;

namespace RankCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
             var loggerFactory = LoggerFactory.Create(builder => {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });
            IStorage storage = new RedisStorage();
            var rankCalculator = new RankCalculator(loggerFactory.CreateLogger<RankCalculator>(), storage);
            rankCalculator.Run();
        }
    }
}