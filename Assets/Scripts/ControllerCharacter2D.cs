using System.Collections;
using Mono.Cecil.Cil;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

//RequireComponent(typeof(Rigidbody2D));
public class ControllerCharacter2D : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float jumpHeight;
    [SerializeField] float doubleJumpHeight;
    [SerializeField, Range(1, 5)] float fallRateMultiplier;
    [SerializeField, Range(1, 5)] float lowJumpRateMultiplier;
    [Header("Ground")]
    [SerializeField] Transform groundTransform;
    [SerializeField] LayerMask groundLayerMask;
    [SerializeField] float groundRadius;

    [Header("Attack")]
    [SerializeField] Transform attackTransform;
    [SerializeField] float attackRadius;

    [Header("Animator")]
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    Rigidbody2D rb;
    Vector2 velocity = Vector3.zero;
    bool faceRight = true;
    float groundAngle = 0;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        // check if the character is on the ground
        bool onGround = UpdateGroundCheck() && (velocity.y <= 0);
        // get direction input
        Vector2 direction = Vector2.zero;
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

            if (Input.GetMouseButtonDown(0))
            {
                animator.SetTrigger("Attack");
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

        // update animator
        animator.SetFloat("Speed", Mathf.Abs(velocity.x));
        animator.SetBool("Fall", !onGround && velocity.y < -0.1f);



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
        Gizmos.DrawSphere(groundTransform.position, groundRadius);
    }
    private bool UpdateGroundCheck()
    {
        // check if the character is on the ground
        Collider2D collider = Physics2D.OverlapCircle(groundTransform.position, groundRadius, groundLayerMask);
        if (collider != null)
        {
            RaycastHit2D raycastHit = Physics2D.Raycast(groundTransform.position, Vector2.down, groundRadius, groundLayerMask);
            if (raycastHit.collider != null)
            {
                // get the angle of the ground (angle between up vector and ground normal)
                groundAngle = Vector2.SignedAngle(Vector2.up, raycastHit.normal);
                Debug.DrawRay(raycastHit.point, raycastHit.normal, Color.red);
            }
        }

        return (collider != null);
    }

    private void CheckAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackTransform.position, attackRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject == gameObject) continue;

            if (collider.gameObject.TryGetComponent<IDamagable>(out var damagable))
            {
                damagable.Damage(10);
            }
        }
    }
}