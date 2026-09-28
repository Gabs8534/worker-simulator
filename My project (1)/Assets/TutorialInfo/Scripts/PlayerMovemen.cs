using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class CanvasUpdate : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
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
    public Transform maoDoPlayer; // Crie um objeto vazio (GameObject Vazio) filho da câmera e arraste aqui!
    [HideInInspector] public GameObject itemAtualSegurado;
<<<<<<< HEAD

    [Header("Efeito da Vassoura")]
    [HideInInspector] public bool estaNaVassoura = false;
    [Range(0.1f, 0.9f)] public float multiplicadorLentidaoVassoura = 0.4f;

    // Variáveis para controlar o bote da vassoura
    private bool sofrendoBote = false;
    private Vector3 direcaoBote;
    private float velocidadeBote;
=======
>>>>>>> 451e7810906e6106680553110ee8ad4af50d24fd

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private bool canMove = true;
    private bool estaNaVassoura;
    private float multiplicadorLentidaoVassoura;
    private bool sofrendoBote;
    private int direcaoBote;
    private int velocidadeBote;

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


















    private Vector3 GetMoveDirection()
    {
<<<<<<< HEAD
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        if (estaminaAtual <= 0) exausto = true;
        if (exausto && estaminaAtual >= estaminaMaxima) exausto = false;

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && estaminaAtual > 0 && !exausto;

        if (isRunning)
        {
            estaminaAtual -= gastoEstamina * Time.deltaTime;
        }
        else if (estaminaAtual < estaminaMaxima)
        {
            estaminaAtual += recargaEstamina * Time.deltaTime;
        }
        estaminaAtual = Mathf.Clamp(estaminaAtual, 0, estaminaMaxima);

        if (barraEstamina != null)
        {
            barraEstamina.value = estaminaAtual;
        }

        float targetWalkSpeed = walkSpeed;
        float targetRunSpeed = runSpeed;

        if (Input.GetKey(KeyCode.R) && canMove)
        {
            characterController.height = crouchHeight;
            targetWalkSpeed = crouchSpeed;
            targetRunSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            targetWalkSpeed = 6f;
            targetRunSpeed = 12f;
        }

        if (estaNaVassoura)
        {
            targetWalkSpeed *= multiplicadorLentidaoVassoura;
            targetRunSpeed *= multiplicadorLentidaoVassoura;
        }

        float curSpeedX = canMove ? (isRunning ? targetRunSpeed : targetWalkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? targetRunSpeed : targetWalkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Se estiver sofrendo o bote da vassoura, adicionamos a força do bote ao movimento
        if (sofrendoBote)
        {
            moveDirection = direcaoBote * velocidadeBote;
            sofrendoBote = false; // Reseta para o próximo frame
        }

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

       
        estaNaVassoura = false;
=======
        return moveDirection;
>>>>>>> 451e7810906e6106680553110ee8ad4af50d24fd
    }

    private int GetDirecaoBote()
    {
        return direcaoBote;
    }

    private Vector3 GetV(int direcaoBote, Vector3 v) => v;













<<<<<<< HEAD
        if (other.CompareTag("Porta"))
        {
            Debug.Log("Chegou perto da porta!");
        }
    }
=======
>>>>>>> 451e7810906e6106680553110ee8ad4af50d24fd

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic) return;
        if (hit.moveDirection.y < -0.3f) return;

        Vector3 direcaoEmpurrao = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.AddForceAtPosition(direcaoEmpurrao * forcaDoEmpurrao, hit.point, ForceMode.Impulse);
    }

    internal void EmpurrarPorBote(Vector3 sweepDirection, float sweepSpeed) => throw new NotImplementedException();
}