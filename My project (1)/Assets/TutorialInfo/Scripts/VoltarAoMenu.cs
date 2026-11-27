using UnityEngine;
using UnityEngine.SceneManagement;

public class VoltarAoMenu : MonoBehaviour
{
    [Header("Cena do Menu")]
    public string nomeCenaMenu = "Menu";

    private void Start()
    {
        // Garante que o mouse aparece nesta cena
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void IrParaMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(nomeCenaMenu);
    }
}