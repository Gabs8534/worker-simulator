using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    [Header("Status da Porta")]
    public bool isWinningDoor = false; // Definido automaticamente quando atinge 10 itens
    private bool isLocked = false;     // Controla se a porta já foi trancada por tentativa errada

    // Chamado pelo Coletavel quando os 10 itens são coletados
    public void ConfigureDoor(bool winningStatus)
    {
        isWinningDoor = winningStatus;
        isLocked = false; // Garante que começam destrancadas para a escolha

        if (isWinningDoor)
        {
            Debug.Log(gameObject.name + " é a verdadeira saída (a última que vai sobrar se errar as outras).");
        }
        else
        {
            Debug.Log(gameObject.name + " é uma porta comum (vai trancar se testada).");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Se o jogador ainda não pegou os 10 itens, não faz nada (ou avisa)
            if (!Coletavel.allCollected)
            {
                Debug.Log("Você precisa coletar todos os 10 objetos primeiro!");
                return;
            }

            // Se a porta já foi trancada antes, ignora
            if (isLocked)
            {
                Debug.Log("Esta porta já está trancada!");
                return;
            }

            // Se é a porta vencedora (a última correta)
            if (isWinningDoor)
            {
                WinGame();
            }
            else
            {
                // Se for uma das 3 portas erradas, ela tranca e bloqueia!
                LockThisDoor();
            }
        }
    }

    void LockThisDoor()
    {
        isLocked = true;
        Debug.Log(gameObject.name + " está BLOQUEADA! Tente outra porta.");

        // Aqui você pode adicionar efeitos visuais ou sonoros de porta trancada (ex: som de "Bang!")
        // Exemplo: GetComponent<AudioSource>().Play();
    }

    void WinGame()
    {
        Debug.Log("PARABÉNS! Você achou a saída correta e fugiu!");

        // Se quiser carregar a cena de vitória, descomente a linha abaixo:
        // SceneManager.LoadScene("CenaVitoria");
    }
}