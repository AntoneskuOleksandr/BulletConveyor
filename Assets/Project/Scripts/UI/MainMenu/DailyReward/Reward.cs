
[System.Serializable]
public class Reward
{
    public enum RewardType
    {
        COIN
    }

    public RewardType type;
    public int value;
    public string name;

}
