using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class AutoScalePanel : MonoBehaviour
{
    [SerializeField] private float animationDuration = 0.3f;

    private RectTransform rectTransform;
    private Vector3 originalScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }
    private void OnEnable()
    {
        rectTransform.DOKill();
        rectTransform.localScale = Vector3.zero;
        rectTransform.DOScale(originalScale, animationDuration).SetUpdate(true).SetLink(gameObject);
    }
    private void OnDisable()
    {
        rectTransform.DOKill();
        rectTransform.localScale = originalScale;
    }
}
