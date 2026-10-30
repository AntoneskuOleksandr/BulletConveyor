using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProjectileStack : MonoBehaviour
{
    [SerializeField] private Transform stackPosition;
    [SerializeField] private float stackOffset = 0.2f;
    [SerializeField] private int rowsCount = 4;
    [SerializeField] private ConveyorBelt conveyor;
    [SerializeField] private TMP_Text projectilesCounter;

    public List<GameObject> projectiles = new List<GameObject>();

    public bool HaveProjectile
    {
        get
        {
            return projectiles.Count > 0;
        }
    }

    private int currentRow = 0;
    private int currentColumn = 0;

    private void Awake()
    {
        projectilesCounter.text = projectiles.Count.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Bullet") || projectiles.Contains(other.gameObject)) return;
        conveyor.onBelt.Remove(other.transform);
        projectiles.Add(other.gameObject);

        projectilesCounter.text = projectiles.Count.ToString();

        other.transform.position = stackPosition.position + new Vector3(0f, currentRow * stackOffset, currentColumn * stackOffset);

        other.gameObject.isStatic = true;

        currentColumn++;
        if (currentColumn >= rowsCount)
        {
            currentColumn = 0;
            currentRow++;
        }
    }

    public GameObject GetProjectileGOFromStack()
    {
        if (projectiles.Count > 0)
        {
            GameObject projectileGO = projectiles[projectiles.Count - 1];

            projectiles.RemoveAt(projectiles.Count - 1);
            projectilesCounter.text = projectiles.Count.ToString();

            currentColumn--;
            if (currentColumn < 0)
            {
                currentColumn = rowsCount - 1;
                currentRow--;
            }

            return projectileGO;
        }

        return null;
    }
}
