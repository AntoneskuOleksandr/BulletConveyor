using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using TMPro;
using DG.Tweening;

public class GunCarousel : MonoBehaviour
{
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject[] gunsPrefabs;  
    [SerializeField] private GameObject platforms;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private GameObject[] arrowButtons;
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private Transform gunsSpawnPosition;
    [SerializeField] private Transform[] spawnPointsOnPlatforms;
    [SerializeField] private Transform platformsOpenPosition;
    [SerializeField] private Transform platformsClosePosition;
    [SerializeField] private Transform gunOpenPosition;
    [SerializeField] private Transform gunClosePosition;
    [SerializeField] private float carouselAnimationDuration;

    private GameObject[] gunsOnPlatforms = new GameObject[3];
    private GameObject[] gunsToShow;
    private int[] gunIndex = new int[3] { -1, -1, -1 };

    public int currentGunIndex;

    private int level;
    private int previousGun;
    private TMP_Text selectButtonText;

    private bool isOpenedUpgradePanel;

    private void Start()
    {
        gunsToShow = new GameObject[gunsPrefabs.Length];

        for (int i = 0; i < gunsPrefabs.Length; i++)
            gunsPrefabs[i].GetComponent<Gun>().gunID = i;

        for (int i = 0; i < gunsToShow.Length; i++)
        {
            GameObject gun;

            gun = Instantiate(gunsPrefabs[i], gunsSpawnPosition);
            gun.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
            gun.SetActive(false);

            CarouselAnimation animatedGun = gun.AddComponent<CarouselAnimation>();
            animatedGun.moveDuration = carouselAnimationDuration;

            animatedGun.initialposition = gunsSpawnPosition.position + new Vector3(0f, gun.transform.position.y, 0f);
            animatedGun.finalposition.y = gun.transform.position.y;

            gunsToShow[i] = gun;
        }

        selectButtonText = selectButton.GetComponentInChildren<TMP_Text>();
        LoadGuns();
        GetGunsCount();
        level = PlayerPrefs.GetInt("CurrentLevel");
        ShowGun(currentGunIndex);
    }

    private void OnEnable()
    {
        gunsSpawnPosition.gameObject.SetActive(true);
        platforms.gameObject.SetActive(true);
        MainMenuManager.Instance.coinsGO.SetActive(false);
    }

    private void OnDisable()
    {
        gunsSpawnPosition.gameObject.SetActive(false);
        platforms.gameObject.SetActive(false);
        if (upgradePanel.activeSelf)
            CloseUpgradePanel();
        MainMenuManager.Instance.coinsGO.SetActive(true);
    }

    private void ShowGun(int _index)
    {
        for (int i = 0; i < gunsToShow.Length; i++)
        {
            gunsToShow[i].transform.rotation = gunsToShow[previousGun].transform.rotation;
            gunsToShow[i].gameObject.SetActive(i == _index);
        }

        UpdateGunState();
    }

    public void ChangeGun(int _change)
    {
        previousGun = currentGunIndex;
        currentGunIndex += _change;

        if (currentGunIndex < 0)
        {
            currentGunIndex = gunsToShow.Length - 1;
        }
        else if (currentGunIndex > gunsToShow.Length - 1)
        {
            currentGunIndex = 0;
        }

        UpdateGunState();
        ShowGun(currentGunIndex);
    }

    public int GetCurrentGunID()
    {
        return gunsPrefabs[currentGunIndex].GetComponent<Gun>().gunID;
    }

    private void SelectGun()
    {
        int freeGunIndex = GetFreeGunIndex();
        gunsOnPlatforms[freeGunIndex] = Instantiate(gunsPrefabs[currentGunIndex], spawnPointsOnPlatforms[freeGunIndex]);

        gunIndex[freeGunIndex] = currentGunIndex;
        MainMenuManager.Instance.ChangeSelectedGunsCount(1);

        PlayerPrefs.SetString("GunID" + freeGunIndex, gunsPrefabs[currentGunIndex].name);
        PlayerPrefs.SetInt("GunIndex" + freeGunIndex, currentGunIndex);

        UpdateGunState();
    }

    private void UnequipGun()
    {
        for (int i = 0; i < gunIndex.Length; i++)
        {
            if (gunIndex[i] == currentGunIndex)
            {
                Destroy(gunsOnPlatforms[i]);
                MainMenuManager.Instance.ChangeSelectedGunsCount(-1);
                gunIndex[i] = -1;
                gunsOnPlatforms[i] = null;

                PlayerPrefs.DeleteKey("GunName" + i);
                PlayerPrefs.DeleteKey("GunIndex" + i);

                UpdateGunState();

                return;
            }
        }
    }

    private void GetGunsCount()
    {
        MainMenuManager.Instance.selectedGunsCount = 0;

        for (int i = 0; i < spawnPointsOnPlatforms.Length; i++)
        {
            if (spawnPointsOnPlatforms[i].transform.childCount > 0)
                MainMenuManager.Instance.ChangeSelectedGunsCount(1);
        }
    }

    private int GetFreeGunIndex()
    {
        for (int i = 0; i < gunIndex.Length; i++)
            if (gunIndex[i] == -1)
                return i;

        return -1;
    }

    private void LoadGuns()
    {
        for (int i = 0; i < spawnPointsOnPlatforms.Length; i++)
        {
            int gunIndex = PlayerPrefs.GetInt("GunIndex" + i, -1);

            if (gunIndex != -1)
            {
                gunsOnPlatforms[i] = Instantiate(gunsPrefabs[gunIndex], spawnPointsOnPlatforms[i]);
                this.gunIndex[i] = gunIndex;
                MainMenuManager.Instance.ChangeSelectedGunsCount(1);
            }
        }
    }

    private void UpdateGunState()
    {
        bool isAvaibleGun = currentGunIndex + 1 <= level * 2;

        if (Array.Exists(gunIndex, element => element == currentGunIndex))
        {
            selectButton.interactable = true;
            selectButtonText.text = "DESELECT";
            ChangeButtonsState(true);
        }
        else if (!isAvaibleGun)
        {
            selectButton.interactable = false;
            selectButtonText.text = "LOCKED";
            ChangeButtonsState(false);
        }
        else if (MainMenuManager.Instance.selectedGunsCount >= 3)
        {
            selectButton.interactable = false;
            selectButtonText.text = "NOT ENOUGH SPACE";
            ChangeButtonsState(false);
        }
        else
        {
            selectButton.interactable = true;
            selectButtonText.text = "SELECT";
            ChangeButtonsState(true);
        }
    }

    private void ChangeButtonsState(bool isInteractable)
    {
        foreach (var button in upgradeButtons)
        {
            button.interactable = isInteractable;
            if (isInteractable)
                button.GetComponent<Upgrade>().UpdateButtonsState();
        }
    }

    public void ToggleGun()
    {
        if (IsGunSelected())
        {
            UnequipGun();
        }
        else
        {
            SelectGun();
        }
    }

    private bool IsGunSelected()
    {
        for (int i = 0; i < gunIndex.Length; i++)
        {
            if (gunIndex[i] == currentGunIndex)
            {
                return true;
            }
        }

        return false;
    }

    public void TryToOpenUpgradePanel()
    {
        if (isOpenedUpgradePanel)
        {
            CloseUpgradePanel();
        }
        else
        {
            OpenUpgradePanel();
        }
    }

    public void OpenUpgradePanel()
    {
        if (DOTween.IsTweening(gunsToShow[currentGunIndex].transform) || DOTween.IsTweening(platforms.transform))
            return;

        upgradeButton.GetComponentInChildren<TMP_Text>().text = "CLOSE";

        foreach (var button in upgradeButtons)
            button.GetComponent<Upgrade>().IconUpdate();

        foreach (var button in arrowButtons)
            button.SetActive(false);

        gunsToShow[currentGunIndex].transform.DOMove(gunOpenPosition.position, 0.5f);

        platforms.transform.DOMove(platformsOpenPosition.position, 0.5f);

        upgradePanel.SetActive(true);
        MainMenuManager.Instance.coinsGO.SetActive(true);
        isOpenedUpgradePanel = true;
    }

    public void CloseUpgradePanel()
    {
        if (DOTween.IsTweening(gunsToShow[currentGunIndex].transform) || DOTween.IsTweening(platforms.transform))
            return;

        upgradeButton.GetComponentInChildren<TMP_Text>().text = "UPGRADE";

        foreach (var button in arrowButtons)
            button.SetActive(true);

        gunsToShow[currentGunIndex].transform.DOMove(gunClosePosition.position, 0.5f);

        platforms.transform.DOMove(platformsClosePosition.position, 0.5f);

        upgradePanel.SetActive(false);
        MainMenuManager.Instance.coinsGO.SetActive(false);
        isOpenedUpgradePanel = false;
    }
}
