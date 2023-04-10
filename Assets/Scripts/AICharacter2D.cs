using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

//RequireComponent(typeof(Rigidbody2D));
public class AICharacter2D : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float jumpHeight;
    [SerializeField] float doubleJumpHeight;
    [SerializeField, Range(1, 5)] float fallRateMultiplier;
    [SerializeField, Range(1, 5)] float lowJumpRateMultiplier;

    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    [Header("Ground")]
    [SerializeField] Transform groundTransform;
    [SerializeField] LayerMask groundLayerMask;
    [SerializeField] float groundRadius;

    [Header("AI")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] float RayDistance;

    Rigidbody2D rb;
    Vector2 velocity = Vector3.zero;
    bool faceRight = true;
    //float groundAngle = 0;
    Transform targetWaypoint;

    enum State
    { 
        IDLE,
        PATROL,
        CHASE,
        ATTACK
    }

    State state = State.IDLE;
    float stateTimer = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        Vector2 direction = Vector2.zero;

        //update ai
        switch (state)
        {
            case State.IDLE:
                if (CanSeePlayer()) state = State.CHASE;
                stateTimer += Time.deltaTime;
                if (stateTimer >= 2)
                {
                    targetWaypoint = waypoints[Random.Range(0, waypoints.Length)];
                    state = State.PATROL;
                }
                break;
            case State.PATROL:
                if (CanSeePlayer()) state = State.CHASE;
                direction.x = Mathf.Sign(transform.position.x - targetWaypoint.position.x);
                
                float dx = Mathf.Abs(transform.position.x - targetWaypoint.position.x);
                if (dx <= 0.5)
                {
                    direction.x = 0;
                }
                break;
            case State.CHASE:
                break;
            case State.ATTACK:
                break;

        }

        // check if the character is on the ground
        bool onGround = Physics.CheckSphere(groundTransform.position, 0.02f, groundLayerMask, QueryTriggerInteraction.Ignore);
        // get direction input
        
        direction.x = Input.GetAxis("Horizontal");

        // set velocity
        if (onGround)
        {
            velocity.x = direction.x * speed;
            if (velocity.y < 0) velocity.y = 0;
            if (Input.GetButtonDown("Jump"))
            {
                velocity.y += Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
                StartCoroutine(DoubleJump());
                animator.SetTrigger("Jump");
            }
        }
        // adjust gravity for jump
        float gravityMultiplier = 1;
        if (!onGround && velocity.y < 0) gravityMultiplier = fallRateMultiplier;
        if (!onGround && velocity.y > 0 && !Input.GetButton("Jump")) gravityMultiplier = lowJumpRateMultiplier;

        velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;

        //move character
        rb.velocity = velocity;

        //flip character
        if (velocity.x > 0 && !faceRight) Flip();
        if (velocity.x < 0 && !faceRight) Flip();

        animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        
    }
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        // no rigidbody
        if (body == null || body.isKinematic)
        {
            return;
        }
        if (hit.moveDirection.y < -0.3)
        {
            return;
        }
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.velocity = pushDir;
    }

    IEnumerator DoubleJump()
    {
        // wait a little after the jump to allow a double jump
        yield return new WaitForSeconds(0.01f);
        // allow a double jump while moving up
        while (velocity.y > 0)
        {
            // if "jump" pressed add jump velocity
            if (Input.GetButtonDown("Jump"))
            {
                velocity.y += Mathf.Sqrt(doubleJumpHeight * -2 * Physics.gravity.y);
                break;
            }
            yield return null;
        }
    }

    private void Flip()
    {
        faceRight = !faceRight;
        spriteRenderer.flipX = faceRight;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundTransform.position,groundRadius);
    }

    private void SetNewWayPointTarget()
    {
        Transform waypoint= null;
        do
        {
            waypoint = waypoints[Random.Range(0, waypoints.Length)];
        }
        while (waypoint == targetWaypoint);
        targetWaypoint = waypoint;
    }

    private bool CanSeePlayer()
    {
        RaycastHit2D raycasthit = Physics2D.Raycast(transform.position, ((faceRight) ? Vector2.right : Vector2.left) * RayDistance);
        Debug.DrawRay(transform.position, ((faceRight) ? Vector2.right : Vector2.left) * RayDistance);
       
        return raycasthit.collider != null && raycasthit.collider.gameObject.CompareTag("Player");
    }

}