using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchingManager : MonoBehaviour
{
    private static MatchingManager instance;
    public static MatchingManager Instance { get => instance; }

    [SerializeField]
    private float animTime = 0.5f;

    private enum DirectionLine
    {
        UP,
        DOWN,
        RIGHTUP,
        LEFTDOWN,
        RIGHTDOWN,
        LEFTUP,

        COUNT
    }
    private enum DirectionCirCle
    {
        UP,
        RIGHTUP,
        RIGHTDOWN,
        DOWN,
        LEFTDOWN,
        LEFTUP,

        COUNT
    }

    private Vector2Int[] dirCoordLine = new Vector2Int[6] 
    { 
        new Vector2Int(0, 2),
        new Vector2Int(0, -2),
        new Vector2Int(1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1) 
    };
    private Vector2Int[] dirCoordCircle = new Vector2Int[6]
    {
        new Vector2Int(0, 2),
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(0, -2),
        new Vector2Int(-1, -1),
        new Vector2Int(-1, 1)
    };


    private const float LEAST_DRAG = 1.5f;
    private readonly Vector2Int GENERATE_SPOT = new Vector2Int(3, 12);

    private Dictionary<Vector2Int, GridHexagon> gridMap = null;

    private Stack<Block> desBlockStack = new Stack<Block>();

    private bool isDropping = false;

    private int comboCount = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance= this;
        }
    }

    public void Initialize()
    {
        gridMap = GridManager.Instance.GridMap;
    }
    
    public List<Vector2Int> MatchingCheckAll()
    {
        List<Vector2Int> matchedAll = new List<Vector2Int>();
        foreach (var grid in gridMap)
        {
            var matched = MatchingCheck(grid.Key);
            if (matched.Count != 0)
            {
                matchedAll.AddRange(matched);
            }
        }
        matchedAll = matchedAll.Distinct().ToList();
        return matchedAll;
    }

    private List<Vector2Int> MatchingCheck(Vector2Int start)
    {
        List<Vector2Int> totalMatch = new List<Vector2Int>();
        if (gridMap[start].Block.Type == BlockType.TOP)
        {
            return totalMatch;
        }

        List<Vector2Int> lineMatch = new List<Vector2Int>();
        lineMatch.Add(start);

        for (int i = 0; i < (int)DirectionLine.COUNT; i++)
        {
            var next = start + dirCoordLine[i];
            if (gridMap.ContainsKey(next))
            {
                if (gridMap[start].Block.Type == gridMap[next].Block.Type)
                {
                    LineCheck(start, dirCoordLine[i], lineMatch);
                }
            }

            if (i % 2 == 1) 
            {
                if (lineMatch.Count >= 3)
                {
                    totalMatch = totalMatch.Union(lineMatch).ToList();
                }
                lineMatch.Clear();
                lineMatch.Add(start);
            }
        }

        for (int i = 0; i < (int)DirectionCirCle.COUNT; i++)
        {
            var next = start + dirCoordCircle[i];
            if (gridMap.ContainsKey(next))
            {
                if (gridMap[start].Block.Type == gridMap[next].Block.Type)
                {
                    List<Vector2Int> lumpMatch = new List<Vector2Int>();
                    lumpMatch.Add(start);
                    lumpMatch.Add(start + dirCoordCircle[i]);
                    LumpCheck(start, start + dirCoordCircle[i], i, lumpMatch);

                    if (lumpMatch.Count >= 4)
                    {
                        totalMatch = totalMatch.Union(lumpMatch).ToList();
                    }
                }
            }
        }

        totalMatch = totalMatch.Distinct().ToList();
        return totalMatch;
    }

    private List<Vector2Int> MatchingCheckTop(List<Vector2Int> desCoordinate)
    {
        List<Vector2Int> topList = new List<Vector2Int>();
        foreach (var des in desCoordinate)
        {
            for (int i = 0; i < (int)DirectionLine.COUNT; i++)
            {
                if (gridMap.ContainsKey(des + dirCoordLine[i]))
                {
                    if (gridMap[des + dirCoordLine[i]].Block.Type == BlockType.TOP)
                    {
                        topList.Add(des + dirCoordLine[i]);
                    }
                }
            }
        }

        List<Vector2Int> topRemoveList = new List<Vector2Int>();
        topList = topList.Distinct().ToList();
        for (int i = 0; i < topList.Count; i++)
        {
            if (gridMap[topList[i]].Block.CheckTopSecond())
            {
                topRemoveList.Add(topList[i]);
            }
        }
        return topRemoveList;
    }


    private void LineCheck(Vector2Int start, Vector2Int dir, List<Vector2Int> matchBlocks)
    {
        var next = start + dir;
        if (!gridMap.ContainsKey(next))
        {
            return;
        }
        if (gridMap[start].Block.Type != gridMap[next].Block.Type)
        {
            return;
        }

        matchBlocks.Add(next);
        LineCheck(next, dir, matchBlocks);
    }

    private void LumpCheck(Vector2Int start, Vector2Int pair, int pairDir, List<Vector2Int> lumpMatchBlocks)
    {
        var leftDir = ((pairDir - 1) + (int)DirectionCirCle.COUNT) % (int)DirectionCirCle.COUNT;
        var sLeft = start + dirCoordCircle[leftDir];

        if (gridMap.ContainsKey(sLeft) && gridMap[start].Block.Type == gridMap[sLeft].Block.Type)
        {
            var pLeft = pair + dirCoordCircle[leftDir];
            lumpMatchBlocks.Add(sLeft);
            if (gridMap.ContainsKey(pLeft) && gridMap[start].Block.Type == gridMap[pLeft].Block.Type)
            {
                lumpMatchBlocks.Add(pLeft);
            }
        }

        var rightDir = ((pairDir + 1) + (int)DirectionCirCle.COUNT) % (int)DirectionCirCle.COUNT;
        var sRight = start + dirCoordCircle[rightDir];
        if (gridMap.ContainsKey(sRight) && gridMap[start].Block.Type == gridMap[sRight].Block.Type)
        {
            var pRight = pair + dirCoordCircle[rightDir];
            lumpMatchBlocks.Add(sRight);
            if (gridMap.ContainsKey(pRight) && gridMap[start].Block.Type == gridMap[pRight].Block.Type)
            {
                lumpMatchBlocks.Add(pRight);
            }
        }
    }

    public void TrySwitchingBlocks(Block block, Vector3 start, Vector3 to)
    {
        if (Vector3.Distance(start, to) < LEAST_DRAG)
        {
            Debug.Log("Too short to drag");
            GameManager.Instance.IsInputPause = false;
            GameManager.Instance.ClearDrag();
            return;
        }

        GameManager.Instance.IsInputPause = true;

        var toCoordinate = block.Coordinate + DragDirection(start, to);
        if (gridMap.ContainsKey(toCoordinate))
        {
            Debug.Log($"Move start:[{block.Coordinate}] to:[{gridMap[toCoordinate].Block.Coordinate}]");
            StartCoroutine(SwitchingAnimation(block, gridMap[toCoordinate].Block));
        }
    }

    private IEnumerator SwitchingAnimation(Block start, Block to) 
    {
        var startPos = start.transform.position;
        var toPos = to.transform.position;

        start.transform.DOMove(toPos, this.animTime).SetEase(Ease.OutExpo);
        to.transform.DOMove(startPos, this.animTime).SetEase(Ease.OutExpo);

        SwitchingGridMap(start.Coordinate, to.Coordinate);
        var resultStart = MatchingCheck(start.Coordinate);
        var resultTo = MatchingCheck(to.Coordinate);

        yield return new WaitForSeconds(this.animTime);

        if (resultStart.Count == 0 && resultTo.Count == 0)
        {
            Debug.Log("Roll back blocks");
            start.transform.DOMove(startPos, this.animTime).SetEase(Ease.OutExpo);
            to.transform.DOMove(toPos, this.animTime).SetEase(Ease.OutExpo);
            SwitchingGridMap(start.Coordinate, to.Coordinate);

            yield return new WaitForSeconds(this.animTime);
            GameManager.Instance.IsInputPause = false;
        }
        else
        {
            Debug.Log("Match !!");
            UIManager.Instance.MoveReduce();
            resultStart = resultStart.Union(resultTo).ToList();
            StartCoroutine(DestroyNGenerateBlocks(resultStart));
        }
        GameManager.Instance.ClearDrag();
    }

    private IEnumerator DestroyNGenerateBlocks(List<Vector2Int> desCoordinate)
    {
        UIManager.Instance.Combo(++this.comboCount);

        var desTopList = MatchingCheckTop(desCoordinate);
        if (desTopList.Count != 0)
        {
            UIManager.Instance.TopRemove(desTopList.Count);
        }

        desCoordinate = desCoordinate.Union(desTopList).ToList();
        foreach (var des in desCoordinate)
        {
            desBlockStack.Push(gridMap[des].Block);
            gridMap[des].Block.transform.DOScale(0.0f, animTime).SetEase(Ease.InBack).OnComplete(() =>
            {
                gridMap[des].Block.gameObject.SetActive(false);
                gridMap[des].Block.transform.localScale = Vector3.one;
                gridMap[des].Block.transform.position = GridManager.Instance.GetGirdPos(GENERATE_SPOT);
                gridMap[des].Block.Coordinate = GENERATE_SPOT;
                BlockManager.Instance.SpawnNewBlock(gridMap[des].Block);
                gridMap[des].Block = null;
            });
        }

        yield return new WaitForSeconds(animTime);

        while (desBlockStack.Count != 0)
        {
            this.isDropping = true;
            StartCoroutine(DropBlocks());
            yield return new WaitUntil(() => !isDropping);
        }

        var again = MatchingCheckAll();
        if (again.Count != 0)
        {
            StartCoroutine(DestroyNGenerateBlocks(again));
        }
        else
        {
            this.comboCount = 0;
            GameManager.Instance.IsInputPause = false;
        }
    }

    private IEnumerator DropBlocks()
    {
        foreach (var grid in gridMap)
        {
            if (grid.Value.Block == null)
            {
                var pull = FindPullBlockUp(grid.Key);
                if (pull == Vector2Int.zero)
                {
                    continue;
                }

                var destination = GridManager.Instance.GetGirdPos(grid.Key);

                if (pull == GENERATE_SPOT && desBlockStack.Count != 0)
                {
                    var newBlock = desBlockStack.Pop();
                    newBlock.gameObject.SetActive(true);
                    newBlock.transform.DOMove(destination, animTime).SetEase(Ease.InQuad);
                    newBlock.transform.DOShakePosition(0.3f, 0.2f, 20).SetDelay(animTime);

                    newBlock.Coordinate = grid.Key;
                    grid.Value.Block = newBlock;
                }
                else
                {
                    gridMap[pull].Block.transform.DOMove(destination, animTime).SetEase(Ease.InQuad);
                    gridMap[pull].Block.transform.DOShakePosition(0.3f, 0.2f, 20).SetDelay(animTime);

                    gridMap[pull].Block.Coordinate = grid.Key;
                    grid.Value.Block = gridMap[pull].Block;
                    gridMap[pull].Block = null;
                }
            }
        }

        yield return new WaitForSeconds(animTime);
        StartCoroutine(DropBlocksSide());
    }

    private IEnumerator DropBlocksSide()
    {
        foreach (var grid in gridMap)
        {
            if (grid.Value.Block == null && !gridMap.ContainsKey(grid.Key + dirCoordLine[(int)DirectionLine.UP]))
            {
                var pull = FindPullBlockSide(grid.Key);
                if (pull == Vector2Int.zero)
                {
                    continue;
                }

                var destination = GridManager.Instance.GetGirdPos(grid.Key);

                gridMap[pull].Block.transform.DOMove(destination, animTime).SetEase(Ease.InQuad);
                gridMap[pull].Block.transform.DOShakePosition(0.3f, 0.2f, 20).SetDelay(animTime);

                gridMap[pull].Block.Coordinate = grid.Key;
                grid.Value.Block = gridMap[pull].Block;
                gridMap[pull].Block = null;
            }
        }
        yield return new WaitForSeconds(animTime);
        this.isDropping = false;
    }

    private Vector2Int FindPullBlockUp(Vector2Int start)
    {
        var next = start;
        while (true)
        {
            next += dirCoordLine[(int)DirectionLine.UP];
            if (next == GENERATE_SPOT)
            {
                break;
            }
            if (!gridMap.ContainsKey(next))
            {
                next = Vector2Int.zero;
                break;
            }

            if (gridMap[next].Block != null)
            {
                break;
            }
        }

        if (next != Vector2Int.zero)
        {
            return next;
        }
        return Vector2Int.zero;
    }

    private Vector2Int FindPullBlockSide(Vector2Int start) 
    {
        while (true)
        {
            if (start.x < GridManager.MAXGRID_SIZE_X / 2)
            {
                start += dirCoordLine[(int)DirectionLine.RIGHTUP];
            }
            else
            {
                start += dirCoordLine[(int)DirectionLine.LEFTUP];
            }

            if (!gridMap.ContainsKey(start))
            {
                return Vector2Int.zero;
            }
            if (gridMap[start].Block != null)
            {
                break;
            }
        }

        return start;
    }


    private void SwitchingGridMap(Vector2Int start, Vector2Int to)
    {
        gridMap[start].Block.Coordinate = to;
        gridMap[to].Block.Coordinate = start;

        var temp = gridMap[start];
        gridMap[start] = gridMap[to];
        gridMap[to] = temp;
    }


    private Vector2Int DragDirection(Vector3 start, Vector3 to)
    {
        Vector3 direction = to - start;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0f)
        {
            angle += 360f;
        }

        DirectionLine dir = DirectionLine.UP;
        if (angle <= 60.0f)
        {
            dir = DirectionLine.RIGHTUP;
        }
        else if (angle <= 120.0f)
        {
            dir = DirectionLine.UP;
        }
        else if (angle <= 180.0f)
        {
            dir = DirectionLine.LEFTUP;
        }
        else if (angle <= 240.0f)
        {
            dir = DirectionLine.LEFTDOWN;
        }
        else if (angle <= 300.0f)
        {
            dir = DirectionLine.DOWN;
        }
        else
        {
            dir = DirectionLine.RIGHTDOWN;
        }

        return dirCoordLine[(int)dir];
    }
}

