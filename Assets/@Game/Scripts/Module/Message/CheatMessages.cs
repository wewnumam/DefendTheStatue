using ProjectTA.Utility;

namespace ProjectTA.Message
{
    /// <summary>
    /// A dev-console flag flipped. The message exists ahead of the console (5.1) so the boss damage
    /// gate can be written and tested now; nothing publishes it until the console lands.
    /// </summary>
    public struct CheatToggledMessage
    {
        public CheatToggledMessage(EnumManager.CheatType cheat, bool enabled)
        {
            Cheat = cheat;
            Enabled = enabled;
        }

        public EnumManager.CheatType Cheat { get; }
        public bool Enabled { get; }
    }
}
