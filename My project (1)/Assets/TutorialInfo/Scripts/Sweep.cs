using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BoxCollider))]
public class Sweep : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float normalSpeed = 3.5f;
    public float sweepSpeed = 15f;
    public LayerMask collisionMask;

    [Header("Controle de Início (Baseado nos Coletáveis)")]
    public int itensNecessariosParaIniciar = 2;
    [Range(0f, 1f)] public float preGameMoveChance = 0.2f;

    [Header("Estados")]
    private bool isSweeping = false;
    private Vector3 sweepDirection;
    private float moveTimer = 0f;

    private NavMeshAgent agent;
    private BoxCollider boxCollider;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        boxCollider = GetComponent<BoxCollider>();

        agent.speed = normalSpeed;
        agent.updateRotation = false;
        agent.acceleration = 999f;

        boxCollider.isTrigger = true;
    }

    void Update()
    {
        bool jogoComecou = Coletavel.objetosColetadosTotais >= itensNecessariosParaIniciar;

        if (isSweeping)
        {
            PerformSweep();
            return;
        }

        if (!jogoComecou)
        {
            HandlePreGameMovement();
            return;
        }

        Patrol();
    }

    void HandlePreGameMovement()
    {
        moveTimer += Time.deltaTime;

        if (moveTimer >= 5f)
        {
            moveTimer = 0f;
            if (Random.value < preGameMoveChance)
            {
                Vector3 randomPoint = transform.position + Random.insideUnitSphere * 20f;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 20f, NavMesh.AllAreas))
                {
                    agent.isStopped = false;
                    agent.SetDestination(hit.position);
                }
            }
            else
            {
                agent.isStopped = true;
            }
        }

        DirecionarParaMovimento();
    }

    void Patrol()
    {
        agent.speed = normalSpeed;

        if (!agent.hasPath || agent.remainingDistance < 1f || agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 25f;
            randomDirection += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 25f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        DirecionarParaMovimento();
    }

    void DirecionarParaMovimento()
    {
        if (agent.velocity.sqrMagnitude > 0.1f && !agent.isStopped)
        {
            Vector3 direcao = agent.velocity.normalized;
            direcao.y = 0;
            if (direcao != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direcao);
            }
        }
    }

    public void TriggerSweep(Vector3 direction)
    {
        bool jogoComecou = Coletavel.objetosColetadosTotais >= itensNecessariosParaIniciar;
        if (!jogoComecou || isSweeping) return;

        isSweeping = true;
        sweepDirection = direction.normalized;
        sweepDirection.y = 0;

        agent.isStopped = true;
        agent.enabled = false;

        if (sweepDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(sweepDirection);
        }

        Debug.Log("Gotta sweep sweep sweep!");
    }

    void PerformSweep()
    {
        transform.position += sweepDirection * sweepSpeed * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y = 0f;
        transform.position = pos;

        if (Physics.Raycast(transform.position, sweepDirection, 1.2f, collisionMask))
        {
            agent.enabled = true;
            agent.isStopped = false;
            isSweeping = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMov = other.GetComponent<PlayerMovement>();

            if (playerMov != null)
            {
                // Aplica a lentidão normal enquanto caminha por cima dela
                playerMov.estaNaVassoura = true;

                // Se estiver no bote, força o player a ser empurrado pelo script dele próprio
                if (isSweeping)
                {
                    playerMov.EmpurrarPorBote(sweepDirection, sweepSpeed);
                }
            }
        }
    }
}