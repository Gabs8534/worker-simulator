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
    [Tooltip("Arraste aqui um Prefab de Partículas (Particle System) para explodir ao coletar.")]
    public GameObject prefabParticulasColeta;
    [Tooltip("Arraste aqui um arquivo de Áudio (AudioClip) para tocar ao coletar.")]
    public AudioClip somColeta;

    private Vector3 posicaoInicial;
    private bool coletado = false;

    // Contador estático global (máximo 10)
    public static int objetosColetadosTotais = 0;
    public const int limiteMaximoObjetos = 10;

    // Referência para o texto na tela
    private static TextMeshProUGUI textoContador;

    // --- VARIÁVEIS PARA O MODO FUGA (ESTILO BALDI) ---
    [Header("Configurações de Fuga")]
    public static bool allCollected = false;

    // --- CONFIGURAÇÃO DA PAREDE DE INÍCIO ---
    [Header("Configurações da Parede de Saída do Início")]
    [Tooltip("Arraste aqui o GameObject da parede que bloqueia a saída no começo do jogo.")]
    public GameObject paredeBloqueioInicio;
    [Tooltip("Quantos itens coletados são necessários para a parede do início sumir.")]
    public int itensParaAbrirParedeInicio = 2;

    void Start()
    {
        posicaoInicial = transform.position;

        // Garante que o jogo começa com o modo fuga resetado (caso reinicie a cena)
        allCollected = false;
        objetosColetadosTotais = 0;

        if (paredeBloqueioInicio != null)
        {
            paredeBloqueioInicio.SetActive(true);
        }

        if (textoContador == null)
        {
            GameObject objetoTexto = GameObject.Find("TextoContador");
            if (objetoTexto != null)
            {
                textoContador = objetoTexto.GetComponent<TextMeshProUGUI>();
            }
        }

        AtualizarTextoUI();
    }

    void Update()
    {
        if (!coletado)
        {
            transform.Rotate(velocidadeRotacao * Time.deltaTime);
            float novoY = posicaoInicial.y + Mathf.Sin(Time.time * velocidadeFlutuacao) * amplitude;
            transform.position = new Vector3(posicaoInicial.x, novoY, posicaoInicial.z);
        }
    }

    private void OnMouseDown()
    {
        if (!coletado)
        {
            if (objetosColetadosTotais >= limiteMaximoObjetos)
            {
                Debug.Log("Você já coletou o limite máximo de 10 objetos!");
                return;
            }

            // --- DISPARA OS EFEITOS VISUAIS E SONOROS ---
            if (somColeta != null)
            {
                AudioSource.PlayClipAtPoint(somColeta, transform.position);
            }

            if (prefabParticulasColeta != null)
            {
                Instantiate(prefabParticulasColeta, transform.position, Quaternion.identity);
            }
            // ---------------------------------------------

            // Incrementa a contagem
            objetosColetadosTotais++;
            AtualizarTextoUI();

            Debug.Log($"Item coletado! Total atual: {objetosColetadosTotais} / {limiteMaximoObjetos}");

            // --- VERIFICA SE DEVE ABRIR A PAREDE DO INÍCIO ---
            if (objetosColetadosTotais >= itensParaAbrirParedeInicio && paredeBloqueioInicio != null)
            {
                if (paredeBloqueioInicio.activeSelf)
                {
                    paredeBloqueioInicio.SetActive(false);
                    Debug.Log("A parede da saída inicial foi removida!");
                }
            }

            // --- CHECA SE ATINGIU OS 10 ITENS PARA ACIONAR O MODO FUGA ---
            if (objetosColetadosTotais >= limiteMaximoObjetos && !allCollected)
            {
                TriggerEndgame();
            }

            // --- CÓDIGO DAS PORTAS AUTOMÁTICAS ---
            PortaAutomatica[] todasAsPortasAuto = FindObjectsByType<PortaAutomatica>(FindObjectsSortMode.None);
            foreach (PortaAutomatica porta in todasAsPortasAuto)
            {
                porta.SomarObjeto();
            }

            // --- AVISA OS PERSEGUIDORES ---
            Perseguidor[] todosOsPerseguidores = FindObjectsByType<Perseguidor>(FindObjectsSortMode.None);
            foreach (Perseguidor inimigo in todosOsPerseguidores)
            {
                inimigo.RegistrarColeta();
            }

            StartCoroutine(AnimacaoColeta());
        }
    }

    void TriggerEndgame()
    {
        allCollected = true;
        Debug.Log("Modo Fuga Ativado! Pintando texturas, abrindo barreiras e sorteando portas.");

        // --- 1. PINTA AS TEXTURAS/MATERIAIS DA CENA DE VERMELHO ---
        Renderer[] todosOsRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        foreach (Renderer rend in todosOsRenderers)
        {
            if (rend.gameObject == gameObject) continue;

            if (rend.material.HasProperty("_Color"))
            {
                rend.material.color = Color.red;
            }
            else if (rend.material.HasProperty("_BaseColor"))
            {
                rend.material.SetColor("_BaseColor", Color.red);
            }
        }
        // -----------------------------------------------------

        // --- 2. ABRE AS 3 BARREIRAS USANDO O NOVO CONTROLADOR ---
        if (ControladorBarreiras.Instance != null)
        {
            ControladorBarreiras.Instance.LigarParedes(false); // false desliga as paredes (abre o caminho)
        }
        else
        {
            Debug.LogWarning("ControladorBarreiras não foi encontrado na cena!");
        }
        // --------------------------------------------------------

        // --- 3. SORTEIA A PORTA VERDADEIRA ENTRE AS SAÍDAS ---
        DoorController[] allDoors = FindObjectsByType<DoorController>(FindObjectsSortMode.None);
        if (allDoors != null && allDoors.Length > 0)
        {
            int winningIndex = Random.Range(0, allDoors.Length);

            for (int i = 0; i < allDoors.Length; i++)
            {
                bool isWinner = (i == winningIndex);
                allDoors[i].ConfigureDoor(isWinner);
            }
        }
    }

    private void AtualizarTextoUI()
    {
        if (textoContador != null)
        {
            textoContador.text = $"{objetosColetadosTotais} / {limiteMaximoObjetos}";
        }
    }

    private IEnumerator AnimacaoColeta()
    {
        coletado = true;

        Vector3 escalaInicial = transform.localScale;
        float tempoDecorrido = 0f;

        if (TryGetComponent<Collider>(out Collider colisor)) colisor.enabled = false;
        if (TryGetComponent<Collider2D>(out Collider2D colisor2D)) colisor2D.enabled = false;

        while (tempoDecorrido < duracaoAnimacaoSumir)
        {
            tempoDecorrido += Time.deltaTime;
            float progresso = tempoDecorrido / duracaoAnimacaoSumir;

            transform.Rotate(velocidadeRotacao * 10f * Time.deltaTime);
            transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, progresso);

            yield return null;
        }

        transform.localScale = Vector3.zero;
        Destroy(gameObject);
    }
}