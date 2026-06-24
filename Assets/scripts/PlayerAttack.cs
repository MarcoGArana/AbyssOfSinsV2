using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    private Animator anim;

    public bool attacking;

    private PlayerInput playerInput;
    private InputAction punchAction;
    private InputAction kickAction;
    private InputAction blockAction;
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
        blockAction = playerInput.actions["Block"];

        airAttackUsed = false;

        movement = GetComponent<FighterMovement>();

        stats = GetComponent<FighterStats>();
    }


    void Update()
    {
        movement.isBlocking =
    blockAction.IsPressed() &&
    !movement.isDead &&
    !movement.isHit && movement.grounded;

        anim.SetBool("isBlocking", movement.isBlocking);
        if (movement.grounded)
        {
            airAttackUsed = false;
        }
        if (movement.isDead)
            return;
        if (movement.isHit)
            return;
        if (movement.isBlocking) {
            return;
    }

        if (attacking)
            return;


        if (punchAction.WasPressedThisFrame())
        {
            if (!movement.grounded && !airAttackUsed)
            {
                Attack("Aerial punch");
                airAttackUsed = true;

            }
            else if (movement.crouching)
            {
                Attack("Low punch");
            }
            else if (movement.grounded)
            {
                Attack("Punch light");
            }
        }


            if (kickAction.WasPressedThisFrame())
            {
                if (!movement.grounded && !airAttackUsed)
                {
                    Attack("Aerial kick");
                    airAttackUsed = true;

                }
                else if (movement.crouching)
                {
                    Attack("Low kick");
                }
                else if (movement.grounded)
            {
                    Attack("Kick light");
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

    public AttackType GetAttackType()
    {
        foreach (AttackData attack in attacks)
        {
            if (attack.attackName == currentAttack)
            {
                return attack.attackType;
            }
        }

        return AttackType.High;
    }
    public void EndAttack()
    {
        attacking = false;
    }
}