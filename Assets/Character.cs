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

    [Dependency]
    protected Animator animator;

    [Dependency]
    protected BoxCollider2D boxCollider;

    

    [Header("Groundcheck")]
    [SerializeField]
    Transform groundCheckTransform;

    [SerializeField]
    LayerMask groundLayerMask;

    [SerializeField]
    float groundCheckRadius;

    PlayerControls playerControls;
    Vector2 move;

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

        rb.gravityScale = 2;
        rb.freezeRotation = true;

        playerControls = new PlayerControls();

        playerControls.GamePlay.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
        playerControls.GamePlay.Move.canceled += ctx => move = Vector2.zero;

        playerControls.GamePlay.Jump.performed += ctx => Jump();

        playerControls.GamePlay.NeutralAttack.performed += ctx => NeutralAttack();

        playerControls.GamePlay.SpecialAttack.performed += ctx => SpecialAttack();

        playerControls.GamePlay.Crouch.performed += ctx => Crouch();
        playerControls.GamePlay.Crouch.canceled += ctx => UnCrouch();

        playerControls.GamePlay.Shield.performed += ctx => Shield();
        playerControls.GamePlay.Shield.canceled += ctx => UnShield();

        playerControls.GamePlay.Grab.performed += ctx => Grab();
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
            } else
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
                } else
                {
                    UpOrDownSpecial("Air");
                }
            }
        } else
        {
            if (!isGrounded)
            {
                UpOrDownSpecial("Air");
            } else
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

    private void Crouch()
    {
        if (isGrounded)
        {
            sr.color = Color.red;
            isCrouching = true;
        } else
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
        } else
        {
            if (!usedAirDodge)
            {
                if (move.x == 0 && move.y == 0)
                {
                    Debug.Log("Neutral airdodge");
                } else
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
    }

    private void Jump()
    {
        if (isGrounded || !usedDoubleJump)
        {
            float jumpForce = groundJumpForce;
            if (isGrounded)
            {
                
            } else
            {
                jumpForce = airJumpForce;
                usedDoubleJump = true;
            }

            
            rb.velocity = new Vector2(move.x, 0);
            rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
            UnCrouch();
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
        if (isShielding)
        {
            return false;
        }
        return true;
    }
}
