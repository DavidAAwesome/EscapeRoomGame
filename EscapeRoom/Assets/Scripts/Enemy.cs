using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class GrannyAI : MonoBehaviour
{
    public enum AIState { Patrol, Investigate, Chase, Attack }
    public AIState CurrentState { get; private set; } = AIState.Patrol;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 2f;

    [Header("Detection")]
    [SerializeField] private float viewDistance = 12f;
    [SerializeField] private float viewAngle = 90f;   
    [SerializeField] private float chaseDistance = 20f;   

    [Header("Movement Speeds")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float investigateSpeed = 2.5f;
    [SerializeField] private float chaseSpeed = 4.5f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDelay = 0.4f;   

    [Header("References")]
    [SerializeField] private Transform eyeTransform; 

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Transform player;

    private int patrolIndex;
    private bool waitingAtPoint;
    private float patrolWaitTimer;

    private Vector3 lastKnownPosition;
    private bool hasTarget;
    private float soundAlertTimer;

    private bool attackTriggered;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();

        // Auto-find player by tag
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) player = playerGO.transform;

        if (eyeTransform == null) eyeTransform = transform; // fallback
    }

    private void Update()
    {
        RunStateMachine();
    }

    private void RunStateMachine()
    {
        switch (CurrentState)
        {
            case AIState.Patrol: 
                StatePatrol(); 
                break;
            case AIState.Investigate: 
                StateInvestigate(); 
                break;
            case AIState.Chase:
                StateChase();
                break;
            // case AIState.Attack:
            //     StateAttack();
            //     break;
        }

        // Vision check runs every frame regardless of state
        if (CurrentState != AIState.Attack && CanSeePlayer())
            TransitionTo(AIState.Chase);
    }

    private void StatePatrol()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (waitingAtPoint)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0f)
            {
                waitingAtPoint = false;
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[patrolIndex].position);
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            waitingAtPoint  = true;
            patrolWaitTimer = patrolWaitTime;
        }
    }

    private void StateInvestigate()
    {
        agent.speed = investigateSpeed;

        if (!agent.pathPending && agent.remainingDistance < 0.6f)
        {
            StartCoroutine(InvestigateRoutine());
        }
    }

    private IEnumerator InvestigateRoutine()
    {
        TransitionTo(AIState.Patrol); // prevent re-entry

        float t = 0f;
        Quaternion startRot = transform.rotation;
        while (t < 2f)
        {
            t += Time.deltaTime;
            float yaw = Mathf.Sin(t * 1.5f) * 40f;
            transform.rotation = startRot * Quaternion.Euler(0f, yaw, 0f);
            yield return null;
        }
        transform.rotation = startRot;

        if (patrolPoints != null && patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    private void StateChase()
    {
        agent.speed = chaseSpeed;

        if (player == null) { TransitionTo(AIState.Patrol); return; }

        float dist = Vector3.Distance(transform.position, player.position);
        
        if (CanSeePlayer() || hasTarget)
        {
            lastKnownPosition = player.position;
            agent.SetDestination(player.position);
        }
        
        // if (dist <= attackRange)
        // {
        //     TransitionTo(AIState.Attack);
        //     return;
        // }
        
        if (!CanSeePlayer() && !hasTarget)
        {
            agent.SetDestination(lastKnownPosition);
            if (!agent.pathPending && agent.remainingDistance < 0.8f)
                TransitionTo(AIState.Patrol);
        }
    }

    private void StateAttack()
    {
        
    }

    private void TransitionTo(AIState newState)
    {
        if (newState == CurrentState) return;

        CurrentState     = newState;
        attackTriggered = false;

        switch (newState)
        {
            case AIState.Patrol:
                if (patrolPoints != null && patrolPoints.Length > 0)
                    agent.SetDestination(patrolPoints[patrolIndex].position);
                break;
            case AIState.Investigate:
                agent.SetDestination(lastKnownPosition);
                break;
            case AIState.Chase:
                break;
            case AIState.Attack:
                break;
        }
    }


    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.position - eyeTransform.position);
        float dist = dirToPlayer.magnitude;

        if (dist > viewDistance) return false;

        float angle = Vector3.Angle(eyeTransform.forward, dirToPlayer);
        if (angle > viewAngle) return false;

        
        if (Physics.Raycast(eyeTransform.position, dirToPlayer.normalized, out RaycastHit hit, dist))
        {
            if (hit.transform == player || hit.transform.IsChildOf(player))
                return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (eyeTransform == null) return;

        // Vision cone
        Gizmos.color = Color.yellow;
        Vector3 left  = Quaternion.Euler(0,  viewAngle, 0) * eyeTransform.forward;
        Vector3 right = Quaternion.Euler(0, -viewAngle, 0) * eyeTransform.forward;
        Gizmos.DrawRay(eyeTransform.position, left  * viewDistance);
        Gizmos.DrawRay(eyeTransform.position, right * viewDistance);
        Gizmos.DrawWireSphere(eyeTransform.position, viewDistance);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}