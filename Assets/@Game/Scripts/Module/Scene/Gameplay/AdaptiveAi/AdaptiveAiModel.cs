using Agate.MVC.Base;
using UnityEngine;

namespace ProjectTA.Module.AdaptiveAi
{
    /// <summary>
    /// GDD §14 adaptive difficulty. Three scores decide a threat level, and the threat decides how much
    /// a freshly spawned monster is buffed (up to 1.5x). The safety net halves the threat whenever the
    /// player or the statue is nearly dead, so the run cannot spiral.
    /// </summary>
    public class AdaptiveAiModel : BaseModel
    {
        /// <summary>Seconds the turtle score takes to catch up to the player's behaviour.</summary>
        public const float SmoothingSeconds = 20f;

        /// <summary>GDD §14: monster quality buff never passes 1.5x.</summary>
        public const float MaxMonsterBuff = 1.5f;

        /// <summary>GDD §14 safety net: below this health fraction the threat is halved.</summary>
        public const float SafetyThreshold = 0.25f;

        public float TurtleScore { get; private set; }
        public float GuardScore { get; private set; }
        public float NeglectScore { get; private set; }
        public float Threat { get; private set; }

        /// <summary>What a monster gets on spawn: 1.0 at zero threat, 1.5 at full threat.</summary>
        public float MonsterBuff => 1f + (MaxMonsterBuff - 1f) * Threat;

        private int _guardShots;
        private int _playerKills;

        public void RegisterPlayerKill()
        {
            _playerKills++;
        }

        /// <summary>
        /// Reference: the guard score is "guard kills vs total". Kills are not tagged by killer here, so
        /// the guard's firing share stands in for its share of the work.
        /// </summary>
        public void RegisterGuardShot()
        {
            _guardShots++;
        }

        /// <summary>One tick of inputs; every number comes from a message, never from another module.</summary>
        public void Tick(
            float deltaSeconds,
            bool playerNearStatue,
            int monstersNearStatue,
            float statueHealthFraction,
            float playerHealthFraction)
        {
            if (deltaSeconds <= 0f)
            {
                return;
            }

            // Turtle: camping the statue while nothing threatens it.
            float turtleTarget = playerNearStatue && monstersNearStatue == 0 ? 1f : 0f;
            TurtleScore = Mathf.MoveTowards(
                TurtleScore, turtleTarget, deltaSeconds / SmoothingSeconds);

            int totalInvolvement = _guardShots + _playerKills;
            GuardScore = totalInvolvement <= 0 ? 0f : (float)_guardShots / totalInvolvement;

            // Neglect: the statue is hurt and something is standing on it.
            NeglectScore = Mathf.Clamp01(
                (1f - Mathf.Clamp01(statueHealthFraction)) * 0.7f
                + (monstersNearStatue > 0 ? 0.3f : 0f));

            float threat = 0.4f * TurtleScore + 0.3f * GuardScore + 0.3f * NeglectScore;

            if (statueHealthFraction < SafetyThreshold || playerHealthFraction < SafetyThreshold)
            {
                threat *= 0.5f;
            }

            Threat = Mathf.Clamp01(threat);
            SetDataAsDirty();
        }

        public void Reset()
        {
            TurtleScore = 0f;
            GuardScore = 0f;
            NeglectScore = 0f;
            Threat = 0f;
            _guardShots = 0;
            _playerKills = 0;
            SetDataAsDirty();
        }
    }
}
