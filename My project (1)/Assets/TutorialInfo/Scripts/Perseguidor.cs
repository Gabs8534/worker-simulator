using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Perseguidor : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform jogador;
    private Camera cameraPrincipal;

    private int itensColetados = 0;
    public int itensNecessarios = 2;
    private bool podePerseguir = false;
    private bool jaTeleportou = false; // Trava para garantir que o teleporte só ocorra 1 vez

    [Header("Configurações de Teleporte do Perseguidor")]
    [Tooltip("Arraste aqui o GameObject vazio (Transform) para onde o monstro será teleportado ao pegar os 2 itens.")]
    public Transform pontoDeTeleporteMonstro;

    [Header("Configurações de Velocidade")]
    public float velocidadeModerada = 2.0f;
    public float aceleracaoInimigo = 4.0f;

    [Header("Configurações do Jumpscare de Foco")]
    [Tooltip("Tempo que o jogador fica olhando para a cara do monstro antes de mudar de cena.")]
    public float tempoDeSusto = 2.0f;
    [Tooltip("Ajuste de altura para a câmera olhar (ex: olhar para a cabeça do monstro).")]
    public float ajusteAlturaOlhar = 1.5f;

    [Header("Configuração de Fim de Jogo")]
    [Tooltip("Nome exato da cena para onde o jogador vai ao morrer.")]
    public string nomeCenaGameOver = "game over";

    [Header("Efeito de Tremor (Screen Shake)")]
    [Tooltip("Força do tremor. Valores entre 0.1 e 0.5 já são bem fortes!")]
    public float forcaDoTremor = 0.3f;
    private Vector3 posicaoOriginalDaCamera;

    private bool jaMorreu = false;

    // ====================================================================
    // CONFIGURAÇÕES DE DROP DA MOEDA
    // ====================================================================
    [Header("Configurações de Drop (Moeda)")]
    [Tooltip("Arraste o Prefab da sua moeda 2D aqui.")]
    public GameObject prefabMoeda;
    [Tooltip("Ajuste a altura que a moeda vai surgir do chão.")]
    public float ajusteAlturaMoeda = 0.5f;
    [Tooltip("Distância para o lado que a moeda vai spawnar para não ficar dentro do corpo.")]
    public float distanciaLadoMoeda = 1.2f;

    private bool moedaJaDropou = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.enabled = false;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            jogador = playerObj.transform;
        }

        cameraPrincipal = Camera.main;
    }

    void Update()
    {
        if (jaMorreu && cameraPrincipal != null)
        {
            Vector3 pontoParaOlhar = transform.position;
            pontoParaOlhar.y += ajusteAlturaOlhar;
            cameraPrincipal.transform.LookAt(pontoParaOlhar);

            Vector3 tremorAleatorio = Random.insideUnitSphere * forcaDoTremor;
            cameraPrincipal.transform.localPosition = posicaoOriginalDaCamera + tremorAleatorio;

            return;
        }

        if (podePerseguir && jogador != null && agent != null && agent.isActiveAndEnabled && !jaMorreu)
        {
            agent.SetDestination(jogador.position);
        }
    }

    // ====================================================================
    // REGISTRAR COLETA
    // ====================================================================
    public void RegistrarColeta()
    {
        itensColetados++;

        // 1. Dropa a moeda no primeiro objeto coletado
        if (!moedaJaDropou)
        {
            DroparMoedaFisica();
            moedaJaDropou = true;
        }

        // 2. Só teleporta e ativa a perseguição se atingiu a meta E ainda não tiver feito isso antes
        if (itensColetados >= itensNecessarios && !jaTeleportou)
        {
            jaTeleportou = true; // Trava ativada para nunca mais repetir
            TeleportarMonstro();
            ComeçarAPerseguir();
        }
    }

    void TeleportarMonstro()
    {
        if (pontoDeTeleporteMonstro != null)
        {
            if (agent != null) agent.enabled = false;

            transform.position = pontoDeTeleporteMonstro.position;
            transform.rotation = pontoDeTeleporteMonstro.rotation;

            if (agent != null) agent.enabled = true;
        }
        else
        {
            Debug.LogWarning("O 'Ponto de Teleporte do Monstro' não foi configurado no Inspector!");
        }
    }

    void ComeçarAPerseguir()
    {
        podePerseguir = true;
        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = velocidadeModerada;
            agent.acceleration = aceleracaoInimigo;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (podePerseguir && other.CompareTag("Player") && !jaMorreu)
        {
            StartCoroutine(RotinaFocarNoMonstro());
        }
    }

    IEnumerator RotinaFocarNoMonstro()
    {
        jaMorreu = true;

        if (cameraPrincipal == null) cameraPrincipal = Camera.main;
        if (cameraPrincipal != null)
        {
            posicaoOriginalDaCamera = cameraPrincipal.transform.localPosition;
        }

        if (agent != null)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        if (jogador != null)
        {
            PlayerMovement controleJogador = jogador.GetComponent<PlayerMovement>();
            if (controleJogador != null)
            {
                controleJogador.enabled = false;
            }
        }

        yield return new WaitForSeconds(tempoDeSusto);

        SceneManager.LoadScene(nomeCenaGameOver);
    }

    private void DroparMoedaFisica()
    {
        if (prefabMoeda != null)
        {
            Vector3 posicaoDrop = transform.position;
            posicaoDrop.y += ajusteAlturaMoeda;
            posicaoDrop += transform.right * distanciaLadoMoeda;

            Instantiate(prefabMoeda, posicaoDrop, Quaternion.identity);
        }
    }
}