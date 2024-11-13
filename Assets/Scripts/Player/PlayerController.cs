using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cinemachine;
using PathCreation;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Variables")]
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _runSpeed = 10f;
    [SerializeField] float _rotationSpeed = 10f;
    [SerializeField] float _dashSpeed = 30;
    [SerializeField] float _dashDelay = 2f;
    [SerializeField] bool _enableMovementsShowcase = false;
    bool _canDash = false;

    [Header("Jump Variables")]
    [SerializeField] int _jumpMaxNumber = 2;
    [SerializeField] float _initialJumpVelocity = 0;
    [SerializeField] float _maxJumpHeight = 4;
    [SerializeField] float _maxJumpTime = 0.75f;
    [SerializeField] float _fallSpeedMult = 2f;
    [SerializeField] float _maxFallSpeed = -20;
    int _jumpsLeft = 0;

    [Header("Gravity")]
    [SerializeField] float _gravity = -9.81f;
    [SerializeField] float _groundGravity = -0.05f;

    [Header("Combat")]
    [SerializeField] float _MAX_HEALTH;
    [SerializeField] float _currentHealth;
    [SerializeField] float _combatTriggeredRadius = 15.0f;
    [SerializeField] LayerMask _combatMask;

    [Header("Debug States")]
    [SerializeField] bool _inCinematic = false;
    [SerializeField] bool _inCombat = false;
    [SerializeField] bool _isOnSpline = false;
    bool _isMoving = false;
    bool _isRunning = false;
    bool _isAttacking = false;
    bool _canDealDamage = false;
    bool _isJumping = false;
    bool _isFalling = false;
    bool _isDashing = false;
    bool _inMenu = false;
    bool _hasTarget = false;

    [Header("Spline")]
    [SerializeField] PathCreator _splinePath;
    [SerializeField] EndOfPathInstruction _splineEnd;
    float _distanceTraveled;

    [Header("Holster Weapon SphereCheck")]
    [SerializeField] LayerMask _buttonMask;
    [SerializeField] float _sphereRadius = 2.5f;
    Weapon _weapon;
    SkinnedMeshRenderer _weaponMeshRenderer;

    [Header("UI")]
    [SerializeField] GameObject _canvas;

    bool _dashActive = false;
    bool _multiJumpActive = false;
    bool _sprintActive = false;

    float _dashDuration = 0f;
    float _multiJumpDuration = 0f;
    float _sprintDuration = 0f;

    float _dashActivationTime = 0f;
    float _multiJumpActivationTime = 0f;
    float _sprintActivationTime = 0f;

    CharacterController _playerCC;
    PlayerInputAction _playerInputActions;
    AnimationController _playerAnimationController;
    Animator _playerAnimator;

    Text _menuText;
    Text _menuInputText;
    Image _menuBackground;

    Text _dashTimerText;
    Image _dashActiveIcon;

    Text _sprintTimerText;
    Image _sprintActiveIcon;

    Text _multiJumpTimerText;
    Image _multiJumpActiveicon;

    Text _healthText;

    Text _gameOverText;

    Vector3 _moveVector;
    Vector3 _appliedMove;
    Vector3 _localAppliedMove;
    Vector2 _leftStickInput;
    Vector2 _rightStickInput;
    bool _jumpButtonPressed = false;

    float _sloperRayLength = 2f;
    RaycastHit _sloperHit;
    
    private void Awake()
    {
        _playerCC = GetComponent<CharacterController>();
        _playerAnimationController = GetComponent<AnimationController>();
        _playerAnimator = GetComponent<Animator>();
        _jumpsLeft = _jumpMaxNumber;
        _weapon = gameObject.GetComponentInChildren<Weapon>();
        _weaponMeshRenderer = _weapon.GetComponent<SkinnedMeshRenderer>();
        SetUpJumpParabola();
        SetUpInputs();
        SetUpCanvas();
        IsInMenu = true;
        InputMapSelector(IsInMenu);
        ToggleMenu(IsInMenu);
        _currentHealth = _MAX_HEALTH;
    }

    private void Update()
    {
        Death();
        SetForwardMovement();
        MovementShowcase();
        HandlePowerUps();
        HandleTimers();
        ResetRun();   
        Move(_leftStickInput);
        PrintHealth();
        TriggerCombat();
        //Debug.Log(InCombat);
    }

    private void FixedUpdate()
    {
        HolsterWeapon();
        IsOnClimbableSloper();
        ApplyGravity();
        ResetJump();
    }

    private void LateUpdate()
    {
        RotateCamera(_rightStickInput);
    }

    private void OnTriggerEnter(Collider other)
    {
        ActivatePowerUps(other);   
    }

    // Getters & Setters
    public bool IsMoving { get { return _isMoving; } set { _isMoving = value; } }
    public bool IsAttacking { get { return _isAttacking; } set { _isAttacking = value; } }
    public bool CanDealDamage { get { return _canDealDamage; } set { _canDealDamage = value; } }
    public bool IsRunning { get { return _isRunning; } set { _isRunning = value; } }
    public bool IsJumping { get { return _isJumping; } set { _isJumping = value; } }
    public bool IsDashing { get { return _isDashing; } set { _isDashing = value; } }
    public bool IsInMenu { get { return _inMenu; } set { _inMenu = value; } }
    public bool InCinematic { get { return _inCinematic; } set { _inCinematic = value; } }
    public bool InCombat { get { return _inCombat; } set { _inCombat = value; } }
    public bool HasTarget { get { return _hasTarget; } set { _hasTarget = value; } }
    public bool IsOnSpline { get { return _isOnSpline; } set {_isOnSpline = value; } }
    public float DistanceTraveled { get { return _distanceTraveled; } set { _distanceTraveled = value; } }

    // Inputs
    private void SetUpInputs()
    {
        _playerInputActions = new PlayerInputAction();

        // Gameplay Map
        _playerInputActions.Gameplay.Move.performed += OnMove;
        _playerInputActions.Gameplay.Look.performed += OnRotate;
        _playerInputActions.Gameplay.Action.started += OnAction;
        _playerInputActions.Gameplay.Attack.started += OnAttack;
        _playerInputActions.Gameplay.Jump.started += OnJump;
        _playerInputActions.Gameplay.Menu.started += OnMenu;
        _playerInputActions.Gameplay.Sprint.started += OnRun;

        _playerInputActions.Gameplay.Move.canceled += OnMove;
        _playerInputActions.Gameplay.Look.canceled += OnRotate;
        _playerInputActions.Gameplay.Jump.canceled += OnJump;
        _playerInputActions.Gameplay.Sprint.canceled += OnRun;

        // Menu Map
        _playerInputActions.Menu.Confirm.started += OnConfirm;
        _playerInputActions.Menu.Exit.started += OnMenu;
        _playerInputActions.Menu.Move.performed += OnMove;
        _playerInputActions.Menu.Move.canceled += OnMove;
    }
    private void OnMove(InputAction.CallbackContext context)
    {
        _leftStickInput = context.ReadValue<Vector2>();
    }
    private void OnRotate(InputAction.CallbackContext context)
    {
        _rightStickInput = context.ReadValue<Vector2>();
    }
    private void OnAction(InputAction.CallbackContext context)
    {
        if (_dashActive)
        {
            if (_canDash)
            {
                _canDash = false;
                Dash();
                StartCoroutine(DashDelay());
            }
        }
    }
    private void OnAttack(InputAction.CallbackContext context)
    {
        Attack();
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        _jumpButtonPressed = context.ReadValueAsButton();

        if (_jumpButtonPressed)
        {
            if (_multiJumpActive)
            {
                MultiJump();
            }
            else
            {
                Jump();
            }
        }
    }
    private void OnMenu(InputAction.CallbackContext context)
    {
        IsInMenu = !IsInMenu;
        
        ToggleMenu(IsInMenu);
        InputMapSelector(IsInMenu);
    }
    private void OnRun(InputAction.CallbackContext context)
    {
        if (_sprintActive)
        {
            IsRunning = context.ReadValueAsButton();
        }
    }
    private void OnConfirm(InputAction.CallbackContext context)
    {
        Debug.Log("HL3 Confirmed");
    }

    // Actions
    private void ApplyGravity()
    {
        // Velocity Verlet Integration

        _isFalling = _moveVector.y <= 0 || !_jumpButtonPressed;

        if (_playerCC.isGrounded)
        {
            _moveVector.y = _groundGravity;
            _appliedMove.y = _groundGravity;
            _isFalling = false;
        }
        else if (_isFalling)
        {
            float prevYVel = _moveVector.y;
            _moveVector.y = _moveVector.y + (_gravity * _fallSpeedMult * Time.deltaTime);
            _appliedMove.y = Mathf.Max((prevYVel + _moveVector.y) * 0.5f, _maxFallSpeed);
        }
        else
        {
            float prevYVel = _moveVector.y;
            _moveVector.y = _moveVector.y + (_gravity * Time.deltaTime);
            _appliedMove.y = Mathf.Max((prevYVel + _moveVector.y) * 0.5f, _maxFallSpeed);
        }
    }
    private void Move(Vector2 movementInput)
    {
        if (!InCinematic && !IsInMenu)
        {
            if (movementInput != Vector2.zero)
            {
                IsMoving = true;
            }
            else
            {
                IsMoving = false;
            }

            SetUpMovementVectors(movementInput);
            _localAppliedMove = transform.TransformDirection(_appliedMove);
            if (IsOnSpline)
            {
                transform.position = _splinePath.path.GetPointAtDistance(_distanceTraveled, _splineEnd);
                transform.rotation = _splinePath.path.GetRotationAtDistance(_distanceTraveled, _splineEnd);

                /*_splineMovement = new Vector3(_splinePath.path.GetPoint(Mathf.FloorToInt(_distanceTraveled)).x, _localAppliedMove.y, _localAppliedMove.z);
                _playerCC.Move(_splineMovement * Time.deltaTime);*/
            }
            else
            {
                _playerCC.Move(_localAppliedMove * Time.deltaTime);
            }
            AnimateMovement(movementInput);
        }
    }
    private void SetUpMovementVectors(Vector2 input)
    {
        if (IsOnSpline)
        {
            _distanceTraveled += (IsRunning ? _leftStickInput.x * _runSpeed : _leftStickInput.x * _moveSpeed) * Time.deltaTime;
            _moveVector.x = 0;
            _moveVector.z = IsRunning ? input.x * _runSpeed : input.x * _moveSpeed;
        }
        else
        {
            _moveVector.x = IsRunning ? input.x * _runSpeed : input.x * _moveSpeed;
            _moveVector.z = IsRunning ? input.y * _runSpeed : input.y * _moveSpeed;
        }
        _appliedMove.x = _moveVector.x;
        _appliedMove.z = _moveVector.z;
    }
    private void RotateCamera(Vector2 rotateInput)
    {
        if (!InCinematic && !IsInMenu && !IsOnSpline)
        {
            transform.RotateAround(transform.position, new Vector3(0, rotateInput.x, 0), _rotationSpeed * Time.deltaTime);
        }
    }
    private void Jump()
    {
        if (!InCinematic && !IsInMenu)
        {
            if (_playerCC.isGrounded && _jumpsLeft > 0)
            {
                float animDuration = _playerAnimationController.PlayerAnimationDuration;
                IsJumping = true;
                _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.IsJumpingHash, IsJumping);
                StartCoroutine(JumpAnimDelay(animDuration));
                _moveVector.y = _initialJumpVelocity;
                _appliedMove.y = _initialJumpVelocity;
                _jumpsLeft -= 1;
            }
            
        }
    }
    IEnumerator JumpAnimDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay / 2);
        _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.IsJumpingHash, false);
    }
    private void MultiJump()
    {
        if (!InCinematic && !IsInMenu)
        {
            if (_leftStickInput.y > 0)
            {
                _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.ForwardHash, true);
            }
            else
            {
                _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.ForwardHash, false);
            }
            if (_playerCC.isGrounded && _jumpsLeft > 0)
            {
                IsJumping = true;
                _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.IsJumpingHash, IsJumping);
                _moveVector.y = _initialJumpVelocity;
                _appliedMove.y = _initialJumpVelocity;
                _jumpsLeft -= 1;
            }
            else if (!_playerCC.isGrounded && IsJumping && _jumpsLeft > 0)
            {
                _moveVector.y = _initialJumpVelocity;
                _appliedMove.y = _initialJumpVelocity;
                _jumpsLeft -= 1;
                //_playerAnimationController.ResetAnimation("JumpForward");
                _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.IsJumpingHash, true);
            }
        }
    }
    private void ResetJump()
    {
        if (_playerCC.isGrounded && IsJumping)
        {
            IsJumping = false;
            _jumpsLeft = _jumpMaxNumber;
            _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.IsJumpingHash, IsJumping);
        }
    }
    private void SetUpJumpParabola()
    {
        float timeToApex;
        timeToApex = _maxJumpTime / 2;
        _gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVelocity = (2 * _maxJumpHeight) / timeToApex;
    }
    private void ResetRun()
    {
        if (!_sprintActive)
        {
            IsRunning = false;
        }
    }
    private void Dash()
    {
        if (!InCinematic && !IsInMenu)
        {
            if (_leftStickInput != Vector2.zero)
            {
                Vector3 dashDirection;
                dashDirection = new Vector3(_localAppliedMove.x, 0, _localAppliedMove.z);
                _playerCC.Move(_dashSpeed * Time.deltaTime * dashDirection.normalized);
            }
            else
            {
                _playerCC.Move(_dashSpeed * Time.deltaTime * transform.forward);
            }           
        }
    }
    IEnumerator DashDelay()
    {
        yield return new WaitForSecondsRealtime(_dashDelay);
        _canDash = true;
    }
    private bool IsOnClimbableSloper()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out _sloperHit, _playerCC.height/2 * _sloperRayLength)) 
        {
            if (!IsOnSpline)
            {
                if (Vector3.Angle(Vector3.up, _sloperHit.normal) > _playerCC.slopeLimit)
                {
                    Debug.Log("I should slide down");
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        return false;
    }
    private void HolsterWeapon()
    {
        if (!InCombat)
        {
            if (Physics.CheckSphere(transform.localPosition + _playerCC.center, _sphereRadius, _buttonMask))
            {
                //Debug.Log("BB Weapon");
                _weaponMeshRenderer.enabled = false;
            }
            else
            {
                //Debug.Log("WB Weapon");
                _weaponMeshRenderer.enabled = true;
            }
        }
        
    }
    private void MovementShowcase()
    {
        if (_enableMovementsShowcase)
        {
            _multiJumpActive = true;
            _multiJumpDuration = Mathf.Infinity;

            _sprintActive = true;
            _sprintDuration = Mathf.Infinity;

            _dashActive = true;
            _canDash = true;
            _dashDuration = Mathf.Infinity;

        }
    }
    private void AnimateMovement(Vector2 movementInput)
    {
        if (IsOnSpline)
        {
            _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.WalkingSpeedHash, Mathf.Abs(movementInput.x));
            _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.IsWalkingHash, IsMoving);
        }
        else
        {
            _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.WalkingSpeedHash, Mathf.Abs(movementInput.y));
            _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.IsWalkingHash, IsMoving);
        }
    }
    private void SetForwardMovement()
    {
        if (_leftStickInput.y > 0 && !IsOnSpline)
        {
            _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.ForwardHash, true);
        }
        else if (_leftStickInput.y < 0 && !IsOnSpline)
        {
            _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.ForwardHash, false);
        }
        else if (_leftStickInput.x > 0 && IsOnSpline)
        {
            _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.ForwardHash, true);
        }
        else if (_leftStickInput.x < 0 && IsOnSpline)
        {
            _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.ForwardHash, false);
        }
        else
        {
            _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.ForwardHash, false);
        }
    }

    // Combat
    public void Attack()
    {
        if (!InCinematic && !IsInMenu)
        {
            float animDuration = _playerAnimationController.PlayerAnimationDuration;

            IsAttacking = true;
            CanDealDamage = true;

            _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.IsAttackingHash, IsAttacking);
            StartCoroutine(AttackDelay(animDuration));
            StartCoroutine(DamageDelay(animDuration));
        }

    }
    IEnumerator AttackDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay / 2);
        IsAttacking = false;
        _playerAnimationController.SetAnimationState(_playerAnimator, _playerAnimationController.IsAttackingHash, IsAttacking);
    }
    IEnumerator DamageDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay / 3);
        CanDealDamage = false;
    }
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        Debug.Log("Please Git Gud, I'm suffering down here");
    }
    private void TriggerCombat()
    {
        if (Physics.CheckSphere(transform.localPosition + _playerCC.center, _combatTriggeredRadius, _combatMask))
        {
            Collider[] enemies;
            int count = 0;
            enemies = Physics.OverlapSphere(transform.localPosition + _playerCC.center, _combatTriggeredRadius, _combatMask);

            foreach (Collider enemy in enemies)
            {
                if (enemy.GetComponentInParent<Enemy>().EnemyState == Enemy.States.Chasing)
                {
                    InCombat = true;
                }
                else
                {
                    count += 1;
                }

                if (count == enemies.Length)
                {
                    InCombat = false;
                }
            }
        }
    }
    private void Death()
    {
        if (_currentHealth <= 0)
        {
            GameOverScreen();
            Destroy(gameObject);
        }
    }

    // Power Ups
    private void ActivatePowerUps(Collider collider)
    {
        if (collider.CompareTag("PickUp"))
        {
            switch (collider.gameObject.GetComponent<PickupBase>().ActivePower)
            {
                case PickupBase.PowerUps.MultiJump:
                    _multiJumpActivationTime = Time.time;
                    _multiJumpActive = true;
                    _multiJumpDuration = collider.GetComponent<PickupBase>().PowerDuration;
                    TogglePowerIcon(_multiJumpActiveicon, _multiJumpTimerText, _multiJumpActive);
                    break;
                case PickupBase.PowerUps.Sprint:
                    _sprintActivationTime = Time.time;
                    _sprintActive = true;
                    _sprintDuration = collider.GetComponent<PickupBase>().PowerDuration;
                    TogglePowerIcon(_sprintActiveIcon, _sprintTimerText, _sprintActive);
                    break;
                case PickupBase.PowerUps.Dash:
                    _dashActivationTime = Time.time;
                    _dashActive = true;
                    _canDash = true;
                    _dashDuration = collider.GetComponent<PickupBase>().PowerDuration;
                    TogglePowerIcon(_dashActiveIcon, _dashTimerText, _dashActive);
                    break;
                default:
                    break;
            }
        }
    }
    private void HandlePowerUps()
    {
        _multiJumpActive = DisablePowerUp(_multiJumpActive, _multiJumpActivationTime, _multiJumpDuration, _multiJumpActiveicon, _multiJumpTimerText);
        _dashActive = DisablePowerUp(_dashActive, _dashActivationTime, _dashDuration, _dashActiveIcon, _dashTimerText);
        _sprintActive = DisablePowerUp(_sprintActive, _sprintActivationTime, _sprintDuration, _sprintActiveIcon, _sprintTimerText);
    }
    private bool DisablePowerUp(bool power, float activationTime, float powerDuration, Image powerIcon, Text powerText)
    {
        if (Time.time >= activationTime + powerDuration)
        {
            power = false;
            TogglePowerIcon(powerIcon, powerText, power);
        }
        return power;
    }
    private void HandleTimers() 
    {
        if (_dashActive)
        {
            TimerPrinter(_dashActivationTime, _dashDuration, _dashTimerText, "Dash: ");
        }
        if (_multiJumpActive)
        {
            TimerPrinter(_multiJumpActivationTime, _multiJumpDuration, _multiJumpTimerText, "MultiJump: ");
        }
        if (_sprintActive)
        {
            TimerPrinter(_sprintActivationTime, _sprintDuration, _sprintTimerText, "Sprint: ");
        }
    }
    private void TimerPrinter(float activationTime, float duration, Text text, string str)
    {
        float timer;
        timer = activationTime + duration - Time.time;
        text.text = (str + (timer)).ToString();
    }

    // UI
    private void SetUpCanvas()
    {
        _menuBackground = GameObject.Find("MenuBG").GetComponent<Image>();
        _menuText = GameObject.Find("MenuText").GetComponent<Text>();
        _menuInputText = GameObject.Find("InputText").GetComponent<Text>();

        _dashTimerText = GameObject.Find("DashTimerText").GetComponent<Text>();
        _dashActiveIcon = GameObject.Find("DashBG").GetComponent<Image>();

        _sprintTimerText = GameObject.Find("SprintTimerText").GetComponent<Text>();
        _sprintActiveIcon = GameObject.Find("SprintBG").GetComponent<Image>();

        _multiJumpTimerText = GameObject.Find("DoubleJumpTimerText").GetComponent<Text>();
        _multiJumpActiveicon = GameObject.Find("DoubleJumpBG").GetComponent<Image>();

        _healthText = GameObject.FindWithTag("HealthText").GetComponent<Text>();

        _gameOverText = GameObject.Find("GameOverText").GetComponent<Text>();

        _menuBackground.enabled = false;
        _menuText.enabled = false;
        _menuInputText.enabled = false;

        _dashTimerText.enabled = false;
        _dashActiveIcon.enabled = false;

        _sprintTimerText.enabled = false;
        _sprintActiveIcon.enabled = false;

        _multiJumpTimerText.enabled = false;
        _multiJumpActiveicon.enabled = false;

        _gameOverText.enabled = false;
    }
    private void TogglePowerIcon(Image bg, Text text, bool value)
    {
        bg.enabled = value;
        text.enabled = value;
    }
    private void ToggleMenu(bool value)
    {
        _menuBackground.enabled = value;
        _menuText.enabled = value;
        _menuInputText.enabled = value;
    }
    private void InputMapSelector(bool menuState)
    {
        if (menuState)
        {
            _playerInputActions.Gameplay.Disable();
            _playerInputActions.Menu.Enable();
        }
        else
        {
            _playerInputActions.Menu.Disable();
            _playerInputActions.Gameplay.Enable();
        }
    }
    private void PrintHealth()
    {
        _healthText.text = "Health: " + _currentHealth.ToString();
    }
    private void GameOverScreen()
    {
        _canvas.transform.parent = null;
        _dashTimerText.enabled = false;
        _multiJumpTimerText.enabled = false;
        _sprintTimerText.enabled = false;
        _menuText.enabled = false;
        _menuInputText.enabled = false;
        _menuBackground.enabled = true;
        _gameOverText.enabled = true;
    }

    // Gizmos
    /*  private void OnDrawGizmos()
      {
          if (Physics.CheckSphere(transform.localPosition + _playerCC.center, _sphereRadius, _buttonMask))
          {
              Gizmos.color = Color.green;
              Gizmos.DrawRay(transform.position + _playerCC.center, transform.forward * _buttonHit.distance);
              Gizmos.DrawWireSphere(transform.localPosition + _playerCC.center, _sphereRadius);
          }
          else
          {
              Gizmos.color = Color.red;
              Gizmos.DrawRay(transform.position + _playerCC.center, transform.forward * _buttonHit.distance);
              Gizmos.DrawWireSphere(transform.localPosition + _playerCC.center, _sphereRadius);
          }
      }*/
}
