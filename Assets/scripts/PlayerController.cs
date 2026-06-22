using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private FighterMovement movement;
    private float moveInput;

    private void Awake()
    {
        movement = GetComponent<FighterMovement>();
    }

    private void FixedUpdate()
    {
        movement.Move(moveInput);
    }

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();

        moveInput = input.x;

        movement.Crouch(input.y < -0.5f);
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            movement.Jump();
        }
    }
}