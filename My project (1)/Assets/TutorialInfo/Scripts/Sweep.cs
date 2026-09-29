using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BoxCollider))]
public class Sweep : MonoBehaviour
{
    [Header("Movimento")]
    public float normalSpeed = 3.5f;
    public float sweepSpeed = 15f;

    [Tooltip("Somente paredes/obstáculos.")]
    public LayerMask collisionMask;

    [Header("Início")]
    public int itensNecessariosParaIniciar = 2;

    [Range(0f, 1f)]
    public float preGameMoveChance = 0.2f;

    [Header("Patrulha")]
    public float distanciaPatrulha = 25f;
    public float distanciaDestino = 1f;

    [Header("Sweep")]
    public float distanciaParede = 1.2f;
    public float duracaoMaximaSweep = 5f;

    [Header("Player")]
    public bool podeEmpurrarPlayer = true;

    private NavMeshAgent agent;
    private BoxCollider boxCollider;

    private bool isSweeping = false;

    private Vector3 sweepDirection =
        Vector3.zero;

    private float moveTimer = 0f;
    private float sweepTimer = 0f;

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        agent =
            GetComponent<NavMeshAgent>();

        boxCollider =
            GetComponent<BoxCollider>();

        agent.speed =
            normalSpeed;

        agent.acceleration =
            999f;

        agent.angularSpeed =
            999f;

        agent.updateRotation =
            false;

        boxCollider.isTrigger =
            true;
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        bool jogoComecou =
            Coletavel.objetosColetadosTotais >=
            itensNecessariosParaIniciar;

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

    // =====================================================
    // PRÉ-JOGO
    // =====================================================

    private void HandlePreGameMovement()
    {
        if (!agent.enabled)
            return;

        moveTimer +=
            Time.deltaTime;

        if (moveTimer < 5f)
            return;

        moveTimer = 0f;

        if (Random.value <
            preGameMoveChance)
        {
            Vector3 randomPoint =
                transform.position +
                Random.insideUnitSphere *
                20f;

            randomPoint.y =
                transform.position.y;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                randomPoint,
                out hit,
                20f,
                NavMesh.AllAreas))
            {
                agent.isStopped =
                    false;

                agent.SetDestination(
                    hit.position
                );
            }
        }
        else
        {
            agent.isStopped =
                true;

            agent.ResetPath();
        }

        DirecionarParaMovimento();
    }

    // =====================================================
    // PATRULHA
    // =====================================================

    private void Patrol()
    {
        if (!agent.enabled)
            return;

        agent.speed =
            normalSpeed;

        agent.isStopped =
            false;

        if (!agent.hasPath ||
            agent.remainingDistance <=
            distanciaDestino ||
            agent.pathStatus ==
            NavMeshPathStatus.PathPartial ||
            agent.pathStatus ==
            NavMeshPathStatus.PathInvalid)
        {
            EscolherDestino();
        }

        DirecionarParaMovimento();
    }

    // =====================================================
    // DESTINO
    // =====================================================

    private void EscolherDestino()
    {
        Vector3 randomDirection =
            Random.insideUnitSphere *
            distanciaPatrulha;

        randomDirection +=
            transform.position;

        randomDirection.y =
            transform.position.y;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            randomDirection,
            out hit,
            distanciaPatrulha,
            NavMesh.AllAreas))
        {
            agent.SetDestination(
                hit.position
            );
        }
    }

    // =====================================================
    // ROTAÇÃO
    // =====================================================

    private void DirecionarParaMovimento()
    {
        if (!agent.enabled ||
            agent.isStopped)
            return;

        if (agent.velocity.sqrMagnitude <
            0.01f)
            return;

        Vector3 direcao =
            agent.velocity;

        direcao.y = 0f;

        if (direcao.sqrMagnitude <
            0.001f)
            return;

        transform.rotation =
            Quaternion.LookRotation(
                direcao.normalized
            );
    }

    // =====================================================
    // INICIAR SWEEP
    // =====================================================

    public void TriggerSweep(
        Vector3 direction)
    {
        bool jogoComecou =
            Coletavel.objetosColetadosTotais >=
            itensNecessariosParaIniciar;

        if (!jogoComecou)
            return;

        if (isSweeping)
            return;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.001f)
            return;

        sweepDirection =
            direction.normalized;

        isSweeping =
            true;

        sweepTimer =
            0f;

        if (agent.enabled)
        {
            agent.isStopped =
                true;

            agent.ResetPath();

            agent.enabled =
                false;
        }

        transform.rotation =
            Quaternion.LookRotation(
                sweepDirection
            );

        Debug.Log(
            "Gotta sweep sweep sweep!"
        );
    }

    // =====================================================
    // EXECUTAR SWEEP
    // =====================================================

    private void PerformSweep()
    {
        sweepTimer +=
            Time.deltaTime;

        if (sweepDirection.sqrMagnitude <
            0.001f)
        {
            FinalizarSweep();
            return;
        }

        // ================================================
        // RAYCAST
        // ================================================

        Vector3 origem =
            boxCollider.bounds.center;

        bool bateu =
            Physics.Raycast(
                origem,
                sweepDirection,
                distanciaParede,
                collisionMask,
                QueryTriggerInteraction.Ignore
            );

        if (bateu)
        {
            FinalizarSweep();
            return;
        }

        // ================================================
        // MOVIMENTO
        // ================================================

        transform.position +=
            sweepDirection *
            sweepSpeed *
            Time.deltaTime;

        // ================================================
        // TIMEOUT
        // ================================================

        if (sweepTimer >=
            duracaoMaximaSweep)
        {
            FinalizarSweep();
        }
    }

    // =====================================================
    // FINALIZAR
    // =====================================================

    private void FinalizarSweep()
    {
        isSweeping =
            false;

        sweepTimer =
            0f;

        sweepDirection =
            Vector3.zero;

        if (!agent.enabled)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                transform.position,
                out hit,
                3f,
                NavMesh.AllAreas))
            {
                transform.position =
                    hit.position;
            }

            agent.enabled =
                true;
        }

        agent.isStopped =
            false;

        agent.speed =
            normalSpeed;
    }

    // =====================================================
    // PLAYER
    // =====================================================

    private void OnTriggerStay(Collider other)
    {
        // =================================================
        // MUITO IMPORTANTE:
        // SÓ O PLAYER PODE ENTRAR NESSA LÓGICA.
        // =================================================

        if (!other.CompareTag("Player"))
            return;

        PlayerMovemen player =
            other.GetComponentInParent<PlayerMovemen>();

        if (player == null)
            return;

        // Player está sobre a vassoura.
        player.estaNaVassoura =
            true;

        // Empurra somente durante o sweep.
        if (isSweeping &&
            podeEmpurrarPlayer)
        {
            player.EmpurrarPorBote(
                sweepDirection,
                sweepSpeed
            );
        }
    }

    // =====================================================
    // DEBUG
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.red;

        Vector3 origem =
            transform.position;

        if (boxCollider != null)
        {
            origem =
                boxCollider.bounds.center;
        }

        Vector3 direcao =
            isSweeping &&
            sweepDirection.sqrMagnitude >
            0.001f
                ? sweepDirection
                : transform.forward;

        Gizmos.DrawLine(
            origem,
            origem +
            direcao.normalized *
            distanciaParede
        );
    }

    // =====================================================
    // ACESSO
    // =====================================================

    public bool EstaFazendoSweep()
    {
        return isSweeping;
    }

    public Vector3 GetSweepDirection()
    {
        return sweepDirection;
    }
}
