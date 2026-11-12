
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BoxCollider))]
public class Sweep : MonoBehaviour
{
    [Header("Sala da Sweep")]
    public Transform salaDaSweep;

    [Header("Movimento")]
    public float velocidadeNormal = 3.5f;
    public float velocidadeSweep = 30f;
    public float aceleracao = 50f;
    public float velocidadeRotacao = 720f;

    [Header("Início")]
    public int itensNecessariosParaIniciar = 2;

    [Header("Patrulha")]
    public float distanciaPatrulha = 25f;
    public float distanciaChegada = 1.5f;

    [Header("Duração da varrida")]
    public float duracaoSweep = 12f;

    [Header("Descanso")]
    public float tempoDescanso = 8f;

    [Header("Colisões")]
    public LayerMask collisionMask;
    public float distanciaParede = 1.2f;

    [Header("Player")]
    public bool podeEmpurrarPlayer = true;

    private NavMeshAgent agent;
    private BoxCollider boxCollider;

    private bool jogoComecou = false;
    private bool isSweeping = false;
    private bool estaDescansando = false;
    private bool voltandoParaSala = false;

    private Vector3 sweepDirection;

    private Coroutine rotina;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        boxCollider = GetComponent<BoxCollider>();

        agent.speed = velocidadeNormal;
        agent.acceleration = aceleracao;
        agent.angularSpeed = velocidadeRotacao;
        agent.updateRotation = true;

        boxCollider.isTrigger = true;

        // Espera os cadernos.
        agent.isStopped = true;
        agent.ResetPath();

        Debug.Log("Sweep está esperando os cadernos.");
    }

    private void Update()
    {
        if (!jogoComecou)
        {
            if (Coletavel.objetosColetadosTotais <
                itensNecessariosParaIniciar)
                return;

            jogoComecou = true;

            Debug.Log("SWEEP COMEÇOU!");

            rotina = StartCoroutine(RotinaSweep());
        }
    }

    // =========================================
    // ROTINA PRINCIPAL
    // =========================================

    private IEnumerator RotinaSweep()
    {
        while (true)
        {
            estaDescansando = false;
            voltandoParaSala = false;

            Debug.Log("SWEEP SAIU DA SALA!");

            // Escolhe destino.
            Vector3 destino = EscolherDestino();

            if ((destino - transform.position).sqrMagnitude > 1f)
            {
                Vector3 direcao = destino - transform.position;
                direcao.y = 0f;

                sweepDirection = direcao.normalized;

                // Começa a varrer.
                isSweeping = true;

                agent.speed = velocidadeSweep;
                agent.acceleration = aceleracao;
                agent.isStopped = false;

                agent.SetDestination(destino);

                Debug.Log("GOTTA SWEEP SWEEP SWEEP!");

                float tempo = 0f;

                while (tempo < duracaoSweep)
                {
                    if (!agent.enabled)
                        break;

                    Vector3 origem = boxCollider.bounds.center;

                    bool bateu = Physics.Raycast(
                        origem,
                        sweepDirection,
                        distanciaParede,
                        collisionMask,
                        QueryTriggerInteraction.Ignore
                    );

                    if (bateu)
                    {
                        Debug.Log("SWEEP ENCONTROU UMA PAREDE!");
                        break;
                    }

                    if (!agent.pathPending &&
                        agent.remainingDistance <= distanciaChegada)
                        break;

                    tempo += Time.deltaTime;

                    yield return null;
                }
            }

            // Finaliza varrida.
            isSweeping = false;
            sweepDirection = Vector3.zero;

            agent.isStopped = true;
            agent.ResetPath();

            Debug.Log("SWEEP TERMINOU!");

            // Volta para casa.
            if (salaDaSweep != null)
            {
                voltandoParaSala = true;

                Debug.Log("SWEEP ESTÁ VOLTANDO PARA CASA!");

                yield return StartCoroutine(
                    IrAteDestino(salaDaSweep.position)
                );

                voltandoParaSala = false;
            }
            else
            {
                Debug.LogWarning("Configure a sala da Sweep!");
            }

            // Descanso.
            estaDescansando = true;

            agent.isStopped = true;

            Debug.Log("SWEEP ESTÁ DESCANSANDO!");

            yield return new WaitForSeconds(tempoDescanso);
        }
    }

    // =========================================
    // ESCOLHER DESTINO
    // =========================================

    private Vector3 EscolherDestino()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 ponto =
                transform.position +
                Random.insideUnitSphere * distanciaPatrulha;

            ponto.y = transform.position.y;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                ponto,
                out hit,
                distanciaPatrulha,
                NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();

                if (agent.CalculatePath(hit.position, path) &&
                    path.status == NavMeshPathStatus.PathComplete)
                {
                    return hit.position;
                }
            }
        }

        return transform.position;
    }

    // =========================================
    // IR ATÉ A SALA
    // =========================================

    private IEnumerator IrAteDestino(Vector3 destino)
    {
        if (!agent.enabled)
            yield break;

        NavMeshHit hit;

        if (!NavMesh.SamplePosition(
            destino,
            out hit,
            5f,
            NavMesh.AllAreas))
        {
            Debug.LogWarning("Destino fora do NavMesh!");
            yield break;
        }

        // MESMA VELOCIDADE DA VARRIDA!
        agent.speed = velocidadeSweep;
        agent.acceleration = aceleracao;
        agent.isStopped = false;

        agent.SetDestination(hit.position);

        while (agent.enabled)
        {
            if (!agent.pathPending &&
                agent.remainingDistance <= distanciaChegada)
                break;

            if (!agent.pathPending &&
                agent.pathStatus == NavMeshPathStatus.PathInvalid)
                break;

            yield return null;
        }

        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    // =========================================
    // PLAYER
    // =========================================

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (other.GetComponentInParent<Perseguidor>() != null)
            return;

        PlayerMovemen player =
            other.GetComponentInParent<PlayerMovemen>();

        if (player == null)
            return;

        player.estaNaVassoura = true;

        if (isSweeping && podeEmpurrarPlayer)
        {
            player.EmpurrarPorBote(
                sweepDirection,
                velocidadeSweep
            );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovemen player =
            other.GetComponentInParent<PlayerMovemen>();

        if (player != null)
            player.estaNaVassoura = false;
    }

    // =========================================
    // INFORMAÇÕES
    // =========================================

    public bool EstaFazendoSweep()
    {
        return isSweeping;
    }

    public bool EstaDescansando()
    {
        return estaDescansando;
    }

    public bool EstaVoltandoParaSala()
    {
        return voltandoParaSala;
    }

    public Vector3 GetSweepDirection()
    {
        return sweepDirection;
    }

    // =========================================
    // DEBUG
    // =========================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 origem = transform.position;

        BoxCollider col = GetComponent<BoxCollider>();

        if (col != null)
            origem = col.bounds.center;

        Vector3 direcao =
            sweepDirection.sqrMagnitude > 0.001f
            ? sweepDirection
            : transform.forward;

        Gizmos.DrawLine(
            origem,
            origem + direcao * distanciaParede
        );

        if (salaDaSweep != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(salaDaSweep.position, 1f);
        }
    }
}
