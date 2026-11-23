
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Perseguidor : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform jogador;
    private PlayerMovemen jogadorMov;
    private Camera cameraPrincipal;
    private EmpurraoRefri empurraoRefri;

    private int itensColetados = 0;

    [Header("Objetos Necessários")]
    public int itensNecessarios = 2;

    private bool podePerseguir = false;
    private bool jaTeleportou = false;
    private bool jaMorreu = false;

    [Header("Configurações de Animação")]
    public Animator animadorInimigo;
    public string nomeParametroCorrida = "isChasing";

    [Header("Configurações de Teleporte")]
    public Transform pontoDeTeleporteMonstro;

    [Header("Configurações de Velocidade")]
    public float velocidadeModerada = 2f;
    public float aceleracaoInimigo = 4f;
    public float aumentoVelocidadePorItem = 1f;

    [Header("Configuração de Captura")]
    public float distanciaCaptura = 1.5f;
    public float intervaloVerificacaoCaptura = 0.05f;

    [Header("Configurações do Jumpscare")]
    public float tempoDeSusto = 2f;
    public float ajusteAlturaOlhar = 1.5f;

    [Header("Configuração de Game Over")]
    public string nomeCenaGameOver = "game over";

    [Header("Efeito de Tremor")]
    public float forcaDoTremor = 0.3f;

    private Vector3 posicaoOriginalDaCamera;

    [Header("Drop da Moeda")]
    public GameObject prefabMoeda;
    public float ajusteAlturaMoeda = 0.5f;
    public float distanciaLadoMoeda = 1.2f;

    private bool moedaJaDropou = false;

    // =====================================================
    // START
    // =====================================================

    void Start()
    {
        EncontrarAgent();

        empurraoRefri = GetComponentInChildren<EmpurraoRefri>();

        if (empurraoRefri == null)
            empurraoRefri = GetComponentInParent<EmpurraoRefri>();

        if (animadorInimigo == null)
            animadorInimigo = GetComponentInChildren<Animator>();

        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            jogador = playerObj.transform;

            jogadorMov =
                playerObj.GetComponent<PlayerMovemen>();

            if (jogadorMov == null)
                jogadorMov = playerObj.GetComponentInChildren<PlayerMovemen>();
        }

        cameraPrincipal = Camera.main;

        podePerseguir = false;

        if (agent != null)
        {
            agent.enabled = false;
        }
        else
        {
            Debug.LogError("NAVMESH AGENT NÃO ENCONTRADO!");
        }

        AtualizarVelocidade();
        AtualizarAnimacao(false);

        Debug.Log("PERSEGUIDOR INICIADO: aguardando os cadernos.");
    }

    // =====================================================
    // ENCONTRAR NAVMESH
    // =====================================================

    private void EncontrarAgent()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
            agent = GetComponentInChildren<NavMeshAgent>();

        if (agent == null)
            agent = GetComponentInParent<NavMeshAgent>();
    }

    // =====================================================
    // UPDATE
    // =====================================================

    void Update()
    {
        if (jaMorreu)
        {
            ExecutarJumpscare();
            return;
        }

        if (empurraoRefri != null &&
            empurraoRefri.EstaSendoEmpurrado())
        {
            AtualizarAnimacao(false);
            return;
        }

        if (podePerseguir &&
            jogador != null &&
            agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.SetDestination(jogador.position);

            AtualizarAnimacao(true);
        }
        else
        {
            AtualizarAnimacao(false);
        }
    }

    // =====================================================
    // ANIMAÇÃO
    // =====================================================

    private void AtualizarAnimacao(bool correndo)
    {
        if (animadorInimigo != null)
        {
            animadorInimigo.SetBool(
                nomeParametroCorrida,
                correndo
            );
        }
    }

    // =====================================================
    // ATUALIZAR VELOCIDADE
    // =====================================================

    private void AtualizarVelocidade()
    {
        if (agent == null)
            EncontrarAgent();

        if (agent == null)
            return;

        float novaVelocidade =
            velocidadeModerada +
            (itensColetados * aumentoVelocidadePorItem);

        agent.speed = novaVelocidade;

        Debug.Log(
            "================================"
        );

        Debug.Log(
            "VELOCIDADE DO PERSEGUIDOR: " +
            novaVelocidade
        );

        Debug.Log(
            "CADERNOS RECEBIDOS: " +
            itensColetados
        );

        Debug.Log(
            "================================"
        );
    }

    // =====================================================
    // JUMPSCARE
    // =====================================================

    private void ExecutarJumpscare()
    {
        if (cameraPrincipal == null)
            return;

        Vector3 pontoParaOlhar = transform.position;

        pontoParaOlhar.y += ajusteAlturaOlhar;

        cameraPrincipal.transform.LookAt(pontoParaOlhar);

        Vector3 tremor =
            Random.insideUnitSphere * forcaDoTremor;

        cameraPrincipal.transform.localPosition =
            posicaoOriginalDaCamera + tremor;
    }

    // =====================================================
    // REGISTRAR COLETA
    // =====================================================

    public void RegistrarColeta()
    {
        if (jaMorreu)
            return;

        // CADA CADERNO AUMENTA A VELOCIDADE
        itensColetados++;

        Debug.Log("CADERNO RECEBIDO PELO PERSEGUIDOR!");

        // ATUALIZAR VELOCIDADE
        AtualizarVelocidade();

        // DROP DA MOEDA
        if (!moedaJaDropou)
        {
            DroparMoedaFisica();
            moedaJaDropou = true;
        }

        // ATIVAR PERSEGUIÇÃO APENAS UMA VEZ
        if (itensColetados >= itensNecessarios &&
            !jaTeleportou)
        {
            jaTeleportou = true;

            Debug.Log(
                "QUANTIDADE NECESSÁRIA ATINGIDA! MONSTRO ATIVADO!"
            );

            TeleportarMonstro();

            ComecarAPerseguir();
        }
    }

    // =====================================================
    // TELEPORTE
    // =====================================================

    private void TeleportarMonstro()
    {
        if (pontoDeTeleporteMonstro == null)
        {
            Debug.LogWarning(
                "Ponto de teleporte do monstro não configurado."
            );

            return;
        }

        if (agent != null)
        {
            agent.enabled = false;
        }

        transform.position =
            pontoDeTeleporteMonstro.position;

        transform.rotation =
            pontoDeTeleporteMonstro.rotation;

        if (agent != null && agent.transform != transform)
        {
            agent.transform.position =
                pontoDeTeleporteMonstro.position;

            agent.transform.rotation =
                pontoDeTeleporteMonstro.rotation;
        }

        Debug.Log("MONSTRO TELEPORTADO!");
    }

    // =====================================================
    // COMEÇAR A PERSEGUIR
    // =====================================================

    private void ComecarAPerseguir()
    {
        podePerseguir = true;

        if (agent != null)
        {
            agent.enabled = true;

            // APLICAR VELOCIDADE ACUMULADA
            AtualizarVelocidade();

            agent.acceleration = aceleracaoInimigo;

            // GARANTIR QUE O AGENT ESTEJA NO NAVMESH
            if (!agent.isOnNavMesh)
            {
                NavMeshHit hit;

                if (NavMesh.SamplePosition(
                    agent.transform.position,
                    out hit,
                    5f,
                    agent.areaMask))
                {
                    agent.Warp(hit.position);
                }
            }

            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.ResetPath();
            }
            else
            {
                Debug.LogWarning(
                    "MONSTRO NÃO ESTÁ SOBRE O NAVMESH!"
                );
            }
        }

        AtualizarAnimacao(true);

        Debug.Log("PERSEGUIDOR COMEÇOU A CORRER!");
    }

    // =====================================================
    // CAPTURA
    // =====================================================

    private void OnTriggerEnter(Collider other)
    {
        if (jaMorreu)
            return;

        if (!podePerseguir)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerMovemen player =
            other.GetComponentInParent<PlayerMovemen>();

        if (player == null)
            return;

        if (player.SofrendoBote)
            return;

        float distancia =
            Vector3.Distance(
                transform.position,
                player.transform.position
            );

        if (distancia > distanciaCaptura)
            return;

        StartCoroutine(RotinaFocarNoMonstro());
    }

    // =====================================================
    // ROTINA DO JUMPSCARE
    // =====================================================

    IEnumerator RotinaFocarNoMonstro()
    {
        if (jaMorreu)
            yield break;

        if (jogadorMov != null &&
            jogadorMov.SofrendoBote)
        {
            yield break;
        }

        if (jogador == null)
            yield break;

        float distancia =
            Vector3.Distance(
                transform.position,
                jogador.position
            );

        if (distancia > distanciaCaptura)
            yield break;

        jaMorreu = true;

        if (cameraPrincipal == null)
        {
            cameraPrincipal = Camera.main;
        }

        if (cameraPrincipal != null)
        {
            posicaoOriginalDaCamera =
                cameraPrincipal.transform.localPosition;
        }

        // PARAR PERSEGUIDOR
        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }

            agent.enabled = false;
        }

        // PARAR ANIMAÇÃO
        AtualizarAnimacao(false);

        // PARAR JOGADOR
        if (jogadorMov != null)
        {
            jogadorMov.BloquearMovimento();
        }

        yield return new WaitForSeconds(tempoDeSusto);

        // GAME OVER
        SceneManager.LoadScene(nomeCenaGameOver);
    }

    // =====================================================
    // DROP DA MOEDA
    // =====================================================

    private void DroparMoedaFisica()
    {
        if (prefabMoeda == null)
            return;

        Vector3 posicaoDrop = transform.position;

        posicaoDrop.y += ajusteAlturaMoeda;

        posicaoDrop +=
            transform.right * distanciaLadoMoeda;

        Instantiate(
            prefabMoeda,
            posicaoDrop,
            Quaternion.identity
        );
    }
}
