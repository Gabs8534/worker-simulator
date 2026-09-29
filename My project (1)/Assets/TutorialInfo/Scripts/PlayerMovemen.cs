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
    public float lookXLimit = 45f;

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

    // O Perseguidor usa isso para saber
    // se o Player está sendo empurrado pela vassoura.
    public bool SofrendoBote
    {
        get
        {
            return sofrendoBote;
        }
    }

    private Vector3 moveDirection = Vector3.zero;

    private float rotationX = 0f;

    private CharacterController characterController;

    private bool canMove = true;

    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        characterController =
            GetComponent<CharacterController>();

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;

        estaminaAtual =
            estaminaMaxima;

        if (barraEstamina != null)
        {
            barraEstamina.maxValue =
                estaminaMaxima;

            barraEstamina.value =
                estaminaMaxima;
        }

        // Garante a altura inicial.
        characterController.height =
            defaultHeight;
    }

    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        MovimentarPlayer();
    }

    // =====================================================
    // MOVIMENTO PRINCIPAL
    // =====================================================

    private void MovimentarPlayer()
    {
        if (characterController == null)
            return;

        // =================================================
        // DIREÇÕES
        // =================================================

        Vector3 forward =
            transform.TransformDirection(
                Vector3.forward
            );

        Vector3 right =
            transform.TransformDirection(
                Vector3.right
            );

        // =================================================
        // ESTAMINA
        // =================================================

        AtualizarEstamina();

        bool isRunning =
            Input.GetKey(KeyCode.LeftShift) &&
            estaminaAtual > 0f &&
            !exausto &&
            canMove;

        // =================================================
        // AGACHAR
        // =================================================

        float velocidadeCaminhada =
            walkSpeed;

        float velocidadeCorrida =
            runSpeed;

        if (Input.GetKey(KeyCode.R) && canMove)
        {
            characterController.height =
                crouchHeight;

            velocidadeCaminhada =
                crouchSpeed;

            velocidadeCorrida =
                crouchSpeed;
        }
        else
        {
            characterController.height =
                defaultHeight;

            velocidadeCaminhada =
                walkSpeed;

            velocidadeCorrida =
                runSpeed;
        }

        // =================================================
        // VASSOURA
        // =================================================

        if (estaNaVassoura)
        {
            velocidadeCaminhada *=
                multiplicadorLentidaoVassoura;

            velocidadeCorrida *=
                multiplicadorLentidaoVassoura;
        }

        // =================================================
        // VELOCIDADE ATUAL
        // =================================================

        float velocidadeAtual =
            isRunning
                ? velocidadeCorrida
                : velocidadeCaminhada;

        // =================================================
        // INPUT
        // =================================================

        float horizontal =
            Input.GetAxis("Horizontal");

        float vertical =
            Input.GetAxis("Vertical");

        float curSpeedX =
            canMove
                ? velocidadeAtual * vertical
                : 0f;

        float curSpeedY =
            canMove
                ? velocidadeAtual * horizontal
                : 0f;

        // Guarda o Y antes de reconstruir
        // o movimento horizontal.
        float movimentoVerticalAnterior =
            moveDirection.y;

        // =================================================
        // MOVIMENTO NORMAL
        // =================================================

        moveDirection =
            (forward * curSpeedX) +
            (right * curSpeedY);

        // =================================================
        // BOTE DA VASSOURA
        // =================================================

        if (sofrendoBote)
        {
            moveDirection.x =
                direcaoBote.x *
                velocidadeBote;

            moveDirection.z =
                direcaoBote.z *
                velocidadeBote;

            // O bote dura este frame.
            sofrendoBote = false;
        }

        // =================================================
        // PULO
        // =================================================

        if (Input.GetButton("Jump") &&
            canMove &&
            characterController.isGrounded)
        {
            moveDirection.y =
                jumpPower;
        }
        else
        {
            moveDirection.y =
                movimentoVerticalAnterior;
        }

        // =================================================
        // GRAVIDADE
        // =================================================

        if (!characterController.isGrounded)
        {
            moveDirection.y -=
                gravity *
                Time.deltaTime;
        }
        else if (moveDirection.y < 0f)
        {
            // Mantém o Player preso ao chão.
            moveDirection.y = -2f;
        }

        // =================================================
        // APLICA MOVIMENTO
        // =================================================

        characterController.Move(
            moveDirection *
            Time.deltaTime
        );

        // =================================================
        // CÂMERA
        // =================================================

        if (canMove)
        {
            RotacionarCamera();
        }

        // =================================================
        // RESET DA VASSOURA
        // =================================================

        /*
         * O Sweep chama esta variável novamente
         * através do OnTriggerStay.
         *
         * Assim, ela só fica verdadeira enquanto
         * o Player estiver realmente sobre a vassoura.
         */

        estaNaVassoura = false;
    }

    // =====================================================
    // ESTAMINA
    // =====================================================

    private void AtualizarEstamina()
    {
        if (estaminaAtual <= 0f)
        {
            estaminaAtual = 0f;
            exausto = true;
        }

        if (exausto &&
            estaminaAtual >= estaminaMaxima)
        {
            exausto = false;
        }

        bool correndo =
            Input.GetKey(KeyCode.LeftShift) &&
            !exausto &&
            estaminaAtual > 0f &&
            canMove;

        if (correndo)
        {
            estaminaAtual -=
                gastoEstamina *
                Time.deltaTime;
        }
        else
        {
            if (estaminaAtual < estaminaMaxima)
            {
                estaminaAtual +=
                    recargaEstamina *
                    Time.deltaTime;
            }
        }

        estaminaAtual =
            Mathf.Clamp(
                estaminaAtual,
                0f,
                estaminaMaxima
            );

        if (barraEstamina != null)
        {
            barraEstamina.value =
                estaminaAtual;
        }
    }

    // =====================================================
    // CÂMERA
    // =====================================================

    private void RotacionarCamera()
    {
        if (playerCamera == null)
            return;

        rotationX +=
            -Input.GetAxis("Mouse Y") *
            lookSpeed;

        rotationX =
            Mathf.Clamp(
                rotationX,
                -lookXLimit,
                lookXLimit
            );

        playerCamera.transform.localRotation =
            Quaternion.Euler(
                rotationX,
                0f,
                0f
            );

        transform.rotation *=
            Quaternion.Euler(
                0f,
                Input.GetAxis("Mouse X") *
                lookSpeed,
                0f
            );
    }

    // =====================================================
    // BOTE DA VASSOURA
    // =====================================================

    public void EmpurrarPorBote(
        Vector3 direcao,
        float velocidade)
    {
        direcao.y = 0f;

        if (direcao.sqrMagnitude <= 0.001f)
            return;

        direcaoBote =
            direcao.normalized;

        velocidadeBote =
            velocidade;

        sofrendoBote = true;
    }

    // =====================================================
    // COLISÃO COM OBJETOS
    // =====================================================

    private void OnControllerColliderHit(
        ControllerColliderHit hit)
    {
        Rigidbody body =
            hit.collider.attachedRigidbody;

        if (body == null)
            return;

        if (body.isKinematic)
            return;

        if (hit.moveDirection.y < -0.3f)
            return;

        Vector3 direcaoEmpurrao =
            new Vector3(
                hit.moveDirection.x,
                0f,
                hit.moveDirection.z
            );

        if (direcaoEmpurrao.sqrMagnitude <= 0.001f)
            return;

        body.AddForceAtPosition(
            direcaoEmpurrao *
            forcaDoEmpurrao,
            hit.point,
            ForceMode.Impulse
        );
    }

    // =====================================================
    // CONTROLE DO PLAYER
    // =====================================================

    public void BloquearMovimento()
    {
        canMove = false;

        moveDirection =
            Vector3.zero;
    }

    public void LiberarMovimento()
    {
        canMove = true;
    }

    public bool PodeMover()
    {
        return canMove;
    }

    // =====================================================
    // VASSOURA
    // =====================================================

    public bool EstaNaVassoura()
    {
        return estaNaVassoura;
    }
}
