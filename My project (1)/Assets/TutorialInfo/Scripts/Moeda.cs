using UnityEngine;

public class Moeda : MonoBehaviour
{
    [Header("Valor da Moeda")]
    public int valor = 1;

    [Header("Efeitos no Chão")]
    public bool flutuar = true;
    public float amplitude = 0.2f;
    public float velocidadeFlutuar = 2f;

    public bool girarNoChao = true;
    public float velocidadeGiro = 50f;

    [Header("Posição na Frente da Câmera")]
    public float distanciaDaCamera = 2f;
    public float alturaNaCamera = -0.3f;
    public float ladoNaCamera = 0f;

    private Vector3 posicaoInicial;
    private Camera cameraPrincipal;
    private PlayerMovemen player;

    private bool estaSendoSegurada = false;

    // Dinheiro do jogador
    public static int dinheiro = 0;

    void Start()
    {
        posicaoInicial = transform.position;

        cameraPrincipal = Camera.main;

        player = FindFirstObjectByType<PlayerMovemen>();
    }

    void Update()
    {
        if (estaSendoSegurada)
        {
            // Q = SOLTAR A MOEDA
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SoltarMoeda();
            }

            // Botão direito NÃO faz nada.
            // A moeda será usada pela máquina.
        }
        else
        {
            // =====================================
            // FLUTUAR
            // =====================================

            if (flutuar)
            {
                float novaY =
                    posicaoInicial.y +
                    Mathf.Sin(Time.time * velocidadeFlutuar) *
                    amplitude;

                transform.position = new Vector3(
                    posicaoInicial.x,
                    novaY,
                    posicaoInicial.z
                );
            }

            // =====================================
            // GIRAR
            // =====================================

            if (girarNoChao)
            {
                transform.Rotate(
                    Vector3.up *
                    velocidadeGiro *
                    Time.deltaTime,
                    Space.World
                );
            }
        }
    }

    // =============================================
    // PEGAR MOEDA
    // =============================================

    private void OnMouseDown()
    {
        if (estaSendoSegurada)
            return;

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerMovemen>();
        }

        if (cameraPrincipal == null)
        {
            cameraPrincipal = Camera.main;
        }

        if (player == null)
        {
            Debug.LogError("PlayerMovemen não encontrado!");
            return;
        }

        if (cameraPrincipal == null)
        {
            Debug.LogError("Câmera principal não encontrada!");
            return;
        }

        // =====================================
        // SE JÁ TEM OUTRO ITEM
        // =====================================

        if (player.itemAtualSegurado != null)
        {
            GameObject itemAtual =
                player.itemAtualSegurado;

            ItemClicavel item =
                itemAtual.GetComponent<ItemClicavel>();

            if (item != null)
            {
                item.SoltarItem();
            }
            else
            {
                player.itemAtualSegurado = null;
            }
        }

        // =====================================
        // PEGA A MOEDA
        // =====================================

        estaSendoSegurada = true;

        player.itemAtualSegurado = gameObject;

        Collider colisor = GetComponent<Collider>();

        if (colisor != null)
        {
            colisor.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // =====================================
        // COLOCA NA FRENTE DA CÂMERA
        // =====================================

        transform.SetParent(cameraPrincipal.transform);

        transform.localPosition = new Vector3(
            ladoNaCamera,
            alturaNaCamera,
            distanciaDaCamera
        );

        transform.localRotation = Quaternion.identity;

        Debug.Log("Moeda pegada!");
    }

    // =============================================
    // VERIFICAR SE ESTÁ SENDO SEGURADA
    // =============================================

    public bool EstaSendoSegurada()
    {
        return estaSendoSegurada;
    }

    // =============================================
    // SOLTAR MOEDA
    // =============================================

    private void SoltarMoeda()
    {
        if (!estaSendoSegurada)
            return;

        estaSendoSegurada = false;

        if (player != null &&
            player.itemAtualSegurado == gameObject)
        {
            player.itemAtualSegurado = null;
        }

        transform.SetParent(null);

        posicaoInicial = transform.position;

        Collider colisor = GetComponent<Collider>();

        if (colisor != null)
        {
            colisor.enabled = true;
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;

            if (cameraPrincipal != null)
            {
                rb.AddForce(
                    cameraPrincipal.transform.forward * 3f,
                    ForceMode.Impulse
                );
            }
        }

        Debug.Log("Moeda solta!");
    }
}