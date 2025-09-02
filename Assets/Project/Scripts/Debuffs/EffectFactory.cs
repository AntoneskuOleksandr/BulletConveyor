using UnityEngine;

public class EffectFactory : MonoBehaviour
{
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite freezeIcon;
    [SerializeField] private Sprite iceIcon;

    public IOnHitEffect CreateEffect(OnHitEffectType type)
    {
        switch (type)
        {
            case OnHitEffectType.Fire:
                return new FireEffect(5f, 3f);
            case OnHitEffectType.Freeze:
                return new FreezeEffect(0.5f, 3f);
            case OnHitEffectType.Ice:
                return new IceEffect(2f);
            default:
                return null;
        }
    }

    public Sprite GetIconByEffectType(OnHitEffectType type)
    {
        switch (type)
        {
            case OnHitEffectType.Fire:
                return fireIcon;
            case OnHitEffectType.Freeze:
                return freezeIcon;
            case OnHitEffectType.Ice:
                return iceIcon;
            default:
                return null;
        }
    }
}
