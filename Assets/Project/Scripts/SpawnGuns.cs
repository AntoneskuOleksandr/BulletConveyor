using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpawnGuns : MonoBehaviour
{
    public GameObject[] guns;
    public Transform[] spawnPoints;
    public Transform adGunSpawnPosition;
    public Button adGunButton;
    public GameObject effectPrefab;

    private GameObject adGun;

    private void Start()
    {
        Spawner.Instance.OnNextWave.AddListener(DestroyAdGun);

        SpawnAllGuns();
    }

    public void GunForAd()
    {
        adGun = Instantiate(guns[Random.Range(0, guns.Length)], adGunSpawnPosition.position, Quaternion.identity);

        adGun.GetComponent<Gun>().enabled = true;
        adGun.GetComponent<Gun>().needBullet = false;

        GameObject effect = Instantiate(effectPrefab, adGunSpawnPosition.position, Quaternion.identity);
        Destroy(effect, 3f);

        adGunButton.gameObject.SetActive(false);
    }

    public void DestroyAdGun()
    {
        if (adGun != null)
            Destroy(adGun);
    }

    public void SpawnAllGuns()
    {
        if (GameManager.Instance.isTutorial)
        {
            Gun tempGun = Instantiate(guns[0], spawnPoints[1].position, Quaternion.identity).GetComponent<Gun>();
            tempGun.enabled = true;
            GameManager.Instance.guns.Add(tempGun);
            return;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int gunIndex = PlayerPrefs.GetInt("GunIndex" + i, -1);
            if (gunIndex != -1)
            {
                Gun tempGun = Instantiate(guns[gunIndex], spawnPoints[i].position, Quaternion.identity).GetComponent<Gun>();
                tempGun.enabled = true;
                GameManager.Instance.guns.Add(tempGun);
            }
        }

        if (GameManager.Instance.guns.Count <= 0)
            Debug.LogError("There's no guns");
    }
}
