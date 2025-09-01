using UnityEngine;

public class EffectFactory : MonoBehaviour
{
    // Можно передавать спрайты иконок через инспектор или ресурсами
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite freezeIcon;

    public IOnHitEffect CreateEffect(OnHitEffectType type)
    {
        switch (type)
        {
            case OnHitEffectType.Fire:
                return new FireEffect(3f, 5f, fireIcon);
            default:
                return null;
        }
    }
}
