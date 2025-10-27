using UnityEngine;

public class Ballistic : MonoBehaviour
{
    [SerializeField] private Transform target;
    private void Update()
    {
        TryAim();
    }
    public bool TryAim()
    {
        Weapon weapon = GetComponent<Weapon>();

        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);
        Transform baseRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BaseRotation);
        Transform barrelRot = weapon.AttachmentPoints.Get(WeaponAttachmentType.BarrelRotation);

        if (firePoint == null || baseRot == null || barrelRot == null)
        {
            Debug.LogError("One of attachment transforms is missing (FirePoint / BaseRotation / BarrelRotation).");
            return false;
        }

        Vector3 targetPos = target.position;

        // Speed (m/s)
        float v = weapon.Data.GetParameter(WeaponParameterType.LaunchSpeed);

        // Используем позицию pivot'а дула как неизменяемую origin для расчётов
        Vector3 origin = barrelRot.position;

        Vector3 toTarget = targetPos - origin;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);

        // Distance to target (XZ)
        float d = toTargetXZ.magnitude;

        // Height diff (target Y relative to origin Y)
        float y = targetPos.y - origin.y;

        float g = Mathf.Abs(Physics.gravity.y);

        // Обрабатываем случай вертикального попадания (почти d == 0)
        if (d < 0.0001f)
        {
            // Цель почти точно над/под origin — поднимаем дула вертикально вверх (в вашей системе -90 = вверх)
            barrelRot.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            // Повернем базу в сторону цели по Y (если требуется)
            Vector3 dirBase = targetPos - baseRot.position;
            Vector3 dirBaseXZ = new Vector3(dirBase.x, 0f, dirBase.z);
            if (dirBaseXZ.sqrMagnitude > 0.0001f)
                baseRot.rotation = Quaternion.LookRotation(dirBaseXZ, Vector3.up);
            return true;
        }

        float discriminant = Mathf.Pow(v, 4) - g * (g * Mathf.Pow(d, 2) + 2 * y * Mathf.Pow(v, 2));
        bool reachable = discriminant >= 0f;

        // Если дискриминант < 0 — цель недостижима. Для направления выбираем "граничный" случай (sqrt = 0),
        // то есть угол, при котором цель была бы на максимально достижимой дистанции для текущей скорости/высоты.
        float sqrtDisc = discriminant > 0f ? Mathf.Sqrt(discriminant) : 0f;

        // Вычисляем низкий угол (выбираем низкую траекторию). Формула безопасна, т.к. d != 0.
        float lowAngleRad = Mathf.Atan((Mathf.Pow(v, 2) - sqrtDisc) / (g * d));
        float lowAngleDeg = lowAngleRad * Mathf.Rad2Deg;

        // В вашей системе положительная степень = вниз, подъём отрицательный: инвертируем знак
        float pitchDeg = -lowAngleDeg;

        // Поворачиваем базу (yaw) в сторону цели по горизонтали
        Vector3 dirForBase = targetPos - baseRot.position;
        Vector3 dirForBaseXZ = new Vector3(dirForBase.x, 0f, dirForBase.z);
        if (dirForBaseXZ.sqrMagnitude > 0.0001f)
        {
            baseRot.rotation = Quaternion.LookRotation(dirForBaseXZ, Vector3.up);
        }

        // Устанавливаем локальный pitch дула (вращение вокруг локальной X)
        barrelRot.localRotation = Quaternion.Euler(pitchDeg, 0f, 0f);

        if (!reachable)
        {
            Debug.LogWarning("Target is out of range for current launch speed. Applied maximal-effort angle (discriminant < 0).");
        }

        return true;
    }
}