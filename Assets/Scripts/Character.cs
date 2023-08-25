using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Character : Playable
{
    PlayerControls playerControls;
    Vector2 move;
    Vector2 fetz;

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
                        if (canTurn)
                        {
                            if (fetz.x > 0)
                            {
                                transform.localScale = new Vector2(1, 1);
                            }
                            else
                            {
                                transform.localScale = new Vector2(-1, 1);
                            }
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



    

    

    private void Shield()
    {
        if (isGrounded)
        {
            shield.SetActive(true);
            isShielding = true;
            canMove = false;
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
        if (canMove)
        {
            anim.SetFloat("MoveX", Mathf.Abs(move.x));
        }
        if (canTurn)
        {
            Flip();
        }
        if (fetz != Vector2.zero)
        {
            FetzAttack();
        }
        else
        {
            isFetzingForward = false;
            isFetzingUp = false;
            isFetzingDown = false;
        }

        anim.SetBool("IsChargingForward", isFetzingForward);
        anim.SetBool("IsChargingDown", isFetzingDown);
        anim.SetBool("IsChargingUp", isFetzingUp);



    }

    public void Jump()
    {
        if ((isGrounded || !usedDoubleJump) && canJump)
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


    private void Flip()
    {
        if ((transform.localScale.x == 1 && move.x < 0) || (transform.localScale.x == -1 && move.x > 0))
        {
            transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
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
}