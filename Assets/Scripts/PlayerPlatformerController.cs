using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlatformerController : BattleObject {

    public float maxSpeed = 7;
    public float jumpTakeOffSpeed = 7;
    public bool alive = true;

    private SpriteRenderer spriteRenderer;
    private bool flip = false;
    private Animator animator;

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
    }

    protected override void PerformIntention(float deltaTime)
    {
        // Debug.Log(Time.deltaTime);
        Vector2 move = Vector2.zero;

        move.x = Input.GetAxis ("Horizontal");
        move.y = Input.GetAxis ("Vertical");

        if (Input.GetButtonDown ("Jump") && CanJump()) {
            verticalGravityVelocity = jumpTakeOffSpeed;
        } else if (Input.GetButtonUp ("Jump")) 
        {
            if (verticalGravityVelocity > 0) {
                verticalGravityVelocity = verticalGravityVelocity * 0.5f;
            }
        }

        if (move.x > 0)
        {
            flip = false;
        } else if (move.x < 0)
        {
            flip = true;
        }
        spriteRenderer.flipX = flip;

        targetVelocity = move * maxSpeed;
        animator.SetFloat("MoveSpeed", Mathf.Abs(move.x));
        animator.SetFloat("VerticalSpeed", verticalGravityVelocity);
        animator.SetBool("Grounded", grounded);
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log(move.x);
            animator.SetTrigger("Attack");
        }
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