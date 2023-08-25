using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playable : MonoBehaviour
{
    public Animator anim;

    private readonly string[] FORWARD_FETZES_NAMES = new string[] { "Forward-Fetz-Initial", "Forward-Fetz-Charge", "Forward-Fetz-Execution" };
    public readonly string NAME = "Luca187";
    public Sprite STOCK;
    public Sprite PROFILE_IMAGE;

    //Components
    [Dependency]
    protected Rigidbody2D rb;

    //UI
    public Gradient gradient;
    public Profile profile;

    //Essentials
    public float damage = 0.1f;
    public int stocks = 3;
    public int weight;

    [Header("Jumpforces")]
    [Tooltip("Jumpforce when on the ground")]
    public float groundJumpForce;
    [Tooltip("Jumpforce when in the air")]
    public float airJumpForce;

    [Header("Speeds")]
    [Tooltip("Speed when on the ground")]
    public float groundSpeed;
    [Tooltip("Speed when in the air")]
    public float airSpeed;
    [Tooltip("Speed when crouching")]
    public float crouchSpeed;

    [Header("Character Specialities")]
    [Tooltip("Can walk while crouching")]
    public bool canWalkWhileCrouching;

    [Header("Shield")]
    [Tooltip("Shield object of the character")]
    public GameObject shield;

    //Ground
    [Header("Groundcheck")]
    [SerializeField]
    public Transform groundCheckTransform;

    [SerializeField]
    public LayerMask groundLayerMask;

    [SerializeField]
    public float groundCheckRadius;

    public bool canMove = true;
    public bool canJump = true;
    public bool canTurn = true;

    //Jump related
    public bool isGrounded = true;
    public bool usedDoubleJump = false;

    //Defence related
    public bool isShielding = false;
    public bool usedAirDodge = false;

    //Other
    public bool isCrouching = false;

    //Fetz-Attack related
    public bool isFetzingForward = false;
    public bool isFetzingDown = false;
    public bool isFetzingUp = false;

    //Attack-movement related
    public bool isAttacking = false;



    //Coroutines
    public IEnumerator coroutine;



    public SpriteRenderer sr;

    public void Grab()
    {
        Debug.Log("Grabbed");
    }

    public void TakeDamage(float amount, Vector2 dir, bool fixedKnockback)
    {
        damage = Mathf.Clamp(damage + amount, 0f, 999.9f);

        rb.AddForce(dir * (!fixedKnockback ? damage / (float) weight : 1), ForceMode2D.Impulse);
        canMove = false;

        profile.damageText.text = damage.ToString("0.0") + "%";
        profile.damageText.color = gradient.Evaluate(damage / 1000.0f);
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(amount, Vector2.zero, false);
    }

    public void Die()
    {
        stocks--;
        if (stocks == 0)
        {
            profile.damageText.gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            transform.position = new Vector2(0f, 5f);
            damage = 0;
            profile.damageText.text = damage.ToString("0.0") + "%";
            rb.velocity = new Vector2(0, 0);
        }
        profile.stocks[stocks].gameObject.SetActive(false);
    }

    public void Crouch()
    {
        if (isGrounded)
        {
            if (isShielding)
            {
                Debug.Log("Spot Dodge");
            }
            else
            {
                sr.color = Color.red;
                isCrouching = true;
            }
        }
        else
        {
            rb.gravityScale = 3;
        }
    }

    public void UnCrouch()
    {
        sr.color = Color.white;
        isCrouching = false;
    }

    public void UnShield()
    {
        shield.SetActive(false);
        isShielding = false;
        canMove = true;
    }

    

    

    public void SetGroundcheck()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayerMask);
        if (isGrounded)
        {
            usedDoubleJump = false;
            usedAirDodge = false;
            rb.gravityScale = 2;
        }
        anim.SetBool("IsGrounded", isGrounded);
    }

    public void SetCanMoveToTrue()
    {
        canMove = true;
        canTurn = true;
    }

    public void SetCanMoveToFalse()
    {
        canMove = false;
        canTurn = false;
    }

    public void SetCanJumpToTrue()
    {
        canJump = true;
    }

    public void SetCanJumpToFalse()
    {
        canJump = false;
    }

    public void SetPosition(float pos)
    {
        transform.position = new Vector3(transform.position.x, pos, transform.position.z);

    }

    public void resetAnimation()
    {
        anim.ResetTrigger("FetzingForward");
        anim.ResetTrigger("FetzingDown");
        anim.ResetTrigger("FetzingUp");
    }

    public void SetGravity(int gravity)
    {
        rb.gravityScale = gravity;
    }

    public void SetIsAttacking(int isAttacking)
    {
        this.isAttacking = (isAttacking == 1 ? true : false);
    }







    public void FindAndSetActive(bool active, string name)
    {
        Attack[] attacks = Resources.FindObjectsOfTypeAll<Attack>();

        foreach (Attack att in attacks)
        {
            if (att.name == name)
            {
                att.gameObject.SetActive(active);
                break;
            }
        }
    }

    public void SetActiveHitbox(string name)
    {
        FindAndSetActive(true, name);
    }

    public void SetInActiveHitbox(string name)
    {
        FindAndSetActive(false, name);
    }

    public void StopFetzAttackMethod(float time)
    {

        if (coroutine == null)
        {
            coroutine = StopFetzAttack(time);
            StartCoroutine(coroutine);
        }
    }

    public void StopFetzAttackMethodCoroutine()
    {
        StopCoroutine(coroutine);
        coroutine = null;
    }

    public IEnumerator StopFetzAttack(float time)
    {
        yield return new WaitForSeconds(time);

        anim.SetBool("IsChargingForward", false);
        anim.SetBool("IsChargingDown", false);
        anim.SetBool("IsChargingUp", false);
        isFetzingForward = false;
        isFetzingUp = false;
        isFetzingDown = false;
    }
}
