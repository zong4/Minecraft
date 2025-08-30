using UnityEngine;

namespace Noise
{
    public static class DiffusionLimitedAggregation
    {
        public static int AddParticle2D(int[,] grid, int number, float density)
        {
            var length = grid.GetLength(0);
            var width = grid.GetLength(1);

            while ((float)number / (length * width) < density)
            {
                AddSingleParticle2D(grid, length, width);
                number++;
            }

            return number;
        }

        private static void AddSingleParticle2D(int[,] grid, int length, int width)
        {
            var x = Mathf.FloorToInt((float)Map.GetNextDouble() * length);
            var y = Mathf.FloorToInt((float)Map.GetNextDouble() * width);
            while (grid[x, y] > 0)
            {
                x = Mathf.FloorToInt((float)Map.GetNextDouble() * length);
                y = Mathf.FloorToInt((float)Map.GetNextDouble() * width);
            }

            while (grid[(x + 1) % length, y] == 0 && grid[(x - 1 + length) % length, y] == 0 &&
                   grid[x, (y + 1) % width] == 0 && grid[x, (y - 1 + width) % width] == 0)
            {
                var direction = Mathf.FloorToInt((float)Map.GetNextDouble() * 4);
                switch (direction)
                {
                    case 0:
                        x = (x + 1) % length;
                        break;
                    case 1:
                        x = (x - 1 + length) % length;
                        break;
                    case 2:
                        y = (y + 1) % width;
                        break;
                    case 3:
                        y = (y - 1 + width) % width;
                        break;
                }
            }

            grid[x, y] = 1;
        }
    }
}