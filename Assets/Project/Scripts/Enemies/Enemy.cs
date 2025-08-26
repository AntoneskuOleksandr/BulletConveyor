using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] private Transform target;
    protected NavMeshAgent agent;
    protected Animator animator;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        //agent.SetDestination(target.position);
        animator.SetFloat("MoveSpeed", agent.speed / 2f);
    }
}
