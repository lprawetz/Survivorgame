using Godot;
using System.Collections.Generic;

namespace SurvivorGame
{
    public enum BiomeType
    {
        Water,
        FrozenWater,
        Swamp,
        Meadow,
        Forest,
        Mountains,
        Desert,
        IceWaste,
        IceMountain,
        Volcano
    }

    /// <summary>
    /// Prozeduraler Weltgenerator. Erzeugt Chunks um den Spieler herum basierend auf drei
    /// Noise-Schichten (Höhe, Feuchtigkeit, Temperatur) und einem Zonensystem:
    ///   Kernzone  (< 800 Tiles): Wald, Wiese, Berge, Sumpf, Wasser
    ///   Außenzone (800–1500):    Übergänge zu Extrembiomen
    ///   Fernzone  (> 1500):      Eiswüste, Vulkan, Wüste, gefrorenes Wasser
    ///
    /// KONFIGURATION IM EDITOR:
    ///   - TileSourceId: ID der TileSet-Atlas-Quelle im Godot-Editor
    ///   - BiomeAtlasRow: Atlas-Zeile; jede Biome-Spalte entspricht einer BiomeType-Spalte
    ///   - Lege im TileMap-Node die TileSet-Ressource an, dann passe die Atlas-Koordinaten
    ///     in der BiomeAtlasCoords-Dictionary an.
    /// </summary>
    public partial class MapGenerator : TileMap
    {
        [Export] public int ChunkSize     { get; set; } = 32;
        [Export] public int TileSourceId  { get; set; } = 0;
        [Export] public int ViewDistance  { get; set; } = 2;  // Chunks um den Spieler

        // Atlas-Koordinaten je Biom – im Editor nach Bedarf anpassen
        private static readonly Dictionary<BiomeType, Vector2I> BiomeAtlasCoords = new()
        {
            { BiomeType.Water,       new Vector2I(0, 0) },
            { BiomeType.FrozenWater, new Vector2I(1, 0) },
            { BiomeType.Swamp,       new Vector2I(2, 0) },
            { BiomeType.Meadow,      new Vector2I(3, 0) },
            { BiomeType.Forest,      new Vector2I(4, 0) },
            { BiomeType.Mountains,   new Vector2I(5, 0) },
            { BiomeType.Desert,      new Vector2I(6, 0) },
            { BiomeType.IceWaste,    new Vector2I(7, 0) },
            { BiomeType.IceMountain, new Vector2I(8, 0) },
            { BiomeType.Volcano,     new Vector2I(9, 0) },
        };

        private FastNoiseLite _altitudeNoise    = new();
        private FastNoiseLite _moistureNoise    = new();
        private FastNoiseLite _temperatureNoise = new();

        private readonly HashSet<Vector2I> _generatedChunks = new();
        private Node2D _player;

        // Zonenradien in Tiles (entspricht den Planvorgaben)
        private const float InnerZoneRadius = 800f;
        private const float OuterZoneRadius = 1500f;

        public override void _Ready()
        {
            var rng = new RandomNumberGenerator();
            rng.Randomize();

            _altitudeNoise.Seed    = (int)rng.Randi();
            _moistureNoise.Seed    = (int)rng.Randi();
            _temperatureNoise.Seed = (int)rng.Randi();

            // Frequenzen bestimmen die „Korngröße" der Biome
            _altitudeNoise.Frequency    = 0.004f;
            _moistureNoise.Frequency    = 0.006f;
            _temperatureNoise.Frequency = 0.005f;

            _altitudeNoise.NoiseType    = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
            _moistureNoise.NoiseType    = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
            _temperatureNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
        }

        public override void _Process(double delta)
        {
            if (_player == null)
                _player = GetTree().GetFirstNodeInGroup("player") as Node2D;

            if (_player != null)
                GenerateChunksAround(_player.GlobalPosition);
        }

        private void GenerateChunksAround(Vector2 worldPos)
        {
            Vector2I centerChunk = WorldToChunk(worldPos);

            for (int cx = -ViewDistance; cx <= ViewDistance; cx++)
            {
                for (int cy = -ViewDistance; cy <= ViewDistance; cy++)
                {
                    Vector2I chunk = centerChunk + new Vector2I(cx, cy);
                    if (!_generatedChunks.Contains(chunk))
                    {
                        GenerateChunk(chunk);
                        _generatedChunks.Add(chunk);
                    }
                }
            }
        }

        private void GenerateChunk(Vector2I chunkCoord)
        {
            int startX = chunkCoord.X * ChunkSize;
            int startY = chunkCoord.Y * ChunkSize;

            for (int x = startX; x < startX + ChunkSize; x++)
            {
                for (int y = startY; y < startY + ChunkSize; y++)
                {
                    BiomeType biome = DetermineBiome(x, y);
                    SetCell(0, new Vector2I(x, y), TileSourceId, BiomeAtlasCoords[biome]);
                }
            }
        }

        private BiomeType DetermineBiome(int x, int y)
        {
            float distFromCenter = Mathf.Sqrt(x * x + y * y);
            float altitude    = _altitudeNoise.GetNoise2D(x, y);
            float moisture    = _moistureNoise.GetNoise2D(x, y);
            float temperature = _temperatureNoise.GetNoise2D(x, y);

            // Temperatur sinkt mit Abstand vom Zentrum (kältere Außenzone)
            float zoneInfluence  = Mathf.Clamp(distFromCenter / OuterZoneRadius, 0f, 2f);
            float adjustedTemp   = temperature - zoneInfluence * 0.55f;

            if (distFromCenter > OuterZoneRadius)
                return DetermineFarBiome(altitude, adjustedTemp);
            if (distFromCenter > InnerZoneRadius)
                return DetermineOuterBiome(altitude, moisture, adjustedTemp);

            return DetermineInnerBiome(altitude, moisture);
        }

        // Kernzone: lebendige, grüne Biome
        private static BiomeType DetermineInnerBiome(float altitude, float moisture)
        {
            if (altitude < -0.35f)                           return BiomeType.Water;
            if (altitude < -0.10f && moisture >  0.15f)     return BiomeType.Swamp;
            if (altitude <  0.25f && moisture >  0.20f)     return BiomeType.Forest;
            if (altitude <  0.25f)                           return BiomeType.Meadow;
            return BiomeType.Mountains;
        }

        // Übergangszone: Extreme Biome beginnen sich einzumischen
        private static BiomeType DetermineOuterBiome(float altitude, float moisture, float adjustedTemp)
        {
            if (adjustedTemp < -0.20f)
            {
                if (altitude < -0.30f) return BiomeType.FrozenWater;
                if (altitude >  0.30f) return BiomeType.IceMountain;
                return BiomeType.IceWaste;
            }
            if (adjustedTemp > 0.25f)
            {
                if (altitude < -0.30f) return BiomeType.Water;
                if (altitude >  0.35f) return BiomeType.Volcano;
                return BiomeType.Desert;
            }
            // Dazwischen: normale Kernzone-Logik
            return DetermineInnerBiome(altitude, moisture);
        }

        // Fernzone: nur extreme Biome
        private static BiomeType DetermineFarBiome(float altitude, float adjustedTemp)
        {
            if (adjustedTemp < -0.10f)
            {
                if (altitude < -0.30f) return BiomeType.FrozenWater;
                if (altitude >  0.30f) return BiomeType.IceMountain;
                return BiomeType.IceWaste;
            }
            if (altitude < -0.30f) return BiomeType.Water;
            if (altitude >  0.40f) return BiomeType.Volcano;
            return BiomeType.Desert;
        }

        private Vector2I WorldToChunk(Vector2 worldPos)
        {
            Vector2I tilePos = LocalToMap(worldPos);
            return new Vector2I(
                Mathf.FloorToInt((float)tilePos.X / ChunkSize),
                Mathf.FloorToInt((float)tilePos.Y / ChunkSize)
            );
        }

        /// <summary>
        /// Gibt das Biom an einer bestimmten Weltposition zurück.
        /// Nützlich für biom-spezifische Gegner oder Boni.
        /// </summary>
        public BiomeType GetBiomeAt(Vector2 worldPos)
        {
            Vector2I tilePos = LocalToMap(worldPos);
            return DetermineBiome(tilePos.X, tilePos.Y);
        }
    }
}

