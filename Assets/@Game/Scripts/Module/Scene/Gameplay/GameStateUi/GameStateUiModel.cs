using Agate.MVC.Base;

namespace ProjectTA.Module.GameStateUi
{
    /// <summary>The numbers the results screen prints. One snapshot per run-over screen.</summary>
    public readonly struct RunStats
    {
        public RunStats(
            float seconds,
            int kills,
            int skill3Uses,
            int trapsTriggered,
            int treesChopped,
            int essence,
            int statueLevel,
            int stackerKills = 0,
            int waltroyKills = 0,
            int deasorKills = 0,
            int deaths = 0,
            float maxMana = 0f)
        {
            Seconds = seconds;
            Kills = kills;
            Skill3Uses = skill3Uses;
            TrapsTriggered = trapsTriggered;
            TreesChopped = treesChopped;
            Essence = essence;
            StatueLevel = statueLevel;
            StackerKills = stackerKills;
            WaltroyKills = waltroyKills;
            DeasorKills = deasorKills;
            Deaths = deaths;
            MaxMana = maxMana;
        }

        public int StackerKills { get; }
        public int WaltroyKills { get; }
        public int DeasorKills { get; }
        public int Deaths { get; }
        public float MaxMana { get; }

        public float Seconds { get; }
        public int Kills { get; }
        public int Skill3Uses { get; }
        public int TrapsTriggered { get; }
        public int TreesChopped { get; }
        public int Essence { get; }
        public int StatueLevel { get; }

        /// <summary>Reference results panel, one stat per line.</summary>
        public string Format()
        {
            int total = Seconds <= 0f ? 0 : (int)Seconds;
            return $"Time {total / 60:00}:{total % 60:00}\n"
                + $"Kills {Kills} (Stacker {StackerKills} / Waltroy {WaltroyKills} / Deasor {DeasorKills})\n"
                + $"Deaths {Deaths}\n"
                + $"Max mana {MaxMana:0}\n"
                + $"Skill 3 used {Skill3Uses}\n"
                + $"Traps triggered {TrapsTriggered}\n"
                + $"Trees chopped {TreesChopped}\n"
                + $"Essence {Essence}\n"
                + $"Statue level {StatueLevel}";
        }
    }

    /// <summary>
    /// Mirrors the run's tallies from messages so the results screen never reaches into five other
    /// modules at the moment of death.
    /// </summary>
    public class GameStateUiModel : BaseModel
    {
        public float Seconds { get; private set; }
        public int Kills { get; private set; }
        public int Skill3Uses { get; private set; }
        public int TrapsTriggered { get; private set; }
        public int TreesChopped { get; private set; }
        public int Essence { get; private set; }
        public int StatueLevel { get; private set; } = 1;
        public int StackerKills { get; private set; }
        public int WaltroyKills { get; private set; }
        public int DeasorKills { get; private set; }
        public int Deaths { get; private set; }
        public float MaxMana { get; private set; }

        public void Reset()
        {
            Seconds = 0f;
            Kills = 0;
            Skill3Uses = 0;
            TrapsTriggered = 0;
            TreesChopped = 0;
            Essence = 0;
            StatueLevel = 1;
            StackerKills = 0;
            WaltroyKills = 0;
            DeasorKills = 0;
            Deaths = 0;
            MaxMana = 0f;
            SetDataAsDirty();
        }

        public void SetClock(int totalSeconds)
        {
            Seconds = totalSeconds < 0 ? 0f : totalSeconds;
            SetDataAsDirty();
        }

        /// <summary>GDD §17: the results screen breaks kills down by monster role.</summary>
        public void RegisterKill(Utility.EnumManager.MonsterKind kind = Utility.EnumManager.MonsterKind.Stacker)
        {
            Kills++;

            switch (kind)
            {
                case Utility.EnumManager.MonsterKind.Waltroy:
                    WaltroyKills++;
                    break;
                case Utility.EnumManager.MonsterKind.Deasor:
                    DeasorKills++;
                    break;
                default:
                    StackerKills++;
                    break;
            }
        }

        public void RegisterDeath()
        {
            Deaths++;
            SetDataAsDirty();
        }

        public void SetMaxMana(float maxMana)
        {
            if (maxMana > MaxMana)
            {
                MaxMana = maxMana;
            }
        }

        public void RegisterSkill3()
        {
            Skill3Uses++;
        }

        public void RegisterTrapTriggered()
        {
            TrapsTriggered++;
        }

        public void RegisterTreeChopped()
        {
            TreesChopped++;
        }

        public void SetEssence(int essence)
        {
            Essence = essence;
        }

        public void SetStatueLevel(int level)
        {
            StatueLevel = level;
        }

        public RunStats Snapshot()
        {
            return new RunStats(
                Seconds,
                Kills,
                Skill3Uses,
                TrapsTriggered,
                TreesChopped,
                Essence,
                StatueLevel,
                StackerKills,
                WaltroyKills,
                DeasorKills,
                Deaths,
                MaxMana);
        }
    }
}
