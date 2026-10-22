
using UnityEngine;

public class BloqueioPorToque : MonoBehaviour
{
    [Header("Gerenciador das saídas")]
    [SerializeField] private ControleSaidas gerenciador;

    private void OnTriggerEnter(Collider outro)
    {
        if (!outro.CompareTag("Player"))
            return;

        // Faz o sorteio somente na primeira tentativa.
        gerenciador?.BloquearSaidas();

        if (gerenciador == null)
        {
            Debug.LogError("Configure o Gerenciador de Saídas!");
            return;
        }

        if (gerenciador.EstaLivre(gameObject))
        {
            Debug.Log("Esta saída está livre!");
        }
        else
        {
            Debug.Log("Esta saída está bloqueada!");
        }
    }
}
