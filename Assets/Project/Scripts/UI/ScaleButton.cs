using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class ScaleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float scaleFactor = 0.9f;
    [SerializeField] private float animationDuration = 0.1f;

    private Vector3 originalScale;
    private Button button;
    private bool isPointerOverButton = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && button.interactable && isPointerOverButton)
            transform.DOScale(originalScale * scaleFactor, animationDuration).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null)
            transform.DOScale(originalScale, animationDuration).SetUpdate(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOverButton = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOverButton = false;
        if (button != null)
            transform.DOScale(originalScale, animationDuration).SetUpdate(true);
    }
}
