using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;

namespace Valuator
{
    public class RedisStorage : IStorage
    {
        private readonly string _host = "localhost";
        private IConnectionMultiplexer _connection;
        
        public RedisStorage()
        {
            _connection = ConnectionMultiplexer.Connect(_host);
        }
        public void Store(string key, string value)
        {
            var db = _connection.GetDatabase();
            db.StringSet(key, value);
        }
        public string Load(string key)
        {
            var db = _connection.GetDatabase();
            return db.StringGet(key);
        }
        public List<string> GetKeysWithPrefix(string prefix)
        {
            var server = _connection.GetServer(_host, 6379);
            List<string> keys = new List<string>();
            foreach (var key in server.Keys(pattern: prefix + "*"))
            {
                keys.Add(key);
            }

            return keys;
        }
    }
}