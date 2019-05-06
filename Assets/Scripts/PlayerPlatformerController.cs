using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPlatformerController : BattleObject {

    /*
     * Status for player:
     * 0 - idle/moving/airbone
     * 1 - attack
     * 2 - air attack
     * 3 - guard
     * 4 - hitted
     */

    public float maxSpeed = 7;
    public float jumpTakeOffSpeed = 7;
    public bool alive = true;
    public float comboGroundAttackInterval = 0.25f;
    public float comboGroundAttackBufferReceiveTime = 0.1f;
    public float airAttackLasting = 0.15f;
    public float airAttackSpeed = 4;
    public int airAttackMaxTimes = 2;

    public Transform groundAttackWavePrefab;
    public Transform airAttackWavePrefab;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform airAttackWave;
    private InteractiveConversationSystem interactiveConversationSystem;
    private DialogueSystem dialogSystem;
    private Slider HpBar;

    private bool flip = false;
    private float lastGroundAttackTimePassed = 99999f;
    private bool groundAttackBuffer = false;
    private float guardRecoveryRemaining = 0f;
    private float hittedRecoveryRemaining = 0f;
    private float airAttackRemaining = 0f;
    private float airAttackWaveSpeedMultiplier = 2.5f;
    private int airAttackTimesRemaining = 2;
    private float guardBackPushSpeed = 0;

    private static float guardRecoveryTimeMultiplier = 0.005f;
    private static float hittedRecoveryTimeMultiplier = 0.01f;
    private static float hittedGravityMultiplier = 10f;
    private static float guardBackPushSpeedMultiplier = 0.005f;

    // Some temp field in development


    protected override void BattleObjectStartInternal()
    {
        spriteRenderer = GetComponent<SpriteRenderer> ();
        animator = GetComponent<Animator>();
        dialogSystem =
            GameObject
            .Find("DialogueSystem")
            .GetComponent<DialogueSystem>();
        interactiveConversationSystem = 
            GameObject
            .Find("InteractiveConversationSystem")
            .GetComponent<InteractiveConversationSystem>();
        HpBar = GameObject.Find("HpBar").GetComponent<Slider>();
        hitPoint = 1000;
        HpBar.value = hitPoint;
    }

    private void resetGroundAttackProperties()
    {
        groundAttackBuffer = false;
        lastGroundAttackTimePassed = 9999f;
    }

    private void resetAirAttackProperties()
    {
        airAttackRemaining = 0f;
        ignoreGravity = false;
        if (airAttackWave != null)
        {
            Destroy(airAttackWave.gameObject);
        }
    }

    private void groundAttack()
    {
        // Initiate attack
        groundAttackBuffer = false;
        lastGroundAttackTimePassed = 0f;
        animator.SetTrigger("Attack");
        status = 1;
        // Release wave
        Vector3 position = transform.position;
        position.z = -1;
        Quaternion quaternion = flip ? Quaternion.Euler(new Vector3(0, 180, 0)) : Quaternion.identity;
        Instantiate(groundAttackWavePrefab, position, quaternion);
        dialogSystem.SetNextWord("Scum!! You shall die! You are not worthy as my opponent! be gone! cutoff", 1f, true);
    }

    private void airAttack()
    {
        // Initiate attack
        airAttackTimesRemaining--;
        airAttackRemaining = airAttackLasting;
        ignoreGravity = true;
        animator.SetTrigger("Attack");
        status = 2;
        // Release wave
        Vector3 position = transform.position;
        position.z = -1;
        Quaternion quaternion = flip ? Quaternion.Euler(new Vector3(0, 180, 0)) : Quaternion.identity;
        airAttackWave = Instantiate(airAttackWavePrefab, position, quaternion);
        // Say something
        dialogSystem.SetNextWord("Dieeee!", 0.5f, true);
    }

    protected override void UpdateIntention(float deltaTime)
    {
    }

    protected override void PerformIntention(float deltaTime)
    {
        Vector2 move = Vector2.zero;
        animator.SetBool("Grounded", dummyGrounded);

        if (dummyGrounded)
        {
            airAttackTimesRemaining = airAttackMaxTimes;
            switch(status)
            {
                case 0:
                case 1: // Idle/Move/Airbone/Ground attack
                    lastGroundAttackTimePassed = 
                        Mathf.Min(
                            lastGroundAttackTimePassed + deltaTime, 
                            comboGroundAttackInterval);
                    // Ground attacks input
                    if (
                        ActionInputUtils.GetAttackButton() && 
                        !groundAttackBuffer &&
                        lastGroundAttackTimePassed >= comboGroundAttackBufferReceiveTime)
                    {
                        // Buffer attack intention.
                        groundAttackBuffer = true;
                    }
                    // When attack ends
                    if (lastGroundAttackTimePassed >= comboGroundAttackInterval)
                    {
                        // When previous attack ends.
                        if (groundAttackBuffer)
                        {
                            groundAttack();
                        }
                        else
                        {
                            // Back to idle
                            status = 0;
                        }
                    }
                    break;
                case 2: // Air attack
                    resetAirAttackProperties();
                    status = 0;
                    break;
                case 3: // Guard
                    guardRecoveryRemaining = Mathf.Max(guardRecoveryRemaining - deltaTime, 0);
                    if (guardRecoveryRemaining <= 0)
                    {
                        status = 0;
                    }
                    else
                    {
                        move.x = flip ? guardBackPushSpeed : -guardBackPushSpeed;
                        flip = move.x > 0;
                    }
                    break;
                case 4: // Hitted
                    hittedRecoveryRemaining = Mathf.Max(hittedRecoveryRemaining - deltaTime, 0);
                    if (hittedRecoveryRemaining <= 0)
                    {
                        gravityMultiplier = 1f;
                        status = 0;
                    }
                    break;
                default:
                    break;
            }
        } else
        {
            resetGroundAttackProperties();
            switch(status)
            {
                case 0: // Airbone
                    if (ActionInputUtils.GetAttackButton() && airAttackTimesRemaining > 0)
                    {
                        airAttack();
                    }
                    break;
                case 1: // Ground attack, falls in middle, cancel attack
                    status = 0;
                    break;
                case 2: // Air attack
                    airAttackRemaining = Mathf.Max(airAttackRemaining - deltaTime, 0);
                    if (airAttackRemaining <= 0)
                    {
                        resetAirAttackProperties();
                        status = 0;
                    } else
                    {
                        move.x = flip ? -airAttackSpeed : airAttackSpeed;
                        airAttackWave.localScale += 
                            new Vector3(Mathf.Abs(move.x * airAttackWaveSpeedMultiplier * deltaTime), 0, 0);
                    }
                    break;
                case 3: // Guard, falls in middle, cancel guard
                    guardRecoveryRemaining = 0;
                    status = 0;
                    break;
                case 4: // Hitted falls in middle or hitted in air, keep status till grounded
                    break;
                default:
                    break;
            }
        }
        
        // Movement input
        if (status == 0)
        {
            // Can only take input when status is idle/walk/airbone
            move.x = ActionInputUtils.GetHorizontalInput();
            move.y = ActionInputUtils.GetVerticalInput();
            flip = move.x < 0;
        }
        targetVelocity = move * maxSpeed;
        animator.SetFloat("MoveSpeed", Mathf.Abs(move.x));

        // Jump input
        if (ActionInputUtils.GetJumpButtonDown() && CanJump()) {
            verticalGravityVelocity = jumpTakeOffSpeed;
        }
        else if (ActionInputUtils.GetJumpButtonUp()) 
        {
            if (verticalGravityVelocity > 0) {
                verticalGravityVelocity = verticalGravityVelocity * 0.5f;
            }
        }
        animator.SetFloat("VerticalSpeed", verticalGravityVelocity);

        
        // Flip character
        spriteRenderer.flipX = flip;

        // Set status in animator
        animator.SetInteger("Status", (int)status);
    }

    protected override bool CanJump()
    {
        return base.CanJump() && status <= 1;
    }

    protected override void TakeHitInternal(Hit hit)
    {
        resetGroundAttackProperties();
        resetAirAttackProperties();
        if (
            (status == 0 || status == 3) && 
            hit.guardable && 
            dummyGrounded && 
            hittedFromFront(hit))
        {
            status = 3;
            guardRecoveryRemaining = hit.power * guardRecoveryTimeMultiplier;
            guardBackPushSpeed = hit.power * guardBackPushSpeedMultiplier;
        } else
        {
            status = 4;
            hitPoint -= hit.damage;
            if (hitPoint < 0)
            {
                hitPoint = 0;
                Die();
            }
            else
            {
                HpBar.value = hitPoint;
            }
            if (!dummyGrounded)
            {
                gravityMultiplier = hittedGravityMultiplier;
                hittedRecoveryRemaining = 0;
            }
            else
            {
                hittedRecoveryRemaining = hit.power * hittedRecoveryTimeMultiplier;
            }
            // invulnerabilityPeriod = hittedRecoveryRemaining / 2f;
        }
    }

    public override bool TrySwitchIntention(double newIntention)
    {
        throw new System.NotImplementedException();
    }

    public bool IsFlipped()
    {
        return flip;
    }

    public void Die()
    {

    }

    private bool hittedFromFront(Hit hit)
    {
        if (flip)
        {
            return hit.position.x < transform.position.x;
        }
        else
        {
            return hit.position.x > transform.position.x;
        }
    }
}