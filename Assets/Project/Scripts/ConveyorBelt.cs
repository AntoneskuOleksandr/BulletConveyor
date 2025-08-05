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
            onBelt[i].position += speed * direction.normalized * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        onBelt.Add(other.transform);
        other.GetComponent<Rigidbody>().isKinematic = true;
    }

    private void OnTriggerExit(Collider other)
    {
        onBelt.Remove(other.transform);
    }
}
