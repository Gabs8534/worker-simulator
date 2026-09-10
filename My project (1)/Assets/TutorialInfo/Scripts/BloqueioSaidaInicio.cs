using UnityEngine;

public class BloqueioSaidaInicio : MonoBehaviour
{
    [Header("Configuração de Bloqueio")]
    [Tooltip("Arraste aqui o GameObject da parede que vai bloquear a saída.")]
    public GameObject paredeBloqueio;

    [Header("Quantidade de Itens")]
    [Tooltip("Número de itens necessários para a parede aparecer e bloquear a saída (ex: 2 objetos).")]
    public int itensParaBloquear = 2;

    private bool saidaBloqueada = false;

    void Start()
    {
        // Garante que a parede começa DESATIVADA (a saída fica livre no início do jogo)
        if (paredeBloqueio != null)
        {
            paredeBloqueio.SetActive(false);
        }
    }

    void Update()
    {
        // Se a parede já foi ativada, não precisa continuar checando
        if (saidaBloqueada) return;

        // Pega diretamente a contagem global de itens coletados do script Coletavel
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
            paredeBloqueio.SetActive(true); // Ativa a parede, bloqueando a saída!
            Debug.Log("A parede apareceu e bloqueou a saída!");
        }
    }
}