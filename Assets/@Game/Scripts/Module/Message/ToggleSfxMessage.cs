namespace ProjectTA.Message
{
    public struct ToggleSfxMessage
    {
        public bool Sfx { get; }

        public ToggleSfxMessage(bool sfx)
        {
            Sfx = sfx;
        }
    }
}