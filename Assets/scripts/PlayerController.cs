using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private FighterMovement movement;
    private PlayerAttack attack;

    private float moveInput;


    private void Awake()
    {
        movement = GetComponent<FighterMovement>();
        attack = GetComponent<PlayerAttack>();
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


    public void OnPunchlight(InputValue value)
    {
        if (value.isPressed)
        {
            attack.LightPunch();
        }
    }


    public void OnKicklight(InputValue value)
    {
        if (value.isPressed)
        {
            attack.LightKick();
        }
    }


    public void OnBlock(InputValue value)
    {
        attack.SetBlock(
            value.isPressed
        );
    }
}