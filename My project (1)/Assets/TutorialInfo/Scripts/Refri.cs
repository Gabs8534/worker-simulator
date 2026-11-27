using UnityEngine;
using System.Collections.Generic;

public class Refri : MonoBehaviour
{
    [Header("Configuração do Spray")]
    [Tooltip("Alcance máximo do empurrão, em metros")]
    public float distancia = 25f;
    public float forca = 20f;
    public float velocidadeJato = 20f;

    [Range(1f, 90f)]
    public float anguloJato = 30f;

    [Header("Visual do Spray")]
    public GameObject efeitoJato;
    [Tooltip("Tempo mínimo que o jato fica visível")]
    public float duracaoJato = 1f;
    public float distanciaInicialJato = 0.5f;

    [Header("Som")]
    public AudioClip som;

    [Header("Inimigos")]
    public bool empurrarSweep = true;
    public bool empurrarPerseguidor = true;

    [Header("Paredes")]
    [Tooltip("Camadas que bloqueiam o spray")]
    public LayerMask camadaParedes = ~0;

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
        Vector3 direcao = cameraPrincipal.transform.forward.normalized;
        Transform raizJogador = cameraPrincipal.transform.root;

        Debug.Log("REFRI USADO!");

        if (som != null)
        {
            AudioSource.PlayClipAtPoint(som, origem);
        }

        // ALVOS (cada um com o momento em que o jato chega nele)
        List<SprayMovimento.Alvo> alvos = new List<SprayMovimento.Alvo>();

        if (empurrarSweep)
        {
            foreach (Sweep s in FindObjectsByType<Sweep>(FindObjectsSortMode.None))
            {
                TestarInimigo(s, origem, direcao, raizJogador, alvos);
            }
        }

        if (empurrarPerseguidor)
        {
            foreach (Perseguidor p in FindObjectsByType<Perseguidor>(FindObjectsSortMode.None))
            {
                TestarInimigo(p, origem, direcao, raizJogador, alvos);
            }
        }

        // OBJETOS COM RIGIDBODY
        TestarCorpos(origem, direcao, raizJogador, alvos);

        // CRIAR O JATO
        Vector3 posicaoInicial = origem + direcao * distanciaInicialJato;
        GameObject jato;

        if (efeitoJato != null)
        {
            jato = Instantiate(
                efeitoJato,
                posicaoInicial,
                Quaternion.LookRotation(direcao)
            );

            Rigidbody[] rigidbodies = jato.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in rigidbodies)
            {
                Destroy(rb);
            }
        }
        else
        {
            jato = new GameObject("SprayInvisivel");
            jato.transform.position = posicaoInicial;
            jato.transform.rotation = Quaternion.LookRotation(direcao);
        }

        // Duração: no mínimo duracaoJato, ou o tempo de chegar no alcance
        float duracao = Mathf.Max(
            duracaoJato,
            distancia / Mathf.Max(velocidadeJato, 0.01f)
        );

        // Se tem inimigo, o jato dura pelo menos até o impacto
        foreach (SprayMovimento.Alvo alvo in alvos)
        {
            duracao = Mathf.Max(duracao, alvo.tempoImpacto + 0.1f);
        }

        SprayMovimento movimento = jato.AddComponent<SprayMovimento>();
        movimento.Configurar(direcao, velocidadeJato, duracao, alvos);
    }

    // =====================================================
    // DETECÇÃO DE INIMIGOS
    // =====================================================

    private void TestarInimigo(
        Component inimigo,
        Vector3 origem,
        Vector3 direcao,
        Transform raizJogador,
        List<SprayMovimento.Alvo> alvos)
    {
        if (inimigo == null)
            return;

        Bounds caixa = ObterBounds(inimigo);

        Vector3 paraAlvo = caixa.center - origem;
        float distanciaAlvo = paraAlvo.magnitude;

        if (distanciaAlvo < 0.01f)
            return;

        if (distanciaAlvo > distancia)
        {
            Debug.Log("REFRI: " + inimigo.name + " longe demais (" +
                      distanciaAlvo.ToString("F1") + " m)");
            return;
        }

        // O ângulo aceito cresce com o tamanho do inimigo
        float margem =
            Mathf.Atan2(caixa.extents.magnitude, distanciaAlvo) * Mathf.Rad2Deg;

        float angulo = Vector3.Angle(direcao, paraAlvo.normalized);

        if (angulo > anguloJato + margem)
        {
            Debug.Log("REFRI: " + inimigo.name + " fora do cone (" +
                      angulo.ToString("F0") + "°)");
            return;
        }

        if (ParedeNoCaminho(origem, caixa, inimigo, raizJogador))
        {
            Debug.Log("REFRI: " + inimigo.name + " bloqueado por parede");
            return;
        }

        float multiplicador = 1f - Mathf.Clamp01(distanciaAlvo / distancia);
        float forcaFinal = forca * Mathf.Max(multiplicador, 0.3f);

        Vector3 direcaoPlana = direcao;
        direcaoPlana.y = 0f;

        float tempoImpacto =
            Mathf.Max(0f, distanciaAlvo - distanciaInicialJato) /
            Mathf.Max(velocidadeJato, 0.01f);

        Debug.Log("REFRI: " + inimigo.name + " será atingido em " +
                  tempoImpacto.ToString("F2") + " s");

        alvos.Add(new SprayMovimento.Alvo
        {
            inimigo = inimigo,
            direcao = direcaoPlana,
            forca = forcaFinal,
            tempoImpacto = tempoImpacto
        });
    }

    private void TestarCorpos(
        Vector3 origem,
        Vector3 direcao,
        Transform raizJogador,
        List<SprayMovimento.Alvo> alvos)
    {
        Collider[] objetos = Physics.OverlapSphere(
            origem,
            distancia,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        HashSet<int> vistos = new HashSet<int>();

        foreach (Collider objeto in objetos)
        {
            if (objeto == null)
                continue;

            if (objeto.transform.IsChildOf(raizJogador))
                continue;

            if (objeto.GetComponentInParent<Sweep>() != null ||
                objeto.GetComponentInParent<Perseguidor>() != null)
                continue;

            Rigidbody corpo = objeto.attachedRigidbody;

            if (corpo == null || corpo.isKinematic)
                continue;

            if (!vistos.Add(corpo.GetInstanceID()))
                continue;

            Vector3 paraAlvo = objeto.bounds.center - origem;
            float distanciaAlvo = paraAlvo.magnitude;

            if (distanciaAlvo < 0.01f)
                continue;

            if (Vector3.Angle(direcao, paraAlvo.normalized) > anguloJato)
                continue;

            float multiplicador = 1f - Mathf.Clamp01(distanciaAlvo / distancia);

            alvos.Add(new SprayMovimento.Alvo
            {
                corpo = corpo,
                direcao = direcao,
                forca = forca * Mathf.Max(multiplicador, 0.2f),
                tempoImpacto =
                    Mathf.Max(0f, distanciaAlvo - distanciaInicialJato) /
                    Mathf.Max(velocidadeJato, 0.01f)
            });
        }
    }

    // Parede entre o jogador e o inimigo (ignora o jogador e o próprio inimigo)
    private bool ParedeNoCaminho(
        Vector3 origem,
        Bounds caixa,
        Component inimigo,
        Transform raizJogador)
    {
        Vector3 paraAlvo = caixa.center - origem;
        float distanciaAlvo = paraAlvo.magnitude;

        RaycastHit[] batidas = Physics.RaycastAll(
            origem,
            paraAlvo.normalized,
            distanciaAlvo,
            camadaParedes,
            QueryTriggerInteraction.Ignore
        );

        foreach (RaycastHit batida in batidas)
        {
            Collider col = batida.collider;

            if (col.transform.IsChildOf(raizJogador))
                continue;

            if (col.GetComponentInParent<Sweep>() != null ||
                col.GetComponentInParent<Perseguidor>() != null)
                continue;

            // Collider que faz parte do corpo do inimigo
            if (caixa.Intersects(col.bounds) &&
                col.transform.IsChildOf(inimigo.transform.root))
                continue;

            // Objetos soltos (Rigidbody móvel) não bloqueiam
            if (col.attachedRigidbody != null && !col.attachedRigidbody.isKinematic)
                continue;

            return true;
        }

        return false;
    }

    // Caixa que cobre colliders e renderers do inimigo (mínimo 1 x 2 x 1 m)
    private Bounds ObterBounds(Component inimigo)
    {
        Bounds caixa = new Bounds(
            inimigo.transform.position + Vector3.up,
            new Vector3(1f, 2f, 1f)
        );

        foreach (Collider c in inimigo.GetComponentsInChildren<Collider>())
        {
            caixa.Encapsulate(c.bounds);
        }

        foreach (Renderer r in inimigo.GetComponentsInChildren<Renderer>())
        {
            caixa.Encapsulate(r.bounds);
        }

        return caixa;
    }

    // =====================================================
    // EMPURRÃO
    // =====================================================

    public static void EmpurrarInimigo(Component inimigo, Vector3 direcao, float forcaFinal)
    {
        if (inimigo == null)
            return;

        EmpurraoRefri empurrao = inimigo.GetComponent<EmpurraoRefri>();

        if (empurrao == null)
            empurrao = inimigo.GetComponentInChildren<EmpurraoRefri>();

        if (empurrao == null)
            empurrao = inimigo.GetComponentInParent<EmpurraoRefri>();

        if (empurrao == null)
        {
            Debug.LogWarning("EmpurraoRefri não encontrado em: " + inimigo.name);
            return;
        }

        Debug.Log("JATO ACERTOU: " + inimigo.name);

        empurrao.Empurrar(direcao.normalized, forcaFinal);
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

// MOVIMENTO DO JATO + EMPURRÃO NO MOMENTO DO IMPACTO
public class SprayMovimento : MonoBehaviour
{
    public class Alvo
    {
        public Component inimigo;
        public Rigidbody corpo;
        public Vector3 direcao;
        public float forca;
        public float tempoImpacto;
        public bool aplicado;
    }

    private Vector3 direcao;
    private float velocidade;
    private float duracao;
    private float tempo;
    private List<Alvo> alvos;

    public void Configurar(
        Vector3 novaDirecao,
        float novaVelocidade,
        float novaDuracao,
        List<Alvo> novosAlvos)
    {
        direcao = novaDirecao.normalized;
        velocidade = novaVelocidade;
        duracao = novaDuracao;
        alvos = novosAlvos;
    }

    private void Update()
    {
        transform.position += direcao * velocidade * Time.deltaTime;

        tempo += Time.deltaTime;

        foreach (Alvo alvo in alvos)
        {
            if (alvo.aplicado || tempo < alvo.tempoImpacto)
                continue;

            alvo.aplicado = true;

            if (alvo.inimigo != null)
            {
                Refri.EmpurrarInimigo(alvo.inimigo, alvo.direcao, alvo.forca);
            }
            else if (alvo.corpo != null)
            {
                alvo.corpo.AddForce(alvo.direcao * alvo.forca, ForceMode.Impulse);
            }
        }

        if (tempo >= duracao)
        {
            Destroy(gameObject);
        }
    }
}