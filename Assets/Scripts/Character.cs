using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    [Dependency]
    protected Rigidbody2D rb;
    private Animator anim;
    
    
    
    //UI
    public TMPro.TextMeshProUGUI damageAmount;
    public Gradient gradient;





    /*
    [Dependency]
    protected Animator animator;
    */



    [Header("Groundcheck")]
    [SerializeField]
    Transform groundCheckTransform;

    [SerializeField]
    LayerMask groundLayerMask;

    [SerializeField]
    float groundCheckRadius;

    PlayerControls playerControls;
    Vector2 move;
    Vector2 fetz;

    //-------------------------------- Private variables --------------------------------
    //Movement related
    private bool canMove = true;

    //Jump related
    private bool isGrounded = true;
    private bool usedDoubleJump = false;

    //Defence related
    private bool isShielding = false;
    private bool usedAirDodge = false;

    //Other
    private bool isCrouching = false;

    //Fetz-Attack related
    private bool isFetzingForward = false;
    private bool isFetzingDown = false;
    private bool isFetzingUp = false;

    //Attack-movement related
    private bool isAttacking = false;

    //Essentials
    public float damage = 0.1f;
    public int stocks = 3;

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

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        rb.gravityScale = 2;
        rb.freezeRotation = true;

        playerControls = new PlayerControls();

        playerControls.GamePlay.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
        playerControls.GamePlay.Move.canceled += ctx => move = Vector2.zero;

        playerControls.GamePlay.FetzAttack.performed += ctx => fetz = ctx.ReadValue<Vector2>();
        playerControls.GamePlay.FetzAttack.canceled += ctx => fetz = Vector2.zero; isFetzingDown = false; isFetzingUp = false; isFetzingForward = false;

        playerControls.GamePlay.Jump.performed += ctx => Jump();

        playerControls.GamePlay.NeutralAttack.performed += ctx => NeutralAttack();

        playerControls.GamePlay.SpecialAttack.performed += ctx => SpecialAttack();

        playerControls.GamePlay.Crouch.performed += ctx => Crouch();
        playerControls.GamePlay.Crouch.canceled += ctx => UnCrouch();

        playerControls.GamePlay.Shield.performed += ctx => Shield();
        playerControls.GamePlay.Shield.canceled += ctx => UnShield();

        playerControls.GamePlay.Grab.performed += ctx => Grab();

        damageAmount.color = gradient.Evaluate(damage);
    }

    private void NeutralAttack()
    {
        if (Mathf.Abs(move.x) > 0)
        {
            if (isGrounded)
            {
                if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
                {
                    Debug.Log("Side Attack");
                }
                else
                {
                    UpOrDownSpecial("Attack");
                }
            }
            else
            {
                if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
                {
                    if ((move.x > 0 && transform.localScale.x == 1) || (move.x < 0 && transform.localScale.x == -1))
                    {
                        Debug.Log("Forward air");
                    }
                    else if ((move.x > 0 && transform.localScale.x == -1) || (move.x < 0 && transform.localScale.x == 1))
                    {
                        Debug.Log("Back air");
                    }
                }
                else
                {
                    UpOrDownSpecial("Air");
                }
            }
        }
        else
        {
            if (!isGrounded)
            {
                UpOrDownSpecial("Air");
            }
            else
            {
                UpOrDownSpecial("Attack");
            }
        }
    }

    private void SpecialAttack()
    {
        if (Mathf.Abs(move.x) > 0)
        {
            if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
            {
                Debug.Log("Side Special");
            }
            else
            {
                UpOrDownSpecial("special");
            }
        }
        else
        {
            UpOrDownSpecial("special");
        }
    }

    private void UpOrDownSpecial(String attackKind)
    {
        if (move.y < 0)
        {
            Debug.Log("Down " + attackKind);
        }
        else if (move.y > 0)
        {
            Debug.Log("Up " + attackKind);
        }
        else
        {
            Debug.Log("Neutral " + attackKind);
        }
    }

    private void FetzAttack()
    {
        if (isGrounded)
        {
            if (Mathf.Abs(fetz.x) > 0 || Mathf.Abs(fetz.y) > 0)
            {
                if (Mathf.Abs(fetz.x) > Mathf.Abs(fetz.y))
                {
                    if (!isFetzingForward && !isFetzingDown && !isFetzingUp)
                    {
                        isFetzingForward = true;
                        anim.SetTrigger("FetzingForward");
                        if (fetz.x > 0)
                        {
                            transform.localScale = new Vector2(1, 1);
                        } else
                        {
                            transform.localScale = new Vector2(-1, 1);
                        }
                    }
                }
                else
                {
                    if (fetz.y < 0)
                    {
                        if (!isFetzingDown && !isFetzingForward && !isFetzingUp)
                        {
                            isFetzingDown = true;
                            anim.SetTrigger("FetzingDown");
                        }
                    }
                    else if (fetz.y > 0)
                    {
                        if (!isFetzingUp && !isFetzingForward && !isFetzingDown)
                        {
                            isFetzingUp = true;
                            anim.SetTrigger("FetzingUp");
                        }
                    }
                }
            }
        }
        else
        {
            if (Mathf.Abs(fetz.x) > 0 || Mathf.Abs(fetz.y) > 0)
            {
                if (Mathf.Abs(fetz.x) > Mathf.Abs(fetz.y))
                {
                    if ((fetz.x > 0 && transform.localScale.x == 1) || (fetz.x < 0 && transform.localScale.x == -1))
                    {
                        Debug.Log("Forward air");
                    }
                    else if ((fetz.x > 0 && transform.localScale.x == -1) || (fetz.x < 0 && transform.localScale.x == 1))
                    {
                        Debug.Log("Back air");
                    }
                }
                else
                {
                    if (fetz.y < 0)
                    {
                        Debug.Log("Down air");
                    }
                    else if (fetz.y > 0)
                    {
                        Debug.Log("Up air");
                    }
                }
            }

        }
    }



    private void Crouch()
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

    private void UnCrouch()
    {
        sr.color = Color.white;
        isCrouching = false;
    }

    private void Shield()
    {
        if (isGrounded)
        {
            shield.SetActive(true);
            isShielding = true;
            UnCrouch();
        }
        else
        {
            if (!usedAirDodge)
            {
                if (move.x == 0 && move.y == 0)
                {
                    Debug.Log("Neutral airdodge");
                }
                else
                {
                    //Make a calculation for the force added
                    float multiplier = 5;
                    rb.AddForce(move * multiplier, ForceMode2D.Force);
                }
                usedAirDodge = true;
            }
        }
    }

    private void UnShield()
    {
        shield.SetActive(false);
        isShielding = false;
    }

    private void Grab()
    {
        Debug.Log("Grabbed");
    }



    private void FixedUpdate()
    {
        if (canMove)
        {
            Vector2 m = new Vector2(move.x, 0) * Time.fixedDeltaTime * ((isGrounded) ? ((isCrouching) ? crouchSpeed : groundSpeed) : airSpeed);
            transform.Translate(m, Space.World);
        }
    }

    private void Update()
    {
        SetGroundcheck();
        canMove = CheckIfAbleToMove();
        if (canMove)
        {
            anim.SetFloat("MoveX", Mathf.Abs(move.x));
        }
        Flip();
        if (fetz != Vector2.zero)
        {
            FetzAttack();
        } else
        {
            isFetzingForward = false;
            isFetzingUp = false;
            isFetzingDown = false;
        }

        anim.SetBool("IsChargingForward", isFetzingForward);
        anim.SetBool("IsChargingDown", isFetzingDown);
        anim.SetBool("IsChargingUp", isFetzingUp);


        
    }

    private void Flip()
    {
        if ((transform.localScale.x == 1 && move.x < 0) || (transform.localScale.x == -1 && move.x > 0))
        {
            transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
        }
        TakeDamage(.1f);
    }

    private void Jump()
    {
        if (isGrounded || !usedDoubleJump)
        {
            float jumpForce = groundJumpForce;
            if (isGrounded)
            {
                UnShield();
            }
            else
            {
                jumpForce = airJumpForce;
                usedDoubleJump = true;
            }


            rb.velocity = new Vector2(move.x, 0);
            rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
            UnCrouch();
            anim.SetTrigger("Jump");
        }
    }


    private void OnEnable()
    {
        playerControls.GamePlay.Enable();
    }

    private void OnDisable()
    {
        playerControls.GamePlay.Disable();
    }

    private void SetGroundcheck()
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

    private bool CheckIfAbleToMove()
    {
        if (isCrouching)
        {
            if (!canWalkWhileCrouching)
            {
                return false;
            }
        }
        if (isShielding || isAttacking)
        {
            return false;
        }
        return true;
    }

    private void SetPosition(float pos)
    {
        transform.position = new Vector3(transform.position.x, pos, transform.position.z);

    }

    private void resetAnimation()
    {
        anim.ResetTrigger("FetzingForward");
        anim.ResetTrigger("FetzingDown");
        anim.ResetTrigger("FetzingUp");
    }

    private void SetGravity(int gravity)
    {
        rb.gravityScale = gravity;
    }

    private void SetIsAttacking(int isAttacking)
    {
        this.isAttacking = (isAttacking == 1 ? true : false);
    }

    public void Die()
    {
        stocks--;
        if (stocks == 0)
        {

        } else
        {
            transform.position = new Vector2(0f, 5f);
            damage = 0;
            rb.velocity = new Vector2(0, 0);
        }
    }

    public void TakeDamage(float amount, bool hasKnockback)
    {
        damage = Mathf.Clamp(damage + amount, 0f, 999.9f);


        damageAmount.text = damage.ToString("0.0") + "%";
        damageAmount.color = gradient.Evaluate(damage / 1000.0f);
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(amount, true);
    }
}
