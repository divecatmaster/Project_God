using UnityEngine;

namespace DiveCat.SaveSystem
{
    public class UnityJsonSerializer : ISerializer
    {
        public string Serialize<T>(T obj, bool prettyPrint)
        {
            return JsonUtility.ToJson(obj, prettyPrint);
        }

        public T Deserialize<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
    }
}
