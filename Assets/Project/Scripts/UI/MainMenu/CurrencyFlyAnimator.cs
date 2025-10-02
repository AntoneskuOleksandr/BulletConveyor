using System.Collections;
using UnityEngine;
using DG.Tweening;
using Zenject;

public class CurrencyFlyAnimator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private RectTransform currencyPrefab;
    [SerializeField] private RectTransform target;
    [SerializeField] private RectTransform spawnParent;
    [SerializeField] private float flyDuration = 0.6f;
    [SerializeField] private float totalAnimationTime = 1.5f;
    [SerializeField] private int maxIcons = 20;
    [SerializeField] private Vector2 spawnSpread = new Vector2(50f, 50f);

    [Inject] private EconomyManager _economyManager;

    private Canvas _rootCanvas;
    private Camera _uiCamera;

    private void Awake()
    {
        if (spawnParent == null) spawnParent = (RectTransform)transform;

        _rootCanvas = spawnParent.GetComponentInParent<Canvas>();
        _uiCamera = (_rootCanvas && _rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? _rootCanvas.worldCamera
            : null;
    }

    public void AnimateCurrencyGain(int amount, RectTransform spawnFrom)
    {
        if (!currencyPrefab || !target || !spawnParent || !spawnFrom)
        {
            Debug.LogError($"[CurrencyFlyAnimator] Missing refs: prefab={currencyPrefab}, target={target}, spawnParent={spawnParent}, spawnFrom={spawnFrom}");
            return;
        }

        StartCoroutine(SpawnAndFly(amount, spawnFrom));
    }

    private IEnumerator SpawnAndFly(int amount, RectTransform spawnFrom)
    {
        bool sameParentForBoth =
            spawnFrom.parent == spawnParent &&
            target.parent == spawnParent;

        int visualCoins = Mathf.Min(amount, maxIcons);

        int baseValue = amount / visualCoins;
        int remainder = amount % visualCoins;

        Vector2 startLocal, targetLocal;

        if (sameParentForBoth)
        {
            startLocal = spawnFrom.anchoredPosition;
            targetLocal = target.anchoredPosition;
        }
        else
        {
            startLocal = ToLocalOn(spawnParent, spawnFrom);
            targetLocal = ToLocalOn(spawnParent, target);
        }

        for (int i = 0; i < visualCoins; i++)
        {
            RectTransform icon = Instantiate(currencyPrefab, spawnParent);
            icon.anchorMin = icon.anchorMax = new Vector2(0.5f, 0.5f);
            icon.pivot = new Vector2(0.5f, 0.5f);
            icon.localScale = Vector3.one;

            Vector2 jitter = new Vector2(
                Random.Range(-spawnSpread.x, spawnSpread.x),
                Random.Range(-spawnSpread.y, spawnSpread.y));

            Vector2 start = startLocal + jitter;
            icon.anchoredPosition = start;

            int reward = baseValue + (i < remainder ? 1 : 0);

            icon.DOAnchorPos(targetLocal, flyDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    Destroy(icon.gameObject);
                    _economyManager?.AddCurrencyDirect(reward);
                });

            yield return new WaitForSeconds(totalAnimationTime / visualCoins);
        }
    }


    private Vector2 ToLocalOn(RectTransform onto, RectTransform rt)
    {
        Vector3 worldCenter = rt.TransformPoint(rt.rect.center);
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(_uiCamera, worldCenter);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(onto, screen, _uiCamera, out Vector2 local);

        return local;
    }
}
