using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    private bool ehPortaVencedora = false;
    private bool jogoLiberado = false;

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player"))
        {
            // --- 1. SE AINDA NÃO COLETOU OS 10 ITENS ---
            if (Coletavel.objetosColetadosTotais < Coletavel.limiteMaximoObjetos)
            {
                Debug.Log("A saída está trancada! Você precisa coletar todos os 10 itens primeiro.");
                return;
            }

            // --- 2. SE JÁ ESTÁ NO MODO FUGA (10 ITENS COLETADOS) ---
            if (!jogoLiberado)
            {
                Debug.Log("As portas ainda estão trancadas pelo sistema!");
                return;
            }

            // --- 3. VERIFICA SE É A PORTA CERTA OU FALSA ---
            if (ehPortaVencedora)
            {
                Debug.Log("Parabéns! Você encontrou a saída certa e escapou!");
                SceneManager.LoadScene("TelaDeVitoria");
            }
            else
            {
                Debug.Log("Esta porta é falsa! Procure outra saída!");
                // Opcional: Aqui você pode tocar um som de erro ou fechar a porta
            }
        }
    }

    // Método chamado pelo Coletavel quando atinge os 10 itens (Modo Fuga)
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
            Debug.Log("Esta porta é uma saída FALSA.", gameObject);
        }
    }
}