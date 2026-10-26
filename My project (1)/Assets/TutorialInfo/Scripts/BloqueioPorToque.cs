
using UnityEngine;

public class BloqueioPorToque : MonoBehaviour
{
    [Header("Gerenciador das quatro saídas")]
    [SerializeField] private ControleSaidas gerenciador;

    [Header("Configuração do detector")]
    [SerializeField] private bool desativarNoInicio = false;

    private Collider detector;

    private void Awake()
    {
        detector = GetComponent<Collider>();

        if (detector == null)
        {
            Debug.LogError(
                "O detector precisa ter um Collider!",
                this
            );
            return;
        }

        detector.isTrigger = true;

        if (desativarNoInicio)
        {
            detector.enabled = false;
        }
    }

    public void AtivarDetector()
    {
        if (detector != null)
        {
            detector.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider outro)
    {
        if (!outro.CompareTag("Player"))
            return;

        if (!Coletavel.allCollected)
        {
            Debug.Log("Colete os 10 cadernos primeiro!");
            return;
        }

        if (gerenciador == null)
        {
            Debug.LogError(
                "Arraste o SistemaSaidas para o campo Gerenciador!",
                this
            );
            return;
        }

        gerenciador.BloquearSaidas();
    }
}
