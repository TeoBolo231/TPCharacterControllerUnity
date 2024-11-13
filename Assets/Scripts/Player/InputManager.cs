using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header ("Input Debug")]
    [SerializeField] Vector2 _leftStickInput;
    [SerializeField] Vector2 _rightStickInput;
    [SerializeField] bool _northButtonPressed;
    [SerializeField] bool _southButtonPressed;
    [SerializeField] bool _eastButtonPressed;
    [SerializeField] bool _westButtonPressed;
    [SerializeField] bool _leftShoulderPressed;
    [SerializeField] bool _dpadRightPressed;
    [SerializeField] bool _dpadLeftPressed;

    PlayerInputAction _playerInputActions;

    private void Awake()
    {
        SetUpInputs();
    }

    // SetUp Functions
    private void SetUpInputs()
    {
        _playerInputActions = new PlayerInputAction();
        _playerInputActions.Gameplay.Enable();

        // Gameplay Map
        _playerInputActions.Gameplay.Move.performed += OnLeftStick;
        _playerInputActions.Gameplay.Look.performed += OnRightStick;
        _playerInputActions.Gameplay.Action.started += OnEastButton;
        _playerInputActions.Gameplay.Attack.started += OnWestButton;
        _playerInputActions.Gameplay.Jump.started += OnSouthButton;
        _playerInputActions.Gameplay.Menu.started += OnNorthButton;
        _playerInputActions.Gameplay.Sprint.started += OnLeftShoulder;
        _playerInputActions.Gameplay.TurnCameraR.started += OnDpadRight;
        _playerInputActions.Gameplay.TurnCameraL.started += OnDpadLeft;

        _playerInputActions.Gameplay.Move.canceled += OnLeftStick;
        _playerInputActions.Gameplay.Look.canceled += OnRightStick;
        _playerInputActions.Gameplay.Action.canceled += OnEastButton;
        _playerInputActions.Gameplay.Attack.canceled += OnWestButton;
        _playerInputActions.Gameplay.Jump.canceled += OnSouthButton;
        _playerInputActions.Gameplay.Menu.canceled += OnNorthButton;
        _playerInputActions.Gameplay.Sprint.canceled += OnLeftShoulder;
        _playerInputActions.Gameplay.TurnCameraR.canceled += OnDpadRight;
        _playerInputActions.Gameplay.TurnCameraL.canceled += OnDpadLeft;

        // Menu Map
        _playerInputActions.Menu.Exit.started += OnNorthButton;
        _playerInputActions.Menu.Exit.canceled += OnNorthButton;
        _playerInputActions.Menu.Confirm.started += OnSouthButton;
        _playerInputActions.Menu.Confirm.canceled += OnSouthButton;
        _playerInputActions.Menu.Move.performed += OnLeftStick;
        _playerInputActions.Menu.Move.canceled += OnLeftStick;
    }

    // Getters & Setters
    public Vector2 LeftStickInput { get { return _leftStickInput; } set { _leftStickInput = value; } }
    public Vector2 RightStickInput { get { return _rightStickInput; } set { _rightStickInput = value; } }
    public bool EastButtonInput { get { return _eastButtonPressed; } set { _eastButtonPressed = value; } }
    public bool WestButtonInput { get { return _westButtonPressed; } set { _westButtonPressed = value; } }
    public bool SouthButtonInput { get { return _southButtonPressed; } set { _southButtonPressed = value; } }
    public bool NorthButtonInput { get { return _northButtonPressed; } set { _northButtonPressed = value; } }
    public bool LeftShoulderInput { get { return _leftShoulderPressed; } set { _leftShoulderPressed = value; } }
    public bool DpadRight { get { return _dpadRightPressed; } set { _dpadRightPressed = value; } }
    public bool DpadLeft { get { return _dpadLeftPressed; } set { _dpadLeftPressed = value; } }

    // Gameplay Action Map
    private void OnLeftStick(InputAction.CallbackContext context)
    {
        LeftStickInput = context.ReadValue<Vector2>();
    }
    private void OnRightStick(InputAction.CallbackContext context)
    {
        RightStickInput = context.ReadValue<Vector2>();
    }
    private void OnNorthButton(InputAction.CallbackContext context)
    {
        NorthButtonInput = context.ReadValueAsButton();
    }
    private void OnSouthButton(InputAction.CallbackContext context)
    {
        SouthButtonInput = context.ReadValueAsButton();
    }
    private void OnEastButton(InputAction.CallbackContext context)
    {
        EastButtonInput = context.ReadValueAsButton();
    }
    private void OnWestButton(InputAction.CallbackContext context)
    {
        WestButtonInput = context.ReadValueAsButton();
    }
    private void OnLeftShoulder(InputAction.CallbackContext context)
    {
        LeftShoulderInput = context.ReadValueAsButton();
    }
    private void OnDpadRight(InputAction.CallbackContext context)
    {
        DpadRight = context.ReadValueAsButton();
    }
    private void OnDpadLeft(InputAction.CallbackContext context)
    {
        DpadLeft = context.ReadValueAsButton();
    }

}
