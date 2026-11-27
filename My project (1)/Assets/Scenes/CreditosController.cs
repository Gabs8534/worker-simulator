using UnityEngine;

public class CreditosController : MonoBehaviour
{
    [Header("Imagem dos créditos")]
    [Tooltip("Objeto Image com a tela de créditos (o botão VOLTAR fica dentro dele)")]
    public GameObject imagemCreditos;

    [Header("Esconder ao abrir (opcional)")]
    [Tooltip("Botões START, CREDITS, título etc. que somem enquanto os créditos aparecem")]
    public GameObject[] esconderAoAbrir;

    [Header("Fechar sem o botão")]
    public bool fecharComEsc = true;
    public bool fecharComClique = false;

    private int frameAbertura = -1;

    private void Start()
    {
        if (imagemCreditos != null)
            imagemCreditos.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        if (imagemCreditos == null || !imagemCreditos.activeSelf)
            return;

        // Evita fechar no mesmo frame em que abriu
        if (Time.frameCount == frameAbertura)
            return;

        if (fecharComEsc && Input.GetKeyDown(KeyCode.Escape))
        {
            FecharCreditos();
        }
        else if (fecharComClique && Input.GetMouseButtonDown(0))
        {
            FecharCreditos();
        }
    }

    public void AbrirCreditos()
    {
        if (imagemCreditos != null)
            imagemCreditos.SetActive(true);

        foreach (GameObject objeto in esconderAoAbrir)
        {
            if (objeto != null)
                objeto.SetActive(false);
        }

        frameAbertura = Time.frameCount;
    }

    public void FecharCreditos()
    {
        if (imagemCreditos != null)
            imagemCreditos.SetActive(false);

        foreach (GameObject objeto in esconderAoAbrir)
        {
            if (objeto != null)
                objeto.SetActive(true);
        }
    }
}