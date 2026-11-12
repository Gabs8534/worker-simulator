
using UnityEngine;

public class ControleSaidas : MonoBehaviour
{
    public GameObject[] paredes = new GameObject[4];
    public DetectorSaida[] detectores = new DetectorSaida[4];

    private bool fugaAtivada = false;

    private void Start()
    {
        foreach (GameObject parede in paredes)
        {
            if (parede != null)
                parede.SetActive(false);
        }
    }

    public void BloquearSaidas()
    {
        if (fugaAtivada)
            return;

        fugaAtivada = true;

        foreach (DetectorSaida detector in detectores)
        {
            if (detector != null)
                detector.AtivarDetector();
        }

        Debug.Log("MODO FUGA ATIVADO!");
    }
}
