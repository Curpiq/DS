using System.Collections.Generic;
namespace Storage
{
    public interface IStorage
    {
        void Store (string key, string value);
        string Load (string key);
        List<string> GetKeysWithPrefix(string prefix);
    }
}