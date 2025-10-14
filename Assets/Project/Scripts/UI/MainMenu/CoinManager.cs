using UnityEngine;
using UnityEngine.Events;

public class CoinManager : MonoBehaviour
{
    [Range(0, 1000)][SerializeField] private float coins = 100;
    public UnityEvent onCoinsChanged;

    public float Coins
    {
        get { return coins; }
        set
        {
            coins = value;
            onCoinsChanged.Invoke();
        }
    }

    public void AddCoins(float amount)
    {
        Coins += amount;
    }

    public void SpendCoins(float amount)
    {
        if (coins >= amount)
            Coins -= amount;
    }
}
