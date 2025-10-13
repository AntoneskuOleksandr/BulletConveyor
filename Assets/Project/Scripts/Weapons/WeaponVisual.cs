using UnityEngine;

public class WeaponVisual : MonoBehaviour
{
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    private ParticleSystem _muzzleInstance;

    private void Awake()
    {
        if (muzzleFlashPrefab != null)
        {
            _muzzleInstance = Instantiate(muzzleFlashPrefab, transform);
            _muzzleInstance.Stop();
        }
    }

    public void PlayShotEffect(Color color)
    {
        if (_muzzleInstance == null) return;

        var main = _muzzleInstance.main;
        main.startColor = color;
        _muzzleInstance.Play();
    }
}
