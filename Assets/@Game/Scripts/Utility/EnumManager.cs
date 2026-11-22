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

        /// <summary>GDD §10: four tree kinds, each with its own hp, drops and respawn.</summary>
        public enum TreeKind
        {
            Red = 0,
            Purple = 1,
            Magic = 2,
            Arcana = 3
        }

        /// <summary>GDD §7: three monster roles, each with its own stat block.</summary>
        public enum MonsterKind
        {
            Stacker = 0,
            Waltroy = 1,
            Deasor = 2
        }

        public enum SkillSlot
        {
            None = 0,
            Skill1 = 1,
            Skill2 = 2,
            Skill3 = 3,
            Skill4 = 4
        }

        /// <summary>Dev-console flags. Order is the DevConsoleModel array index.</summary>
        public enum CheatType
        {
            GodMode = 0,
            InfiniteStamina = 1,
            InfiniteArrows = 2,
            FreeBuild = 3,
            NoCooldown = 4,
            NoSpawn = 5,
            FastForward = 6,
            OneHitKill = 7,
            InfiniteEssence = 8
        }
    }
}