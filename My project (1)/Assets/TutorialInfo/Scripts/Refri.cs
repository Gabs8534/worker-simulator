
using UnityEngine;
using UnityEngine.AI;

public class Refri : MonoBehaviour
{
    [Header("Configuração do Spray")]
    public float distancia = 10f;
    public float forca = 20f;
    public float velocidadeJato = 20f;
    public float tamanhoJato = 0.5f;

    [Range(1f, 90f)]
    public float anguloJato = 30f;

    [Header("Visual do Spray")]
    public GameObject efeitoJato;
    public float duracaoJato = 1f;

    [Header("Som")]
    public AudioClip som;

    [Header("Inimigos")]
    public bool empurrarSweep = true;
    public bool empurrarPerseguidor = true;

    [Header("Detecção")]
    public LayerMask camadaDeteccao = ~0;

    private Camera cameraPrincipal;

    public void Usar()
    {
        cameraPrincipal = Camera.main;

        if (cameraPrincipal == null)
        {
            Debug.LogError(
                "Câmera principal não encontrada!"
            );
            return;
        }

        Debug.Log("REFRI USADO!");

        // SOM
        if (som != null)
        {
            AudioSource.PlayClipAtPoint(
                som,
                cameraPrincipal.transform.position
            );
        }

        // EFEITO VISUAL
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

            Rigidbody rbJato =
                jato.GetComponent<Rigidbody>();

            if (rbJato == null)
                rbJato = jato.AddComponent<Rigidbody>();

            rbJato.useGravity = false;
            rbJato.isKinematic = false;

            rbJato.linearVelocity =
                cameraPrincipal.transform.forward *
                velocidadeJato;

            Destroy(jato, duracaoJato);
        }

        // DETECTAR OBJETOS
        Collider[] objetos = Physics.OverlapSphere(
            cameraPrincipal.transform.position,
            distancia,
            camadaDeteccao,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider objeto in objetos)
        {
            if (objeto == null)
                continue;

            // NÃO EMPURRAR O PLAYER
            if (objeto.transform.root ==
                cameraPrincipal.transform.root)
                continue;

            Vector3 direcao =
                objeto.bounds.center -
                cameraPrincipal.transform.position;

            float distanciaObjeto = direcao.magnitude;

            if (distanciaObjeto <= 0.01f)
                continue;

            // VERIFICAR SE ESTÁ NA FRENTE
            float angulo = Vector3.Angle(
                cameraPrincipal.transform.forward,
                direcao.normalized
            );

            if (angulo > anguloJato)
                continue;

            // VERIFICAR OBSTÁCULOS
            if (Physics.Raycast(
                cameraPrincipal.transform.position,
                direcao.normalized,
                out RaycastHit hit,
                distanciaObjeto,
                camadaDeteccao,
                QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform.root !=
                    objeto.transform.root)
                    continue;
            }

            // FORÇA CONFORME DISTÂNCIA
            float multiplicador =
                1f - Mathf.Clamp01(
                    distanciaObjeto / distancia
                );

            float forcaFinal =
                forca * Mathf.Max(multiplicador, 0.2f);

            // EMPURRAR SWEEP
            Sweep sweep =
                objeto.GetComponentInParent<Sweep>();

            if (sweep != null)
            {
                if (!empurrarSweep)
                    continue;

                EmpurraoRefri empurrao =
                    sweep.GetComponent<EmpurraoRefri>();

                if (empurrao != null)
                {
                    empurrao.Empurrar(
                        direcao,
                        forcaFinal
                    );

                    Debug.Log("REFRI EMPURROU A SWEEP!");
                }

                continue;
            }

            // EMPURRAR PERSEGUIDOR
            Perseguidor perseguidor =
                objeto.GetComponentInParent<Perseguidor>();

            if (perseguidor != null)
            {
                if (!empurrarPerseguidor)
                    continue;

                EmpurraoRefri empurrao =
                    perseguidor.GetComponent<EmpurraoRefri>();

                if (empurrao != null)
                {
                    empurrao.Empurrar(
                        direcao,
                        forcaFinal
                    );

                    Debug.Log(
                        "REFRI EMPURROU O PERSEGUIDOR!"
                    );
                }

                continue;
            }

            // EMPURRAR OBJETOS COM RIGIDBODY
            Rigidbody rb = objeto.attachedRigidbody;

            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(
                    direcao.normalized * forcaFinal,
                    ForceMode.Impulse
                );

                Debug.Log(
                    "REFRI EMPURROU: " + objeto.name
                );
            }
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
