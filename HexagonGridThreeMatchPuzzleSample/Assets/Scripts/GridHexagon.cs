using System.ComponentModel;
using UnityEngine;

public class GridHexagon : MonoBehaviour
{
    [ReadOnly(true)]
    [SerializeField]
    private Vector2Int coordinate;
    public Vector2Int Coordinate { get => coordinate; }

    [ReadOnly(true)]
    [SerializeField]
    private Block block = null;
    public Block Block { get => block; set => block = value; }


    public void Initialize(Vector2Int coordinate)
    {
        this.coordinate.x = coordinate.x;
        this.coordinate.y = coordinate.y;

        this.transform.position = GridManager.Instance.GetGirdPos(coordinate);
    }
}
