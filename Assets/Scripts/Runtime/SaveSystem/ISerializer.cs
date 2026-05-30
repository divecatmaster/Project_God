namespace DiveCat.SaveSystem
{
    public interface ISerializer
    {
        string Serialize<T>(T obj, bool prettyPrint);
        T Deserialize<T>(string json);
    }
}
