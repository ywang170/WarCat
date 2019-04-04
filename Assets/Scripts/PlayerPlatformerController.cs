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

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool flip = false;
    private float lastGroundAttackTimePassed = 99999f;
    private bool groundAttackBuffer = false;


    protected override void BattleObjectStartInternal()
    {
        spriteRenderer = GetComponent<SpriteRenderer> ();
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Enemy")
        {
            Debug.Log("Run into enemy");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.transform.tag == "Enemy")
        {
            Debug.Log("Collided into enemy");
        }
    }

    protected override void UpdateIntention(float deltaTime)
    {
        // Setup some fields not affected by user input.
        lastGroundAttackTimePassed = 
            Mathf.Min(
                lastGroundAttackTimePassed + deltaTime, 
                comboGroundAttackInterval);
        animator.SetBool("Grounded", dummyGrounded);
    }

    protected override void PerformIntention(float deltaTime)
    {
        // Movement input
        Vector2 move = Vector2.zero;
        if (status == 0)
        {
            // Can only take input when status is idle/walk/airbone
            move.x = Input.GetAxis ("Horizontal");
            move.y = Input.GetAxis ("Vertical");
        }
        targetVelocity = move * maxSpeed;
        animator.SetFloat("MoveSpeed", Mathf.Abs(move.x));

        // Jump input
        if (Input.GetButtonDown ("Jump") && CanJump()) {
            verticalGravityVelocity = jumpTakeOffSpeed;
        } else if (Input.GetButtonUp ("Jump")) 
        {
            if (verticalGravityVelocity > 0) {
                verticalGravityVelocity = verticalGravityVelocity * 0.5f;
            }
        }
        animator.SetFloat("VerticalSpeed", verticalGravityVelocity);

        if (grounded)
        {
            // Ground attacks input
            if (
                Input.GetMouseButtonDown(0) && 
                !groundAttackBuffer &&
                status <= 1 &&
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
                    // Land attack
                    groundAttackBuffer = false;
                    lastGroundAttackTimePassed = 0f;
                    animator.SetTrigger("Attack");
                    status = 1;
                }
                else
                {
                    // Back to idle
                    status = 0;
                }
            }
        } else
        {
            // Clean up some fields
            if (status == 1)
            {
                // Clean up ground attack
                groundAttackBuffer = false;
                lastGroundAttackTimePassed = 9999f;
                status = 0;
            }
        }
        
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
        // All previous condition plus status is 0
        return base.CanJump() && status == 0;
    }

    protected override void TakeHitInternal(Hit hit)
    {
        throw new System.NotImplementedException();
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