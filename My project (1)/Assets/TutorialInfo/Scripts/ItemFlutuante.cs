using UnityEngine;

public class ItemFlutuante : MonoBehaviour
{
    [Header("Configuração de Distância")]
    public float distanciaMaxima = 3f; // Distância máxima para coletar o item

    [Header("Configurações do Inventário")]
    public string nomeDoItem;      // Digite o nome do item aqui (ex: Chocolate, Chave)
    public Sprite fotoDoItem;      // Arraste a fotinho dele para aparecer no slot

    [Header("Configurações de Flutuação")]
    public float amplitude = 0.2f;
    public float velocidadeFlutuar = 2f;

    private Vector3 posicaoInicial;
    private Camera cameraPrincipal;

    void Start()
    {
        posicaoInicial = transform.position;
        cameraPrincipal = Camera.main;
    }

    void Update()
    {
        // 1. Efeito de Flutuar (Sobe e desce)
        float novaY = posicaoInicial.y + Mathf.Sin(Time.time * velocidadeFlutuar) * amplitude;
        transform.position = new Vector3(posicaoInicial.x, novaY, posicaoInicial.z);
    }

    void LateUpdate()
    {
        if (cameraPrincipal != null)
        {
            // 2. Efeito Billboard (Sempre encarar a câmera)
            transform.LookAt(transform.position + cameraPrincipal.transform.rotation * Vector3.forward,
                             cameraPrincipal.transform.rotation * Vector3.up);
        }
    }

    private void OnMouseDown()
    {
        // Procura o player na cena para medir a distância
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            float distancia = Vector3.Distance(player.transform.position, transform.position);
            if (distancia > distanciaMaxima)
            {
                return; // Se estiver longe, não faz nada
            }
        }

        // Procura o nosso script de inventário
        InventarioUmSlot inventario = FindFirstObjectByType<InventarioUmSlot>();

        if (inventario != null)
        {
            inventario.PegarItem(nomeDoItem, fotoDoItem);
            Destroy(gameObject);
        }
    }
}