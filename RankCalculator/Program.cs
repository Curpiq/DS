using NATS.Client;
using System;
using System.Text;
using Valuator;

namespace RankCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            IStorage storage = new RedisStorage();
            var rankCalculator = new RankCalculator(storage);
            rankCalculator.Run();
        }
    }
}