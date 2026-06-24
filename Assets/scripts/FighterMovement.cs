using UnityEngine;

public class FighterMovement : MonoBehaviour
{
    private FighterStats stats;
    public bool isHit;
    [Header("Opponent")]
    public Transform opponent;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    public bool crouching;
    public bool grounded;
    public bool isDead;
    public bool isBlocking;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        stats = GetComponent<FighterStats>();
    }

    void Update()
    {
        FaceOpponent();
        
            
        
        // Velocidad para animación de caminar
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    public void Move(float direction)
{
        if (isHit)
            return;
        if (isDead)
            return;
        if (isBlocking)
        {
            Debug.Log("Bloqueando");

            rb.linearVelocity = new Vector2(
                0,
                rb.linearVelocity.y
            );
            return;
        }
        if (crouching)
    {
        rb.linearVelocity = new Vector2(
            0,
            rb.linearVelocity.y
        );

        return;
    }

        rb.linearVelocity = new Vector2(
        direction * stats.moveSpeed,
        rb.linearVelocity.y
    );

}

    public void Jump()
    {
        if (isHit)
            return;
        if (isDead)
            return;
        if (isBlocking)
            return;
        if (grounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                stats.jumpForce
            );
        }
    }

    private void FaceOpponent()
    {
        if (opponent == null)
            return;

        sr.flipX = opponent.position.x > transform.position.x;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Ground"))
        {
            grounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            grounded = false;
        }
    }
    public void Crouch(bool value)
    {
        crouching = grounded && value;
        
        anim.SetBool("isCrouching", crouching);
    }
}