using UnityEngine;

public class LevelDisplay : MonoBehaviour
{
    [Header("MiniLevels")]
    [SerializeField] private GameObject[] miniLevelPrefabs;
    [SerializeField] private Transform miniLevelSpawnPoint;

    [SerializeField] private BackgroundGenerator backgroundGenerator;

    private void Start()
    {
        ShowLevel(0);
    }

    private void ShowLevel(int index)
    {
        backgroundGenerator.SetColor(Color.HSVToRGB(index / (float)miniLevelPrefabs.Length, 0.5f, 0.8f), Color.HSVToRGB(index / (float)miniLevelPrefabs.Length, 0.5f, 0.4f));
        Instantiate(miniLevelPrefabs[index], miniLevelSpawnPoint);
    }
}
