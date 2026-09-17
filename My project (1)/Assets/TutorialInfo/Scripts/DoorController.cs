using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    private bool ehPortaVencedora = false;
    private bool jogoLiberado = false;

    // Método chamado pelo Coletavel quando os 10 itens são atingidos
    public void ConfigureDoor(bool isWinner)
    {
        jogoLiberado = true;
        ehPortaVencedora = isWinner;

        if (ehPortaVencedora)
        {
            Debug.Log("Esta porta foi sorteada como a SAÍDA REAL!", gameObject);
        }
        else
        {
            Debug.Log("Esta porta é uma saída FALSAS/TRANCADA.", gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player"))
        {
            // Se ainda não pegou os 10 itens
            if (!jogoLiberado || Coletavel.objetosColetadosTotais < Coletavel.limiteMaximoObjetos)
            {
                Debug.Log("A saída está trancada. Colete todos os 10 itens primeiro!");
                return;
            }

            // Se já pegou os 10 itens, verifica se esta é a porta correta sorteadas
            if (ehPortaVencedora)
            {
                Debug.Log("Parabéns! Você encontrou a saída certa e escapou!");
                SceneManager.LoadScene("TelaDeVitoria");
            }
            else
            {
                Debug.Log("Esta porta é falsa! Procure outra das 4 saídas!");
                // Aqui você pode adicionar algum efeito, som de erro ou susto se quiser
            }
        }
    }
}