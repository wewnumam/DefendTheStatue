using Agate.MVC.Base;
using ProjectTA.Message;
using ProjectTA.Module.Build;
using ProjectTA.Module.GameConstants;
using ProjectTA.Module.Territory;
using UnityEngine;

namespace ProjectTA.Module.GameFlow
{
    /// <summary>
    /// Drives the run: ticks the clock, spawns monsters on the reference cadence, and keeps the tally.
    /// It never calls into the monster pool directly — spawns go out as a message.
    ///
    /// The ring the monsters appear outside is tracked from wall-built messages rather than read off
    /// the territory controller, so the two modules stay independent.
    /// </summary>
    public class GameFlowController : ObjectController<GameFlowController, GameFlowModel, GameFlowView>
    {
        private bool[] _ringHasWall = System.Array.Empty<bool>();

        /// <summary>Ring geometry, so the spawner measures the same rectangle the walls are built on.</summary>
        private TerritoryLayout _layout = new TerritoryLayout(new TerritoryConstants());

        private int _outermostWallIndex = -1;

        /// <summary>Spawn distances are game data, not code: read once from the GameConstants asset.</summary>
        private MonsterConstants _monsterConstants = new MonsterConstants();

        /// <summary>Pause and slow motion are run-local: both are just a time scale, no new run state.</summary>
        private bool _isPaused;
        private bool _isSlowMotion;

        /// <summary>-1 so the very first tick publishes 0:00; the HUD never has to guess.</summary>
        private int _lastClockSecond = -1;
        private bool _noSpawn;
        private bool _fastForward;

        public void InitModel(IGameConstantsModel gameConstants)
        {
            if (gameConstants == null || gameConstants.GameConstants == null)
            {
                Debug.LogError("GAMECONSTANTS IS NULL");
                return;
            }

            _layout = new TerritoryLayout(gameConstants.GameConstants.TerritoryConstants);
            _ringHasWall = new bool[_layout.RingCount];

            if (gameConstants.GameConstants.MonsterConstants != null)
            {
                _monsterConstants = gameConstants.GameConstants.MonsterConstants;
            }
        }

        public override void SetView(GameFlowView view)
        {
            base.SetView(view);
            _model.Reset();
            view.SetCallback(OnTick);
            view.SetPauseCallback(OnPausePressed);
            view.SetTimeScale(1f);
            view.SetSpawnDistance(_monsterConstants.CameraSpawnDistance);
            _isPaused = false;
            _isSlowMotion = false;
            _lastClockSecond = -1;
        }

        /// <summary>Escape toggles pause. Time scale 0 freezes FixedUpdate, so nothing else needs a flag.</summary>
        private void OnPausePressed()
        {
            if (_model.IsDefeat || _model.IsVictory)
            {
                return;
            }

            _isPaused = !_isPaused;
            ApplyTimeScale();
            Publish(new PauseToggledMessage(_isPaused));
        }

        /// <summary>The skill upgrade interface asks for slow motion; pause wins when both are on.</summary>
        public void OnSkillUpgradePanelToggled(SkillUpgradePanelToggledMessage message)
        {
            if (_model.IsDefeat || _model.IsVictory)
            {
                return;
            }

            _isSlowMotion = message.IsOpen;
            ApplyTimeScale();
        }

        private void ApplyTimeScale()
        {
            if (_view == null)
            {
                return;
            }

            if (_isPaused)
            {
                _view.SetTimeScale(0f);
                return;
            }

            _view.SetTimeScale(_isSlowMotion ? _view.SlowMotionScale : 1f);
        }

        private void OnTick(float deltaSeconds)
        {
            int due = _model.Advance(deltaSeconds * (_fastForward ? 2f : 1f));
            for (int i = 0; i < due && !_noSpawn; i++)
            {
                SpawnMonster();
            }

            int wholeSecond = Mathf.FloorToInt(_model.GameSeconds);
            if (wholeSecond != _lastClockSecond)
            {
                _lastClockSecond = wholeSecond;
                Publish(new RunClockMessage(wholeSecond));
            }

            // The ten-minute boss fires once; the boss module (4.2/4.3) consumes the message.
            if (_model.TryTriggerBoss())
            {
                Publish(new BossSpawnedMessage());
            }
        }

        // Player death is no longer a GameOver (GDD §4: -30 statue HP + respawn). The statue's
        // death is the only defeat path, and it arrives as GameStateMessage(GameOver) from Statue.

        public void OnCheatToggled(CheatToggledMessage message)
        {
            switch (message.Cheat)
            {
                case Utility.EnumManager.CheatType.NoSpawn:
                    _noSpawn = message.Enabled;
                    break;
                case Utility.EnumManager.CheatType.FastForward:
                    _fastForward = message.Enabled;
                    break;
            }
        }

        public void OnBossDefeated(BossDefeatedMessage message)
        {
            if (_model.MarkVictory())
            {
                _view?.SetTimeScale(0f);
                Publish(new GameStateMessage(Utility.EnumManager.GameState.GameWin));
            }
        }

        private void SpawnMonster()
        {
            Vector2 statue = _view.StatuePosition;
            int side = Random.Range(0, 4);
            Vector2 position;

            if (_outermostWallIndex >= 0)
            {
                Vector2 ring = _layout.GetRingSize(_outermostWallIndex);
                position = GameFlowModel.GetRingSpawnPosition(
                    side, statue, ring.x, ring.y, Random.value, _monsterConstants.SpawnOffset);
            }
            else
            {
                _view.GetHalfViewSize(out float halfWidth, out float halfHeight);
                Camera camera = _view.Camera;
                Vector2 cameraPosition = camera != null ? (Vector2)camera.transform.position : statue;
                position = GameFlowModel.GetCameraSpawnPosition(
                    side, cameraPosition, halfWidth, halfHeight, Random.value, _monsterConstants.CameraSpawnDistance);
            }

            // Reference spawns always land outside the statue heal radius, so the tougher, richer
            // variant never appears from this path. The flag stays in the message for the boss pass.
            Utility.EnumManager.MonsterKind kind = BuildRules.PickMonsterKind(_model.GameSeconds, UnityEngine.Random.value);
            Publish(new RequestSpawnMonsterMessage(position, false, kind));
        }

        public void OnWallSegmentBuilt(WallSegmentBuiltMessage message)
        {
            if (message.TerritoryIndex < 0 || message.TerritoryIndex >= _ringHasWall.Length)
            {
                return;
            }

            _ringHasWall[message.TerritoryIndex] = true;
            RecomputeOutermostWall();
        }

        public void OnTowerBuilt(TowerBuiltMessage message)
        {
            if (message.TerritoryIndex < 0 || message.TerritoryIndex >= _ringHasWall.Length)
            {
                return;
            }

            _ringHasWall[message.TerritoryIndex] = true;
            RecomputeOutermostWall();
        }

        private void RecomputeOutermostWall()
        {
            for (int i = _ringHasWall.Length - 1; i >= 0; i--)
            {
                if (_ringHasWall[i])
                {
                    _outermostWallIndex = i;
                    return;
                }
            }

            _outermostWallIndex = -1;
        }

        public void OnMonsterKilled(MonsterKilledMessage message)
        {
            _model.RegisterMonsterKilled(message.EssenceValue);
        }

        public void OnTreeChopped(TreeChoppedMessage message)
        {
            _model.RegisterTreeChopped();
        }

        /// <summary>
        /// Playing means "a new run starts", so the reset lives here. GameOver marks the defeat flag so
        /// the spawn clock stops for good, and freezes the world — nothing reads a GameState.Pause,
        /// pause is a time scale on this side only.
        /// </summary>
        public void OnGameState(GameStateMessage message)
        {
            if (message.GameState == Utility.EnumManager.GameState.GameOver)
            {
                _model.MarkDefeat();
                _isPaused = false;
                _isSlowMotion = false;

                if (_view != null)
                {
                    _view.SetTimeScale(0f);
                }

                return;
            }

            if (message.GameState == Utility.EnumManager.GameState.GameWin)
            {
                _model.MarkVictory();
                _isPaused = false;
                _isSlowMotion = false;

                if (_view != null)
                {
                    _view.SetTimeScale(0f);
                }

                return;
            }

            if (message.GameState != Utility.EnumManager.GameState.Playing)
            {
                return;
            }

            _model.Reset();
            for (int i = 0; i < _ringHasWall.Length; i++)
            {
                _ringHasWall[i] = false;
            }

            _outermostWallIndex = -1;
            _isPaused = false;
            _isSlowMotion = false;
            _lastClockSecond = -1;

            if (_view != null)
            {
                _view.SetTimeScale(1f);
            }
        }
    }
}
