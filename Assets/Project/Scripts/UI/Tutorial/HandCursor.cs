
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class HandCursor : MonoBehaviour
{
    [SerializeField] private Transform buyButton;
    [SerializeField] private Vector3 buyButtonOffSet;
    [SerializeField] private float pulseDuration = 0.5f;
    public Transform[] pathPoints;
    public bool isUIHand = false;
    private Tweener tweener;
    private bool stopMoving = false;
    private bool stopPulse = false;

    private void Start()
    {
        if (isUIHand)
        {
            GameManager.Instance.onEnoughMoneyToBuyBlock.AddListener(PulseInBuyButton);
            this.gameObject.SetActive(false);
            GameManager.Instance.onBlockBuy.AddListener(HideHand);
        }
        else
        {
            GameManager.Instance.onBlockPlace.AddListener(HideHand);
            MoveToNextPoint();
            GameManager.Instance.onBlockBuy.AddListener(ShowMerge);
            GameManager.Instance.onMerge.AddListener(HideHand);
        }
    }

    private void MoveToNextPoint()
    {
        if (stopMoving)
            return;

        transform.position = pathPoints[0].position;
        tweener = transform.DOMove(pathPoints[1].position, 1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Restart);
    }

    private void PulseInBuyButton()
    {
        if (stopPulse)
            return;

        stopPulse = true;
        this.gameObject.SetActive(true);

        transform.position = buyButton.position + buyButtonOffSet;

        transform.DOScale(transform.localScale.x * 1.2f, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void HideHand()
    {
        this.gameObject.SetActive(false);
        stopMoving = true;
    }

    private void ShowMerge()
    {
        this.gameObject.SetActive(true);
        stopMoving = false;

        if (pathPoints.Length < 4)
        {
            Debug.LogError("Not enough points in pathPoints");
            return;
        }

        if (tweener != null)
        {
            tweener.Kill();
        }

        transform.position = pathPoints[2].position;
        tweener = transform.DOMove(pathPoints[3].position, 1f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Restart);
    }

}
