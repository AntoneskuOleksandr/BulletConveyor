using UnityEngine;

public class EffectFactory : MonoBehaviour
{
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite freezeIcon;

    public IOnHitEffect CreateEffect(OnHitEffectType type)
    {
        switch (type)
        {
            case OnHitEffectType.Fire:
                return new FireEffect(5f, 3f, fireIcon);
            case OnHitEffectType.Freeze:
                return new FreezeEffect(0.5f, 3f, freezeIcon);
            default:
                return null;
        }
    }
}
