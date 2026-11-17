
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
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
            agent = GetComponentInChildren<NavMeshAgent>();

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
        }

        cameraPrincipal = Camera.main;

        // COMEÇA PARADO
        podePerseguir = false;

        if (agent != null)
        {
            agent.enabled = false;
        }

        // ANIMAÇÃO DESLIGADA NO COMEÇO
        AtualizarAnimacao(false);

        Debug.Log("PERSEGUIDOR INICIADO: aguardando os objetos.");
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

        // SE ESTIVER SENDO EMPURRADO PELO REFRIGERANTE
        if (empurraoRefri != null &&
            empurraoRefri.EstaSendoEmpurrado())
        {
            AtualizarAnimacao(false);
            return;
        }

        // SÓ PERSEGUE DEPOIS DOS OBJETOS
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
    // JUMPSCARE
    // =====================================================

    private void ExecutarJumpscare()
    {
        if (cameraPrincipal == null)
            return;

        Vector3 pontoParaOlhar =
            transform.position;

        pontoParaOlhar.y += ajusteAlturaOlhar;

        cameraPrincipal.transform.LookAt(
            pontoParaOlhar
        );

        Vector3 tremor =
            Random.insideUnitSphere *
            forcaDoTremor;

        cameraPrincipal.transform.localPosition =
            posicaoOriginalDaCamera +
            tremor;
    }

    // =====================================================
    // REGISTRAR COLETA
    // =====================================================

    public void RegistrarColeta()
    {
        if (jaTeleportou || jaMorreu)
            return;

        itensColetados++;

        Debug.Log(
            "OBJETOS COLETADOS: " +
            itensColetados
        );

        // AUMENTAR VELOCIDADE
        if (agent != null)
        {
            agent.speed =
                velocidadeModerada +
                (
                    itensColetados *
                    aumentoVelocidadePorItem
                );
        }

        // DROP DA MOEDA
        if (!moedaJaDropou)
        {
            DroparMoedaFisica();

            moedaJaDropou = true;
        }

        // ATIVAR APENAS QUANDO PEGAR OS 2 OBJETOS
        if (itensColetados >= itensNecessarios)
        {
            jaTeleportou = true;

            Debug.Log(
                "DOIS OBJETOS COLETADOS! PERSEGUIDOR ATIVADO!"
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

            agent.speed =
                velocidadeModerada +
                (
                    itensColetados *
                    aumentoVelocidadePorItem
                );

            agent.acceleration =
                aceleracaoInimigo;

            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
        }

        // COMEÇAR ANIMAÇÃO DE CORRIDA
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

        StartCoroutine(
            RotinaFocarNoMonstro()
        );
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

        // CÂMERA
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

        // ESPERAR
        yield return new WaitForSeconds(
            tempoDeSusto
        );

        // GAME OVER
        SceneManager.LoadScene(
            nomeCenaGameOver
        );
    }

    // =====================================================
    // DROP DA MOEDA
    // =====================================================

    private void DroparMoedaFisica()
    {
        if (prefabMoeda == null)
            return;

        Vector3 posicaoDrop =
            transform.position;

        posicaoDrop.y +=
            ajusteAlturaMoeda;

        posicaoDrop +=
            transform.right *
            distanciaLadoMoeda;

        Instantiate(
            prefabMoeda,
            posicaoDrop,
            Quaternion.identity
        );
    }
}
