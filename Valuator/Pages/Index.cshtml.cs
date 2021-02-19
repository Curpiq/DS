using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

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

        public IActionResult OnPost(string text)
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            string textKey = "TEXT-" + id;
            //TODO: сохранить в БД text по ключу textKey
             _storage.Store(textKey, text);
             
            string rankKey = "RANK-" + id;
            //TODO: посчитать rank и сохранить в БД по ключу rankKey
            _storage.Store(rankKey, getRank(text).ToString());

            string similarityKey = "SIMILARITY-" + id;
            //TODO: посчитать similarity и сохранить в БД по ключу similarityKey
             double similarity = GetSimilarity(text, id);
            _storage.Store(similarityKey, similarity.ToString());

            return Redirect($"summary?id={id}");
        }
        
        private double getRank(string text)
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

        private double GetSimilarity(string text, string id)
        {
            id = "TEXT-" + id;
            var keys = _storage.GetKeysWithPrefix("TEXT-");
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
