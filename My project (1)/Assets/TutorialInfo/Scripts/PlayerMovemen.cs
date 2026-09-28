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
        return moveDirection;
    }

    private int GetDirecaoBote()
    {
        return direcaoBote;
    }

    private Vector3 GetV(int direcaoBote, Vector3 v) => v;














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