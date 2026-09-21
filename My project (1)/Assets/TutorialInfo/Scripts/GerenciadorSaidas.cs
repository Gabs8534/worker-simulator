using UnityEngine;

public class ControladorBarreiras : MonoBehaviour
{
    [Header("Arraste as 3 paredes aqui (Direto da Hierarquia!)")]
    public GameObject paredeSaida1;
    public GameObject paredeSaida2;
    public GameObject paredeSaida3;

    public static ControladorBarreiras Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Garante que as 3 paredes começam ativas bloqueando o caminho
        LigarParedes(true);
    }

    // Método para ligar ou desligar as paredes de forma bem visual
    public void LigarParedes(bool ligar)
    {
        if (paredeSaida1 != null) paredeSaida1.SetActive(ligar);
        if (paredeSaida2 != null) paredeSaida2.SetActive(ligar);
        if (paredeSaida3 != null) paredeSaida3.SetActive(ligar);

        Debug.Log(ligar ? "As barreiras foram ATIVADAS (bloqueadas)." : "As barreiras foram DESATIVADAS (abertas)!");
    }

    // Botão para testar direto no Inspector do Unity com botão direito
    [ContextMenu("Testar Abertura das Barreiras")]
    public void TestarAbertura()
    {
        LigarParedes(false);
    }
}