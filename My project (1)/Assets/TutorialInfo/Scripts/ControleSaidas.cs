
using UnityEngine;

public class ControleSaidas : MonoBehaviour
{
    [Header("Arraste somente as 4 paredes NOVAS")]
    [SerializeField] private GameObject[] bloqueios = new GameObject[4];

    private bool sorteado;

    private void Awake()
    {
        sorteado = false;

        if (bloqueios == null || bloqueios.Length != 4)
        {
            Debug.LogError("Configure exatamente 4 paredes novas!", this);
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            if (bloqueios[i] == null)
            {
                Debug.LogError("A parede " + i + " não foi atribuída!", this);
                continue;
            }

            bloqueios[i].SetActive(false);
        }

        Debug.Log("As 4 paredes novas começaram desativadas.");
    }

    public void BloquearSaidas()
    {
        if (!Coletavel.allCollected)
        {
            Debug.LogWarning("Ainda não foram coletados os 10 cadernos.");
            return;
        }

        if (sorteado)
        {
            Debug.Log("As saídas já foram sorteadas.");
            return;
        }

        if (bloqueios == null || bloqueios.Length != 4)
        {
            Debug.LogError("O sistema precisa de 4 paredes novas.");
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            if (bloqueios[i] == null)
            {
                Debug.LogError("Configure a parede " + i + " no Inspector.");
                return;
            }
        }

        // Escolhe qual das quatro saídas ficará livre.
        int saidaLivre = Random.Range(0, 4);

        for (int i = 0; i < 4; i++)
        {
            bool bloquear = (i != saidaLivre);

            // Ativa três paredes e desativa uma.
            bloqueios[i].SetActive(bloquear);

            // Garante o estado dos colisores da parede.
            Collider[] colisores =
                bloqueios[i].GetComponentsInChildren<Collider>(true);

            foreach (Collider colisor in colisores)
            {
                colisor.enabled = bloquear;
            }

            Debug.Log(
                bloqueios[i].name + ": " +
                (bloquear ? "BLOQUEADA" : "LIVRE")
            );
        }

        sorteado = true;

        Debug.Log("Sorteio concluído. Saída livre número " +
                  (saidaLivre + 1));
    }
}
