using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    [Header("Configuração da Porta")]
    [Tooltip("Marque se esta porta é uma das 3 que precisam ser bloqueadas.")]
    public bool precisaSerBloqueada = true;

    [Header("Animação / Visual")]
    public Animator animadorPorta;
    public string nomeBoolAnimacao = "Fechar"; // Parâmetro booleano no Animator

    private bool jaInteragida = false;
    private static int portasBloqueadasPeloJogador = 0;
    private const int totalPortasParaBloquear = 3;

    public void ConfigureDoor(bool isWinner)
    {
        // Esta função é chamada pelo Coletavel quando atinge os 10 itens (se você quiser usar lógica de porta vencedora aleatória).
        // Como o seu foco principal é bloquear as 3 saídas, vamos manter a lógica baseada na interação do jogador abaixo.
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player"))
        {
            // CASO 1: É uma das 3 saídas que precisam ser bloqueadas
            if (precisaSerBloqueada && !jaInteragida)
            {
                jaInteragida = true;
                portasBloqueadasPeloJogador++;

                // Executa a animação de fechar/bloquear
                if (animadorPorta != null)
                {
                    animadorPorta.SetBool(nomeBoolAnimacao, true);
                }

                Debug.Log($"Saída bloqueada! Total de saídas trancadas: {portasBloqueadasPeloJogador} / {totalPortasParaBloquear}");
            }

            // CASO 2: É a 4ª saída (A saída final de fuga)
            else if (!precisaSerBloqueada)
            {
                // Verifica se já pegou os 10 itens E se já bloqueou as 3 saídas anteriores
                if (Coletavel.objetosColetadosTotais >= Coletavel.limiteMaximoObjetos && portasBloqueadasPeloJogador >= totalPortasParaBloquear)
                {
                    Debug.Log("Parabéns! Você trancou todas as saídas e escapou com sucesso!");
                    SceneManagementHelper(); // Carrega a vitória
                }
                else
                {
                    Debug.Log("A saída final está trancada! Você precisa coletar os 10 itens e bloquear as 3 outras saídas primeiro.");
                }
            }
        }
    }

    void SceneManagementHelper()
    {
        // Substitua "TelaDeVitoria" pelo nome exato da sua cena final/menu
        SceneManager.LoadScene("TelaDeVitoria");
    }
}