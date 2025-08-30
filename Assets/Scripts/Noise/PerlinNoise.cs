using UnityEngine;

namespace Noise
{
    public static class PerlinNoise
    {
        public static void Generate2DMap(float[,,] noiseMap)
        {
            for (var x = 0; x < noiseMap.GetLength(0); x++)
            {
                for (var y = 0; y < noiseMap.GetLength(1); y++)
                {
                    var radius = (float)Map.GetNextDouble() * 2 * Mathf.PI;
                    noiseMap[x, y, 0] = Mathf.Cos(radius);
                    noiseMap[x, y, 1] = Mathf.Sin(radius);
                }
            }
        }

        public static void Generate3DMap(float[,,,] noiseMap)
        {
            for (var x = 0; x < noiseMap.GetLength(0); x++)
            {
                for (var y = 0; y < noiseMap.GetLength(1); y++)
                {
                    for (var z = 0; z < noiseMap.GetLength(2); z++)
                    {
                        var alpha = (float)Map.GetNextDouble() * 2 * Mathf.PI;
                        var beta = (float)Map.GetNextDouble() * 2 * Mathf.PI;

                        noiseMap[x, y, z, 0] = Mathf.Cos(beta) * Mathf.Cos(alpha);
                        noiseMap[x, y, z, 1] = Mathf.Cos(beta) * Mathf.Sin(alpha);
                        noiseMap[x, y, z, 2] = Mathf.Sin(beta);
                    }
                }
            }
        }
    }
}