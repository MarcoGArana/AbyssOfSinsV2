using UnityEngine;

public class FighterMovement : MonoBehaviour
{
    private FighterStats stats;

    [Header("Opponent")]
    public Transform opponent;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    public bool crouching;
    public bool grounded;

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