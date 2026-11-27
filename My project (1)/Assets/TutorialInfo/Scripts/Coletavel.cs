using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Coletavel : MonoBehaviour
{
    [Header("Configurações de Rotação e Flutuação")]
    public Vector3 velocidadeRotacao = new Vector3(0f, 50f, 0f);
    public float amplitude = 0.5f;
    public float velocidadeFlutuacao = 1f;

    [Header("Configurações de Coleta")]
    public float duracaoAnimacaoSumir = 0.5f;

    [Header("Efeitos Visuais e Sonoros")]
    public GameObject prefabParticulasColeta;
    public AudioClip somColeta;

    [Header("Parede de Saída Inicial")]
    public GameObject paredeBloqueioInicio;
    public int itensParaAbrirParedeInicio = 2;

    [Header("Mundo Vermelho")]
    public Color corMundo = Color.red;
    public int cadernosParaMundoVermelho = 7;

    [Header("Vitória")]
    public string nomeCenaVitoria = "Vitoria";

    private Vector3 posicaoInicial;
    private bool coletado = false;

    private static bool mundoVermelhoAtivado = false;

    public static int objetosColetadosTotais = 0;
    public const int limiteMaximoObjetos = 10;
    public static bool allCollected = false;

    private static TextMeshProUGUI textoContador;

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        posicaoInicial = transform.position;

        if (textoContador == null)
        {
            GameObject objetoTexto = GameObject.Find("TextoContador");

            if (objetoTexto != null)
            {
                textoContador = objetoTexto.GetComponent<TextMeshProUGUI>();
            }
        }

        if (paredeBloqueioInicio != null)
        {
            paredeBloqueioInicio.SetActive(
                objetosColetadosTotais < itensParaAbrirParedeInicio
            );
        }

        AtualizarTextoUI();
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (coletado)
            return;

        transform.Rotate(velocidadeRotacao * Time.deltaTime);

        float novoY =
            posicaoInicial.y +
            Mathf.Sin(Time.time * velocidadeFlutuacao) * amplitude;

        transform.position = new Vector3(
            posicaoInicial.x,
            novoY,
            posicaoInicial.z
        );
    }

    // =====================================================
    // COLETA
    // =====================================================

    private void OnMouseDown()
    {
        if (coletado || objetosColetadosTotais >= limiteMaximoObjetos)
            return;

        coletado = true;

        if (somColeta != null)
        {
            AudioSource.PlayClipAtPoint(somColeta, transform.position);
        }

        if (prefabParticulasColeta != null)
        {
            Instantiate(
                prefabParticulasColeta,
                transform.position,
                Quaternion.identity
            );
        }

        objetosColetadosTotais++;

        AtualizarTextoUI();

        Debug.Log(
            "CADERNOS COLETADOS: " +
            objetosColetadosTotais +
            "/" +
            limiteMaximoObjetos
        );

        // VITÓRIA (fica antes das portas e dos perseguidores,
        // para nenhum erro ali impedir a vitória)
        if (objetosColetadosTotais >= limiteMaximoObjetos && !allCollected)
        {
            allCollected = true;
            Debug.Log("10 CADERNOS COLETADOS! VITÓRIA!");
            StartCoroutine(RotinaVitoria());
        }

        // ABRIR PAREDE INICIAL
        if (objetosColetadosTotais >= itensParaAbrirParedeInicio &&
            paredeBloqueioInicio != null)
        {
            paredeBloqueioInicio.SetActive(false);
            Debug.Log("PAREDE INICIAL ABERTA!");
        }

        // ATIVAR MUNDO VERMELHO
        if (objetosColetadosTotais >= cadernosParaMundoVermelho &&
            !mundoVermelhoAtivado)
        {
            AtivarMundoVermelho();
        }

        // ATUALIZAR PORTAS
        PortaAutomatica[] portas =
            FindObjectsByType<PortaAutomatica>(FindObjectsSortMode.None);

        foreach (PortaAutomatica porta in portas)
        {
            if (porta != null)
                porta.SomarObjeto();
        }

        // AVISAR TODOS OS PERSEGUIDORES
        Perseguidor[] perseguidores =
            FindObjectsByType<Perseguidor>(FindObjectsSortMode.None);

        foreach (Perseguidor inimigo in perseguidores)
        {
            if (inimigo != null)
            {
                inimigo.RegistrarColeta();

                Debug.Log(
                    "COLETA ENVIADA AO PERSEGUIDOR: " + inimigo.name
                );
            }
        }

        StartCoroutine(AnimacaoColeta());
    }

    // =====================================================
    // MUNDO VERMELHO
    // =====================================================

    private void AtivarMundoVermelho()
    {
        mundoVermelhoAtivado = true;

        Debug.Log("O MUNDO FICOU VERMELHO!");

        Renderer[] renderers =
            FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer rend in renderers)
        {
            if (rend == null)
                continue;

            foreach (Material material in rend.materials)
            {
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", corMundo);
                }
                else if (material.HasProperty("_Color"))
                {
                    material.SetColor("_Color", corMundo);
                }
            }
        }
    }

    // =====================================================
    // VITÓRIA
    // =====================================================

    private IEnumerator RotinaVitoria()
    {
        // WaitForSecondsRealtime funciona mesmo com Time.timeScale = 0
        yield return new WaitForSecondsRealtime(0.6f);
        IrParaVitoria();
    }

    private void IrParaVitoria()
    {
        Debug.Log("CARREGANDO CENA: " + nomeCenaVitoria);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(nomeCenaVitoria);
    }

    // Chame no Jogar() do menu, antes de carregar a cena do jogo
    public static void ResetarEstado()
    {
        objetosColetadosTotais = 0;
        allCollected = false;
        mundoVermelhoAtivado = false;
        textoContador = null;
    }

    // =====================================================
    // TEXTO UI
    // =====================================================

    private void AtualizarTextoUI()
    {
        if (textoContador == null)
        {
            GameObject objetoTexto = GameObject.Find("TextoContador");

            if (objetoTexto != null)
            {
                textoContador = objetoTexto.GetComponent<TextMeshProUGUI>();
            }
        }

        if (textoContador != null)
        {
            textoContador.text =
                objetosColetadosTotais + "/" + limiteMaximoObjetos;
        }
    }

    // =====================================================
    // ANIMAÇÃO DE COLETA
    // =====================================================

    private IEnumerator AnimacaoColeta()
    {
        Vector3 escalaInicial = transform.localScale;

        if (TryGetComponent<Collider>(out Collider colisor))
            colisor.enabled = false;

        if (TryGetComponent<Collider2D>(out Collider2D colisor2D))
            colisor2D.enabled = false;

        if (duracaoAnimacaoSumir > 0f)
        {
            float tempoDecorrido = 0f;

            while (tempoDecorrido < duracaoAnimacaoSumir)
            {
                tempoDecorrido += Time.deltaTime;

                float progresso = Mathf.Clamp01(
                    tempoDecorrido / duracaoAnimacaoSumir
                );

                transform.Rotate(velocidadeRotacao * 10f * Time.deltaTime);

                transform.localScale = Vector3.Lerp(
                    escalaInicial,
                    Vector3.zero,
                    progresso
                );

                yield return null;
            }
        }

        transform.localScale = Vector3.zero;

        // No último caderno não destrói o objeto: a RotinaVitoria roda nele
        if (!allCollected)
            Destroy(gameObject);
    }
}