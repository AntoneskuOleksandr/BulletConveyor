using UnityEngine;
using UnityEngine.SceneManagement;

public class DefaultSettings : MonoBehaviour
{
    [Header("Game")]
    [SerializeField] private float coins = 99999;
    [SerializeField] private int currentLevel = 1;

    [Header("Guns")]
    [SerializeField] private GunSettings[] guns;

    [Header("Scenes")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string tutorialScene = "Tutorial";

    [Header("Debug")]
    [SerializeField] private bool debug = false;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        if (!PlayerPrefs.HasKey("SetStartValues"))
        {
            PlayerPrefs.SetInt("SetStartValues", 1);
            PlayerPrefs.SetFloat("Coins", coins);
            PlayerPrefs.SetInt("CurrentLevel", currentLevel);

            PlayerPrefs.SetString("GunID" + 0, guns[0].ID.ToString());
            PlayerPrefs.SetInt("GunIndex" + 0, 0);

            PlayerPrefs.SetInt("SelectedGunsCount", 1);

            foreach (GunSettings gun in guns)
            {
                PlayerPrefs.SetFloat("Damage" + gun.ID, gun.damage);
                PlayerPrefs.SetFloat("FireRate" + gun.ID, gun.fireRate);
                PlayerPrefs.SetFloat("Health" + gun.ID, gun.health);
                PlayerPrefs.SetFloat("BulletSpeed" + gun.ID, gun.bulletSpeed);
            }
        }
    }

    private void Start()
    {
        if (debug)
        {
            SceneManager.LoadScene(mainMenuScene);
            return;
        }

        if (PlayerPrefs.HasKey("TutorialCompleted"))
            SceneManager.LoadScene(mainMenuScene);
        else
            SceneManager.LoadScene(tutorialScene);
    }
}
