using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("Configuração de Cena")]
    [Tooltip("Nome exato da cena do seu menu principal.")]
    public string nomeCenaMenu = "New Scene";

    [Header("Configuração de Tempo")]
    [Tooltip("Tempo em segundos que a tela de Game Over fica visível antes de voltar pro menu.")]
    public float tempoAteVoltar = 5.0f;

    [Header("Configuração de Imagens (Até 5 ou mais)")]
    [Tooltip("Arraste as 5 imagens (Sprites) aqui. O jogo escolherá uma aleatoriamente a cada morte.")]
    public Sprite[] imagensGameOver;
    [Tooltip("Arraste o objeto UI Image da sua tela de Game Over aqui.")]
    public Image imagemUI;

    void Start()
    {
        // Se houver imagens cadastradas e o componente Image estiver conectado, escolhe uma aleatória
        if (imagensGameOver != null && imagensGameOver.Length > 0 && imagemUI != null)
        {
            int indiceAleatorio = Random.Range(0, imagensGameOver.Length);
            imagemUI.sprite = imagensGameOver[indiceAleatorio];
        }

        // Inicia a contagem para voltar ao menu
        StartCoroutine(VoltarAutomaticamente());
    }

    IEnumerator VoltarAutomaticamente()
    {
        yield return new WaitForSeconds(tempoAteVoltar);
        SceneManager.LoadScene(nomeCenaMenu);
    }

    public void VoltarParaMenu()
    {
        SceneManager.LoadScene(nomeCenaMenu);
    }
}