using System.Collections;
using System.Data.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    BoxCollider2D FeetCollider;
    Animator myanimator;
    CapsuleCollider2D BodyCollider;
    [SerializeField] TrailRenderer dashTrail;
    [SerializeField] LayerMask DamageableLayer;
    float fallSpeedDampingChangeThreshold;

    #region Gravity
    [SerializeField] float baseGravity = 2f;
    [SerializeField] float maxFallSpeed = 20f;
    [SerializeField] float gravityMultiplier = 2.5f;
    #endregion

    #region Dashing
    bool canDash = true;
    bool isDashing;
    float dashingCooldown = 1f;
    [SerializeField] float dashingPower = 24f;
    [SerializeField] float dashingTime = 0.2f;
    #endregion

    #region  Inputs
    Vector2 MoveInput;
    bool jumpPressed;
    #endregion

    #region Movement Variables
    [SerializeField] float moveSpeed = 9f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float coyoteTime = 0.15f;
    [SerializeField] float jumpBufferTime = 0.1f;
    [SerializeField] float acceleration = 9f;
    [SerializeField] float decceleration = 9f;
    [SerializeField] float velPower = 1.2f;
    
    float coyoteCounter;
    float jumpBufferCounter;
    #endregion

    #region Attack
    bool isAttacking = false;
    float timeBetweenAttacks, timeSinceLastAttack;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        dashTrail = GetComponent<TrailRenderer>();
        myanimator = GetComponent<Animator>();
        BodyCollider = GetComponent<CapsuleCollider2D>();
        FeetCollider = GetComponent<BoxCollider2D>();

        fallSpeedDampingChangeThreshold = CameraManager.instance.fallSpeedDampingChangeThreshold;
    }

    void Update()
    {  
        if (isDashing) return;
        UpdateCoyoteTime();
        UpdateJumpBuffer();
        Run();
        FlipSprite();
        Gravity();
        if (myanimator != null)
        {
            myanimator.SetBool("isJumping", !IsGrounded());
        }

        if (rb.linearVelocity.y < fallSpeedDampingChangeThreshold && !CameraManager.instance.IsLerpingYDamping && !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }
        else if (rb.linearVelocity.y >= 0f && CameraManager.instance.LerpedFromPlayerFalling && !CameraManager.instance.IsLerpingYDamping)
        {
            CameraManager.instance.LerpedFromPlayerFalling = false;
            CameraManager.instance.LerpYDamping(false);
        }
    }

    IEnumerator Dash(){
        canDash = false;
        isDashing = true;
        if (myanimator != null) myanimator.SetBool("isDashing", true);
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        dashTrail.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        dashTrail.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        if (myanimator != null) myanimator.SetBool("isDashing", false);
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }


    void OnAttack(InputValue value)
    {
        bool isAttacking = value.isPressed;
        timeSinceLastAttack += Time.deltaTime;
        if (isAttacking && timeSinceLastAttack >= timeBetweenAttacks){
            timeSinceLastAttack = 0f;
            myanimator.SetTrigger("attack");
        }
    }

    void OnDash(InputValue value){
        if(value.isPressed && canDash){
            StartCoroutine(Dash());
        }
    }

    public void Gravity()
    {   
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = baseGravity * gravityMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
        else if (rb.linearVelocity.y > 0 && jumpPressed == false)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        jumpPressed = value.isPressed;
        if(jumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        
        if((IsGrounded() || coyoteCounter > 0) && jumpBufferCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (myanimator != null) myanimator.SetBool("isJumping", true);
            jumpBufferCounter = 0;
            coyoteCounter = 0;
        }

    }

    void UpdateCoyoteTime()
    {
        if(IsGrounded())
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }
    }

    void UpdateJumpBuffer()
    {
        jumpBufferCounter -= Time.deltaTime;
    }


    void Run()
    {
        float targetSpeed = MoveInput.x * moveSpeed;
        float speedDif = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right);
        bool hasHorizontalSpeed = Mathf.Abs(rb.linearVelocity.x) > Mathf.Epsilon;
        if (hasHorizontalSpeed)
        {
            myanimator.SetBool("isRunning", true);
        }else{
            myanimator.SetBool("isRunning", false);            
        }
    }

        void FlipSprite()
    {
        bool hasHorizontalSpeed = Mathf.Abs(rb.linearVelocity.x) > Mathf.Epsilon;
        if (hasHorizontalSpeed){
            transform.localScale = new Vector2(Mathf.Sign(rb.linearVelocity.x), 1f);
        }
    }

        bool IsGrounded(){
            if (FeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"))){
                return true;
        } else{
            return false;
            }
        }

}