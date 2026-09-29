using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovemen : MonoBehaviour
{
    public Camera playerCamera;

    [Header("Movimento")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
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

    [Header("Configuração do Empurrão")]
    public float forcaDoEmpurrao = 5f;

    [Header("Sistema de Segurar Item")]
    public Transform maoDoPlayer;

    [HideInInspector]
    public GameObject itemAtualSegurado;

    [Header("Efeito da Vassoura")]
    [HideInInspector]
    public bool estaNaVassoura = false;

    [Range(0.1f, 0.9f)]
    public float multiplicadorLentidaoVassoura = 0.4f;

    [Header("Bote da Vassoura")]
    private bool sofrendoBote = false;
    private Vector3 direcaoBote = Vector3.zero;
    private float velocidadeBote = 0f;

    // Essa variável será usada pelo perseguidor.
    public bool SofrendoBote
    {
        get { return sofrendoBote; }
    }

    private Vector3 moveDirection = Vector3.zero;

    private float rotationX = 0f;

    private CharacterController characterController;

    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        estaminaAtual = estaminaMaxima;

        if (barraEstamina != null)
        {
            barraEstamina.maxValue = estaminaMaxima;
            barraEstamina.value = estaminaMaxima;
        }
    }

    void Update()
    {
        GetMoveDirection();
    }

    private Vector3 GetMoveDirection()
    {
        Vector3 forward =
            transform.TransformDirection(Vector3.forward);

        Vector3 right =
            transform.TransformDirection(Vector3.right);

        // =====================================================
        // ESTAMINA
        // =====================================================

        if (estaminaAtual <= 0f)
        {
            exausto = true;
        }

        if (exausto &&
            estaminaAtual >= estaminaMaxima)
        {
            exausto = false;
        }

        bool isRunning =
            Input.GetKey(KeyCode.LeftShift) &&
            estaminaAtual > 0f &&
            !exausto;

        if (isRunning)
        {
            estaminaAtual -=
                gastoEstamina * Time.deltaTime;
        }
        else if (estaminaAtual < estaminaMaxima)
        {
            estaminaAtual +=
                recargaEstamina * Time.deltaTime;
        }

        estaminaAtual = Mathf.Clamp(
            estaminaAtual,
            0f,
            estaminaMaxima
        );

        if (barraEstamina != null)
        {
            barraEstamina.value = estaminaAtual;
        }

        // =====================================================
        // AGACHAR
        // =====================================================

        float targetWalkSpeed = walkSpeed;
        float targetRunSpeed = runSpeed;

        if (Input.GetKey(KeyCode.R) && canMove)
        {
            characterController.height =
                crouchHeight;

            targetWalkSpeed =
                crouchSpeed;

            targetRunSpeed =
                crouchSpeed;
        }
        else
        {
            characterController.height =
                defaultHeight;

            targetWalkSpeed =
                walkSpeed;

            targetRunSpeed =
                runSpeed;
        }

        // =====================================================
        // LENTIDÃO DA VASSOURA
        // =====================================================

        if (estaNaVassoura)
        {
            targetWalkSpeed *=
                multiplicadorLentidaoVassoura;

            targetRunSpeed *=
                multiplicadorLentidaoVassoura;
        }

        // =====================================================
        // VELOCIDADE
        // =====================================================

        float velocidadeAtual =
            isRunning
                ? targetRunSpeed
                : targetWalkSpeed;

        float curSpeedX =
            canMove
                ? velocidadeAtual *
                  Input.GetAxis("Vertical")
                : 0f;

        float curSpeedY =
            canMove
                ? velocidadeAtual *
                  Input.GetAxis("Horizontal")
                : 0f;

        float movimentoVerticalAnterior =
            moveDirection.y;

        moveDirection =
            (forward * curSpeedX) +
            (right * curSpeedY);

        // =====================================================
        // BOTE DA VASSOURA
        // =====================================================

        if (sofrendoBote)
        {
            moveDirection.x =
                direcaoBote.x *
                velocidadeBote;

            moveDirection.z =
                direcaoBote.z *
                velocidadeBote;

            // Consome o bote deste frame.
            sofrendoBote = false;
        }

        // =====================================================
        // PULO
        // =====================================================

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

        // =====================================================
        // GRAVIDADE
        // =====================================================

        if (!characterController.isGrounded)
        {
            moveDirection.y -=
                gravity *
                Time.deltaTime;
        }

        // =====================================================
        // MOVIMENTO
        // =====================================================

        characterController.Move(
            moveDirection *
            Time.deltaTime
        );

        // =====================================================
        // CÂMERA
        // =====================================================

        if (canMove)
        {
            rotationX +=
                -Input.GetAxis("Mouse Y") *
                lookSpeed;

            rotationX = Mathf.Clamp(
                rotationX,
                -lookXLimit,
                lookXLimit
            );

            if (playerCamera != null)
            {
                playerCamera.transform.localRotation =
                    Quaternion.Euler(
                        rotationX,
                        0f,
                        0f
                    );
            }

            transform.rotation *=
                Quaternion.Euler(
                    0f,
                    Input.GetAxis("Mouse X") *
                    lookSpeed,
                    0f
                );
        }

        // IMPORTANTE:
        // A vassoura precisa marcar novamente
        // esta variável no próximo frame.
        estaNaVassoura = false;

        return moveDirection;
    }

    // =====================================================
    // EMPURRÃO DA VASSOURA
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
    // COLISÃO COM RIGIDBODY
    // =====================================================

    private void OnControllerColliderHit(
        ControllerColliderHit hit)
    {
        Rigidbody body =
            hit.collider.attachedRigidbody;

        if (body == null ||
            body.isKinematic)
            return;

        if (hit.moveDirection.y < -0.3f)
            return;

        Vector3 direcaoEmpurrao =
            new Vector3(
                hit.moveDirection.x,
                0f,
                hit.moveDirection.z
            );

        body.AddForceAtPosition(
            direcaoEmpurrao *
            forcaDoEmpurrao,
            hit.point,
            ForceMode.Impulse
        );

        if (hit.collider.CompareTag("Porta"))
        {
            Debug.Log(
                "Chegou perto da porta!"
            );
        }
    }

    // =====================================================
    // TRIGGER
    // =====================================================

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Porta"))
        {
            Debug.Log(
                "Entrou no trigger da porta!"
            );
        }
    }

    // =====================================================
    // CONTROLE EXTERNO
    // =====================================================

    public void BloquearMovimento()
    {
        canMove = false;
    }

    public void LiberarMovimento()
    {
        canMove = true;
    }
}
