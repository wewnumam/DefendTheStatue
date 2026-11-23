namespace ProjectTA.Message
{
    /// <summary>One tick of the run clock, published only when the whole second changes.</summary>
    public struct RunClockMessage
    {
        public RunClockMessage(int totalSeconds)
        {
            TotalSeconds = totalSeconds;
        }

        public int TotalSeconds { get; }
    }
}
