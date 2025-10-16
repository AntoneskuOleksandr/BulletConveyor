using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Weapon Database")]
public class WeaponDatabaseSO : ScriptableObject
{
    public List<WeaponSO> weapons;
}
