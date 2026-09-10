using UnityEngine;

public class GerenciadorRemoverParedes : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Digite exatamente a Tag que você colocou nas paredes que devem sumir.")]
    public string tagDasParedes = "ParedeBloqueio";

    [Tooltip("Quantos itens são necessários para sumir com as paredes (ex: 10).")]
    public int itensNecessarios = 10;

    private bool paredesRemovidas = false;

    void Update()
    {
        // Se já removeu, não precisa continuar checando todo frame
        if (paredesRemovidas) return;

        // Pega a contagem atual do seu script Coletavel
        if (Coletavel.objetosColetadosTotais >= itensNecessarios)
        {
            RemoverTodasAsParedes();
        }
    }

    void RemoverTodasAsParedes()
    {
        paredesRemovidas = true;

        // Encontra todos os objetos na cena que possuem a Tag configurada
        GameObject[] paredes = GameObject.FindGameObjectsWithTag(tagDasParedes);

        foreach (GameObject parede in paredes)
        {
            // Você pode escolher entre desativarlas (SetActive(false)) ou destruí-las (Destroy(parede))
            Destroy(parede);
        }

        Debug.Log($"Pronto! {paredes.Length} paredes foram removidas após coletar {itensNecessarios} itens.");
    }
}