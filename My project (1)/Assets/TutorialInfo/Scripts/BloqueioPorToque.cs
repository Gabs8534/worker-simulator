using UnityEngine;

public class BloqueioPorToque : MonoBehaviour
{
    [Header("Cubo que bloqueia a porta")]
    [SerializeField] private GameObject cuboBloqueio;

    private void OnTriggerEnter(Collider outro)
    {
        if (!outro.CompareTag("Player"))
            return;

        Debug.Log("Jogador encostou no detector!");

        // Ativa o cubo que já está posicionado na porta.
        if (cuboBloqueio != null)
        {
            cuboBloqueio.SetActive(true);
            Debug.Log("Cubo de bloqueio ativado!");
        }
        else
        {
            Debug.LogError("Arraste o cubo para o campo Cubo Bloqueio!");
        }
    }
}