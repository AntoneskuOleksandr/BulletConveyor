using UnityEngine;
using UnityEngine.AI;
using Zenject;

public abstract class Enemy : MonoBehaviour
{
    protected Transform target;
    protected NavMeshAgent agent;
    protected Animator animator;
    [SerializeField] protected Transform hitTransform;

    public Transform HitTransform => hitTransform;


    [Inject]
    public void Construct(WeaponManager weaponManager)
    {
        target = weaponManager.GetRandomWeapon().transform;

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        agent.SetDestination(target.position);
        animator.SetFloat("MoveSpeed", agent.speed / 2f);
    }
}
