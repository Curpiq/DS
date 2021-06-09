using System;
using StackExchange.Redis;
using System.Collections.Generic;
using Tools;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Storage
{
    public class RedisStorage : IStorage
    {
        private readonly string _host = "localhost";
        private readonly IConnectionMultiplexer _connection;

        private readonly Dictionary<string, IConnectionMultiplexer> _DB;
        
        public RedisStorage()
        {
            _connection = ConnectionMultiplexer.Connect(_host);
            _DB = new Dictionary<string, IConnectionMultiplexer>()
            {
                { Constants.RusId, ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("DB_RUS", EnvironmentVariableTarget.User)) },
                { Constants.EUId, ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("DB_EU", EnvironmentVariableTarget.User)) },
                { Constants.OtherId, ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("DB_OTHER", EnvironmentVariableTarget.User)) }
            };
        }
        public void Store(string shardKey, string key, string value)
        {
            var db = GetConnection(shardKey).GetDatabase();;
            db.StringSet(key, value);
        }

        public void StoreShardKey(string id, string shardKey)
        {
            var db = _connection.GetDatabase();
            db.StringSet(id, shardKey);
        }

        public void StoreToSet(string setId, string shardKey, string value)
        {
            var db = GetConnection(shardKey).GetDatabase();
            db.SetAdd(setId, value);
        }

        public string GetShardKey(string id)
        {
            var db = _connection.GetDatabase();     
            return db.StringGet(id);
        }
        
        public bool CheckingValue(string id, string shardKey, string value)
        {
             IDatabase db = GetConnection(shardKey).GetDatabase();
            return db.SetContains(id, value);
        }

        private IConnectionMultiplexer GetConnection(string sKey)
        {
            IConnectionMultiplexer connection;

            if (_DB.TryGetValue(sKey, out connection))
            {
                var db = connection.GetDatabase();
                return connection;
            }
            return _connection;
        }

        public string Load(string shardKey, string key)
        {
            var db = GetConnection(shardKey).GetDatabase();;
            return db.StringGet(key);
        }
    }
}