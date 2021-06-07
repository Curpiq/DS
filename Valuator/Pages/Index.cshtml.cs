using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
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

        public async Task<IActionResult> OnPost(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Redirect("/");
            }

            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            string textKey = Constants.TextKeyPrefix + id;
            _storage.Store(textKey, text);

            string similarityKey = Constants.SimilarityKeyPrefix + id;
            double similarity = GetSimilarity(text, id);

            _storage.Store(similarityKey, similarity.ToString());

            CancellationTokenSource cts = new CancellationTokenSource();

            await CreateCalculatingRankTask(id);
            
            return Redirect($"summary?id={id}");
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
            id = Constants.TextKeyPrefix + id;
            var keys = _storage.GetKeysWithPrefix(Constants.TextKeyPrefix);
            foreach (var key in keys)
            {
                if(key != id && _storage.Load(key) == text)
                {
                    return 1;
                }
            }
            return 0;
        }
    }
}
