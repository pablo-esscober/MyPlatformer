[System.Serializable]
public class SaveData
{
    public const int CurrentVersion = 2;

    public int version = CurrentVersion;
    public int coins;
    public int levelIndex;
}
