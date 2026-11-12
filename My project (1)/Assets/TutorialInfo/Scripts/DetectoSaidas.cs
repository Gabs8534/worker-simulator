
using UnityEngine;

public class DetectorSaida : MonoBehaviour
{
    [Header("Configuração")]
    public Transform jogador;
    public GameObject parede;

    private Collider colliderParede;
    private bool fugaAtivada = false;
    private bool portaoFechado = false;

    private void Start()
    {
        if (jogador == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                jogador = player.transform;
        }

        if (parede != null)
        {
            parede.SetActive(false);
            colliderParede = parede.GetComponent<Collider>();

            if (colliderParede != null)
                colliderParede.enabled = false;
        }
    }

    public void AtivarDetector()
    {
        fugaAtivada = true;

        if (parede != null)
            parede.SetActive(true);

        Debug.Log("Portão apareceu!");
    }

    private void Update()
    {
        if (!fugaAtivada || portaoFechado || jogador == null)
            return;

        Collider detector = GetComponent<Collider>();

        if (detector != null &&
            detector.bounds.Contains(jogador.position))
        {
            portaoFechado = true;

            if (colliderParede != null)
                colliderParede.enabled = true;

            Debug.Log("PORTÃO FECHADO!");
        }
    }
}
