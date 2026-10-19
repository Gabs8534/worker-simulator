
using UnityEngine;

public class BloqueioPorToque : MonoBehaviour
{
    [Header("Gerenciador das saídas")]
    [SerializeField] private ControleSaidas gerenciador;

    private void OnTriggerEnter(Collider outro)
    {
        if (!outro.CompareTag("Player"))
            return;

        Debug.Log("Jogador tentou sair!");

        if (gerenciador != null)
        {
            gerenciador.BloquearSaidas();
        }
        else
        {
            Debug.LogError("Arraste o GerenciadorSaidas!");
        }
    }
}
