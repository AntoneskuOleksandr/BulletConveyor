using UnityEngine;

public class DragBlock : MonoBehaviour
{
    private Vector3 screenPoint;
    private Vector3 offset;

    void OnMouseDown()
    {
        // Перетворюємо координати об'єкта з простору світу в простір екрану
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        // Розраховуємо зміщення між позицією об'єкта та позицією миші
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    void OnMouseDrag()
    {
        // Розраховуємо нову позицію об'єкта на основі позиції миші
        Vector3 cursorScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorScreenPoint) + offset;

        // Переміщуємо об'єкт на нову позицію
        transform.position = cursorPosition;
    }
}
