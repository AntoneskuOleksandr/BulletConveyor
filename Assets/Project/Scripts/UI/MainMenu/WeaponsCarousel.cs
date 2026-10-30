using UnityEngine;
using DG.Tweening;
using System;

public class WeaponsCarousel : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private WeaponSO[] weapons;
    [SerializeField] private Transform weaponSpawnPoint;

    [Header("Rotation Settings")]
    [SerializeField] private float startRotationY = 150f;

    [Header("Animation Settings")]
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float animDuration = 0.5f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;
    [SerializeField] private Ease rotateEase = Ease.OutQuad;

    public event Action<WeaponSO> OnWeaponChanged;
    public WeaponSO CurrentWeapon => weapons != null && currentIndex >= 0 && currentIndex < weapons.Length ? weapons[currentIndex] : null;

    private int currentIndex = -1;
    private GameObject currentWeapon;
    private bool isAnimating;

    private void OnEnable()
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogWarning("[WeaponsCarousel] No weapons assigned!");
            return;
        }

        ShowGun(0);
    }

    private void OnDisable()
    {
        KillTweens();
        if (currentWeapon != null) Destroy(currentWeapon);
        currentIndex = -1;
        isAnimating = false;

        if (weaponSpawnPoint == null) return;
        foreach (Transform child in weaponSpawnPoint)
        {
            if (child != null)
            {
                child.DOKill();
                Destroy(child.gameObject);
            }
        }

        weaponSpawnPoint.gameObject.SetActive(false);
    }

    public void Next() => ShowGun(WrapIndex(currentIndex + 1), true);
    public void Prev() => ShowGun(WrapIndex(currentIndex - 1), false);

    public void ShowGun(int newIndex, bool? forceDir = null)
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogError("[WeaponsCarousel] No weapons assigned!");
            return;
        }

        newIndex = WrapIndex(newIndex);
        if (newIndex == currentIndex || isAnimating) return;

        weaponSpawnPoint.gameObject.SetActive(true);
        bool toRight = forceDir ?? IsToRight(currentIndex, newIndex);
        AnimateSwitch(newIndex, toRight);
    }

    private void AnimateSwitch(int newIndex, bool toRight)
    {
        isAnimating = true;

        if (currentWeapon != null)
            AnimateOut(currentWeapon, toRight);

        currentWeapon = CreateWeapon(newIndex);
        AnimateIn(currentWeapon, toRight);

        currentIndex = newIndex;

        OnWeaponChanged?.Invoke(weapons[currentIndex]);
    }

    private GameObject CreateWeapon(int index)
    {
        var weapon = Instantiate(weapons[index].WeaponGO, weaponSpawnPoint);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.Euler(0f, startRotationY, 0f);
        return weapon;
    }

    private void AnimateOut(GameObject weapon, bool toRight)
    {
        Vector3 dir = toRight ? Vector3.left : Vector3.right;

        weapon.transform
            .DOLocalMove(dir * moveDistance, animDuration)
            .SetEase(moveEase).SetLink(weapon)
            .OnComplete(() => Destroy(weapon));

        weapon.transform
            .DOLocalRotate(new Vector3(0, startRotationY + (toRight ? -90 : 90), 0), animDuration)
            .SetEase(rotateEase).SetLink(weapon);
    }

    private void AnimateIn(GameObject weapon, bool toRight, bool fromZero = false)
    {
        Vector3 dir = toRight ? Vector3.right : Vector3.left;

        if (!fromZero)
            weapon.transform.localPosition = dir * moveDistance;

        weapon.transform
            .DOLocalMove(Vector3.zero, animDuration)
            .SetEase(moveEase).SetLink(weapon);

        weapon.transform.localRotation = Quaternion.Euler(0, startRotationY + (toRight ? 90 : -90), 0);
        weapon.transform
            .DOLocalRotate(new Vector3(0, startRotationY, 0), animDuration)
            .SetEase(rotateEase).SetLink(weapon)
            .OnComplete(() => isAnimating = false);
    }

    private void KillTweens()
    {
        if (currentWeapon != null) DOTween.Kill(currentWeapon.transform, complete: false);
        if (weaponSpawnPoint != null) DOTween.Kill(weaponSpawnPoint, complete: false);
        DOTween.Kill(this, complete: false);
    }

    private int WrapIndex(int idx)
    {
        if (weapons == null || weapons.Length == 0) return 0;
        int len = weapons.Length;
        int m = idx % len;
        return m < 0 ? m + len : m;
    }

    private bool IsToRight(int from, int to)
    {
        int len = weapons.Length;
        int forward = ((to - from) % len + len) % len;
        int backward = ((from - to) % len + len) % len;
        return forward <= backward;
    }
}
