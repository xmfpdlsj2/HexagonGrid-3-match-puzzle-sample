using System.ComponentModel;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField]
    private BlockType type = BlockType.BLUE;
    public BlockType Type { get => type; }

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [ReadOnly(false)]
    [SerializeField]
    private Vector2Int coordinate;
    public Vector2Int Coordinate { get => coordinate; set => coordinate = value; }


    public void Initialize(BlockType type, Sprite image, Vector2Int coordinate)
    {
        this.type = type;
        this.spriteRenderer.sprite = image;
        this.coordinate = coordinate;
        this.transform.position = GridManager.Instance.GetGirdPos(coordinate);
        this.name = type.ToString();

        this.gameObject.GetComponent<Animator>().enabled = false;
    }

    public bool CheckTopSecond()
    {
        if (this.type == BlockType.TOP)
        {
            var animator = this.gameObject.GetComponent<Animator>();
            if (!animator.enabled)
            {
                animator.enabled = true;
                return false;
            }

            return true;
        }
        return false;
    }
}
