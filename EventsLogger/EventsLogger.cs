using NATS.Client;
using System;
using System.Text;
using System.Linq;
using System.Text.Json;
using Tools;

namespace EventsLogger
{
    public class EventsLogger
    {
        private readonly IConnection _connection;

        private readonly IAsyncSubscription _subscription;

        public EventsLogger()
        {
            _connection = new ConnectionFactory().CreateConnection();

            _subscription = _connection.SubscribeAsync("rankCalculator.logging.rank", (sender, args) =>
            {
                var rankMsg = JsonSerializer.Deserialize<RankMessage>(args.Message.Data);
                 Console.WriteLine($"Event: {args.Message.Subject}\n" +
                                  $"Id: {rankMsg.Id}\n" +
                                  $"Rank: {rankMsg.Rank}\n");
            });

            _subscription = _connection.SubscribeAsync("rankCalculator.logging.rank", (sender, args) =>
            {
                var similarityMsg = JsonSerializer.Deserialize<SimilarityMessage>(args.Message.Data);
                 Console.WriteLine($"Event: {args.Message.Subject}\n" +
                                  $"Id: {similarityMsg.Id}\n" +
                                  $"Similarity: {similarityMsg.Similarity}\n");
            });

        }

        public void Run()
        {
            _subscription.Start();
            
            Console.WriteLine("Press \"Enter\" to exit.\n");
            Console.ReadLine();

            _subscription.Unsubscribe();

            _connection.Drain();
            _connection.Close();
        }
    }
}