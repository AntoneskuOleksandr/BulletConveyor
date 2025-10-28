using System;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponAttachmentType
{
    BaseRotation,   
    BarrelRotation,
    FirePoint,
    StaticFirePoint,
}

[Serializable]
public struct AttachmentEntry
{
    public WeaponAttachmentType type;
    public Transform transform;
}

public class WeaponAttachmentPoints : MonoBehaviour
{
    [SerializeField] private List<AttachmentEntry> attachments = new();

    private Dictionary<WeaponAttachmentType, Transform> _map;

    private void Awake()
    {
        _map = new Dictionary<WeaponAttachmentType, Transform>();
        foreach (var entry in attachments)
        {
            if (entry.transform != null && !_map.ContainsKey(entry.type))
                _map.Add(entry.type, entry.transform);
        }
    }

    public Transform Get(WeaponAttachmentType type)
    {
        if (_map.TryGetValue(type, out var tr))
            return tr;

        Debug.LogWarning($"[WeaponAttachmentPoints] Attachment of type {type} not found on {gameObject.name}");
        return null;
    }
}
