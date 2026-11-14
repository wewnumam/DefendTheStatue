using Agate.MVC.Base;
using ProjectTA.Module.Build;
using ProjectTA.Utility;
using UnityEngine;

namespace ProjectTA.Module.GameFlow
{
    /// <summary>
    /// Run clock, spawn cadence, kill tally and the defeat flag. Endless survival only: the boss
    /// and the victory branch stay out of the core scope.
    /// </summary>
    public class GameFlowModel : BaseModel
    {
        public float GameSeconds { get; private set; }
        public int MonstersKilled { get; private set; }
        public int TreesChopped { get; private set; }
        public bool IsDefeat { get; private set; }
        public bool IsVictory { get; private set; }

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

        public void RegisterMonsterKilled(int goldValue)
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

        public void MarkVictory()
        {
            IsVictory = true;
            SetDataAsDirty();
        }

        public const float SpawnOffset = 100f;
        public const float CameraSpawnDistance = 500f;

        /// <summary>
        /// Reference spawnMonster: monsters appear just outside the outermost built wall ring, on a
        /// random side, spread along that side but never within 50 of a corner.
        /// </summary>
        public static Vector2 GetRingSpawnPosition(int side, Vector2 statuePosition, float ringWidth, float ringHeight, float random01)
        {
            float halfW = ringWidth * 0.5f;
            float halfH = ringHeight * 0.5f;
            float spreadX = (random01 - 0.5f) * (ringWidth - SpawnOffset);
            float spreadY = (random01 - 0.5f) * (ringHeight - SpawnOffset);

            switch (side)
            {
                case 0:
                    return new Vector2(statuePosition.x + spreadX, statuePosition.y - halfH - SpawnOffset);
                case 1:
                    return new Vector2(statuePosition.x + halfW + SpawnOffset, statuePosition.y + spreadY);
                case 2:
                    return new Vector2(statuePosition.x + spreadX, statuePosition.y + halfH + SpawnOffset);
                default:
                    return new Vector2(statuePosition.x - halfW - SpawnOffset, statuePosition.y + spreadY);
            }
        }

        /// <summary>
        /// Fallback used before any wall exists: spawn on a camera edge, 500 away from the camera
        /// centre. viewWidth / viewHeight are the half extents of what the camera sees.
        /// </summary>
        public static Vector2 GetCameraSpawnPosition(int side, Vector2 cameraPosition, float halfViewWidth, float halfViewHeight, float random01)
        {
            float spreadX = (random01 - 0.5f) * (halfViewWidth * 2f);
            float spreadY = (random01 - 0.5f) * (halfViewHeight * 2f);

            switch (side)
            {
                case 0:
                    return new Vector2(cameraPosition.x + spreadX, cameraPosition.y - CameraSpawnDistance);
                case 1:
                    return new Vector2(cameraPosition.x + CameraSpawnDistance, cameraPosition.y + spreadY);
                case 2:
                    return new Vector2(cameraPosition.x + spreadX, cameraPosition.y + CameraSpawnDistance);
                default:
                    return new Vector2(cameraPosition.x - CameraSpawnDistance, cameraPosition.y + spreadY);
            }
        }
    }
}
