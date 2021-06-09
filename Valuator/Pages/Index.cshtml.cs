using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NATS.Client;
using Storage;
using Tools;

namespace Valuator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private IStorage _storage;

        public IndexModel(ILogger<IndexModel> logger, IStorage storage)
        {
            _logger = logger;
            _storage = storage;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPost(string text, string shardKey)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Redirect("/");
            }

            string id = Guid.NewGuid().ToString();

            _storage.StoreShardKey(id, shardKey);

            _logger.LogDebug("LOOKUP: {id}, {shardKey}", id, shardKey);

            string textKey = Constants.TextKeyPrefix + id;
            _storage.Store(shardKey, textKey, text);

            string similarityKey = Constants.SimilarityKeyPrefix + id;
            double similarity = GetSimilarity(text, id);

            _storage.Store(shardKey, similarityKey, similarity.ToString());

            _storage.StoreToSet(Constants.textsSetKey, shardKey, text);

            await CreateCalculatingRankTask(id);
            
            SimilarityMessage similarityMessage = new SimilarityMessage(id, similarity);
            await SentMessage(similarityMessage);

            return Redirect($"summary?id={id}");
        }

        private async Task SentMessage(SimilarityMessage similarityMsg)
        {            
            ConnectionFactory cf = new ConnectionFactory();
            using (IConnection c = cf.CreateConnection())
            {
                var data = JsonSerializer.Serialize(similarityMsg);
                c.Publish("valuator.logging.similarity", Encoding.UTF8.GetBytes(data));

                await Task.Delay(1000);

                c.Drain();
                c.Close();
            }
        }
        
        private async Task CreateCalculatingRankTask(string id)
        {
            ConnectionFactory cf = new ConnectionFactory();
            using (IConnection con = cf.CreateConnection())
            {
                byte[] data = Encoding.UTF8.GetBytes(id);
                con.Publish("valuator.processing.rank", data);
                await Task.Delay(1000);

                con.Drain();
                con.Close();
            }
        }

        private double GetSimilarity(string text, string id)
        {
            if (_storage.CheckingValue(Constants.TextKeyPrefix, Constants.RusId, text) ||
                _storage.CheckingValue(Constants.TextKeyPrefix, Constants.EUId, text) ||
                _storage.CheckingValue(Constants.TextKeyPrefix, Constants.OtherId, text))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
