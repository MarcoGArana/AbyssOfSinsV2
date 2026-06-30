using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum AIState { Idle, Approach, Attack, Block, Retreat }

    [System.Serializable]
    public class AIProfile
    {
        [Header("Comportamiento general")]
        [Range(0f, 1f)] public float aggressiveness = 0.85f;   // prob. de atacar estando en rango
        [Range(0f, 1f)] public float blockChance = 0.4f;       // prob. de intentar bloquear un ataque detectado
        [Range(0f, 1f)] public float mistakeRate = 0.15f;      // prob. de "fallar" la reaccion (humaniza a la IA)
        public float reactionTimeMin = 0.08f;
        public float reactionTimeMax = 0.25f;
        public float blockHoldTime = 0.4f;

        [Header("Variedad de golpe")]
        [Range(0f, 1f)] public float lowAttackChance = 0.2f;     // agacharse y pegar bajo
        [Range(0f, 1f)] public float aerialAttackChance = 0.15f; // saltar y pegar en el aire
    }

    [Header("Perfil de dificultad")]
    public AIProfile profile;

    [Header("Current State")]
    [SerializeField] private AIState currentState = AIState.Idle;

    [Header("Ranges")]
    public float attackRange = 8.5f;
    public float safeRange = 6.5f;

    [Header("Limites del escenario")]
    public float stageMinX = -10f;
    public float stageMaxX = 10f;

    [Header("Timers")]
    [SerializeField] private float decisionCooldown = 0.5f;
    private float decisionTimer;
    [SerializeField] private float attackDuration = 0.4f;

    private FighterMovement movement;
    private PlayerAttack attackScript;
    private Transform player;

    private FighterMovement playerMovement;
    private PlayerAttack playerAttack;

    private bool playerWasAttacking;
    private bool waitingAerialAttack;
    private AttackType pendingBlockType;

    void Awake()
    {
        movement = GetComponent<FighterMovement>();
        attackScript = GetComponent<PlayerAttack>();
        player = movement.opponent;

        if (player != null)
        {
            playerMovement = player.GetComponent<FighterMovement>();
            playerAttack = player.GetComponent<PlayerAttack>();
        }
    }

    void Update()
    {
        if (movement.isDead || movement.isHit) return;
        if (player == null) return;

        float distanceToPlayer = Mathf.Abs(transform.position.x - player.position.x);

        // Deteccion de ataque entrante del jugador
        bool playerAttackingNow = playerAttack != null && playerAttack.attacking;
        if (playerAttackingNow && !playerWasAttacking &&
            currentState != AIState.Attack && currentState != AIState.Block &&
            distanceToPlayer <= attackRange)
        {
            TryReactToIncomingAttack();
        }
        playerWasAttacking = playerAttackingNow;

        if (waitingAerialAttack && !movement.grounded)
        {
            waitingAerialAttack = false;
            DoAttack();
        }

        decisionTimer += Time.deltaTime;
        if (decisionTimer >= decisionCooldown)
        {
            MakeDecision(distanceToPlayer);
            decisionTimer = 0f;
        }

        ExecuteState();
    }

    // ---------- DECISIONES ----------

    void MakeDecision(float distance)
    {
        if (currentState == AIState.Attack || currentState == AIState.Block) return;

        if (distance <= attackRange)
        {
            bool isCornered = IsCornered();
            float attackChance = profile.aggressiveness;

            if (isCornered) attackChance = Mathf.Min(1f, attackChance + 0.15f);

            if (Random.value < attackChance)
            {
                currentState = AIState.Attack;
                TriggerAttack();
                Invoke(nameof(ResetAfterAttack), attackDuration);
            }
            else if (!isCornered)
            {
                currentState = AIState.Retreat;
            }
            else
            {
                currentState = AIState.Idle; // no retrocede hacia la pared
            }
        }
        else if (distance > safeRange)
        {
            currentState = AIState.Approach;
        }
        else
        {
            currentState = (Random.value < 0.5f) ? AIState.Idle : AIState.Approach;
        }
    }

    void TryReactToIncomingAttack()
    {
        // mistakeRate simula errores para castigarlos
        if (Random.value < profile.mistakeRate) return;
        if (Random.value >= profile.blockChance) return;
        if (playerAttack == null) return;

        pendingBlockType = playerAttack.GetAttackType();

        float reaction = Random.Range(profile.reactionTimeMin, profile.reactionTimeMax);
        CancelInvoke(nameof(CommitBlock));
        Invoke(nameof(CommitBlock), reaction);
    }

    void CommitBlock()
    {
        if (movement.isDead || movement.isHit) return;

        // Para bloquear un golpe bajo hay que estar agachado;
        // para uno alto o aereo, hay que estar de pie.
        bool shouldCrouch = pendingBlockType == AttackType.Low;
        movement.Crouch(shouldCrouch);

        attackScript.SetBlock(true);
        currentState = AIState.Block;

        CancelInvoke(nameof(ReleaseBlock));
        Invoke(nameof(ReleaseBlock), profile.blockHoldTime);
    }

    void ReleaseBlock()
    {
        attackScript.SetBlock(false);
        movement.Crouch(false);

        if (currentState == AIState.Block)
            currentState = AIState.Idle;
    }

    // ---------- EJECUCION DE ESTADO ----------

    void ExecuteState()
    {
        if (currentState == AIState.Block) return;

        float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);

        switch (currentState)
        {
            case AIState.Idle:
                movement.Move(0f);
                break;
            case AIState.Approach:
                movement.Move(directionToPlayer);
                break;
            case AIState.Retreat:
                movement.Move(-directionToPlayer);
                break;
            case AIState.Attack:
                movement.Move(0f);
                break;
        }
    }

    void ResetAfterAttack()
    {
        if (currentState != AIState.Block)
            currentState = IsCornered() ? AIState.Idle : AIState.Retreat;
    }

    // ---------- ATAQUE ----------

    void TriggerAttack()
    {
        if (attackScript == null) return;

        float stanceRoll = Random.value;

        if (stanceRoll < profile.lowAttackChance && movement.grounded)
        {
            // Golpe bajo: agacharse activa automaticamente "Low punch/kick"
            // dentro de LightPunch()/LightKick().
            movement.Crouch(true);
            DoAttack();
            Invoke(nameof(StandUpAfterAttack), attackDuration);
        }
        else if (stanceRoll < profile.lowAttackChance + profile.aerialAttackChance && movement.grounded)
        {
            // Golpe aereo
            movement.Crouch(false);
            movement.Jump();
            waitingAerialAttack = true;
        }
        else
        {
            movement.Crouch(false);
            DoAttack();
        }
    }

    void DoAttack()
    {
        if (Random.value < 0.5f) attackScript.LightKick();
        else attackScript.LightPunch();
    }

    void StandUpAfterAttack()
    {
        if (!movement.isHit && !movement.isDead)
            movement.Crouch(false);
    }

    // ---------- UTILIDAD ----------

    bool IsCornered()
    {
        return transform.position.x <= stageMinX + 0.5f || transform.position.x >= stageMaxX - 0.5f;
    }

    public void ResetAI()
    {
        CancelInvoke();
        currentState = AIState.Idle;
        decisionTimer = 0f;
        waitingAerialAttack = false;
        playerWasAttacking = false;
    }
}