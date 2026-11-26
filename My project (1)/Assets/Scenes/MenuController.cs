using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void Jogar()
    {
        // Coloque aqui o nome exato da sua cena entre aspas
        SceneManager.LoadScene("Jogo");
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}