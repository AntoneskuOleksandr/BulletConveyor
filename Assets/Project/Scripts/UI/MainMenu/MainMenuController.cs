using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject weaponsPanel;

    [Header("Buttons")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button weaponsButton;

    private void Awake()
    {
        homeButton.onClick.AddListener(() =>
        {
            ShowPanel(homePanel);
        });

        weaponsButton.onClick.AddListener(() =>
        {
            ShowPanel(weaponsPanel);
        });
    }

    private void Start()
    {
        ShowPanel(homePanel);
    }

    private void ShowPanel(GameObject panelToShow)
    {
        homePanel.SetActive(false);
        weaponsPanel.SetActive(false);
        panelToShow.SetActive(true);
    }
}
