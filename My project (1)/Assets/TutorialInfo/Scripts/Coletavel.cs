using System.Collections;
using UnityEngine;

public class Coletavel : MonoBehaviour
{
    [Header("Configurações de Rotação e Flutuação")]
    public Vector3 velocidadeRotacao = new Vector3(0f, 50f, 0f);
    public float amplitude = 0.5f;
    public float velocidadeFlutuacao = 1f;

    [Header("Configurações de Coleta")]
    public float duracaoAnimacaoSumir = 0.5f; // Tempo em segundos para encolher

    private Vector3 posicaoInicial;
    private bool coletado = false;

    void Start()
    {
        posicaoInicial = transform.position;
    }

    void Update()
    {
        // 1. ESTADO NORMAL: Flutua e gira enquanto não for coletado
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
            // --- O SEU CÓDIGO DAS PORTAS (MANTIDO) ---
            PortaAutomatica[] todasAsPortas = FindObjectsByType<PortaAutomatica>(FindObjectsSortMode.None);
            foreach (PortaAutomatica porta in todasAsPortas)
            {
                porta.SomarObjeto();
            }

            // --- AVISA TODOS OS PERSEGUIDORES DA CENA ---
            Perseguidor[] todosOsPerseguidores = FindObjectsByType<Perseguidor>(FindObjectsSortMode.None);
            foreach (Perseguidor inimigo in todosOsPerseguidores)
            {
                inimigo.RegistrarColeta();
            }

            StartCoroutine(AnimacaoColeta());
        }
    }

    // ANIMAÇÃO DE SUMIR: Executada ao longo do tempo (Corrotina)
    private IEnumerator AnimacaoColeta()
    {
        coletado = true; // Para o movimento do Update imediatamente

        Vector3 escalaInicial = transform.localScale;
        float tempoDecorrido = 0f;

        // Desliga o colisor para evitar múltiplos cliques no mesmo item
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