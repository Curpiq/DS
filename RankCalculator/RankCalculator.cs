using NATS.Client;
using System;
using System.Text;
using Storage;
using Tools;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace RankCalculator
{
    public class RankCalculator
    {
        private readonly IConnection _connection;
        private readonly IAsyncSubscription _subscription;
        private readonly ILogger<RankCalculator> _logger;
        private readonly IStorage _storage;
        
        public RankCalculator(ILogger<RankCalculator> logger, IStorage storage)
        {
            _logger = logger;
            _storage = storage;
            _connection = new ConnectionFactory().CreateConnection();
            _subscription = _connection.SubscribeAsync("valuator.processing.rank", "rank_calculator", async (sender, args)
                =>
            {
                string id = Encoding.UTF8.GetString(args.Message.Data);

                string shardKey = storage.GetShardKey(id);

                string textKey = Constants.TextKeyPrefix + id;
                string text = _storage.Load(shardKey, textKey);

                string rankKey = Constants.RankKeyPrefix + id;
                double rank = GetRank(text);
                _storage.Store(shardKey, rankKey, rank.ToString());

                _logger.LogDebug($"Rank = {rank}");

                RankMessage rankMessage = new RankMessage(id, rank);
                await SentMessage(rankMessage);
            });
        }

        private async Task SentMessage(RankMessage rankMsg)
        {            
            ConnectionFactory cf = new ConnectionFactory();
            using (IConnection c = cf.CreateConnection())
            {
                var data = JsonSerializer.Serialize(rankMsg);
                c.Publish("rankCalculator.logging.rank", Encoding.UTF8.GetBytes(data));
                await Task.Delay(1000);

                c.Drain();
                c.Close();
            }
        }

         public void Run()
        {
            _subscription.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();   

            _subscription.Unsubscribe();

            _connection.Drain();
            _connection.Close();      
        }
        
        private double GetRank(string text)
        {
            int charsCounter = 0;
            foreach (var ch in text)
            {
                if (!Char.IsLetter(ch))
                {
                    charsCounter++;
                }
            }
            return Math.Round(((double)charsCounter / text.Length), 3);
        }
    }
}