using UnityEngine;
using UnityEngine.UI;

public class LosePanel : MonoBehaviour
{
    [SerializeField] private Button getButton;

    private void OnEnable()
    {
        if (getButton != null)
        {
            getButton.onClick.AddListener(OnGetButtonClicked);
        }
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }

    private void OnGetButtonClicked()
    {
        Debug.Log("Get button clicked.");
    }
}
