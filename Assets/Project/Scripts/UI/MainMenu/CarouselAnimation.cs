using UnityEngine;
using DG.Tweening;

public class CarouselAnimation : MonoBehaviour
{
    public float moveDuration = 1f;
    public bool useDoMove = true;
    public float rotationSpeed = 40f;

    public Vector3 finalposition;
    public Vector3 initialposition;

    private void Update()
    {
        transform.RotateAround(gameObject.transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnEnable()
    {
        if (useDoMove)
        {
            transform.position = initialposition;
            transform.DOMove(finalposition, moveDuration);
        }
    }
}
