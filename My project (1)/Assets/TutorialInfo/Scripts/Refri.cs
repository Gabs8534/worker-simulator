using UnityEngine;

public class Refri : MonoBehaviour
{
    [Header("Configuração do Spray")]
    public float distancia = 10f;
    public float forca = 20f;
    public float velocidadeJato = 20f;
    public float tamanhoJato = 0.5f;

    [Header("Visual do Spray")]
    public GameObject efeitoJato;
    public float duracaoJato = 1f;

    [Header("Som")]
    public AudioClip som;

    private Camera cameraPrincipal;

    public void Usar()
    {
        cameraPrincipal = Camera.main;

        if (cameraPrincipal == null)
        {
            Debug.LogError("Câmera principal não encontrada!");
            return;
        }

        Debug.Log("REFRI USADO!");

        // =====================================
        // SOM
        // =====================================

        if (som != null)
        {
            AudioSource.PlayClipAtPoint(
                som,
                cameraPrincipal.transform.position
            );
        }

        // =====================================
        // CRIA O JATO
        // =====================================

        if (efeitoJato != null)
        {
            Vector3 posicaoInicial =
                cameraPrincipal.transform.position +
                cameraPrincipal.transform.forward * 0.5f;

            GameObject jato = Instantiate(
                efeitoJato,
                posicaoInicial,
                cameraPrincipal.transform.rotation
            );

            // Coloca o jato para viajar para frente
            Rigidbody rbJato = jato.GetComponent<Rigidbody>();

            if (rbJato == null)
            {
                rbJato = jato.AddComponent<Rigidbody>();
            }

            rbJato.useGravity = false;
            rbJato.isKinematic = false;

            rbJato.linearVelocity =
                cameraPrincipal.transform.forward *
                velocidadeJato;

            Destroy(jato, duracaoJato);
        }

        // =====================================
        // EMPURRA OS OBJETOS
        // =====================================

        Collider[] objetos = Physics.OverlapSphere(
            cameraPrincipal.transform.position,
            distancia
        );

        foreach (Collider objeto in objetos)
        {
            // Não empurra o próprio Player
            if (objeto.transform.root ==
                cameraPrincipal.transform.root)
            {
                continue;
            }

            Rigidbody rb = objeto.attachedRigidbody;

            if (rb == null)
                continue;

            Vector3 direcao =
                objeto.transform.position -
                cameraPrincipal.transform.position;

            float distanciaObjeto = direcao.magnitude;

            if (distanciaObjeto <= 0.01f)
                continue;

            // Verifica se está na frente
            float angulo = Vector3.Angle(
                cameraPrincipal.transform.forward,
                direcao.normalized
            );

            if (angulo > 20f)
                continue;

            // Quanto mais perto, mais forte
            float multiplicador =
                1f -
                Mathf.Clamp01(
                    distanciaObjeto / distancia
                );

            float forcaFinal =
                forca *
                Mathf.Max(multiplicador, 0.2f);

            rb.AddForce(
                cameraPrincipal.transform.forward *
                forcaFinal,
                ForceMode.Impulse
            );

            Debug.Log(
                "SPRAY EMPURROU: " +
                objeto.gameObject.name
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Camera.main == null)
            return;

        Gizmos.color = Color.cyan;

        Gizmos.DrawRay(
            Camera.main.transform.position,
            Camera.main.transform.forward * distancia
        );
    }
}