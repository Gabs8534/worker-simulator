using UnityEngine;
using UnityEngine.UI;

public class InventarioUmSlot : MonoBehaviour
{
    [Header("Referências da UI")]
    public GameObject painelDoSlot;    // O objeto visual do slot (painel ou imagem de fundo)
    public Image imagemIconeItem;      // O componente Image onde vai aparecer a foto do item

    private string nomeItemAtual = "";
    private bool temItem = false;

    void Start()
    {
        // Garante que começa vazio e escondido (ou sem imagem)
        if (imagemIconeItem != null)
        {
            imagemIconeItem.gameObject.SetActive(false);
        }
    }

    // Chamado pelo ItemFlutuante quando você clica nele
    public void PegarItem(string nome, Sprite icone)
    {
        if (temItem)
        {
            Debug.Log("O inventário já está cheio!");
            return;
        }

        nomeItemAtual = nome;
        temItem = true;

        if (imagemIconeItem != null && icone != null)
        {
            imagemIconeItem.sprite = icone;
            imagemIconeItem.gameObject.SetActive(true);
        }

        Debug.Log("Item guardado no inventário: " + nome);
    }

    // Se você quiser usar ou descartar o item do slot por código ou botão
    public void UsarOuDescartarItem()
    {
        if (!temItem) return;

        Debug.Log("Usou o item: " + nomeItemAtual);

        // Limpa o slot
        nomeItemAtual = "";
        temItem = false;

        if (imagemIconeItem != null)
        {
            imagemIconeItem.sprite = null;
            imagemIconeItem.gameObject.SetActive(false);
        }
    }
}