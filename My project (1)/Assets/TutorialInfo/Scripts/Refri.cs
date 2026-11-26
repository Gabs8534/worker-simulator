using UnityEngine;
using System.Collections.Generic;

public class Refri : MonoBehaviour
{
    [Header("Configuração do Spray")]
    [Tooltip("Alcance máximo do jato, em metros")]
    public float distancia = 25f;
    public float forca = 20f;
    public float velocidadeJato = 20f;

    [Tooltip("Folga ao redor do jato e do inimigo para contar como acerto")]
    public float raioAcerto = 1f;

    [Header("Visual do Spray")]
    public GameObject efeitoJato;
    public float distanciaInicialJato = 0.5f;

    [Header("Som")]
    public AudioClip som;

    [Header("Inimigos")]
    public bool empurrarSweep = true;
    public bool empurrarPerseguidor = true;

    [Header("Detecção")]
    public LayerMask camadaDeteccao = ~0;

    private Camera cameraPrincipal;
    private Sweep[] sweeps = new Sweep[0];
    private Perseguidor[] perseguidores = new Perseguidor[0];

    public void Usar()
    {
        cameraPrincipal = Camera.main;

        if (cameraPrincipal == null)
        {
            Debug.LogError("Câmera principal não encontrada!");
            return;
        }

        // Lista de inimigos atualizada a cada tiro
        sweeps = FindObjectsByType<Sweep>(FindObjectsSortMode.None);
        perseguidores = FindObjectsByType<Perseguidor>(FindObjectsSortMode.None);

        Vector3 origem = cameraPrincipal.transform.position;
        Vector3 direcao = cameraPrincipal.transform.forward.normalized;

        Debug.Log("REFRI USADO! Inimigos na cena: " +
                  (sweeps.Length + perseguidores.Length));

        if (som != null)
        {
            AudioSource.PlayClipAtPoint(som, origem);
        }

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

        SprayMovimento movimento = jato.AddComponent<SprayMovimento>();

        movimento.Configurar(
            this,
            origem,
            direcao,
            velocidadeJato,
            distancia / Mathf.Max(velocidadeJato, 0.01f),
            cameraPrincipal.transform.root
        );
    }

    // =====================================================
    // OBSTÁCULOS (o jato para ao bater neles)
    // =====================================================

    public bool EhObstaculo(Collider col, Transform jato, Transform raizJogador)
    {
        if (col == null || col.isTrigger)
            return false;

        if (col.transform.IsChildOf(jato))
            return false;

        if (col.transform.IsChildOf(raizJogador))
            return false;

        if (EhAlvo(col))
            return false;

        return true;
    }

    private bool EhAlvo(Collider col)
    {
        if (col.GetComponentInParent<Sweep>() != null)
            return true;

        if (col.GetComponentInParent<Perseguidor>() != null)
            return true;

        Rigidbody rb = col.attachedRigidbody;

        if (rb != null && !rb.isKinematic)
            return true;

        // Collider que está dentro da área de algum inimigo
        foreach (Sweep s in sweeps)
        {
            if (s != null && ObterBounds(s).Intersects(col.bounds))
                return true;
        }

        foreach (Perseguidor p in perseguidores)
        {
            if (p != null && ObterBounds(p).Intersects(col.bounds))
                return true;
        }

        return false;
    }

    // =====================================================
    // ACERTOS
    // =====================================================

    // Chamado pelo jato a cada frame, com o trecho que ele acabou de percorrer.
    public void ProcessarAcerto(
        Vector3 de,
        Vector3 ate,
        Vector3 origem,
        Vector3 direcao,
        Transform jato,
        Transform raizJogador,
        HashSet<int> detectados)
    {
        Vector3 direcaoPlana = direcao;
        direcaoPlana.y = 0f;

        // INIMIGOS (detecção pela caixa do inimigo, sem depender de collider)
        if (empurrarSweep)
        {
            foreach (Sweep s in sweeps)
            {
                if (s != null)
                    TestarInimigo(s, de, ate, origem, direcaoPlana, detectados);
            }
        }

        if (empurrarPerseguidor)
        {
            foreach (Perseguidor p in perseguidores)
            {
                if (p != null)
                    TestarInimigo(p, de, ate, origem, direcaoPlana, detectados);
            }
        }

        // OBJETOS COM RIGIDBODY
        Collider[] objetos = Physics.OverlapCapsule(
            de,
            ate,
            raioAcerto,
            camadaDeteccao,
            QueryTriggerInteraction.Collide
        );

        foreach (Collider objeto in objetos)
        {
            if (objeto == null)
                continue;

            if (objeto.transform.IsChildOf(jato))
                continue;

            if (objeto.transform.IsChildOf(raizJogador))
                continue;

            if (objeto.GetComponentInParent<Sweep>() != null ||
                objeto.GetComponentInParent<Perseguidor>() != null)
                continue;

            Rigidbody corpo = objeto.attachedRigidbody;

            if (corpo != null && !corpo.isKinematic)
            {
                if (!detectados.Add(corpo.GetInstanceID()))
                    continue;

                float forcaFinal = CalcularForca(origem, objeto.bounds.center);
                corpo.AddForce(direcao * forcaFinal, ForceMode.Impulse);
            }
        }
    }

    private void TestarInimigo(
        Component inimigo,
        Vector3 de,
        Vector3 ate,
        Vector3 origem,
        Vector3 direcaoPlana,
        HashSet<int> detectados)
    {
        int id = inimigo.GetInstanceID();

        if (detectados.Contains(id))
            return;

        Bounds caixa = ObterBounds(inimigo);

        Vector3 pontoMaisPerto = PontoMaisPertoNoSegmento(de, ate, caixa.center);

        if (caixa.SqrDistance(pontoMaisPerto) > raioAcerto * raioAcerto)
            return;

        detectados.Add(id);

        float forcaFinal = CalcularForca(origem, caixa.center);

        EmpurrarInimigo(inimigo, direcaoPlana, forcaFinal);
    }

    private float CalcularForca(Vector3 origem, Vector3 alvo)
    {
        float distanciaAlvo = Vector3.Distance(origem, alvo);
        float multiplicador = 1f - Mathf.Clamp01(distanciaAlvo / distancia);

        return forca * Mathf.Max(multiplicador, 0.3f);
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

    private Vector3 PontoMaisPertoNoSegmento(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float tamanho = ab.sqrMagnitude;

        if (tamanho < 0.0001f)
            return a;

        float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / tamanho);

        return a + ab * t;
    }

    private void EmpurrarInimigo(Component inimigo, Vector3 direcao, float forcaFinal)
    {
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

// MOVIMENTO DO JATO + DETECÇÃO DE ACERTO
public class SprayMovimento : MonoBehaviour
{
    private Refri refri;
    private Vector3 origem;
    private Vector3 direcao;
    private Transform raizJogador;
    private float velocidade;
    private float duracao;
    private float tempo;

    private HashSet<int> detectados = new HashSet<int>();

    public void Configurar(
        Refri novoRefri,
        Vector3 novaOrigem,
        Vector3 novaDirecao,
        float novaVelocidade,
        float novaDuracao,
        Transform novaRaizJogador)
    {
        refri = novoRefri;
        origem = novaOrigem;
        direcao = novaDirecao.normalized;
        velocidade = novaVelocidade;
        duracao = novaDuracao;
        raizJogador = novaRaizJogador;
    }

    private void Update()
    {
        Vector3 anterior = transform.position;
        Vector3 destino = anterior + direcao * velocidade * Time.deltaTime;
        bool bateuParede = false;

        if (refri != null)
        {
            float tamanhoTrecho = Vector3.Distance(anterior, destino);

            RaycastHit[] batidas = Physics.RaycastAll(
                anterior,
                direcao,
                tamanhoTrecho,
                refri.camadaDeteccao,
                QueryTriggerInteraction.Ignore
            );

            float menorDistancia = float.MaxValue;

            foreach (RaycastHit batida in batidas)
            {
                if (!refri.EhObstaculo(batida.collider, transform, raizJogador))
                    continue;

                if (batida.distance < menorDistancia)
                {
                    menorDistancia = batida.distance;
                    destino = batida.point;
                    bateuParede = true;
                }
            }

            refri.ProcessarAcerto(
                anterior,
                destino,
                origem,
                direcao,
                transform,
                raizJogador,
                detectados
            );
        }

        transform.position = destino;

        tempo += Time.deltaTime;

        if (bateuParede || tempo >= duracao)
        {
            Destroy(gameObject);
        }
    }
}