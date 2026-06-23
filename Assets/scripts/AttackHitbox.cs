using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private PlayerAttack playerAttack;

    private bool hitSomething;


    void Awake()
    {
        playerAttack = GetComponentInParent<PlayerAttack>();
    }


    private void OnEnable()
    {
        hitSomething = false;
        Debug.Log("Hitbox activada");
    }

    private void OnDisable()
    {
        Debug.Log("Hitbox desactivada");
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hitSomething)
            return;

        Health health = other.GetComponent<Health>();

        if (health != null)
        {
            hitSomething = true;

            int damage = playerAttack.GetAttackDamage();

            health.TakeDamage(damage);
        }
    }


}