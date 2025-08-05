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
        rectTransform.localScale = Vector3.zero;
        rectTransform.DOScale(originalScale, animationDuration);
    }
    private void OnDisable()
    {
        rectTransform.DOScale(0, animationDuration);
    }
}