using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    public Vector3 revivePosition = new Vector3(0, 1.72f, 0);
    private Vector3 previousPosition;
    private const float updateInterval = 2.0f; // Interval for updating the server in seconds
    private float timer = 0.0f;

    //input field
    private ThirdPersonInputActions inputActions;
    private InputAction move;
    //movement field
    private Rigidbody rb;
    [SerializeField] private float movementForce = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float maxSpeed = 5f;
    private Vector3 forceDirection = Vector3.zero;

    [SerializeField] private Camera playerCamera;

    private bool isAttacking = false;
    private bool isDead = false;
    
    private Animator animator;

    //health system
    public float maxHealth = 100f;
    private float currentHealth;

    public HealthBar healthBar;
    private UIManager uiManager;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new ThirdPersonInputActions();
        animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        inputActions.Player.Jump.started += DoJump;
        inputActions.Player.Attack.started += DoAttack;
        move = inputActions.Player.Move;
        inputActions.Player.Enable();
    }
    private void OnDisable()
    {
        inputActions.Player.Jump.started -= DoJump;
        inputActions.Player.Attack.started -= DoAttack;
        inputActions.Player.Disable();
    }
    private void FixedUpdate()
    {
        if (isDead) return;
        forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(playerCamera) * movementForce;
        forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCamera) * movementForce;

        rb.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        if(rb.velocity.y < 0f)
        {
            rb.velocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime;
        }
        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0f;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.velocity = (horizontalVelocity.normalized * maxSpeed) + (Vector3.up * rb.velocity.y);
        }
        LookAt();
    }
    private void LookAt()
    {
        Vector3 direction = rb.velocity;
        direction.y = 0f;
        if(move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
            this.rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        else
            rb.angularVelocity = Vector3.zero;
    }
    private Vector3 GetCameraForward(Camera playerCamera)
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera playerCamera)
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        if (IsGrounded())
        {
            forceDirection += Vector3.up * jumpForce;
        }
    }
    private bool IsGrounded()
    {
        Ray ray = new Ray(this.transform.position + Vector3.up * 0.25f, Vector3.down);
        if(Physics.Raycast(ray, out RaycastHit hit, 0.3f))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void DoAttack(InputAction.CallbackContext obj)
    {
        isAttacking = true;
        animator.SetTrigger("attack");
        StartCoroutine(ResetAttackState());
    }
    private IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }
    public bool IsAttacking()
    {
        return isAttacking;
    }
    private void Start()
    {
        previousPosition = transform.position;
        //currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        HealthSystemManager.Instance.RegisterPlayer(currentHealth);
        uiManager = FindObjectOfType<UIManager>();
    }

    void Update()
    {
        if (isDead) return;

        timer += Time.deltaTime;
        if (previousPosition != transform.position && timer >= updateInterval)
        {
            timer = 0.0f;
            previousPosition = transform.position;
            ServerTalker.Instance.PostCharacterPosition(transform.position);
        }
        if (transform.position.y < -10)
        {
            Revive();
        }
    }
    private void Revive() 
    {
        //set transform = revivePosition
        transform.position = revivePosition;
        ServerTalker.Instance.PostCharacterPosition(transform.position);
        previousPosition = transform.position;
    }
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        else if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        healthBar.SetHealth(currentHealth);
        HealthSystemManager.Instance.UpdatePlayerHealth(currentHealth);
        ServerTalker.Instance.PostPlayerHealth(currentHealth);
        DamageTextManager.instance.ShowDamageText(damage, transform.position);
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Heal(float amount)
    {
        if (currentHealth <= 0) 
        {
            return;
        } 
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        healthBar.SetHealth(currentHealth);
        HealthSystemManager.Instance.UpdatePlayerHealth(currentHealth);
        ServerTalker.Instance.PostPlayerHealth(currentHealth);
    }
    private void Die()
    {
        if (isDead) return;
        isDead = true;
        // Handle player death
        animator.SetBool("isDead", true);
        rb.velocity = Vector3.zero; //stop player movement
        rb.isKinematic = true;
        //show game over
        uiManager.ShowGameOverPanel();
        ServerTalker.Instance.ResetGameData();
        ServerTalker.Instance.PostPlayerHealth(100);
    }
    public void SetCurrentHealth(float health)
    {
        currentHealth = health;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        healthBar.SetHealth(currentHealth);
        HealthSystemManager.Instance.UpdatePlayerHealth(currentHealth);
    }
    public void BackToLife()
    {
        PlayerController playerController = FindAnyObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.DestroyCurrentAssets();
        }
        ResourceManager.Instance.ResetResources();
        ScoreManager.instance.ResetScore();
        isDead = false;
        currentHealth = maxHealth;
        SetCurrentHealth(currentHealth);
        animator.SetBool("isDead", false);
        animator.SetFloat("speed", 0f);
        rb.isKinematic = false;
        transform.position = revivePosition;
        ServerTalker.Instance.PostCharacterPosition(transform.position);
        ServerTalker.Instance.PostPlayerHealth(currentHealth);
    }
}
