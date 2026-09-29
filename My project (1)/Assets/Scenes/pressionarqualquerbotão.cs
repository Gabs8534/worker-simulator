using UnityEngine;
using UnityEngine.SceneManagement; // IMPORTANTE: Necessário para carregar cenas

public class pressionarqualquerbotão : MonoBehaviour
{
    // Altere para o nome exato da cena do seu jogo no Unity
    public string nomeDaCena = "Jogo";

    void Start()
    {

    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(nomeDaCena);
        }
    }
}