using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovemen : MonoBehaviour
{
    [Header("Câmera")]
    public Camera playerCamera;

    [Header("Movimento")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;

    [Header("Mouse")]
    public float lookSpeed = 2f;

    [Header("Agachar")]
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    [Header("Estamina")]
    public Slider barraEstamina;
    public float estaminaMaxima = 100f;
    public float estaminaAtual;
    public float gastoEstamina = 20f;
    public float recargaEstamina = 15f;

    [Tooltip("Se marcado, a estamina só recupera com o jogador parado")]
    public bool recuperarSoParado = true;

    private bool exausto = false;

    [Header("Empurrar Objetos")]
    public float forcaDoEmpurrao = 5f;

    [Header("Sistema de Segurar Item")]
    public Transform maoDoPlayer;

    [HideInInspector]
    public GameObject itemAtualSegurado;

    [Header("Vassoura")]
    [HideInInspector]
    public bool estaNaVassoura = false;

    [Range(0.1f, 0.9f)]
    public float multiplicadorLentidaoVassoura = 0.4f;

    [Header("Bote da Vassoura")]
    private bool sofrendoBote = false;
    private Vector3 direcaoBote = Vector3.zero;
    private float velocidadeBote = 0f;

    public bool SofrendoBote
    {
        get { return sofrendoBote; }
    }

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController characterController;
    private bool canMove = true;

    // START

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        TravarMouse();

        estaminaAtual = estaminaMaxima;

        if (barraEstamina != null)
        {
            barraEstamina.maxValue = estaminaMaxima;
            barraEstamina.value = estaminaMaxima;
        }

        characterController.height = defaultHeight;
    }

    // MOUSE

    private void TravarMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LiberarMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Quando a janela do jogo volta a ficar em foco, trava de novo
    private void OnApplicationFocus(bool temFoco)
    {
        if (temFoco && canMove)
            TravarMouse();
    }

    // Libera o mouse ao sair da cena / ser destruído (menu e vitória)
    private void OnDestroy()
    {
        LiberarMouse();
    }

    // UPDATE

    private void Update()
    {
        // Garante que o mouse fica escondido durante a partida
        if (canMove &&
            (Cursor.lockState != CursorLockMode.Locked || Cursor.visible))
        {
            TravarMouse();
        }

        MovimentarPlayer();
    }

    // MOVIMENTO PRINCIPAL

    private void MovimentarPlayer()
    {
        if (characterController == null)
            return;

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        AtualizarEstamina();

        bool isRunning =
            Input.GetKey(KeyCode.LeftShift) &&
            estaminaAtual > 0f &&
            !exausto &&
            canMove;

        float velocidadeCaminhada = walkSpeed;
        float velocidadeCorrida = runSpeed;

        if (Input.GetKey(KeyCode.R) && canMove)
        {
            characterController.height = crouchHeight;
            velocidadeCaminhada = crouchSpeed;
            velocidadeCorrida = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            velocidadeCaminhada = walkSpeed;
            velocidadeCorrida = runSpeed;
        }

        if (estaNaVassoura)
        {
            velocidadeCaminhada *= multiplicadorLentidaoVassoura;
            velocidadeCorrida *= multiplicadorLentidaoVassoura;
        }

        float velocidadeAtual = isRunning ? velocidadeCorrida : velocidadeCaminhada;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float curSpeedX = canMove ? velocidadeAtual * vertical : 0f;
        float curSpeedY = canMove ? velocidadeAtual * horizontal : 0f;

        float movimentoVerticalAnterior = moveDirection.y;

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (sofrendoBote)
        {
            moveDirection.x = direcaoBote.x * velocidadeBote;
            moveDirection.z = direcaoBote.z * velocidadeBote;
            sofrendoBote = false;
        }

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movimentoVerticalAnterior;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else if (moveDirection.y < 0f)
        {
            moveDirection.y = -2f;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            RotacionarCamera();
        }

        estaNaVassoura = false;
    }

    // ESTAMINA

    private void AtualizarEstamina()
    {
        if (estaminaAtual <= 0f)
        {
            estaminaAtual = 0f;
            exausto = true;
        }

        if (exausto && estaminaAtual >= estaminaMaxima)
        {
            exausto = false;
        }

        bool correndo =
            Input.GetKey(KeyCode.LeftShift) &&
            !exausto &&
            estaminaAtual > 0f &&
            canMove;

        // Parado = nenhuma tecla de movimento apertada
        // (GetAxisRaw não tem suavização, então reage na hora)
        bool parado =
            !canMove ||
            (Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.01f &&
             Mathf.Abs(Input.GetAxisRaw("Vertical")) < 0.01f);

        if (correndo)
        {
            estaminaAtual -= gastoEstamina * Time.deltaTime;
        }
        else if (!recuperarSoParado || parado)
        {
            if (estaminaAtual < estaminaMaxima)
            {
                estaminaAtual += recargaEstamina * Time.deltaTime;
            }
        }
        // Andando sem correr: a estamina fica como está (não gasta e não recupera)

        estaminaAtual = Mathf.Clamp(estaminaAtual, 0f, estaminaMaxima);

        if (barraEstamina != null)
        {
            barraEstamina.value = estaminaAtual;
        }
    }

    // CÂMERA - SOMENTE ESQUERDA E DIREITA

    private void RotacionarCamera()
    {
        if (playerCamera == null)
            return;

        float mouseX = Input.GetAxis("Mouse X");
        transform.Rotate(0f, mouseX * lookSpeed, 0f);
    }

    // BOTE DA VASSOURA

    public void EmpurrarPorBote(Vector3 direcao, float velocidade)
    {
        direcao.y = 0f;

        if (direcao.sqrMagnitude <= 0.001f)
            return;

        direcaoBote = direcao.normalized;
        velocidadeBote = velocidade;
        sofrendoBote = true;
    }

    // COLISÃO COM OBJETOS

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null)
            return;

        if (body.isKinematic)
            return;

        if (hit.moveDirection.y < -0.3f)
            return;

        Vector3 direcaoEmpurrao = new Vector3(
            hit.moveDirection.x,
            0f,
            hit.moveDirection.z
        );

        if (direcaoEmpurrao.sqrMagnitude <= 0.001f)
            return;

        body.AddForceAtPosition(
            direcaoEmpurrao * forcaDoEmpurrao,
            hit.point,
            ForceMode.Impulse
        );
    }

    // CONTROLE DO PLAYER

    public void BloquearMovimento()
    {
        canMove = false;
        moveDirection = Vector3.zero;
    }

    public void LiberarMovimento()
    {
        canMove = true;
    }

    public bool PodeMover()
    {
        return canMove;
    }

    // VASSOURA

    public bool EstaNaVassoura()
    {
        return estaNaVassoura;
    }
}