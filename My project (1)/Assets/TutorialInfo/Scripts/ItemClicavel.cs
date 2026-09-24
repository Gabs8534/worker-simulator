using UnityEngine;

public class ItemClicavel : MonoBehaviour
{
    [Header("Configuração de Distância")]
    public float distanciaMaxima = 3f; // Distância máxima para conseguir interagir/coletar

    [Header("Configurações do Inventário")]
    public string nomeDoItem;      // Nome do item (ex: Chave, Poção)
    public Sprite fotoDoItem;      // Ícone do item para o inventário

    [Header("Efeitos Visuais no Chão")]
    public bool flutuar = true;
    public float amplitude = 0.2f;
    public float velocidadeFlutuar = 2f;
    public bool girarNoChao = true;
    public float velocidadeGiro = 50f;

    private Vector3 posicaoInicial;
    private PlayerMovement player;

    void Start()
    {
        posicaoInicial = transform.position;
        player = FindFirstObjectByType<PlayerMovement>();
    }

    void Update()
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

    private void OnMouseDown()
    {
        if (player == null) return;

        // VERIFICAÇÃO DE DISTÂNCIA: Se o player estiver longe, cancela a ação!
        float distancia = Vector3.Distance(player.transform.position, transform.position);
        if (distancia > distanciaMaxima)
        {
            return; // Muito longe, não faz nada
        }

        // Procura o inventário na cena para guardar o item
        InventarioUmSlot inventario = FindFirstObjectByType<InventarioUmSlot>();

        if (inventario != null)
        {
            // Envia para o inventário
            inventario.PegarItem(nomeDoItem, fotoDoItem);

            // Destroi o item do chão
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("Nenhum InventarioUmSlot encontrado na cena!");
        }
    }
}