using UnityEngine;

public class BloqueioSaidaInicio : MonoBehaviour
{
    [Header("Configuração de Bloqueio")]
    [Tooltip("Arraste aqui o GameObject da parede que vai bloquear a saída.")]
    public GameObject paredeBloqueio;

    [Header("Quantidade de Itens")]
    [Tooltip("Número de itens necessários para a parede aparecer e bloquear a saída.")]
    public int itensParaBloquear = 2;

    private bool saidaBloqueada = false;

    void Start()
    {
        // Reseta o contador ao iniciar o jogo (caso tenha ficado salvo do teste anterior)
        Coletavel.objetosColetadosTotais = 0;

        if (paredeBloqueio != null)
        {
            paredeBloqueio.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Atenção: A 'Parede de Bloqueio' não foi atribuída no Inspector!");
        }
    }

    void Update()
    {
        if (saidaBloqueada) return;

        // Mostra no Console o progresso para sabermos se o script está lendo a contagem
        // (Você pode ver isso na aba Console do Unity)
        // Debug.Log("Itens atuais: " + Coletavel.objetosColetadosTotais);

        if (Coletavel.objetosColetadosTotais >= itensParaBloquear)
        {
            BloquearSaida();
        }
    }

    void BloquearSaida()
    {
        saidaBloqueada = true;

        if (paredeBloqueio != null)
        {
            paredeBloqueio.SetActive(true); // Ativa a parede
            Debug.Log("Sucesso! A parede apareceu e bloqueou a saída!");
        }
    }
}