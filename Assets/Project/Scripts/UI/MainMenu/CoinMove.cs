using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CoinMove : MonoBehaviour
{
    public static CoinMove Instance;

    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject pileOfCoins;
    [SerializeField] private GameObject counterCoin;
    [SerializeField] private int coinsAmount = 10;
    [SerializeField] private float coinsSpawnOffSet;
    [SerializeField] private float coinsSpawnDelay = 0.1f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < coinsAmount; i++)
        {
            GameObject coin = Instantiate(coinPrefab, pileOfCoins.transform);
            coin.SetActive(false);
        }
    }

    public void CountCoins(int rewardValue, Transform coinsInitPos)
    {
        pileOfCoins.SetActive(true);
        var delay = 0f;

        int coinsCount = rewardValue / 10;

        coinsCount = Mathf.Min(coinsCount, pileOfCoins.transform.childCount);

        Vector2 counterPosition = counterCoin.transform.position;

        for (int i = 0; i < coinsCount; i++)
        {
            GameObject coin = pileOfCoins.transform.GetChild(i).gameObject;
            coin.SetActive(true);

            Vector3 randomOffset = new Vector3(Random.Range(-coinsSpawnOffSet, coinsSpawnOffSet), Random.Range(-coinsSpawnOffSet, coinsSpawnOffSet), 0);
            coin.GetComponent<RectTransform>().anchoredPosition = coinsInitPos.localPosition + randomOffset;

            coin.transform.DOScale(1f, 0.5f).SetDelay(delay).SetEase(Ease.OutBack);

            coin.transform.DOMove(counterPosition, 0.8f)
                .SetDelay(delay).SetEase(Ease.InBack);

            coin.transform.DORotate(Vector3.zero, 0.5f).SetDelay(delay + 0.5f)
                .SetEase(Ease.Flash);

            coin.transform.DOScale(0f, 0.3f).SetDelay(delay + 0.8f).SetEase(Ease.OutBack);

            counterCoin.transform.DOScale(1.1f, coinsSpawnDelay / 2).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine).SetDelay(delay + 0.8f);

            delay += coinsSpawnDelay; // увеличьте это значение, чтобы замедлить появление монет
        }

        StartCoroutine(CountReward(rewardValue));
    }

    IEnumerator CountReward(int reward)
    {
        yield return new WaitForSecondsRealtime(1f);

        MainMenuManager.Instance.AddCoins(reward);
    }
}
