using NATS.Client;
using System;
using System.Linq;
using System.Text;
using Valuator;

namespace RankCalculator
{
    public class RankCalculator
    {
        private readonly IConnection _connection;
        private readonly IAsyncSubscription _subscription;
        
        public RankCalculator(IStorage storage)
        {
            _connection = new ConnectionFactory().CreateConnection();
            _subscription = _connection.SubscribeAsync("valuator.processing.rank", "rank_calculator", (sender, args)
                =>
            {
                string id = Encoding.UTF8.GetString(args.Message.Data);

                string textKey = Constants.TextKeyPrefix + id;
                string text = storage.Load(textKey);

                string rankKey = Constants.RankKeyPrefix + id;
                string rank = GetRank(text).ToString();
                storage.Store(rankKey, rank);
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