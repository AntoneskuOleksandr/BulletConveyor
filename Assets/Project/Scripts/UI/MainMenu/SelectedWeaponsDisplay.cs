using UnityEngine;
using Zenject;

public class SelectedWeaponsDisplay : MonoBehaviour
{
    [SerializeField] private GameObject platformPrefab;
    [Inject] private WeaponsService weapons;
    [Inject] private MainMenuController menu;
    private Transform displayRoot;
    private readonly Transform[] slots = new Transform[3];
    private readonly GameObject[] previews = new GameObject[3];
    private readonly int[] weaponIds = { -1, -1, -1 };
    private bool subscribed;
    public int PlatformCount => displayRoot != null ? slots.Length : 0;
    public int PreviewCount
    {
        get
        {
            int count = 0;
            foreach (var preview in previews) if (preview != null) count++;
            return count;
        }
    }

    private void Start() => OnEnable();

    private void OnEnable()
    {
        if (weapons == null || menu == null || subscribed) return;
        subscribed = true;
        BuildPlatforms();
        weapons.OnSelectionChanged += Refresh;
        menu.OnPanelChanged += RefreshVisibility;
        Refresh();
    }

    private void OnDisable()
    {
        if (!subscribed) return;
        subscribed = false;
        weapons.OnSelectionChanged -= Refresh;
        menu.OnPanelChanged -= RefreshVisibility;
        if (displayRoot != null) displayRoot.gameObject.SetActive(false);
    }

    private void BuildPlatforms()
    {
        if (displayRoot != null) return;
        displayRoot = new GameObject("Selected loadout").transform;
        displayRoot.SetParent(transform, false);
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new GameObject($"Loadout slot {i + 1}").transform;
            slots[i].SetParent(displayRoot, false);
            var platform = Instantiate(platformPrefab, slots[i]);
            platform.transform.localPosition = Vector3.zero;
            var bounds = GetBounds(platform);
            platform.transform.localScale *= 1.7f / Mathf.Max(bounds.size.x, bounds.size.z, 0.01f);
            bounds = GetBounds(platform);
            platform.transform.position += new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
            MakePreview(platform);
        }
        UpdateLayout();
    }

    private void Refresh()
    {
        var selected = weapons.GetSelectedWeapons();
        for (int i = 0; i < slots.Length; i++)
        {
            int id = i < selected.Count ? selected[i].WeaponId : -1;
            if (id == weaponIds[i]) continue;
            if (previews[i] != null) Destroy(previews[i]);
            previews[i] = null;
            weaponIds[i] = id;
            if (id < 0) continue;
            var preview = Instantiate(selected[i].WeaponGO, slots[i]);
            preview.name = $"Selected weapon {id}";
            preview.transform.localPosition = Vector3.zero;
            preview.transform.localRotation = Quaternion.Euler(0f, 150f, 0f);
            var bounds = GetBounds(preview);
            float scale = Mathf.Min(1.5f / Mathf.Max(bounds.size.y, 0.01f),
                1.4f / Mathf.Max(bounds.size.x, bounds.size.z, 0.01f));
            preview.transform.localScale *= scale;
            bounds = GetBounds(preview);
            float top = GetBounds(slots[i].GetChild(0).gameObject).max.y;
            preview.transform.position += new Vector3(slots[i].position.x - bounds.center.x,
                top - bounds.min.y, slots[i].position.z - bounds.center.z);
            MakePreview(preview);
            previews[i] = preview;
        }
        RefreshVisibility();
    }

    private void RefreshVisibility() => displayRoot.gameObject.SetActive(menu.IsHomeVisible);

    private void LateUpdate()
    {
        if (displayRoot != null && displayRoot.gameObject.activeInHierarchy) UpdateLayout();
    }

    private void UpdateLayout()
    {
        var camera = Camera.main;
        if (camera == null) return;
        var plane = new Plane(Vector3.up, Vector3.up * 0.25f);
        for (int i = 0; i < slots.Length; i++)
        {
            var ray = camera.ViewportPointToRay(new Vector3(0.3f + i * 0.2f, 0.72f, 0f));
            if (plane.Raycast(ray, out float distance)) slots[i].position = ray.GetPoint(distance);
        }
    }

    private static void MakePreview(GameObject root)
    {
        foreach (var collider in root.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        foreach (var body in root.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.useGravity = false;
        }
    }

    private static Bounds GetBounds(GameObject root)
    {
        var result = new Bounds(root.transform.position, Vector3.zero);
        bool found = false;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer is ParticleSystemRenderer) continue;
            if (!found) { result = renderer.bounds; found = true; }
            else result.Encapsulate(renderer.bounds);
        }
        return result;
    }
}
