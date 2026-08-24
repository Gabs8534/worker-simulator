using UnityEngine;

public class OlharParaPlayer : MonoBehaviour
{
    public Transform alvo; // Aqui vai o seu Player
    public float velocidadeGiro = 5f;

    void Update()
    {
        if (alvo != null)
        {
            // Calcula a direção ignorando o eixo Y (para o NPC não inclinar para cima/baixo)
            Vector3 direcao = alvo.position - transform.position;
            direcao.y = 0;

            // Cria a rotação desejada
            if (direcao != Vector3.zero)
            {
                Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);
                // Gira suavemente
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, velocidadeGiro * Time.deltaTime);
            }
        }
    }
}