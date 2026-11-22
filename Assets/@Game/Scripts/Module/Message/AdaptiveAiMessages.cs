namespace ProjectTA.Message
{
    /// <summary>
    /// GDD §14: the difficulty dial. Threat is 0..1; the buff is what a freshly spawned monster gets
    /// (1.0 .. 1.5x on hp, damage and speed).
    /// </summary>
    public struct ThreatLevelMessage
    {
        public ThreatLevelMessage(float threat, float monsterBuff)
        {
            Threat = threat;
            MonsterBuff = monsterBuff;
        }

        public float Threat { get; }
        public float MonsterBuff { get; }
    }
}
