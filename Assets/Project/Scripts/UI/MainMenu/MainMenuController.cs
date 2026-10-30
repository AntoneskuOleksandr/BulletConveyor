using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject weaponsPanel;

    [Header("Buttons")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button weaponsButton;

    public event Action OnPanelChanged;
    [Inject] private TutorialProgressService tutorialProgress;

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
        var overlay = RuntimeUI.Overlay("Practice controls", null, 40);
        var rect = RuntimeUI.Rect("Tutorial", overlay, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(210f, 64f), new Vector2(135f, -122f));
        RuntimeUI.Button("Tutorial", rect, "Tutorial", TMPro.TMP_Settings.defaultFontAsset, () =>
        {
            tutorialProgress.RequestReplay();
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        });
    }

    private void ShowPanel(GameObject panelToShow)
    {
        homePanel.SetActive(false);
        weaponsPanel.SetActive(false);
        panelToShow.SetActive(true);

        OnPanelChanged?.Invoke();
    }
}
