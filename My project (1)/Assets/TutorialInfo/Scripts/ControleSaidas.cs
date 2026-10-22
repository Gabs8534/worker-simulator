
using UnityEngine;

public class ControleSaidas : MonoBehaviour
{
    [Header("Os 4 bloqueios das saídas")]
    [SerializeField] private GameObject[] bloqueios = new GameObject[4];

    private bool ativado = false;
    private int saidaLivre = -1;

    public void BloquearSaidas()
    {
        if (ativado)
            return;

        if (bloqueios == null || bloqueios.Length != 4)
        {
            Debug.LogError("Configure exatamente 4 bloqueios!");
            return;
        }

        for (int i = 0; i < bloqueios.Length; i++)
        {
            if (bloqueios[i] == null)
            {
                Debug.LogError("Falta configurar o bloqueio " + (i + 1));
                return;
            }
        }

        // Sorteia uma única saída livre.
        saidaLivre = Random.Range(0, 4);

        // Bloqueia 3 saídas e libera 1.
        for (int i = 0; i < bloqueios.Length; i++)
        {
            bloqueios[i].SetActive(i != saidaLivre);
        }

        ativado = true;

        Debug.Log("Saída livre: " + (saidaLivre + 1));
    }

    public bool EstaLivre(GameObject bloqueio)
    {
        if (!ativado || bloqueio == null)
            return false;

        for (int i = 0; i < bloqueios.Length; i++)
        {
            if (bloqueios[i] == bloqueio)
                return i == saidaLivre;
        }

        return false;
    }
}
