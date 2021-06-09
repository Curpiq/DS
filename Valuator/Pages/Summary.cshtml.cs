using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Storage;
using Tools;

namespace Valuator.Pages
{
    public class SummaryModel : PageModel
    {
        private readonly ILogger<SummaryModel> _logger;
        private IStorage _storage;

        public SummaryModel(ILogger<SummaryModel> logger, IStorage storage)
        {
            _logger = logger;
            _storage = storage;
        }

        public double Rank { get; set; }
        public double Similarity { get; set; }

        public void OnGet(string id)
        {
            string shardKey = _storage.GetShardKey(id);

            _logger.LogDebug("LOOKUP: {id}, {shardKey}", id, shardKey);
            
             Rank = Convert.ToDouble(_storage.Load(shardKey, Constants.RankKeyPrefix + id.ToString()));
             Similarity = Convert.ToDouble(_storage.Load(shardKey, Constants.SimilarityKeyPrefix + id.ToString()));
        }
    }
}
