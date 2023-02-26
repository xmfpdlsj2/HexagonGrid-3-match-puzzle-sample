using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private static GridManager instance;
    public static GridManager Instance{ get => instance; }

    public const int MAXGRID_SIZE_X = 7;
    public const int MAXGRID_SIZE_Y = 11;
    public const float GRIDGAP_X = 1.93f;
    public const float GRIDGAP_Y = 1.115f;

    [SerializeField]
    private GridHexagon gridPrefab = null;

    private Transform parent;

    private Dictionary<Vector2Int, GridHexagon> gridMap = new Dictionary<Vector2Int, GridHexagon>();
    public Dictionary<Vector2Int, GridHexagon> GridMap { get => gridMap; set => gridMap = value; }


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        parent = this.transform;
    }

    public void Initialize()
    {
        for (int x = 0; x < MAXGRID_SIZE_X; x++)
        {
            for (int y = 0; y < MAXGRID_SIZE_Y; y++)
            {
                if ((x + y) % 2 == 0)
                {
                    continue;
                }
                if ((x == 0 && y == 1) || (x == 0 && y == MAXGRID_SIZE_Y - 2) ||
                    (x == 1 && y == 0) || (x == 1 && y == MAXGRID_SIZE_Y - 1))
                {
                    continue;
                }
                if ((x == MAXGRID_SIZE_X - 1 && y == 1) || (x == MAXGRID_SIZE_X - 1 && y == MAXGRID_SIZE_Y - 2) ||
                    (x == MAXGRID_SIZE_X - 2 && y == 0) || (x == MAXGRID_SIZE_X - 2 && y == MAXGRID_SIZE_Y - 1))
                {
                    continue;
                }
                CreateGrid(x, y);
            }
        }
    }

    private void CreateGrid(int x, int y)
    {
        var coordinate = new Vector2Int(x, y); 
        var grid = Instantiate(gridPrefab, parent);
        grid.Initialize(coordinate);
        gridMap.Add(coordinate, grid);
    }


    public Vector3 GetGirdPos(Vector2Int coordinate)
    {
        return new Vector3((coordinate.x - MAXGRID_SIZE_X / 2) * GRIDGAP_X,
            (coordinate.y - MAXGRID_SIZE_Y / 2) * GRIDGAP_Y);
    }
}
