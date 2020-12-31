namespace fastdee
{
    /// <summary>
    /// Thread-safe storage for current difficulty multiplier.
    /// Writing is just a matter of getting the values from "mining.set_difficulty".
    /// Reading is more complicated, it comes in the form of a number and a "target".
    /// </summary>
    interface ICurrentDifficulty
    {
        /// <exception cref="System.ArgumentException">Parameter is 0 or less.</exception>
        void Set(double difficulty);
        DifficultyTarget DifficultyTarget { get; }
    }
}
