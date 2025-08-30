using Noise;
using UnityEngine;

public class LandformRandomV1 : MonoBehaviour
{
    private int _maxLength;
    private int _maxWidth;
    public int index = 2;
    [Range(0, 1f)] public float sparsity = 0.5f;
    private float _oldSparsity;
    private float[,] _height;

    public GameObject blockPrefab;

    private void Awake()
    {
        _maxLength = 4 + index * Map.Height;
        _maxWidth = 4 + index * Map.Height;

        _oldSparsity = sparsity;
    }

    private void Start()
    {
        GenerateHeightMap();
        VisualizeHeightMap();
    }

    private void Update()
    {
        if (Mathf.Abs(_oldSparsity - sparsity) > Mathf.Epsilon)
        {
            _oldSparsity = sparsity;

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            GenerateHeightMap();
            VisualizeHeightMap();
        }
    }

    private void GenerateHeightMap()
    {
        _height = new float[_maxLength, _maxWidth];

        var length = _maxLength;
        var width = _maxWidth;
        for (var i = 0; i < Map.Height; i++)
        {
            // Add
            for (var x = (_maxLength - length) / 2; x < _maxLength - (_maxLength - length) / 2; x++)
            {
                for (var y = (_maxWidth - width) / 2; y < _maxWidth - (_maxWidth - width) / 2; y++)
                {
                    if (Random.value < sparsity) continue;

                    _height[x, y] += Random.value;
                }
            }

            length -= index;
            width -= index;

            // Blur
            var newHeight = new float[_maxLength, _maxWidth];
            for (var x = 0; x < _maxLength; x++)
            {
                for (var y = 0; y < _maxWidth; y++)
                {
                    newHeight[x, y] = _height[x, y] + _height[(x - 1 + _maxLength) % _maxLength, y] +
                                      _height[(x + 1) % _maxLength, y] +
                                      _height[x, (y - 1 + _maxWidth) % _maxWidth] +
                                      _height[x, (y + 1) % _maxWidth];
                    newHeight[x, y] /= 5f;
                }
            }

            _height = newHeight;
        }
    }

    private void VisualizeHeightMap()
    {
        for (var x = 0; x < _maxLength; x++)
        {
            for (var y = 0; y < _maxWidth; y++)
            {
                if (_height[x, y] > 0)
                {
                    // var h = Mathf.FloorToInt(_height[x, y]);
                    //
                    // for (var z = 0; z < h; z++)
                    // {
                    //     var block = Instantiate(blockPrefab, new Vector3(x, z + 0.5f, y),
                    //         Quaternion.identity, this.transform);
                    //     block.transform.localScale = new Vector3(1, 1, 1);
                    // }

                    var h = _height[x, y];

                    var block = Instantiate(blockPrefab, new Vector3(x, h * 0.5f, y),
                        Quaternion.identity, this.transform);
                    block.transform.localScale = new Vector3(1, h, 1);
                }
            }
        }
    }
}