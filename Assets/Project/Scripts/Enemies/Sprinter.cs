using UnityEngine;

public class Sprinter : Enemy
{
    [Header("Specifics")]
    public float accelerationSpeedMin;
    public float accelerationSpeedMax;
    public float distanceToSlowDown;
    public float slowDownTime = 3f; // время замедления

    private bool isSlowing = false;
    private bool hasSlowed = false;
    private float accelerationSpeed;
    private float slowDownSpeed;

    public override void Start()
    {
        base.Start();
        accelerationSpeed = Random.Range(accelerationSpeedMin, accelerationSpeedMax);
        agent.speed = accelerationSpeed;
    }

    public override void Update()
    {
        base.Update();

        if (hasSlowed)
            return;

        if (!isSlowing)
        {
            if (TargetGun != null)
            {
                if (Vector3.Distance(transform.position, TargetGun.transform.position) <= distanceToSlowDown)
                {
                    slowDownSpeed = Random.Range(minSpeed, maxSpeed);
                    isSlowing = true;
                }
            }
        }
        else
        {
            agent.speed = Mathf.Lerp(agent.speed, slowDownSpeed, Time.deltaTime / slowDownTime);
            if (Mathf.Abs(agent.speed - slowDownSpeed) < 0.01f)
            {
                agent.speed = slowDownSpeed;
                agent.acceleration = 1f;
                isSlowing = false;
                hasSlowed = true;
            }
        }
    }
}
