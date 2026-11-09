namespace ProjectTA.Message
{
    public struct ToggleBgmMessage
    {
        public bool Bgm { get; }

        public ToggleBgmMessage(bool bgm)
        {
            Bgm = bgm;
        }
    }
}