using UnityEngine;

public class ControladorPortao : MonoBehaviour
{
    [Header("Configurações dos Portões")]
    [SerializeField] private Transform portaoEsquerdo;
    [SerializeField] private Transform portaoDireito;

    [Header("Configurações de Rotação")]
    [SerializeField] private float anguloAbertura = 90f; // Quanto ele vai girar
    [SerializeField] private float velocidade = 2f;

    private bool abrir = false;
    private Quaternion rotacaoAlvoEsquerda;
    private Quaternion rotacaoAlvoDireita;

    void Start()
    {
        if (portaoEsquerdo != null)
        {
            // O esquerdo gira para um lado (ex: negativo)
            rotacaoAlvoEsquerda = Quaternion.Euler(portaoEsquerdo.eulerAngles.x, portaoEsquerdo.eulerAngles.y - anguloAbertura, portaoEsquerdo.eulerAngles.z);
        }

        if (portaoDireito != null)
        {
            // O direito gira para o outro (ex: positivo)
            rotacaoAlvoDireita = Quaternion.Euler(portaoDireito.eulerAngles.x, portaoDireito.eulerAngles.y + anguloAbertura, portaoDireito.eulerAngles.z);
        }
    }

    void Update()
    {
        if (abrir)
        {
            // Move os dois portões suavemente até o ângulo final ("colar na parede")
            if (portaoEsquerdo != null)
                portaoEsquerdo.rotation = Quaternion.Slerp(portaoEsquerdo.rotation, r_TargetEsquerda(), Time.deltaTime * velocidade);

            if (portaoDireito != null)
                portaoDireito.rotation = Quaternion.Slerp(portaoDireito.rotation, r_TargetDireita(), Time.deltaTime * velocidade);
        }
    }

    // Métodos auxiliares para retornar a rotação correta
    private Quaternion r_TargetEsquerda() => rotacaoAlvoEsquerda;
    private Quaternion r_TargetDireita() => BlackBoxRotacaoDireita();
    private Quaternion BlackBoxRotacaoDireita() => rotacaoAlvoDireita;

    private void OnTriggerEnter(Collider other)
    {
        // Se o Player pisar no sensor, ativa a abertura
        if (other.CompareTag("Player"))
        {
            abrir = true;
        }
    }
}