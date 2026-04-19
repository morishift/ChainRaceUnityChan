namespace Sample
{
    /// <summary>
    /// Sound type enumeration used by SoundPlayer to specify sound to play
    /// </summary>
    public enum SEType
    {
        Gao1,
        Pong1,
        Pong2,
        Pong3,
        Pong4,
        Pong5
    }

    /// <summary>
    /// simple class representing player information such as name and score
    /// </summary>
    [System.Serializable]
    public class PlayerInfo
    {
        public string name = "";
        public int score = 0;
        public int bonus = 0;
    }

}

