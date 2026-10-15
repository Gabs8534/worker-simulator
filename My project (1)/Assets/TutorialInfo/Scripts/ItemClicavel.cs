using UnityEngine;

public class ItemClicavel : MonoBehaviour
{
    [Header("Efeitos Visuais no Chão")]
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
    private bool estaSendoSegurado = false;
    private PlayerMovemen player;

    void Start()
    {
        posicaoInicial = transform.position;

        cameraPrincipal = Camera.main;

        player = FindFirstObjectByType<PlayerMovemen>();
    }

    void Update()
    {
        if (estaSendoSegurado)
        {
            // =====================================
            // BOTÃO DIREITO = USAR ITEM
            // =====================================

            if (Input.GetMouseButtonDown(1))
            {
                // Verifica se o item é um Refri
                Refri refri = GetComponent<Refri>();

                if (refri != null)
                {
                    // Usa a habilidade do Refri
                    refri.Usar();

                    // Remove o item do Player
                    if (player != null &&
                        player.itemAtualSegurado == gameObject)
                    {
                        player.itemAtualSegurado = null;
                    }

                    // Destrói o Refri
                    Destroy(gameObject);
                }
                else
                {
                    // Item normal
                    UsarItem();
                }
            }

            // =====================================
            // Q = SOLTAR ITEM
            // =====================================

            else if (Input.GetKeyDown(KeyCode.Q))
            {
                SoltarItem();
            }
        }
        else
        {
            // =====================================
            // FLUTUAR NO CHÃO
            // =====================================

            if (flutuar)
            {
                float novaY =
                    posicaoInicial.y +
                    Mathf.Sin(Time.time * velocidadeFlutuar) * amplitude;

                transform.position = new Vector3(
                    posicaoInicial.x,
                    novaY,
                    posicaoInicial.z
                );
            }

            // =====================================
            // GIRAR NO CHÃO
            // =====================================

            if (girarNoChao)
            {
                transform.Rotate(
                    Vector3.up * velocidadeGiro * Time.deltaTime,
                    Space.World
                );
            }
        }
    }

    // =============================================
    // PEGAR ITEM
    // =============================================

    private void OnMouseDown()
    {
        if (estaSendoSegurado)
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
            ItemClicavel outroItem =
                player.itemAtualSegurado.GetComponent<ItemClicavel>();

            if (outroItem != null)
            {
                outroItem.SoltarItem();
            }
        }

        // =====================================
        // PEGA O ITEM
        // =====================================

        estaSendoSegurado = true;

        player.itemAtualSegurado = gameObject;

        // =====================================
        // DESATIVA COLLIDER
        // =====================================

        Collider colisor = GetComponent<Collider>();

        if (colisor != null)
        {
            colisor.enabled = false;
        }

        // =====================================
        // DESATIVA FÍSICA
        // =====================================

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
    }

    // =============================================
    // USAR ITEM NORMAL
    // =============================================

    public void UsarItem()
    {
        if (!estaSendoSegurado)
            return;

        if (player != null &&
            player.itemAtualSegurado == gameObject)
        {
            player.itemAtualSegurado = null;
        }

        Destroy(gameObject);
    }

    // =============================================
    // SOLTAR ITEM
    // =============================================

    public void SoltarItem()
    {
        if (!estaSendoSegurado)
            return;

        estaSendoSegurado = false;

        // Remove do Player
        if (player != null &&
            player.itemAtualSegurado == gameObject)
        {
            player.itemAtualSegurado = null;
        }

        // Remove da câmera
        transform.SetParent(null);

        // Atualiza posição inicial
        posicaoInicial = transform.position;

        // =====================================
        // ATIVA COLLIDER
        // =====================================

        Collider colisor = GetComponent<Collider>();

        if (colisor != null)
        {
            colisor.enabled = true;
        }

        // =====================================
        // ATIVA FÍSICA
        // =====================================

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;

            // Joga o item para frente
            if (cameraPrincipal != null)
            {
                rb.AddForce(
                    cameraPrincipal.transform.forward * 3f,
                    ForceMode.Impulse
                );
            }
        }
    }
}