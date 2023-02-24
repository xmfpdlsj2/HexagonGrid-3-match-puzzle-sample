using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum BlockType
{
    BLUE,
    GREEN,
    ORANGE,
    PURPLE,
    RED,
    YELLOW,
    TOP,

    COUNT
}

public class BlockManager : MonoBehaviour
{
    private static BlockManager instance;
    public static BlockManager Instance { get => instance; }

    [SerializeField]
    private Sprite[] images = new Sprite[7];

    [SerializeField]
    private Block blockPrefabs;

    private int maxTopCount = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void CreateFirstBlockSet(List<Vector2Int> topPos)
    {
        this.maxTopCount = topPos.Count;
        foreach (var grid in GridManager.Instance.GridMap)
        {
            var block = Instantiate(blockPrefabs, this.transform);
            grid.Value.Block = block;

            var rand = UnityEngine.Random.Range(0, (int)BlockType.YELLOW);
            if (topPos.Contains(grid.Key))
            {
                rand = (int)BlockType.TOP;
            }
            block.Initialize((BlockType)rand, images[rand], grid.Key);
        }
    }

    public void ResetBlockSet(List<Vector2Int> topPos)
    {
        foreach (var grid in GridManager.Instance.GridMap)
        {
            if (topPos.Contains(grid.Key))
            {
                grid.Value.Block.Initialize(BlockType.TOP, images[(int)BlockType.TOP], grid.Key);
                continue;
            }
            var rand = UnityEngine.Random.Range(0, (int)BlockType.YELLOW);
            grid.Value.Block.Initialize((BlockType)rand, images[rand], grid.Key);
        }
    }

    public void SpawnNewBlock(Block block)
    {
        int type = (int)BlockType.TOP;
        if (UIManager.Instance.TopCount >= this.maxTopCount)
        {
            type = (int)BlockType.YELLOW;
        }
        var rand = UnityEngine.Random.Range(0, type);
        block.Initialize((BlockType)rand, images[rand], block.Coordinate);
    }
}
