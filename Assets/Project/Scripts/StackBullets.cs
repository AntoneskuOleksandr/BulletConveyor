using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StackBullets : MonoBehaviour
{
    public static StackBullets Instance;

    [SerializeField] private Transform stackPosition;
    [SerializeField] private float stackOffset = 0.2f;
    [SerializeField] private int rowsCount = 4;
    [SerializeField] private ConveyorBelt conveyor;
    [SerializeField] private TMP_Text bulletsCounter;

    public List<GameObject> bullets = new List<GameObject>();

    public bool HaveBullet
    {
        get
        {
            return bullets.Count > 0;
        }
        private set
        {

        }
    }

    private int currentRow = 0;
    private int currentColumn = 0;

    private void Awake()
    {
        Instance = this;
        bulletsCounter.text = bullets.Count.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        conveyor.onBelt.Remove(other.transform);
        bullets.Add(other.gameObject);

        bulletsCounter.text = bullets.Count.ToString();

        other.transform.position = stackPosition.position + new Vector3(0f, currentRow * stackOffset, currentColumn * stackOffset);

        other.gameObject.isStatic = true;

        currentColumn++;
        if (currentColumn >= rowsCount)
        {
            currentColumn = 0;
            currentRow++;
        }
    }

    public void RemoveBulletFromStack()
    {
        if (bullets.Count > 0)
        {
            bullets.RemoveAt(bullets.Count - 1);
            bulletsCounter.text = bullets.Count.ToString();

            currentColumn--;
            if (currentColumn < 0)
            {
                currentColumn = rowsCount - 1;
                currentRow--;
            }
        }
    }
}
