using DG.Tweening.Plugins;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get => instance; set => instance = value; }

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private List<Vector2Int> topPositions;

    [SerializeField]
    private int moveLimitCount = 20;


    private bool isInputPause = false;
    public bool IsInputPause { get => isInputPause; set => isInputPause = value; }

    private bool gameEnd = false;

    private Block startBlock = null;
    private Vector3 startPos = Vector3.zero;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        GridManager.Instance.Initialize();
        BlockManager.Instance.CreateFirstBlockSet(this.topPositions);
        MatchingManager.Instance.Initialize();
        UIManager.Instance.Initialize(this.moveLimitCount, this.topPositions.Count);

        ClearGrid();
    }

    private void Update()
    {
        if (gameEnd) 
        {
            return;
        }
        if (isInputPause)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            this.startPos = this.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D ray = Physics2D.Raycast(this.startPos, Vector2.zero);
            if (ray.collider != null && ray.collider.GetComponent<Block>() != null)
            {
                this.startBlock = ray.collider.GetComponent<Block>();
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (this.startBlock == null) 
            {
                return;
            }
            var toPos = this.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            MatchingManager.Instance.TrySwitchingBlocks(this.startBlock, this.startPos, toPos);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            ReTryGame();
        }
    }

    private void ClearGrid()
    {
        while (true)
        {
            var result = MatchingManager.Instance.MatchingCheckAll();
            if (result.Count == 0)
            {
                Debug.Log("Grid Clear!");
                break;
            }
            BlockManager.Instance.ResetBlockSet(this.topPositions);
            Debug.Log("Grid reset!");
        }
    }

    public void ClearDrag()
    {
        this.startBlock = null;
        this.startPos = Vector3.zero;
    }

    public void ReTryGame()
    {
        if (this.isInputPause)
        {
            return;
        }
        BlockManager.Instance.ResetBlockSet(this.topPositions);
        ClearGrid();
        UIManager.Instance.Initialize(this.moveLimitCount, this.topPositions.Count);
        this.gameEnd = false;
        Debug.Log("Retry Game!");
    }

    public void EndGame()
    {
        Debug.Log("Game End!!");
        this.gameEnd = true;

        UIManager.Instance.Finish();
    }
}
