using UnityEngine;


[CreateAssetMenu(fileName = "New Weapon", menuName = "Scriptable Objects/Weapon")]
public class WeaponSO : ScriptableObject
{
    public GameObject WeaponGO;
    public string WeaponName;
}
