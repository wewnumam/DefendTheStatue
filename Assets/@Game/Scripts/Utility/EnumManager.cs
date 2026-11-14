namespace ProjectTA.Utility
{
    public class EnumManager
    {
        public enum GameState
        {
            PreGame,
            Playing,
            Pause,
            GameOver,
            GameWin
        }

        /// <summary>Reference game: weapons[0] = sword, weapons[1] = bow.</summary>
        public enum WeaponKind
        {
            Sword = 0,
            Bow = 1
        }

        public enum SkillSlot
        {
            None = 0,
            Skill1 = 1,
            Skill2 = 2,
            Skill3 = 3
        }
    }
}