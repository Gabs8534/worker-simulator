
using System.Collections;
using UnityEngine;
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

    [Header("Configurações de Fuga")]
    [SerializeField] private ControleSaidas controleSaidas;

    [Header("Parede de Saída Inicial")]
    public GameObject paredeBloqueioInicio;
    public int itensParaAbrirParedeInicio = 2;

    private Vector3 posicaoInicial;
    private bool coletado = false;

    public static int objetosColetadosTotais = 0;
    public const int limiteMaximoObjetos = 10;
    public static bool allCollected = false;

    private static TextMeshProUGUI textoContador;

    private void Awake()
    {
        if (controleSaidas == null)
            controleSaidas = FindFirstObjectByType<ControleSaidas>();
    }

    private void Start()
    {
        posicaoInicial = transform.position;

        if (textoContador == null)
        {
            GameObject objetoTexto = GameObject.Find("TextoContador");

            if (objetoTexto != null)
                textoContador = objetoTexto.GetComponent<TextMeshProUGUI>();
        }

        if (paredeBloqueioInicio != null)
        {
            paredeBloqueioInicio.SetActive(
                objetosColetadosTotais < itensParaAbrirParedeInicio
            );
        }

        AtualizarTextoUI();
    }

    private void Update()
    {
        if (coletado)
            return;

        transform.Rotate(velocidadeRotacao * Time.deltaTime);

        float novoY = posicaoInicial.y +
            Mathf.Sin(Time.time * velocidadeFlutuacao) * amplitude;

        transform.position = new Vector3(
            posicaoInicial.x,
            novoY,
            posicaoInicial.z
        );
    }

    private void OnMouseDown()
    {
        if (coletado || objetosColetadosTotais >= limiteMaximoObjetos)
            return;

        coletado = true;

        if (somColeta != null)
            AudioSource.PlayClipAtPoint(somColeta, transform.position);

        if (prefabParticulasColeta != null)
            Instantiate(prefabParticulasColeta, transform.position, Quaternion.identity);

        objetosColetadosTotais++;
        AtualizarTextoUI();

        Debug.Log(
            $"Cadernos coletados: {objetosColetadosTotais}/{limiteMaximoObjetos}"
        );

        // Abre a parede inicial ao atingir a quantidade configurada.
        if (
            objetosColetadosTotais >= itensParaAbrirParedeInicio &&
            paredeBloqueioInicio != null
        )
        {
            paredeBloqueioInicio.SetActive(false);
            Debug.Log("Parede inicial aberta.");
        }

        PortaAutomatica[] portas =
            FindObjectsByType<PortaAutomatica>(FindObjectsSortMode.None);

        foreach (PortaAutomatica porta in portas)
        {
            if (porta != null)
                porta.SomarObjeto();
        }

        Perseguidor[] perseguidores =
            FindObjectsByType<Perseguidor>(FindObjectsSortMode.None);

        foreach (Perseguidor inimigo in perseguidores)
        {
            if (inimigo != null)
                inimigo.RegistrarColeta();
        }

        if (objetosColetadosTotais >= limiteMaximoObjetos && !allCollected)
            TriggerEndgame();

        StartCoroutine(AnimacaoColeta());
    }

    private void TriggerEndgame()
    {
        allCollected = true;

        Debug.Log("10 cadernos coletados! Ativando modo fuga.");

        // Libera a parede inicial.
        if (paredeBloqueioInicio != null)
            paredeBloqueioInicio.SetActive(false);

        // Efeito visual vermelho.
        Renderer[] renderers =
            FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer rend in renderers)
        {
            if (rend == null || rend.gameObject == gameObject)
                continue;

            Material material = rend.material;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", Color.red);
            else if (material.HasProperty("_Color"))
                material.color = Color.red;
        }

        // Desativa as barreiras antigas.
        if (ControladorBarreiras.Instance != null)
            ControladorBarreiras.Instance.LigarParedes(false);
        else
            Debug.LogWarning("ControladorBarreiras não encontrado.");

        // Localiza o gerenciador novamente, se necessário.
        if (controleSaidas == null)
            controleSaidas = FindFirstObjectByType<ControleSaidas>();

        // Ativa os três bloqueios finais imediatamente.
        if (controleSaidas != null)
        {
            controleSaidas.BloquearSaidas();
        }
        else
        {
            Debug.LogError(
                "Não foi encontrado ControleSaidas ativo na cena!"
            );
        }
    }

    private void AtualizarTextoUI()
    {
        if (textoContador == null)
        {
            GameObject objetoTexto = GameObject.Find("TextoContador");

            if (objetoTexto != null)
                textoContador = objetoTexto.GetComponent<TextMeshProUGUI>();
        }

        if (textoContador != null)
            textoContador.text = $"{objetosColetadosTotais}/{limiteMaximoObjetos}";
    }

    private IEnumerator AnimacaoColeta()
    {
        Vector3 escalaInicial = transform.localScale;

        if (TryGetComponent<Collider>(out Collider colisor))
            colisor.enabled = false;

        if (TryGetComponent<Collider2D>(out Collider2D colisor2D))
            colisor2D.enabled = false;

        if (duracaoAnimacaoSumir <= 0f)
        {
            transform.localScale = Vector3.zero;
            Destroy(gameObject);
            yield break;
        }

        float tempoDecorrido = 0f;

        while (tempoDecorrido < duracaoAnimacaoSumir)
        {
            tempoDecorrido += Time.deltaTime;

            float progresso = Mathf.Clamp01(
                tempoDecorrido / duracaoAnimacaoSumir
            );

            transform.Rotate(
                velocidadeRotacao * 10f * Time.deltaTime
            );

            transform.localScale = Vector3.Lerp(
                escalaInicial,
                Vector3.zero,
                progresso
            );

            yield return null;
        }

        transform.localScale = Vector3.zero;
        Destroy(gameObject);
    }
}
