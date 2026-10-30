using System.Collections;
using UnityEngine;
using DG.Tweening;
using System;

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

    public void AnimateCurrencyGain(
        float amount,
        RectTransform fromUI,
        Action<float> addCurrencyCallback,
        Action onComplete = null)
    {
        if (!currencyPrefab || !target || !spawnParent || !fromUI)
        {
            Debug.LogError("[CurrencyFlyAnimator] Missing references!");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(SpawnAndFly(amount, fromUI, addCurrencyCallback, onComplete));
    }

    private IEnumerator SpawnAndFly(
        float amount,
        RectTransform spawnFrom,
        Action<float> addCurrencyCallback,
        Action onComplete)
    {
        bool sameParentForBoth =
            spawnFrom.parent == spawnParent &&
            target.parent == spawnParent;

        float visualCoins = Mathf.Min(amount, maxIcons);

        float baseValue = amount / visualCoins;
        float remainder = amount % visualCoins;

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

        int completed = 0;

        for (int i = 0; i < visualCoins; i++)
        {
            RectTransform icon = Instantiate(currencyPrefab, spawnParent);
            icon.anchorMin = icon.anchorMax = new Vector2(0.5f, 0.5f);
            icon.pivot = new Vector2(0.5f, 0.5f);
            icon.localScale = Vector3.one;

            Vector2 jitter = new Vector2(
                UnityEngine.Random.Range(-spawnSpread.x, spawnSpread.x),
                UnityEngine.Random.Range(-spawnSpread.y, spawnSpread.y));

            Vector2 start = startLocal + jitter;
            icon.anchoredPosition = start;

            float reward = baseValue + (i < remainder ? 1 : 0);

            icon.DOAnchorPos(targetLocal, flyDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    Destroy(icon.gameObject);
                    addCurrencyCallback?.Invoke(reward);
                    completed++;
                    if (completed == visualCoins)
                        onComplete?.Invoke();
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
