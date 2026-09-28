using UnityEngine;

public class ItemClicavel : MonoBehaviour
{
    [Header("Efeitos Visuais no Chão")]
    public bool flutuar = true;
    public float amplitude = 0.2f;
    public float velocidadeFlutuar = 2f;
    public bool girarNoChao = true;
    public float velocidadeGiro = 50f;

    private Vector3 posicaoInicial;
    private Camera cameraPrincipal;
    private bool estaSendoSegurado = false;
    private CanvasUpdate player;

    void Start()
    {
        posicaoInicial = transform.position;
        cameraPrincipal = Camera.main;
        player = FindFirstObjectByType<CanvasUpdate>();
    }

    void Update()
    {
        if (estaSendoSegurado)
        {
            // Botão Direito do Mouse: USA o item (some da mão)
            if (Input.GetMouseButtonDown(1))
            {
                UsarItem();
            }
            // Tecla Q: SOLTA o item no chão
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                SoltarItem();
            }
        }
        else
        {
            // Animação de flutuar e girar enquanto estiver no chão
            if (flutuar)
            {
                float novaY = posicaoInicial.y + Mathf.Sin(Time.time * velocidadeFlutuar) * amplitude;
                transform.position = new Vector3(posicaoInicial.x, novaY, posicaoInicial.z);
            }

            if (girarNoChao)
            {
                transform.Rotate(Vector3.up * velocidadeGiro * Time.deltaTime, Space.World);
            }
        }
    }

    private void OnMouseDown()
    {
        if (estaSendoSegurado || player == null) return;

        // Se já tiver um item na mão, solta ele antes
        if (player.itemAtualSegurado != null)
        {
            ItemClicavel outroItem = player.itemAtualSegurado.GetComponent<ItemClicavel>();
            if (outroItem != null)
            {
                outroItem.SoltarItem();
            }
        }

        if (player.maoDoPlayer == null)
        {
            Debug.LogError("ERRO: Arraste o objeto 'Mao Do Player' no script do Player!");
            return;
        }

        estaSendoSegurado = true;
        player.itemAtualSegurado = this.gameObject;

        // Desliga física e colisor
        Collider colisor = GetComponent<Collider>();
        if (colisor != null) colisor.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // Gruda na mão do player (na frente da câmera)
        transform.SetParent(player.maoDoPlayer);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void UsarItem()
    {
        if (!estaSendoSegurado) return;

        if (player != null && player.itemAtualSegurado == this.gameObject)
        {
            player.itemAtualSegurado = null;
        }

        Destroy(gameObject);
    }

    public void SoltarItem()
    {
        if (!estaSendoSegurado) return;

        estaSendoSegurado = false;

        if (player != null && player.itemAtualSegurado == this.gameObject)
        {
            player.itemAtualSegurado = null;
        }

        transform.SetParent(null);
        posicaoInicial = transform.position; // Atualiza a posição inicial para o chão

        Collider colisor = GetComponent<Collider>();
        if (colisor != null) colisor.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.AddForce(cameraPrincipal.transform.forward * 3f, ForceMode.Impulse);
        }
    }
}