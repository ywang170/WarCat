using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicsObject : MonoBehaviour {

    // Minimum normal to be considered ground
    public float minGroundNormalY = .65f;
    // Is the character affected by gravity
    public bool ignoreGravity = false;

    // Desired velocity. While grounded and affected by velocity, this value
    // will be normalized. Otherwise the real velocity will be as it is.
    protected Vector2 targetVelocity;
    // If the character is on ground
    protected bool grounded;
    // Set this to positive value to jump
    protected float verticalGravityVelocity;
    // To prevent some uneven on ground causing "ungrounded"
    // We have this dummyGrounded to trick animator
    protected bool dummyGrounded;

    private Vector2 horizontalMovementNormal;
    private Rigidbody2D rb2d;
    private ContactFilter2D contactFilter;
    private RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    private List<RaycastHit2D> groundHitBufferList = new List<RaycastHit2D> (16);
    private Vector2 velocity;

    private const float minMoveDistance = 0.001f;
    private const float shellRadius = 0.01f;
    private const float verticalShellRadiusExtra = 0.1f;
    private const float verticalGravityVelocityMaxSpeed = -10f;

    void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D> ();
    }

    void Start () 
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask (Physics2D.GetLayerCollisionMask (gameObject.layer));
        contactFilter.useLayerMask = true;
        velocity = new Vector2();
        PhysicsObjectStartInternal();
    }
    
    void Update () 
    {
        // targetVelocity = Vector2.zero;
        PhysicsObjectUpdateInternal (Time.deltaTime); 
    }

    protected abstract void PhysicsObjectStartInternal();

    protected abstract void PhysicsObjectUpdateInternal(float deltaTime);

    protected virtual bool CanJump()
    {
        return !ignoreGravity && grounded && horizontalMovementNormal.y > minGroundNormalY;
    }

    void FixedUpdate()
    {
        if (ignoreGravity)
        {
            verticalGravityVelocity = 0;
            velocity = targetVelocity;
            horizontalMovementNormal = Vector2.up;
        } else
        {
            velocity.x = targetVelocity.x;
            if (!grounded)
            {
                verticalGravityVelocity += Physics2D.gravity.y * Time.deltaTime;
                verticalGravityVelocity = Mathf.Max(verticalGravityVelocity, verticalGravityVelocityMaxSpeed);
            } else if (horizontalMovementNormal.y < minGroundNormalY)
            {
                // Slip if the ground normal is not big enough
                velocity.x = horizontalMovementNormal.x > 0 ? 5 : -5;
            }
            velocity.y = verticalGravityVelocity;
        }

        Vector2 deltaPosition = velocity * Time.deltaTime;

        Vector2 moveAlongGround = new Vector2 (horizontalMovementNormal.y, -horizontalMovementNormal.x);

        // By default we want to fall even play has no input (If ground suddenly disappear, etc)
        grounded = false;
        dummyGrounded = false;

        // Move horizontally
        Movement (moveAlongGround * deltaPosition.x);

        // Move vertically
        Movement (Vector2.up * deltaPosition.y, !ignoreGravity);

        // If we are in air, reset ground normal
        if (!grounded)
        {
            horizontalMovementNormal = Vector2.up;
        }

    }

    // The flag verticalMovement is only used when gravity applies
    // It updates ground normal
    void Movement(Vector2 move, bool verticalMovement = false)
    {
        float distance = move.magnitude;
        float shellRadiusExtra = verticalMovement ? verticalShellRadiusExtra : 0;

        if (distance > minMoveDistance) 
        {
            // Collect all grounds this movement run into
            groundHitBufferList.Clear ();
            int count = rb2d.Cast (move, contactFilter, hitBuffer, distance + shellRadius + verticalShellRadiusExtra);
            for (int i = 0; i < count; i++) {
                if (hitBuffer[i].transform.tag == "Ground")
                {
                    groundHitBufferList.Add(hitBuffer[i]);
                }
            }

            if (verticalMovement)
            {
                float minDistance = distance;
                for (int i = 0; i < groundHitBufferList.Count; i++) 
                {
                    Vector2 currentNormal = groundHitBufferList[i].normal;
                    if (currentNormal.y > 0)
                    {
                        dummyGrounded = true;
                        if (groundHitBufferList[i].distance > distance + shellRadius)
                        {
                            continue;
                        }
                        grounded = true;
                        horizontalMovementNormal = currentNormal;
                        verticalGravityVelocity = -0.1f;
                    }
                    float modifiedDistance = groundHitBufferList [i].distance - shellRadius;
                    minDistance = modifiedDistance < minDistance ? modifiedDistance : minDistance;
                }
                distance = minDistance;
            } else
            {
                // Horizontal movement.
                // For all grounds, get which one has the least distance on this direction
                // Move the object for that minimum distance
                for (int i = 0; i < groundHitBufferList.Count; i++) 
                {
                    Vector2 currentNormal = groundHitBufferList[i].normal;
                    float modifiedDistance = groundHitBufferList [i].distance - shellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }
            }
            rb2d.position = rb2d.position + move.normalized * distance;
        }
    }
}