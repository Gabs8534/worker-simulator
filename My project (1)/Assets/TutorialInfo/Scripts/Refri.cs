
using UnityEngine;
using System.Collections.Generic;

public class Refri : MonoBehaviour
{
    [Header("Configuração do Spray")]
    public float distancia = 10f;
    public float forca = 20f;
    public float velocidadeJato = 20f;

    [Range(1f, 90f)]
    public float anguloJato = 30f;

    [Header("Visual do Spray")]
    public GameObject efeitoJato;
    public float duracaoJato = 1f;
    public float distanciaInicialJato = 0.5f;

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
            Debug.LogError("Câmera principal não encontrada!");
            return;
        }

        Vector3 origem = cameraPrincipal.transform.position;

        Vector3 direcao =
            cameraPrincipal.transform.forward.normalized;

        Debug.Log("REFRI USADO!");

        // SOM
        if (som != null)
        {
            AudioSource.PlayClipAtPoint(som, origem);
        }

        // CRIAR ESPUMA
        if (efeitoJato != null)
        {
            Vector3 posicaoInicial =
                origem + direcao * distanciaInicialJato;

            GameObject jato = Instantiate(
                efeitoJato,
                posicaoInicial,
                Quaternion.LookRotation(direcao)
            );

            // Remove Rigidbody, caso exista.
            Rigidbody[] rigidbodies =
                jato.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in rigidbodies)
            {
                Destroy(rb);
            }

            // Movimento reto, sem física.
            SprayMovimento movimento =
                jato.AddComponent<SprayMovimento>();

            movimento.Configurar(
                direcao,
                velocidadeJato,
                duracaoJato
            );
        }

        // DETECTAR INIMIGOS
        Collider[] objetos = Physics.OverlapSphere(
            origem,
            distancia,
            camadaDeteccao,
            QueryTriggerInteraction.Collide
        );

        HashSet<int> detectados = new HashSet<int>();

        foreach (Collider objeto in objetos)
        {
            if (objeto == null)
                continue;

            Transform raiz = objeto.transform.root;

            if (raiz == cameraPrincipal.transform.root)
                continue;

            Vector3 direcaoAlvo =
                objeto.bounds.center - origem;

            float distanciaAlvo = direcaoAlvo.magnitude;

            if (distanciaAlvo < 0.01f)
                continue;

            float angulo = Vector3.Angle(
                direcao,
                direcaoAlvo.normalized
            );

            if (angulo > anguloJato)
                continue;

            if (Physics.Raycast(
                origem,
                direcaoAlvo.normalized,
                out RaycastHit hit,
                distanciaAlvo,
                camadaDeteccao,
                QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform.root != raiz)
                    continue;
            }

            if (!detectados.Add(raiz.GetInstanceID()))
                continue;

            float multiplicador =
                1f - Mathf.Clamp01(distanciaAlvo / distancia);

            float forcaFinal =
                forca * Mathf.Max(multiplicador, 0.2f);

            // SWEEP
            Sweep sweep =
                objeto.GetComponentInParent<Sweep>();

            if (sweep == null)
                sweep = raiz.GetComponentInChildren<Sweep>();

            if (sweep != null)
            {
                if (!empurrarSweep)
                    continue;

                EmpurraoRefri empurrao =
                    sweep.GetComponent<EmpurraoRefri>();

                if (empurrao == null)
                    empurrao = sweep.GetComponentInChildren<EmpurraoRefri>();

                if (empurrao == null)
                    empurrao = sweep.GetComponentInParent<EmpurraoRefri>();

                if (empurrao != null)
                    empurrao.Empurrar(direcaoAlvo.normalized, forcaFinal);

                continue;
            }

            // PERSEGUIDOR
            Perseguidor perseguidor =
                objeto.GetComponentInParent<Perseguidor>();

            if (perseguidor == null)
                perseguidor = raiz.GetComponentInChildren<Perseguidor>();

            if (perseguidor != null)
            {
                if (!empurrarPerseguidor)
                    continue;

                EmpurraoRefri empurrao =
                    perseguidor.GetComponent<EmpurraoRefri>();

                if (empurrao == null)
                    empurrao = perseguidor.GetComponentInChildren<EmpurraoRefri>();

                if (empurrao == null)
                    empurrao = perseguidor.GetComponentInParent<EmpurraoRefri>();

                if (empurrao != null)
                    empurrao.Empurrar(direcaoAlvo.normalized, forcaFinal);

                continue;
            }

            // OBJETOS COM RIGIDBODY
            Rigidbody corpo = objeto.attachedRigidbody;

            if (corpo != null && !corpo.isKinematic)
            {
                corpo.AddForce(
                    direcaoAlvo.normalized * forcaFinal,
                    ForceMode.Impulse
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

// MOVIMENTO VISUAL DA ESPUMA
public class SprayMovimento : MonoBehaviour
{
    private Vector3 direcao;
    private float velocidade;
    private float duracao;
    private float tempo;

    public void Configurar(
        Vector3 novaDirecao,
        float novaVelocidade,
        float novaDuracao)
    {
        direcao = novaDirecao.normalized;
        velocidade = novaVelocidade;
        duracao = novaDuracao;
    }

    private void Update()
    {
        transform.position +=
            direcao * velocidade * Time.deltaTime;

        tempo += Time.deltaTime;

        if (tempo >= duracao)
        {
            Destroy(gameObject);
        }
    }
}
