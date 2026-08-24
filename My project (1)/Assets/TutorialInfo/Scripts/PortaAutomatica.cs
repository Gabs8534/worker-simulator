using UnityEngine;

public class PortaAutomatica : MonoBehaviour
{
    public int objetosNecessarios = 2; // Quantos precisa coletar
    private int objetosColetados = 0;

    public void SomarObjeto()
    {
        objetosColetados++;

        // Quando chega em 2 (ou o valor que você colocar), a porta some/abre
        if (objetosColetados >= objetosNecessarios)
        {
            gameObject.SetActive(false); // Faz a porta sumir do mapa
        }
    }
}