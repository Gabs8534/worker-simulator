using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    [Header("Bloqueio da porta")]
    [SerializeField] private GameObject bloqueioDaPorta;

    [Header("Cena de vitória")]
    [SerializeField] private string cenaVitoria = "TelaDeVitoria";

    private bool ehPortaVencedora;
    private bool jogoLiberado;

    private void Start()
    {
        AtualizarBloqueio();
    }

    private void Update()
    {
        if (Coletavel.allCollected && !jogoLiberado)
        {
            jogoLiberado = true;
            AtualizarBloqueio();

            Debug.Log("A passagem foi liberada!");
        }
    }

    private void AtualizarBloqueio()
    {
        if (bloqueioDaPorta != null)
        {
            bloqueioDaPorta.SetActive(!jogoLiberado);
        }
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (!outro.CompareTag("Player"))
            return;

        if (!Coletavel.allCollected)
        {
            Debug.Log("Você precisa coletar os 10 objetos!");
            return;
        }

        if (!ehPortaVencedora)
        {
            Debug.Log("Esta porta está errada! Procure outra.");
            return;
        }

        SceneManager.LoadScene(cenaVitoria);
    }

    public void ConfigureDoor(bool isWinner)
    {
        ehPortaVencedora = isWinner;
        jogoLiberado = Coletavel.allCollected;

        AtualizarBloqueio();
    }
}