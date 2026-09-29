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

    private int itensColetados = 0;

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
    [Tooltip("Distância mínima para confirmar a captura.")]
    public float distanciaCaptura = 1.5f;

    [Tooltip("Tempo entre verificações da captura.")]
    public float intervaloVerificacaoCaptura = 0.05f;

    private float ultimoCheckCaptura = 0f;

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
        agent =
            GetComponent<NavMeshAgent>();

        if (animadorInimigo == null)
        {
            animadorInimigo =
                GetComponent<Animator>();
        }

        GameObject playerObj =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            jogador =
                playerObj.transform;

            jogadorMov =
                playerObj.GetComponent<PlayerMovemen>();
        }

        cameraPrincipal =
            Camera.main;

        // Começa desativado.
        if (agent != null)
        {
            agent.enabled = false;
        }
    }

    // =====================================================
    // UPDATE
    // =====================================================

    void Update()
    {
        // =================================================
        // JÁ MORREU
        // =================================================

        if (jaMorreu)
        {
            ExecutarJumpscare();
            return;
        }

        // =================================================
        // PERSEGUIÇÃO
        // =================================================

        if (podePerseguir &&
            jogador != null &&
            agent != null &&
            agent.isActiveAndEnabled)
        {
            agent.SetDestination(
                jogador.position
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

        pontoParaOlhar.y +=
            ajusteAlturaOlhar;

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
        itensColetados++;

        // Aumenta velocidade.
        if (agent != null)
        {
            agent.speed =
                velocidadeModerada +
                (
                    itensColetados *
                    aumentoVelocidadePorItem
                );
        }

        // Drop da moeda.
        if (!moedaJaDropou)
        {
            DroparMoedaFisica();

            moedaJaDropou = true;
        }

        // Ativa perseguição.
        if (itensColetados >= itensNecessarios &&
            !jaTeleportou)
        {
            jaTeleportou = true;

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

        if (animadorInimigo != null)
        {
            animadorInimigo.SetBool(
                nomeParametroCorrida,
                true
            );
        }

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
        }
    }

    // =====================================================
    // CAPTURA
    // =====================================================

    private void OnTriggerEnter(Collider other)
    {
        // Não captura se já morreu.
        if (jaMorreu)
            return;

        // Não captura antes da perseguição.
        if (!podePerseguir)
            return;

        // =================================================
        // SÓ PLAYER
        // =================================================

        if (!other.CompareTag("Player"))
            return;

        // =================================================
        // PEGAR PlayerMovemen
        // =================================================

        PlayerMovemen player =
            other.GetComponentInParent<PlayerMovemen>();

        if (player == null)
            return;

        // =================================================
        // PROTEÇÃO CONTRA A VASSOURA
        // =================================================

        /*
         * Se o player está sendo empurrado pela vassoura,
         * não inicia o jumpscare neste frame.
         */

        if (player.SofrendoBote)
        {
            return;
        }

        // =================================================
        // DISTÂNCIA REAL
        // =================================================

        float distancia =
            Vector3.Distance(
                transform.position,
                player.transform.position
            );

        if (distancia > distanciaCaptura)
        {
            return;
        }

        // =================================================
        // INICIA CAPTURA
        // =================================================

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

        // ================================================
        // ÚLTIMA VERIFICAÇÃO
        // ================================================

        if (jogadorMov != null &&
            jogadorMov.SofrendoBote)
        {
            yield break;
        }

        float distancia =
            Vector3.Distance(
                transform.position,
                jogador.position
            );

        if (distancia > distanciaCaptura)
        {
            yield break;
        }

        // ================================================
        // MORTE
        // ================================================

        jaMorreu = true;

        // ================================================
        // CÂMERA
        // ================================================

        if (cameraPrincipal == null)
        {
            cameraPrincipal =
                Camera.main;
        }

        if (cameraPrincipal != null)
        {
            posicaoOriginalDaCamera =
                cameraPrincipal.transform.localPosition;
        }

        // ================================================
        // PARAR PERSEGUIDOR
        // ================================================

        if (agent != null)
        {
            agent.ResetPath();

            agent.velocity =
                Vector3.zero;

            agent.enabled = false;
        }

        // ================================================
        // PARAR ANIMAÇÃO
        // ================================================

        if (animadorInimigo != null)
        {
            animadorInimigo.SetBool(
                nomeParametroCorrida,
                false
            );
        }

        // ================================================
        // PARAR PLAYER
        // ================================================

        if (jogadorMov != null)
        {
            jogadorMov.BloquearMovimento();
        }

        // ================================================
        // ESPERA
        // ================================================

        yield return new WaitForSeconds(
            tempoDeSusto
        );

        // ================================================
        // GAME OVER
        // ================================================

        SceneManager.LoadScene(
            nomeCenaGameOver
        );
    }

    // =====================================================
    // MOEDA
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
