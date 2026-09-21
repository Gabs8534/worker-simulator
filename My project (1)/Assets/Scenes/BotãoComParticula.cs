using UnityEngine;
using UnityEngine.EventSystems; // Necessário para detectar quando o mouse passa por cima

public class BotãoComParticulas : MonoBehaviour, IPointerEnterHandler
{
    [Header("Referências")]
    [Tooltip("Arraste o seu Particle System aqui")]
    public ParticleSystem particulas;

    [Header("Configuração")]
    [Tooltip("Se marcado, a explosão ocorre onde o mouse entrou. Se desmarcado, sai do centro do botão.")]
    public bool seguirMouse = true;

    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    // Esta função roda AUTOMATICAMENTE da Unity quando o mouse passa por cima
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (particulas == null)
        {
            Debug.LogWarning("Arraste o Particle System para o slot no Inspector!");
            return;
        }

        // Posiciona as partículas onde o mouse está passando
        if (seguirMouse && cam != null)
        {
            Vector3 posMouse = Input.mousePosition;
            posMouse.z = Mathf.Abs(cam.transform.position.z - transform.position.z);
            particulas.transform.position = cam.ScreenToWorldPoint(posMouse);
        }
        else
        {
            // Sai do centro do botão
            particulas.transform.position = transform.position;
        }

        // Reseta e dispara o Burst de partículas
        particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particulas.Play();
    }
}