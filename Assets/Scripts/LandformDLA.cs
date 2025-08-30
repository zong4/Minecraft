using Noise;
using UnityEngine;

// ReSharper disable once InconsistentNaming
public class LandformDLA : MonoBehaviour
{
    public int maxLength = 64;
    public int maxWidth = 64;
    [Range(0, 1f)] public float density = 0.5f;
    private float[,] _heightMap;

    public GameObject blockPrefab;

    private void Awake()
    {
        _heightMap = new float[maxLength, maxWidth];
    }

    private void Start()
    {
        var length = 4;
        var width = 4;
        var grid = new int[length, width];

        var number = 1;
        grid[2, 2] = 1;

        while (length <= maxLength && width <= maxWidth)
        {
            number = DiffusionLimitedAggregation.AddParticle2D(grid, number, density);

            for (var x = 0; x < length; x++)
            {
                for (var y = 0; y < width; y++)
                {
                    _heightMap[x + (maxLength - length) / 2, y + (maxWidth - width) / 2] += grid[x, y];
                }
            }

            Blur(ref _heightMap);

            length += 4;
            width += 4;
            var oldGrid = grid;
            grid = new int[length, width];
            for (var x = 2; x < 2 + oldGrid.GetLength(0); x++)
            {
                for (var y = 2; y < 2 + oldGrid.GetLength(1); y++)
                {
                    grid[x, y] = oldGrid[x - 2, y - 2];
                }
            }
        }

        // Blur(ref _heightMap);

        VisualizeHeightMap();
    }

    public static void Blur(ref float[,] map)
    {
        var length = map.GetLength(0);
        var width = map.GetLength(1);
        var newMap = new float[length, width];

        for (var x = 0; x < length; x++)
        {
            for (var y = 0; y < width; y++)
            {
                newMap[x, y] = map[x, y] + map[(x - 1 + length) % length, y] +
                               map[(x + 1) % length, y] +
                               map[x, (y - 1 + width) % width] +
                               map[x, (y + 1) % width];
                newMap[x, y] /= 5f;
            }
        }

        map = newMap;
    }

    private void VisualizeHeightMap()
    {
        for (var x = 0; x < maxLength; x++)
        {
            for (var y = 0; y < maxWidth; y++)
            {
                if (_heightMap[x, y] > 0)
                {
                    // var h = Mathf.FloorToInt(_height[x, y]);
                    //
                    // for (var z = 0; z < h; z++)
                    // {
                    //     var block = Instantiate(blockPrefab, new Vector3(x, z + 0.5f, y),
                    //         Quaternion.identity, this.transform);
                    //     block.transform.localScale = new Vector3(1, 1, 1);
                    // }

                    var h = _heightMap[x, y];

                    var block = Instantiate(blockPrefab, new Vector3(x, h * 0.5f, y),
                        Quaternion.identity, this.transform);
                    block.transform.localScale = new Vector3(1, h, 1);
                }
            }
        }
    }
}