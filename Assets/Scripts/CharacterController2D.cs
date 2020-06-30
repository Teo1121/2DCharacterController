using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class CharacterController2D : MonoBehaviour
{

    [SerializeField, Tooltip("Max speed, in units per second, that the character moves.")]
    float speed = 9;

    [SerializeField, Tooltip("Acceleration while grounded.")]
    float walkAcceleration = 75;

    [SerializeField, Tooltip("Acceleration while in the air.")]
    float airAcceleration = 30;

    [SerializeField, Tooltip("Deceleration applied when character is grounded and not attempting to move.")]
    float groundDeceleration = 70;

    [SerializeField, Tooltip("Max height the character will jump regardless of gravity")]
    float jumpHeight = 4;


    //private BoxCollider2D boxCollider;
    private CapsuleCollider2D capsuleCollider;
    private Vector2 velocity;
    private float moveInput = 0;
    private bool jump = false;
    private bool doubleJump = true;

    private bool facingRight = true;

    /// <summary>
    /// Set to true when the character intersects a collider beneath
    /// them in the previous frame.
    /// </summary>
    private bool grounded;

    private void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        jump = jump ? true : Input.GetButtonDown("Jump");
    }

    private void FixedUpdate()
    {
        if (grounded)
        {   
            velocity.y = 0;    
            
            if (jump) 
            {
                // Calculate the velocity required to achieve the target jump height.
                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
                jump = false;
            }
        }
        else if (doubleJump && jump)
        {
            velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
            jump = false;
            doubleJump = false;
        }

        float acceleration = grounded ? walkAcceleration : airAcceleration;
        float deceleration = grounded ? groundDeceleration : 0;

        if (moveInput != 0)
        {
            velocity.x = Mathf.MoveTowards(velocity.x, speed * moveInput, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.fixedDeltaTime);
        }

        velocity.y += Physics2D.gravity.y * Time.fixedDeltaTime;

        transform.Translate(velocity * Time.fixedDeltaTime);

        grounded = false;

        // Retrieve all colliders we have intersected after velocity has been applied.
        Collider2D[] hits = Physics2D.OverlapCapsuleAll(transform.position, capsuleCollider.size, CapsuleDirection2D.Vertical, 0);

        foreach (Collider2D hit in hits)
        {
            // Ignore our own collider.
            if (hit == capsuleCollider || hit.isTrigger)
                continue;

            ColliderDistance2D colliderDistance = hit.Distance(capsuleCollider);
            
            
            // Ensure that we are still overlapping this collider.
            // The overlap may no longer exist due to another intersected collider
            // pushing us out of this one.
            if (colliderDistance.isOverlapped)
            {
                transform.Translate(colliderDistance.pointA - colliderDistance.pointB);
                velocity += (colliderDistance.pointA - colliderDistance.pointB)/Time.fixedDeltaTime;

                // If we intersect an object beneath us, set grounded to true. 
                if (Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && velocity.y < 0.01f)
                {
                    
                    grounded = true;
                    doubleJump = true;
                }
            }
        }

        if (velocity.x > 0.01f && !facingRight)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            facingRight = true;
        }
        else if (velocity.x < -0.01f && facingRight)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            facingRight = false;
        }

        jump = false;
    }
}