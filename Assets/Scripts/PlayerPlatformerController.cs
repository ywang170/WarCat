using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public DialogSystem dialogSystem;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform airAttackWave;

    private bool flip = false;
    private float lastGroundAttackTimePassed = 99999f;
    private bool groundAttackBuffer = false;
    private float guardRecoverRemaining = 0f;
    private float hittedRecoverRemaining = 0f;
    private float airAttackRemaining = 0f;
    private float airAttackWaveSpeedMultiplier = 2.5f;
    private int airAttackTimesRemaining = 2;

    // Some temp field in development
    private InteractiveConversationSystem interactiveConversationSystem;


    protected override void BattleObjectStartInternal()
    {
        spriteRenderer = GetComponent<SpriteRenderer> ();
        animator = GetComponent<Animator>();
        interactiveConversationSystem = 
            GameObject
            .Find("InteractiveConversationSystem")
            .GetComponent<InteractiveConversationSystem>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Enemy")
        {
            Debug.Log("Run into enemy");
        }
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
                    guardRecoverRemaining = Mathf.Max(guardRecoverRemaining - deltaTime, 0);
                    if (guardRecoverRemaining <= 0)
                    {
                        status = 0;
                    }
                    break;
                case 4: // Hitted
                    hittedRecoverRemaining = Mathf.Max(hittedRecoverRemaining - deltaTime, 0);
                    if (hittedRecoverRemaining <= 0)
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
                    guardRecoverRemaining = 0;
                    status = 0;
                    break;
                case 4: // Hitted falls in middle or hitted in air, keep status till grounded
                    gravityMultiplier = 10f;
                    hittedRecoverRemaining = Mathf.Max(hittedRecoverRemaining - deltaTime, 0);
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
        if (move.x > 0)
        {
            flip = false;
        } else if (move.x < 0)
        {
            flip = true;
        }
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
    }

    public override bool TrySwitchIntention(double newIntention)
    {
        throw new System.NotImplementedException();
    }

    public bool IsFlipped()
    {
        return flip;
    }
}