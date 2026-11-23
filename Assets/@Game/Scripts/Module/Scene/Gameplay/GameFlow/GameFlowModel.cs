using Agate.MVC.Base;
using ProjectTA.Module.Build;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.GameFlow
{
    /// <summary>
    /// Run clock, spawn cadence, kill tally and the run's outcome. The boss arrives once at ten
    /// minutes; defeat lands when the statue or the player dies, victory when the boss dies.
    /// </summary>
    public class GameFlowModel : BaseModel
    {
        /// <summary>Reference game: the boss arrives at the ten-minute mark.</summary>
        public const float BossTriggerSeconds = 600f;

        public float GameSeconds { get; private set; }
        public int MonstersKilled { get; private set; }
        public int TreesChopped { get; private set; }
        public bool IsDefeat { get; private set; }
        public bool IsVictory { get; private set; }
        public bool HasBossSpawned { get; private set; }

        public bool IsRunOver => IsDefeat || IsVictory;

        private float _spawnAccumulator;

        /// <summary>Reference game: max(40, 180 - minutes * 14) frames, so cadence speeds up over time.</summary>
        public float CurrentSpawnIntervalSeconds =>
            GameTime.FramesToSeconds(BuildRules.GetSpawnIntervalFrames(GameSeconds));

        public void Reset()
        {
            GameSeconds = 0f;
            MonstersKilled = 0;
            TreesChopped = 0;
            IsDefeat = false;
            IsVictory = false;
            HasBossSpawned = false;
            _spawnAccumulator = 0f;
            SetDataAsDirty();
        }

        /// <summary>
        /// Advances the clock and reports how many monsters are due this frame. More than one is
        /// possible after a hitch or a long pause, so the caller spawns them all.
        /// </summary>
        public int Advance(float deltaSeconds)
        {
            if (deltaSeconds <= 0f || IsDefeat || IsVictory)
            {
                return 0;
            }

            GameSeconds += deltaSeconds;
            _spawnAccumulator += deltaSeconds;

            int spawns = 0;
            float interval = CurrentSpawnIntervalSeconds;
            while (interval > 0f && _spawnAccumulator >= interval)
            {
                _spawnAccumulator -= interval;
                spawns++;
            }

            if (spawns > 0)
            {
                SetDataAsDirty();
            }

            return spawns;
        }

        public void RegisterMonsterKilled(int essenceValue)
        {
            MonstersKilled++;
            SetDataAsDirty();
        }

        public void RegisterTreeChopped()
        {
            TreesChopped++;
            SetDataAsDirty();
        }

        public void MarkDefeat()
        {
            if (IsDefeat)
            {
                return;
            }

            IsDefeat = true;
            SetDataAsDirty();
        }

        /// <summary>Returns true only on the frame a health pool hits zero.</summary>
        public bool MarkDefeatIfDead(float health)
        {
            if (health > 0f || IsDefeat)
            {
                return false;
            }

            MarkDefeat();
            return true;
        }

        /// <summary>Victory is final: a defeat can never be upgraded into a win.</summary>
        public bool MarkVictory()
        {
            if (IsVictory || IsDefeat)
            {
                return false;
            }

            IsVictory = true;
            SetDataAsDirty();
            return true;
        }

        /// <summary>Reference boss timer: fires exactly once, ten minutes into a live run.</summary>
        public bool TryTriggerBoss()
        {
            if (HasBossSpawned || IsRunOver || GameSeconds < BossTriggerSeconds)
            {
                return false;
            }

            HasBossSpawned = true;
            SetDataAsDirty();
            return true;
        }

        /// <summary>
        /// Reference spawnMonster: monsters appear just outside the outermost built wall ring, on a
        /// random side, spread along that side but never within <paramref name="spawnOffset"/> of a corner.
        /// </summary>
        public static Vector2 GetRingSpawnPosition(int side, Vector2 statuePosition, float ringWidth, float ringHeight, float random01, float spawnOffset)
        {
            float halfW = ringWidth * 0.5f;
            float halfH = ringHeight * 0.5f;
            float spreadX = (random01 - 0.5f) * (ringWidth - spawnOffset);
            float spreadY = (random01 - 0.5f) * (ringHeight - spawnOffset);

            switch (side)
            {
                case 0:
                    return new Vector2(statuePosition.x + spreadX, statuePosition.y - halfH - spawnOffset);
                case 1:
                    return new Vector2(statuePosition.x + halfW + spawnOffset, statuePosition.y + spreadY);
                case 2:
                    return new Vector2(statuePosition.x + spreadX, statuePosition.y + halfH + spawnOffset);
                default:
                    return new Vector2(statuePosition.x - halfW - spawnOffset, statuePosition.y + spreadY);
            }
        }

        /// <summary>
        /// Fallback used before any wall exists: spawn on a camera edge, <paramref name="cameraSpawnDistance"/>
        /// away from the camera centre. viewWidth / viewHeight are the half extents of what the camera sees.
        /// </summary>
        public static Vector2 GetCameraSpawnPosition(int side, Vector2 cameraPosition, float halfViewWidth, float halfViewHeight, float random01, float cameraSpawnDistance)
        {
            float spreadX = (random01 - 0.5f) * (halfViewWidth * 2f);
            float spreadY = (random01 - 0.5f) * (halfViewHeight * 2f);

            switch (side)
            {
                case 0:
                    return new Vector2(cameraPosition.x + spreadX, cameraPosition.y - cameraSpawnDistance);
                case 1:
                    return new Vector2(cameraPosition.x + cameraSpawnDistance, cameraPosition.y + spreadY);
                case 2:
                    return new Vector2(cameraPosition.x + spreadX, cameraPosition.y + cameraSpawnDistance);
                default:
                    return new Vector2(cameraPosition.x - cameraSpawnDistance, cameraPosition.y + spreadY);
            }
        }
    }
}
