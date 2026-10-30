using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private float speed, conveyorTextureSpeed;
    [SerializeField] private Vector3 direction;
    [SerializeField] private Texture texture;

    public List<Transform> onBelt = new List<Transform>();
    private Material material;

    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        if (texture != null)
            material.mainTexture = texture;
    }

    private void Update()
    {
        material.mainTextureOffset += new Vector2(0, 1) * conveyorTextureSpeed * Time.deltaTime;

        for (int i = 0; i <= onBelt.Count - 1; i++)
        {
            if (onBelt[i] == null)
            {
                onBelt.RemoveAt(i--);
                continue;
            }
            onBelt[i].position += speed * direction.normalized * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bullet") && !onBelt.Contains(other.transform))
            onBelt.Add(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        onBelt.Remove(other.transform);
    }
}
