using UnityEngine;

public class CarouselAnimation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 40f;

    private void Update()
    {
        transform.RotateAround(gameObject.transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
