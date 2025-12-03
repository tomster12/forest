public interface ISaveable
{
    public string SaveKey { get; }

    public string SaveToString();

    public void LoadFromString(string data);
}
