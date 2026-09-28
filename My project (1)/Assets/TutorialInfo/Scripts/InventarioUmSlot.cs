using UnityEngine;
using UnityEngine.UI;

public class InventarioUmSlot : MonoBehaviour
{
    [System.Serializable]
    public class ItemDados
    {
        public string nome;
        public Sprite foto;
    }

    [Header("Bolsos Invisíveis")]
    public ItemDados[] slots = new ItemDados[3];
    public int slotSelecionado = 0;

    [Header("A Única UI na Tela")]
    public Image slotDaTela;
    public Text textoNomeDoItem;

    [Header("Sistema de Seleção Visual")]
    public Image quadradinhoVermelho;
    // NOVO: Arraste aqui as posições ou os próprios objetos dos 3 slots da sua tela
    public Transform[] posicoesDosSlots = new Transform[3];

    [Header("Configurações")]
    public Sprite imagemVazia;

    void Start()
    {
        // Inicializa os 3 bolsos vazios para evitar erros de referência
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new ItemDados { nome = "", foto = null };
        }
        AtualizarInterface();
    }

    void Update()
    {
        // Troca de bolso e atualiza a interface
        if (Input.GetKeyDown(KeyCode.Alpha1)) { slotSelecionado = 0; AtualizarInterface(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { slotSelecionado = 1; AtualizarInterface(); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { slotSelecionado = 2; AtualizarInterface(); }

        // Usa o item
        if (Input.GetMouseButtonDown(0))
        {
            // Se clicou em um item do chão, não gasta o da mão
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.GetComponent<ItemClicavel>() != null) return;
            }

            UsarItemAtivo();
        }
    }

    public void PegarItem(string nomeDoItem, Sprite fotoDoItem)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (string.IsNullOrEmpty(slots[i].nome))
            {
                slots[i].nome = nomeDoItem;
                slots[i].foto = fotoDoItem;
                AtualizarInterface();
                return;
            }
        }
        Debug.Log("Inventário cheio!");
    }

    void AtualizarInterface()
    {
        if (slotDaTela == null || textoNomeDoItem == null) return;

        // --- NOVO: Move o quadradinho vermelho para o slot correto ---
        if (quadradinhoVermelho != null && posicoesDosSlots.Length > slotSelecionado && posicoesDosSlots[slotSelecionado] != null)
        {
            quadradinhoVermelho.enabled = true;
            // Faz o quadradinho pular para a exata posição do slot selecionado
            quadradinhoVermelho.transform.position = posicoesDosSlots[slotSelecionado].position;
        }

        // Mostra o item do bolso atual na UI principal
        ItemDados itemAtual = slots[slotSelecionado];

        if (itemAtual != null && !string.IsNullOrEmpty(itemAtual.nome))
        {
            slotDaTela.sprite = itemAtual.foto;
            textoNomeDoItem.text = itemAtual.nome;
        }
        else
        {
            slotDaTela.sprite = imagemVazia;
            textoNomeDoItem.text = "";
        }
    }

    void UsarItemAtivo()
    {
        if (slots[slotSelecionado] != null && !string.IsNullOrEmpty(slots[slotSelecionado].nome))
        {
            Debug.Log("Usou: " + slots[slotSelecionado].nome);

            slots[slotSelecionado].nome = "";
            slots[slotSelecionado].foto = null;

            AtualizarInterface();
        }
    }
}