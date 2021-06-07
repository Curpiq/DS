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
            _logger.LogDebug(id);
            
             Rank = Convert.ToDouble(_storage.Load(Constants.RankKeyPrefix + id.ToString()));
             Similarity = Convert.ToDouble(_storage.Load(Constants.SimilarityKeyPrefix + id.ToString()));
        }
    }
}
