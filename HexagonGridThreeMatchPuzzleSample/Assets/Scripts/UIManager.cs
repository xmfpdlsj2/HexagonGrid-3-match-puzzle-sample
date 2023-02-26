using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance { get => instance; }

    [SerializeField]
    private Text move = null;

    [SerializeField]
    private Text top = null;

    [SerializeField]
    private Text combo = null;

    [SerializeField]    
    private Text score = null;

    [SerializeField]
    private RectTransform[] finishImages;

    [SerializeField]
    private Button reTryButton = null;


    private int topCount = 0;
    public int TopCount { get => topCount; }

    private int moveCount = 0;

    private int currentScore = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        reTryButton.onClick.AddListener(() =>
        {
            GameManager.Instance.ReTryGame();
            for (int i = 0; i < finishImages.Length; i++)
            {
                finishImages[i].gameObject.SetActive(false);
                finishImages[i].localScale = Vector3.zero;
            }
        });
    }

    public void Initialize(int moveLimitCount, int topCount)
    {
        this.moveCount = moveLimitCount;
        this.topCount = topCount;

        this.move.text = moveLimitCount.ToString();
        this.top.text = topCount.ToString();
        this.currentScore = 0;
        this.combo.text = currentScore.ToString();
        this.score.text = currentScore.ToString();
    }

    public void MoveReduce()
    {
        this.move.transform.DOPunchScale(Vector3.one, 0.3f, 7);
        this.move.text = (--this.moveCount).ToString();

        if (this.moveCount <= 0)
        {
            GameManager.Instance.EndGame();
        }
    }

    public void TopRemove(int count)
    {
        this.top.transform.DOPunchScale(Vector3.one, 0.3f, 7);
        this.topCount -= count;
        this.top.text = this.topCount.ToString();

        if (this.topCount <= 0)
        {
            GameManager.Instance.EndGame();
        }
    }

    public void Combo(int count)
    {
        this.combo.transform.DOPunchScale(Vector3.one, 0.3f, 7);
        this.combo.text = count.ToString();

        this.currentScore += (int)Math.Pow(count, count);
        this.score.transform.DOPunchScale(Vector3.one, 0.3f, 7);
        this.score.text = currentScore.ToString();
    }

    public void Finish()
    {
        int index = 0;

        if (this.moveCount > 13)
        {
            index = 2;
        }
        else if (this.moveCount > 9)
        {
            index = 1;
        }
        else if (this.moveCount > 0)
        {
            index = 0;
        }
        else
        {
            index = 3;
        }
        this.finishImages[index].gameObject.SetActive(true);
        this.finishImages[index].DOScale(Vector3.one, 1.5f).SetEase(Ease.OutBounce);
    }

}
