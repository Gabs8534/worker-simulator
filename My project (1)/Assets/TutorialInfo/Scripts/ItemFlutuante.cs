using UnityEngine;

public class ItemFlutuante : MonoBehaviour
{
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
        // Salva a posição original do item
        posicaoInicial = transform.position;

        // Encontra a câmera principal do jogo automaticamente
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

    // 3. MODIFICADO: Agora detecta o clique do mouse diretamente no item
    private void OnMouseDown()
    {
        // Procura o nosso script de inventário (o InventarioUmSlot)
        InventarioUmSlot inventario = FindFirstObjectByType<InventarioUmSlot>();

        if (inventario != null)
        {
            // Envia o nome e a foto deste item para o inventário
            inventario.PegarItem(nomeDoItem, fotoDoItem);

            // Some com o item do chão, pois ele foi coletado
            Destroy(gameObject);
        }
    }
}