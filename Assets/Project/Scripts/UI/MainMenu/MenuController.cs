using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;

    private void Awake()
    {
        OpenMenu(mainMenu);
    }

    public void OpenMenu(GameObject menu)
    {
        if (!menu.activeSelf)
        {
            CloseAllMenus();

            menu.SetActive(true);
        }
    }

    public void CloseAllMenus()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
