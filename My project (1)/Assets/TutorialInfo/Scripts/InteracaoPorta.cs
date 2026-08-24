using UnityEngine;
using System.Collections; // Necessário para usar o temporizador (Coroutine)

public class InteracaoPorta : MonoBehaviour
{
    private Animator animator;
    private bool estaAberta = false;

    [SerializeField] private float tempoParaFechar = 3f; // Tempo em segundos que a porta fica aberta

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnMouseDown()
    {
        // Só abre se a porta estiver totalmente fechada
        if (!estaAberta)
        {
            animator.SetTrigger("Abrir");
            estaAberta = true;

            // Inicia o temporizador para fechar sozinha
            StartCoroutine(FecharPortaAutomaticamente());
        }
    }

    IEnumerator FecharPortaAutomaticamente()
    {
        // Espera o tempo definido (ex: 3 segundos)
        yield return new WaitForSeconds(tempoParaFechar);

        // Dispara o comando de fechar no Animator
        animator.SetTrigger("Fechar");
        estaAberta = false;
    }
}