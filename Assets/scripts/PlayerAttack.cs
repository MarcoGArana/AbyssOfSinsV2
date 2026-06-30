using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator anim;

    public bool attacking;

    private bool airAttackUsed;

    private FighterMovement movement;

    private FighterStats stats;


    [Header("Attack Data")]
    public AttackData[] attacks;


    private string currentAttack;



    void Awake()
    {
        anim = GetComponent<Animator>();

        movement = GetComponent<FighterMovement>();

        stats = GetComponent<FighterStats>();
    }



    void Update()
    {
        if (movement.grounded)
        {
            airAttackUsed = false;
        }
    }



    public void LightPunch()
    {
        if (movement.isDead)
            return;

        if (movement.isHit)
            return;

        if (movement.isBlocking)
            return;


        if (attacking)
            return;



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



    public void LightKick()
    {
        if (movement.isDead)
            return;

        if (movement.isHit)
            return;

        if (movement.isBlocking)
            return;


        if (attacking)
            return;



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



    public void SetBlock(bool value)
    {
        movement.isBlocking =
            value &&
            !movement.isDead &&
            !movement.isHit &&
            movement.grounded;


        anim.SetBool(
            "isBlocking",
            movement.isBlocking
        );
    }



    private void Attack(string attackName)
    {
        attacking = true;


        currentAttack = attackName;


        anim.SetTrigger(attackName);
    }



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

    public void ResetAttack()
    {
        attacking = false;
        airAttackUsed = false;
    }
}