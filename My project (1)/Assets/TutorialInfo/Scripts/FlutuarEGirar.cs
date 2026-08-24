using UnityEngine;

public class FlutuarEGirar : MonoBehaviour
{
    [Header("Configurações de Rotação")]
    public Vector3 velocidadeRotacao = new Vector3(0f, 50f, 0f); // Gira no eixo Y

    [Header("Configurações de Flutuação")]
    public float amplitude = 0.5f; // Altura máxima do movimento (para cima e para baixo)
    public float velocidadeFlutuacao = 1f; // Quão rápido ele sobe e desce

    private Vector3 posicaoInicial;

    void Start()
    {
        // Salva a posição inicial do objeto
        posicaoInicial = transform.position;
    }

    void Update()
    {
        // 1. Faz o objeto girar
        transform.Rotate(velocidadeRotacao * Time.deltaTime);

        // 2. Faz o objeto subir e descer lentamente usando Seno
        float novoY = posicaoInicial.y + Mathf.Sin(Time.time * velocidadeFlutuacao) * amplitude;

        // Aplica a nova posição mantendo o X e o Z originais
        transform.position = new Vector3(posicaoInicial.x, novoY, posicaoInicial.z);
    }
}