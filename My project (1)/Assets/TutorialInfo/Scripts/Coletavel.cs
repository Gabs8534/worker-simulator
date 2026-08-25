using System.Collections;
using UnityEngine;
using TMPro; // <-- 1. Adicione esta linha no topo do script

public class Coletavel : MonoBehaviour
{
    [Header("Configurações de Rotação e Flutuação")]
    public Vector3 velocidadeRotacao = new Vector3(0f, 50f, 0f);
    public float amplitude = 0.5f;
    public float velocidadeFlutuacao = 1f;

    [Header("Configurações de Coleta")]
    public float duracaoAnimacaoSumir = 0.5f;

    private Vector3 posicaoInicial;
    private bool coletado = false;

    // Contador estático global (máximo 10)
    public static int objetosColetadosTotais = 0;
    public const int limiteMaximoObjetos = 10;

    // <-- 2. Referência para o texto na tela
    private static TextMeshProUGUI textoContador;

    void Start()
    {
        posicaoInicial = transform.position;

        // Procura o texto na cena automaticamente pelo nome do GameObject (substitua "TextoContador" pelo nome exato do seu objeto de texto na Hierarchy)
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

            // Incrementa a contagem
            objetosColetadosTotais++;
            AtualizarTextoUI(); // Atualiza na tela na mesma hora

            // --- CÓDIGO DAS PORTAS ---
            PortaAutomatica[] todasAsPortas = FindObjectsByType<PortaAutomatica>(FindObjectsSortMode.None);
            foreach (PortaAutomatica porta in todasAsPortas)
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

    // Função para atualizar o texto visualmente
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