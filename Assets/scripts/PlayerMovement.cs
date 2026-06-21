using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float runSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 80f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    private Vector2 moveInput;

    [Header("Ground Check")]
    public bool isGrounded;

    void Awake()
    {
        // Obtener componentes
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // Evitar rotación
        rb.freezeRotation = true;
    }

    void Update()
    {
        Jump();

        // Animación caminar
        anim.SetBool("walk", moveInput.x != 0);
    }

    void FixedUpdate()
    {
        Move();
        FlipSprite();
    }

    // -----------------------------
    // Movimiento horizontal
    // -----------------------------
    void Move()
    {
        rb.linearVelocity = new Vector2(moveInput.x * runSpeed, rb.linearVelocity.y);
    }

    // -----------------------------
    // Salto
    // -----------------------------
    void Jump()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    // -----------------------------
    // Girar sprite
    // -----------------------------
    void FlipSprite()
    {
        if (moveInput.x != 0)
        {
            // Cambia esto si queda invertido
            sr.flipX = moveInput.x > 0;
        }
    }

    // -----------------------------
    // Input System - Movimiento
    // -----------------------------
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // -----------------------------
    // Golpe
    // -----------------------------
    public void Punch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Punch");

            anim.SetTrigger("punch");
        }
    }

    // -----------------------------
    // Patada
    // -----------------------------
    public void Kick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Kick");

            anim.SetTrigger("kick");
        }
    }
    public void Dodge(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Dodge");
            anim.SetTrigger("dodge");
        }
    }

    // -----------------------------
    // Detectar suelo
    // -----------------------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}