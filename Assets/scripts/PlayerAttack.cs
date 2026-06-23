using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private Animator anim;

    public bool attacking;

    private PlayerInput playerInput;
    private InputAction punchAction;
    private InputAction kickAction;

    private bool airAttackUsed;
    private FighterMovement movement;

    private FighterStats stats;


    [Header("Attack Data")]
    public AttackData[] attacks;


    private string currentAttack;


    void Awake()
    {
        anim = GetComponent<Animator>();

        playerInput = GetComponent<PlayerInput>();

        punchAction = playerInput.actions["Punch light"];
        kickAction = playerInput.actions["Kick light"];

        airAttackUsed = false;

        movement = GetComponent<FighterMovement>();

        stats = GetComponent<FighterStats>();
    }


    void Update()
    {
        if (movement.grounded)
        {
            airAttackUsed = false;
        }


        if (attacking)
            return;


        if (punchAction.WasPressedThisFrame())
        {
            if (movement.grounded)
                Attack("Punch light");
        }


        if (kickAction.WasPressedThisFrame())
        {
            if (movement.grounded)
            {
                Attack("Kick light");
            }
            else if (!airAttackUsed)
            {
                Attack("Aerial kick");
                airAttackUsed = true;
            }
        }
    }


    void Attack(string attackName)
    {
        attacking = true;

        // Guardamos qué ataque está haciendo
        currentAttack = attackName;

        anim.SetTrigger(attackName);
    }


    // Esta función la llamará el Hitbox cuando golpee
    public int GetAttackDamage()
    {
        foreach (AttackData attack in attacks)
        {
            if (attack.attackName == currentAttack)
            {
                return Mathf.RoundToInt(
                    attack.damage * stats.power
                );
            }
        }

        return 0;
    }


    public void EndAttack()
    {
        attacking = false;
    }
}