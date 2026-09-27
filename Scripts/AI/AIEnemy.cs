using UnityEngine;
using System.Collections;
using System;
using FMODUnity;

[RequireComponent(typeof(AIAgent))]
[RequireComponent(typeof(FieldOfView))]
public class AIEnemy : MonoBehaviour, NoiseListener
{

    [Header("Functional Settings")]
    [SerializeField] private bool canTakeDamage = true;
    [SerializeField] private bool canDealDamage = true;

    [Header("Enemy Attributes")]
    [SerializeField] private int maxHealth = 100;
    public int currentHealth { get; private set;}

    [Header("Enemy Attack Settings")]
    [SerializeField] private int damage = 20;
    [SerializeField] private float timeBetweenAttacks = 2f;
    [SerializeField] private float extraPursuitTime = 5f;

    [Header("Animation Helpers")] // TODO: Remove animation helpers
    [SerializeField] private float damagePlayerDelay = 1f;

    [Header("Sound Settings")]
    [SerializeField] private StudioEventEmitter footstepEmitter;
    [SerializeField] private StudioEventEmitter attackEmitter;
    [SerializeField] private StudioEventEmitter injuredEmitter;
    [SerializeField] private StudioEventEmitter retreatEmitter; // TODO: Add retreat sound
    [SerializeField] private StudioEventEmitter deathEmitter;
    [SerializeField] private StudioEventEmitter spotAPlayerEmitter;
    [SerializeField] private StudioEventEmitter justLostPlayerEmitter;
    [SerializeField] private StudioEventEmitter stoppedPursuingPlayerEmitter;
    [SerializeField] private StudioEventEmitter heardSoundEmitter;

    // Enemy State
    public enum EnemyState
    {
        Wandering,
        Pursuing,
        Investigating
    }
    public EnemyState enemyState { get; private set;}

    public AIAgent aiAgent { get; private set;}
    private FieldOfView fov;
    private Animator animator;

    // Time when the player was last seen
    private float lastSeenTime = -Mathf.Infinity;

    // Actions
    public Action OnDeath;

    void Awake()
    {
        // Get the AIAgent and FieldOfView components
        aiAgent = GetComponent<AIAgent>();
        fov = GetComponent<FieldOfView>();
        animator = GetComponentInChildren<Animator>();

        // Set the enemy state to wandering
        enemyState = EnemyState.Wandering;
        currentHealth = maxHealth;
    }

    void Update()
    {
        HandleSight();
        ProcessState();
        UpdateAnimator();
    }

    void HandleEnemySound() {

        // Handle Footstep Sounds
        if (aiAgent.agent.velocity.magnitude > 0.1f)
        {
            if(!footstepEmitter.IsPlaying())
                footstepEmitter?.Play();
        }
        else
        {
            footstepEmitter?.Stop();
        }
        
    }

    void HandleSight() {
        if (fov.canSeePlayer)
        {
            // The player is in sight, start pursuit
            SetPursuingState();
            FirstPersonController.Instance.ambienceEmitter.SetParameter("Monsterness", 2);
        }
        else if (Time.time > lastSeenTime + extraPursuitTime)
        {
            // The player is not in sight, start investigating
            SetWanderingState();
        }
        else if (!fov.canSeePlayer && enemyState == EnemyState.Pursuing)
        {
            justLostPlayerEmitter?.Play();
        }
    }

    void SetPursuingState() {
        lastSeenTime = Time.time;
        aiAgent.UpdateIsWanderingState(false);
        
        if (enemyState != EnemyState.Pursuing)
            spotAPlayerEmitter?.Play();
            FirstPersonController.Instance.ambienceEmitter.SetParameter("Monsterness", 2);

        enemyState = EnemyState.Pursuing;
    }

    void SetWanderingState() {
        aiAgent.UpdateIsWanderingState(true);

        if(enemyState == EnemyState.Pursuing)
            stoppedPursuingPlayerEmitter?.Play();

        enemyState = EnemyState.Wandering;
        FirstPersonController.Instance.ambienceEmitter.SetParameter("Monsterness", 0);
    }

    void ProcessState() {
        switch (enemyState)
        {
            case EnemyState.Wandering:
                // The enemy is wandering, do nothing
                break;
                FirstPersonController.Instance.ambienceEmitter.SetParameter("Monsterness", 0);
            case EnemyState.Pursuing:
                // The enemy is pursuing, follow the player
                aiAgent.SetTargetPosition(fov.playerRef.transform.position);
                FirstPersonController.Instance.ambienceEmitter.SetParameter("Monsterness", 2);
                break;
            case EnemyState.Investigating:
                // The enemy is investigating, do nothing
                FirstPersonController.Instance.ambienceEmitter.SetParameter("Monsterness", 1);
                break;
        }
    }

    public void OnNoiseHeard(Vector3 noiseSourcePosition)
    {
        if(enemyState == EnemyState.Pursuing)
            return;

        // The enemy heard a noise, start pursuit
        lastSeenTime = Time.time;
        aiAgent.SetTargetPosition(noiseSourcePosition);
        aiAgent.UpdateIsWanderingState(false);

        // TODO: make this trigger when we want it to
        if(enemyState != EnemyState.Investigating)
            heardSoundEmitter?.Play();
            FirstPersonController.Instance.ambienceEmitter.SetParameter("Monsterness", 1);

        enemyState = EnemyState.Investigating;
        print("Heard a noise!");
    }

    public void OnPlayerEnter(FirstPersonController player)
    {
        StartCoroutine(DealDamage(player));
    }

    public void OnPlayerExit(FirstPersonController player)
    {
        
    }

    public void TakeDamage(int damage, GameObject damageSource = null)
    {
        if (!canTakeDamage)
            return;

        currentHealth -= damage;

        if(damageSource != null)
            OnNoiseHeard(damageSource.transform.position);

        if (currentHealth <= 0)
            Death();
        else
            Injured();
    }

    void Injured()
    {
        injuredEmitter?.Play();
    }

    void Death()
    {
        deathEmitter?.Play();

        OnDeath?.Invoke();
        
        Destroy(gameObject);
    }

    public IEnumerator DealDamage(FirstPersonController player)
    {
        if (!canDealDamage)
            yield break;

        animator.SetTrigger("attack");
        attackEmitter?.Play();

        canDealDamage = false;
        yield return new WaitForSeconds(damagePlayerDelay);
        canDealDamage = true;

        player.ApplyDamage(damage);
        SetPursuingState();

        StartCoroutine(AttackCooldown());
    }

    IEnumerator AttackCooldown()
    {
        canDealDamage = false;
        yield return new WaitForSeconds(timeBetweenAttacks);
        canDealDamage = true;
    }

    void UpdateAnimator() {
        animator.SetFloat("speed", aiAgent.agent.velocity.magnitude);
    }
}
