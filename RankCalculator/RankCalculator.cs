using NATS.Client;
using System;
using System.Text;
using Storage;
using Tools;
using Microsoft.Extensions.Logging;

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
            _subscription = _connection.SubscribeAsync("valuator.processing.rank", "rank_calculator", (sender, args)
                =>
            {
                string id = Encoding.UTF8.GetString(args.Message.Data);

                string textKey = Constants.TextKeyPrefix + id;
                string text = _storage.Load(textKey);

                string rankKey = Constants.RankKeyPrefix + id;
                string rank = GetRank(text).ToString();
                _storage.Store(rankKey, rank);

                _logger.LogDebug($"Rank = {rank}");
            });
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