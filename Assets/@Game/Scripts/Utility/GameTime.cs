namespace ProjectTA.Utility
{
    /// <summary>
    /// The reference game counts everything in 60 fps frames. Everything here is in seconds, so
    /// this is the one place the conversion lives.
    /// </summary>
    public static class GameTime
    {
        public const float FramesPerSecond = 60f;

        public static float FramesToSeconds(float frames)
        {
            return frames / FramesPerSecond;
        }

        public static float SecondsToFrames(float seconds)
        {
            return seconds * FramesPerSecond;
        }
    }
}
