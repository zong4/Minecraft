using System;
using UnityEngine;
using Noise;

public enum NoiseType
{
    Random,
    Random4,
    Perlin,
    UnityPerlin,
    PerlinFractal,
    UnityPerlinFractal
}

[Serializable]
public class NoiseParams
{
    public NoiseType noiseType;
    public int scale;
    public int octaves;

    public NoiseParams New()
    {
        return new NoiseParams
        {
            noiseType = noiseType,
            scale = scale,
            octaves = octaves
        };
    }

    public bool Equal(NoiseParams other)
    {
        return noiseType == other.noiseType && scale == other.scale && octaves == other.octaves;
    }
}

public enum LandformType
{
    Default,
    IslandEuclidean,
    IslandSquareBump,
    IslandEuclidean2,
    IslandManhattan
}

[Serializable]
public class LandformParams
{
    public LandformType landformType = LandformType.Default;
    [Range(0, 1f)] public float landformMix = 0.5f;

    public LandformParams New()
    {
        return new LandformParams
        {
            landformType = landformType,
            landformMix = landformMix
        };
    }

    public bool Equal(LandformParams other)
    {
        return landformType == other.landformType && Mathf.Approximately(landformMix, other.landformMix);
    }
}

[Serializable]
public class BiomeParams
{
    [Range(0, 1f)] public float caveThreshold = 0.5f;
    public int seaLevel;

    [Range(0, 1f)] public float minTemperatureThreshold = 0.333f;
    [Range(0, 1f)] public float maxTemperatureThreshold = 0.666f;

    [Range(0, 1f)] public float humidityThreshold = 0.5f;

    public BiomeParams New()
    {
        return new BiomeParams
        {
            caveThreshold = caveThreshold,
            seaLevel = seaLevel,

            minTemperatureThreshold = minTemperatureThreshold,
            maxTemperatureThreshold = maxTemperatureThreshold,

            humidityThreshold = humidityThreshold,
        };
    }

    public bool Equal(BiomeParams other)
    {
        return Mathf.Approximately(caveThreshold, other.caveThreshold) && seaLevel == other.seaLevel &&
               Mathf.Approximately(minTemperatureThreshold, other.minTemperatureThreshold) &&
               Mathf.Approximately(maxTemperatureThreshold, other.maxTemperatureThreshold) &&
               Mathf.Approximately(humidityThreshold, other.humidityThreshold);
    }
}

public class Landform : MonoBehaviour
{
    public int length;
    private int _oldLength;
    public int width;
    private int _oldWidth;

    // Noise
    public NoiseParams noiseParams;
    private NoiseParams _oldNoiseParams;
    private float[,] _noiseMap2D; // todo

    // Landform
    public LandformParams landformParams;
    private LandformParams _oldLandformParams;
    private int[,] _heightMap;

    // Biome
    public BiomeParams biomeParams;
    private BiomeParams _oldBiomeParams;
    private float[,] _temperatureMap;
    private float[,] _humidityMap;

    // Prefab
    public GameObject rockBlockPrefab;
    public GameObject waterBlockPrefab;
    public GameObject coldBlockPrefab;
    public GameObject coldDryBlockPrefab;
    public GameObject temperateBlockPrefab;
    public GameObject temperateDryBlockPrefab;
    public GameObject hotBlockPrefab;
    public GameObject hotDryBlockPrefab;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        GenerateNoiseMap();
        GenerateHeightMap();
        GenerateTemperatureMap();
        GenerateHumidityMap();

        VisualizeMap();
    }

    private void Update()
    {
        if (_oldLength != length || _oldWidth != width)
        {
            Initialize();

            GenerateNoiseMap();
            GenerateHeightMap();
            GenerateTemperatureMap();
            GenerateHumidityMap();

            VisualizeMap();
        }

        if (!_oldNoiseParams.Equal(noiseParams) ||
            !_oldLandformParams.Equal(landformParams))
        {
            Initialize();

            GenerateHeightMap();
            GenerateHumidityMap();

            VisualizeMap();
        }

        if (!_oldBiomeParams.Equal(biomeParams))
        {
            Initialize();

            VisualizeMap();
        }
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        Initialize();

        GenerateNoiseMap();
        GenerateHeightMap();
        GenerateTemperatureMap();
        GenerateHumidityMap();

        VisualizeMap();
    }


    private void Initialize()
    {
        _oldLength = length;
        _oldWidth = width;

        _oldNoiseParams = noiseParams.New();
        _oldLandformParams = landformParams.New();
        _oldBiomeParams = biomeParams.New();
    }

    private void GenerateNoiseMap()
    {
        _noiseMap2D = new float[length, width];
        RandomNoise.GenerateMap(_noiseMap2D);
    }

    private void GenerateHeightMap()
    {
        _heightMap = new int[length, width];

        for (var x = 0; x < length; x++)
        {
            for (var y = 0; y < width; y++)
            {
                float value = 0;

                switch (noiseParams.noiseType)
                {
                    case NoiseType.Random:
                        value = _noiseMap2D[x, y];
                        break;
                    case NoiseType.Random4:
                        value = (_noiseMap2D[x, y] + _noiseMap2D[(x + 1) % length, y] +
                                 _noiseMap2D[x, (y + 1) % width] +
                                 _noiseMap2D[(x + 1) % length, (y + 1) % width]) *
                                0.25f;
                        break;
                    case NoiseType.Perlin:
                    case NoiseType.UnityPerlin:
                    case NoiseType.PerlinFractal:
                    case NoiseType.UnityPerlinFractal:
                    default:
                    {
                        var xf = x / (float)length * noiseParams.scale + Map.OffsetX;
                        var yf = y / (float)width * noiseParams.scale + Map.OffsetY;

                        switch (noiseParams.noiseType)
                        {
                            case NoiseType.Perlin:
                                value = Map.GetPerlinValue(xf, yf);
                                break;
                            case NoiseType.UnityPerlin:
                                value = Mathf.PerlinNoise(xf, yf);
                                break;
                            case NoiseType.PerlinFractal:
                                value = Map.GetPerlinFractalValue(xf, yf, noiseParams.octaves);
                                break;
                            case NoiseType.UnityPerlinFractal:
                                value = Map.GetUnityPerlinFractalValue(xf, yf, noiseParams.octaves);
                                break;
                            case NoiseType.Random:
                            case NoiseType.Random4:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        value = RedistributionNoise(value, x, y);
                        break;
                    }
                }

                _heightMap[x, y] = Mathf.FloorToInt(value * Map.Height);
            }
        }
    }

    private void GenerateTemperatureMap()
    {
        _temperatureMap = new float[length, width];
        var temperatureScale = Mathf.FloorToInt((float)Map.GetNextDouble() * Map.Length);
        var temperatureOffsetX = (float)Map.GetNextDouble() * Map.Length;
        var temperatureOffsetY = (float)Map.GetNextDouble() * Map.Width;

        for (var x = 0; x < length; x++)
        {
            for (var y = 0; y < width; y++)
            {
                _temperatureMap[x, y] = Mathf.PerlinNoise(
                    x / (float)length * temperatureScale + temperatureOffsetX,
                    y / (float)width * temperatureScale + temperatureOffsetY);
            }
        }

        LandformDLA.Blur(ref _temperatureMap);
    }

    private void GenerateHumidityMap()
    {
        _humidityMap = new float[length, width];

        for (var x = 0; x < length; x++)
        {
            for (var y = 0; y < width; y++)
            {
                _humidityMap[x, y] = _heightMap[x, y] < biomeParams.seaLevel ? 1 : 0;
            }
        }

        Expand(ref _humidityMap);
        Expand(ref _humidityMap);
        // Expand(ref _humidityMap);
    }

    private void VisualizeMap()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (var x = 0; x < length; x++)
        {
            for (var y = 0; y < width; y++)
            {
                var height = _heightMap[x, y];

                // var block = Instantiate(rockBlockPrefab, new Vector3(x, height * 0.5f, y),
                //     Quaternion.identity, this.transform);
                // block.transform.localScale = new Vector3(1, height, 1);

                int z;
                for (z = 0; z < biomeParams.seaLevel; z++)
                {
                    if (z < height)
                    {
                        var density = Map.GetPerlin3DValue(
                            x / (float)length * noiseParams.scale + Map.OffsetX,
                            y / (float)width * noiseParams.scale + Map.OffsetY,
                            z / (float)Map.Height * noiseParams.scale + Map.OffsetZ);
                
                        if (density <= biomeParams.caveThreshold)
                            continue;
                
                        var block = Instantiate(rockBlockPrefab, new Vector3(x, z + 0.5f, y),
                            Quaternion.identity, this.transform);
                        block.transform.localScale = new Vector3(1, 1, 1);
                    }
                    else
                    {
                        var block = Instantiate(waterBlockPrefab, new Vector3(x, z + 0.5f, y),
                            Quaternion.identity, this.transform);
                        block.transform.localScale = new Vector3(1, 1, 1);
                    }
                }
                
                for (; z < height; z++)
                {
                    var density = Map.GetPerlin3DValue(x / (float)length * noiseParams.scale + Map.OffsetX,
                        y / (float)width * noiseParams.scale + Map.OffsetY,
                        z / (float)Map.Height * noiseParams.scale + Map.OffsetZ);
                
                    if (density <= biomeParams.caveThreshold)
                        continue;
                
                    // todo
                    if (_temperatureMap[x, y] - 0.01 * (z - biomeParams.seaLevel) < biomeParams.minTemperatureThreshold)
                    {
                        if (_humidityMap[x, y] - 0.01 * (z - biomeParams.seaLevel) > biomeParams.humidityThreshold)
                        {
                            var block = Instantiate(coldBlockPrefab, new Vector3(x, z + 0.5f, y),
                                Quaternion.identity, this.transform);
                            block.transform.localScale = new Vector3(1, 1, 1);
                        }
                        else
                        {
                            var block = Instantiate(coldDryBlockPrefab, new Vector3(x, z + 0.5f, y),
                                Quaternion.identity, this.transform);
                            block.transform.localScale = new Vector3(1, 1, 1);
                        }
                    }
                    else if (_temperatureMap[x, y] - 0.01 * (z - biomeParams.seaLevel) <
                             biomeParams.maxTemperatureThreshold)
                    {
                        if (_humidityMap[x, y] - 0.01 * (z - biomeParams.seaLevel) > biomeParams.humidityThreshold)
                        {
                            var block = Instantiate(temperateBlockPrefab, new Vector3(x, z + 0.5f, y),
                                Quaternion.identity, this.transform);
                            block.transform.localScale = new Vector3(1, 1, 1);
                        }
                        else
                        {
                            var block = Instantiate(temperateDryBlockPrefab, new Vector3(x, z + 0.5f, y),
                                Quaternion.identity, this.transform);
                            block.transform.localScale = new Vector3(1, 1, 1);
                        }
                    }
                    else
                    {
                        if (_humidityMap[x, y] - 0.01 * (z - biomeParams.seaLevel) > biomeParams.humidityThreshold)
                        {
                            var block = Instantiate(hotBlockPrefab, new Vector3(x, z + 0.5f, y),
                                Quaternion.identity, this.transform);
                            block.transform.localScale = new Vector3(1, 1, 1);
                        }
                        else
                        {
                            var block = Instantiate(hotDryBlockPrefab, new Vector3(x, z + 0.5f, y),
                                Quaternion.identity, this.transform);
                            block.transform.localScale = new Vector3(1, 1, 1);
                        }
                    }
                }
            }
        }
    }

    private float RedistributionNoise(float value, int x, int y)
    {
        if (landformParams.landformType == LandformType.Default)
            return value;

        // [-1, 1]
        var xf = 2 * x / (float)length - 1;
        var yf = 2 * y / (float)width - 1;

        var distance = landformParams.landformType switch
        {
            LandformType.Default => 1,

            LandformType.IslandEuclidean => Mathf.Sqrt(xf * xf + yf * yf) / Mathf.Sqrt(2),

            // Square Bump d = 1 - (1-x²) * (1-y²)
            LandformType.IslandSquareBump => 1 - (1 - xf * xf) * (1 - yf * yf),

            // Euclidean² d = min(1, (nx² + ny²) / sqrt(2))
            LandformType.IslandEuclidean2 => Math.Min(1, (xf * xf + yf * yf) / Mathf.Sqrt(2)),

            // Manhattan d = |nx| + |ny|
            LandformType.IslandManhattan => (Mathf.Abs(xf) + Mathf.Abs(yf)) / 2,

            _ => throw new NotImplementedException()
        };
        value = Mathf.Lerp(value, 1 - distance, landformParams.landformMix);

        // todo
        value = Mathf.Clamp(value, 0, 1);

        return value;
    }

    public static void Expand(ref float[,] map)
    {
        var length = map.GetLength(0);
        var width = map.GetLength(1);
        var newMap = map;

        var directions = new[]
        {
            (-1, 0), (1, 0), (0, -1), (0, 1)
        };

        for (var x = 0; x < length; x++)
        {
            for (var y = 0; y < width; y++)
            {
                map[x, y] = Mathf.Clamp01(map[x, y]);

                foreach (var (dx, dy) in directions)
                {
                    newMap[(x + dx + length) % length, (y + dy + width) % width] += map[x, y] * 0.25f;
                }
            }
        }

        map = newMap;
    }
}