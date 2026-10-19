
using UnityEngine;

public class ControleSaidas : MonoBehaviour
{
    [Header("Cubos que bloqueiam as 4 saídas")]
    [SerializeField] private GameObject[] bloqueios = new GameObject[4];

    private bool ativado = false;

    public void BloquearSaidas()
    {
        if (ativado)
            return;

        if (bloqueios.Length != 4)
        {
            Debug.LogError("Configure os 4 bloqueios!");
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            if (bloqueios[i] == null)
            {
                Debug.LogError("Falta configurar o bloqueio " + (i + 1));
                return;
            }
        }

        // Escolhe qual saída ficará livre.
        int saidaLivre = Random.Range(0, 4);

        for (int i = 0; i < 4; i++)
        {
            // Três bloqueios ativos e um desativado.
            bloqueios[i].SetActive(i != saidaLivre);
        }

        ativado = true;

        Debug.Log("Saída livre: " + (saidaLivre + 1));
    }
}
