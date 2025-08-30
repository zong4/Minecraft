using UnityEngine;

namespace Noise
{
    public class Map : MonoBehaviour
    {
        public int seed;
        private static System.Random _random;

        // todo
        public const int Length = 256;
        public const int Width = 256;
        public const int Height = 32;

        public static float OffsetX;
        public static float OffsetY;
        public static float OffsetZ;

        private static readonly float[,,] NoiseMap2D = new float[Length, Width, 2];
        private static readonly float[,,,] NoiseMap3D = new float[Length, Width, Height, 3];

        private void Awake()
        {
            _random = new System.Random(seed);

            OffsetX = (float)_random.NextDouble() * Length;
            OffsetY = (float)_random.NextDouble() * Width;
            OffsetZ = (float)_random.NextDouble() * Height;
        }

        private void Start()
        {
            PerlinNoise.Generate2DMap(NoiseMap2D);
            PerlinNoise.Generate3DMap(NoiseMap3D);
        }

        public static double GetNextDouble()
        {
            return _random.NextDouble();
        }

        public static float GetPerlinValue(float x, float y)
        {
            var x0 = Mathf.FloorToInt(x);
            var y0 = Mathf.FloorToInt(y);
            var dx = x - x0;
            var dy = y - y0;

            var x0W = Wrap(x0, Length);
            var y0W = Wrap(y0, Width);
            var x1W = Wrap(x0 + 1, Length);
            var y1W = Wrap(y0 + 1, Width);

            var value00 = Vector2.Dot(new Vector2(NoiseMap2D[x0W, y0W, 0], NoiseMap2D[x0W, y0W, 1]),
                new Vector2(dx, dy));
            var value10 = Vector2.Dot(new Vector2(NoiseMap2D[x1W, y0W, 0], NoiseMap2D[x1W, y0W, 1]),
                new Vector2(dx - 1, dy));
            var value01 = Vector2.Dot(new Vector2(NoiseMap2D[x0W, y1W, 0], NoiseMap2D[x0W, y1W, 1]),
                new Vector2(dx, dy - 1));
            var value11 = Vector2.Dot(new Vector2(NoiseMap2D[x1W, y1W, 0], NoiseMap2D[x1W, y1W, 1]),
                new Vector2(dx - 1, dy - 1));

            // var u = Fade(x - x0);
            // var v = Fade(y - y0);
            var u = x - x0;
            var v = y - y0;

            var noise = Mathf.Lerp(Mathf.Lerp(value00, value10, u), Mathf.Lerp(value01, value11, u), v);

            return (noise + 1) * 0.5f; // [0, 1]
        }

        public static float GetPerlinFractalValue(float x, float y, int octaves, float lacunarity = 2,
            float persistence = 0.5f)
        {
            var total = 0f;
            var frequency = 1f;
            var amplitude = 1f;
            var maxValue = 0f;

            for (var i = 0; i < octaves; i++)
            {
                total += GetPerlinValue(x * frequency, y * frequency) * amplitude;
                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return total / maxValue;
        }

        public static float GetUnityPerlinFractalValue(float x, float y, int octaves, float lacunarity = 2,
            float persistence = 0.5f)
        {
            var total = 0f;
            var frequency = 1f;
            var amplitude = 1f;
            var maxValue = 0f;

            for (var i = 0; i < octaves; i++)
            {
                total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return total / maxValue;
        }

        public static float GetPerlin3DValue(float x, float y, float z)
        {
            var x0 = Mathf.FloorToInt(x);
            var y0 = Mathf.FloorToInt(y);
            var z0 = Mathf.FloorToInt(z);
            var dx = x - x0;
            var dy = y - y0;
            var dz = z - z0;

            var x0W = Wrap(x0, Length);
            var y0W = Wrap(y0, Width);
            var z0W = Wrap(z0, Height);
            var x1W = Wrap(x0 + 1, Length);
            var y1W = Wrap(y0 + 1, Width);
            var z1W = Wrap(z0 + 1, Height);

            var value000 = Vector3.Dot(new Vector3(NoiseMap3D[x0W, y0W, z0W, 0], NoiseMap3D[x0W, y0W, z0W, 1],
                NoiseMap3D[x0W, y0W, z0W, 2]), new Vector3(dx, dy, dz));
            var value100 = Vector3.Dot(new Vector3(NoiseMap3D[x1W, y0W, z0W, 0], NoiseMap3D[x1W, y0W, z0W, 1],
                NoiseMap3D[x1W, y0W, z0W, 2]), new Vector3(dx - 1, dy, dz));
            var value010 = Vector3.Dot(new Vector3(NoiseMap3D[x0W, y1W, z0W, 0], NoiseMap3D[x0W, y1W, z0W, 1],
                NoiseMap3D[x0W, y1W, z0W, 2]), new Vector3(dx, dy - 1, dz));
            var value110 = Vector3.Dot(new Vector3(NoiseMap3D[x1W, y1W, z0W, 0], NoiseMap3D[x1W, y1W, z0W, 1],
                NoiseMap3D[x1W, y1W, z0W, 2]), new Vector3(dx - 1, dy - 1, dz));

            var value001 = Vector3.Dot(new Vector3(NoiseMap3D[x0W, y0W, z1W, 0], NoiseMap3D[x0W, y0W, z1W, 1],
                NoiseMap3D[x0W, y0W, z1W, 2]), new Vector3(dx, dy, dz - 1));
            var value101 = Vector3.Dot(new Vector3(NoiseMap3D[x1W, y0W, z1W, 0], NoiseMap3D[x1W, y0W, z1W, 1],
                NoiseMap3D[x1W, y0W, z1W, 2]), new Vector3(dx - 1, dy, dz - 1));
            var value011 = Vector3.Dot(new Vector3(NoiseMap3D[x0W, y1W, z1W, 0], NoiseMap3D[x0W, y1W, z1W, 1],
                NoiseMap3D[x0W, y1W, z1W, 2]), new Vector3(dx, dy - 1, dz - 1));
            var value111 = Vector3.Dot(new Vector3(NoiseMap3D[x1W, y1W, z1W, 0], NoiseMap3D[x1W, y1W, z1W, 1],
                NoiseMap3D[x1W, y1W, z1W, 2]), new Vector3(dx - 1, dy - 1, dz - 1));

            // var u = Fade(x - x0);
            // var v = Fade(y - y0);
            // var w = Fade(z - z0);
            var u = x - x0;
            var v = y - y0;
            var w = z - z0;

            var noise = Mathf.Lerp(
                Mathf.Lerp(Mathf.Lerp(value000, value100, u), Mathf.Lerp(value010, value110, u), v),
                Mathf.Lerp(Mathf.Lerp(value001, value101, u), Mathf.Lerp(value011, value111, u), v), w);

            return (noise + 1) * 0.5f; // [0, 1]
        }

        private static int Wrap(int coord, int size)
        {
            coord %= size;
            return coord < 0 ? coord + size : coord;
        }

        private static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }
    }
}