using System;
using System.Collections.Generic;
namespace Storage
{
    public interface IStorage
    {
        void Store (string shardKey, string key, string value);
        void StoreShardKey(string id, string shardKey);
        public void StoreToSet(string setId, string shardKey, string value);
        string GetShardKey(string id);
        string Load (string shardKey, string key);
        bool CheckingValue(string id, string shardKey, string value); 
    }
}