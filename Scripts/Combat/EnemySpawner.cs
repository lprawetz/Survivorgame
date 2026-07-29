using Godot;
using System;

namespace SurvivorGame.Combat
{
    /// <summary>
    /// Spawnt Gegner um den Spieler herum. Schwierigkeit und Spawn-Rate steigen über Zeit.
    /// Verbinde das EnemyDied-Signal jedes Gegners mit dem ExperienceSystem.
    ///
    /// Im Godot-Editor:
    ///   - EnemyScenes: Array mit PackedScene-Ressourcen der Gegner-Szenen
    ///   - Nach ca. 30 Minuten (1800 s) spawnt zusätzlich ein Endboss (TODO: BossScene setzen)
    /// </summary>
    public partial class EnemySpawner : Node
    {
        [Export] public PackedScene[] EnemyScenes             { get; set; } = Array.Empty<PackedScene>();
        [Export] public PackedScene   BossScene                { get; set; }
        [Export] public float         BaseSpawnInterval        { get; set; } = 2.0f;
        [Export] public float         SpawnRadius              { get; set; } = 500f;
        [Export] public float         DifficultyIncreaseInterval{ get; set; } = 30f;
        [Export] public float         BossSpawnTime            { get; set; } = 1800f; // 30 Minuten

        private Timer  _spawnTimer;
        private Timer  _difficultyTimer;
        private Timer  _bossTimer;
        private Node2D _player;
        private int    _difficultyLevel = 1;
        private bool   _bossSpawned;
        private readonly RandomNumberGenerator _rng = new();

        // Gibt Erfahrung und Gold weiter – verbinde dies mit ExperienceSystem
        [Signal] public delegate void DropCollectedEventHandler(int experience, int gold);

        public override void _Ready()
        {
            _rng.Randomize();

            _spawnTimer = new Timer { WaitTime = BaseSpawnInterval, Autostart = true };
            _spawnTimer.Timeout += OnSpawnTimerTimeout;
            AddChild(_spawnTimer);

            _difficultyTimer = new Timer { WaitTime = DifficultyIncreaseInterval, Autostart = true };
            _difficultyTimer.Timeout += OnDifficultyTimerTimeout;
            AddChild(_difficultyTimer);

            _bossTimer = new Timer { WaitTime = BossSpawnTime, OneShot = true, Autostart = true };
            _bossTimer.Timeout += OnBossTimerTimeout;
            AddChild(_bossTimer);
        }

        private void OnSpawnTimerTimeout()
        {
            if (_player == null)
                _player = GetTree().GetFirstNodeInGroup("player") as Node2D;
            if (_player == null || EnemyScenes.Length == 0) return;

            // Mit steigendem Schwierigkeitsgrad mehr Gegner pro Welle
            int spawnCount = 1 + (_difficultyLevel - 1) / 3;
            for (int i = 0; i < spawnCount; i++)
                SpawnEnemy();
        }

        private void SpawnEnemy()
        {
            int sceneIndex = _rng.RandiRange(0, EnemyScenes.Length - 1);
            var scene = EnemyScenes[sceneIndex];
            if (scene == null) return;

            var enemy = scene.Instantiate<EnemyBase>();
            enemy.GlobalPosition = RandomPositionAroundPlayer();
            enemy.ApplyDifficultyScale(_difficultyLevel);
            enemy.EnemyDied += OnEnemyDied;

            GetParent().AddChild(enemy);
        }

        private void OnEnemyDied(Vector2 position, int experience, int gold)
        {
            EmitSignal(SignalName.DropCollected, experience, gold);
        }

        private void OnDifficultyTimerTimeout()
        {
            _difficultyLevel++;
            float newInterval = Mathf.Max(0.5f, BaseSpawnInterval - _difficultyLevel * 0.08f);
            _spawnTimer.WaitTime = newInterval;
        }

        private void OnBossTimerTimeout()
        {
            if (_bossSpawned || BossScene == null) return;
            _bossSpawned = true;

            var boss = BossScene.Instantiate<EnemyBase>();
            boss.GlobalPosition = RandomPositionAroundPlayer();
            boss.EnemyDied += OnEnemyDied;
            GetParent().AddChild(boss);
        }

        private Vector2 RandomPositionAroundPlayer()
        {
            float angle = _rng.RandfRange(0f, Mathf.Tau);
            float dist  = _rng.RandfRange(SpawnRadius * 0.8f, SpawnRadius);
            return _player.GlobalPosition + new Vector2(
                Mathf.Cos(angle) * dist,
                Mathf.Sin(angle) * dist
            );
        }
    }
}
