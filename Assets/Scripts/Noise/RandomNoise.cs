namespace Noise
{
    public static class RandomNoise
    {
        public static void GenerateMap(float[,] noiseMap)
        {
            for (var x = 0; x < noiseMap.GetLength(0); x++)
            {
                for (var y = 0; y < noiseMap.GetLength(1); y++)
                {
                    noiseMap[x, y] = (float)Map.GetNextDouble();
                }
            }
        }
    }
}