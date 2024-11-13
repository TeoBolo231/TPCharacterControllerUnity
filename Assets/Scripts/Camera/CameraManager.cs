using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [Header("Camera Sokets")]
    [SerializeField] List<CinemachineVirtualCamera> _90DegCameras;
    [SerializeField] CinemachineVirtualCamera _cinematicCamera;
    [SerializeField] CinemachineVirtualCamera _zoomInCamera;
    [SerializeField] CinemachineVirtualCamera _lockOnCamera;
    int _activeCamera;

    [Header("ZoomIn Camera")]
    [SerializeField] bool _activeZoomIn;
    [SerializeField] float _sphereRadiusZoomIn = 10f;
    [SerializeField] LayerMask _buttonMask;
    Collider[] _buttons;

    [Header("LockOn Camera")]
    [SerializeField] float _sphereRadiusLockOn = 100f;
    [SerializeField] float _breakLockDistance = 200f;
    [SerializeField] LayerMask _targetMask;
    Collider[] _hitTargets;
    Collider _currentTarget;
    GameObject _markTarget;
    Color _currentTargetColor;

    PlayerInputAction _playerInputActions;
    GameObject _player;
    PlayerController _playerController;
    CharacterController _playerCC;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player"); 
        _playerController = _player.GetComponent<PlayerController>();
        _playerCC = _player.GetComponent<CharacterController>();
        SetUpInputs();
        _currentTarget = null;
        _markTarget = null;
    }
    private void Start()
    {
        SetUpCameras();
    }
    private void Update()
    {
        if (_playerController.IsOnSpline)
        {
            SplineCamera();
        }
        else
        {
            Normal3DCamera();
        }
    }

    // Inputs
    private void SetUpInputs()
    {
        _playerInputActions = new PlayerInputAction();
        _playerInputActions.Gameplay.Enable();

        // Gameplay Map
        _playerInputActions.Gameplay.TurnCameraR.performed += OnTurnCameraR;
        _playerInputActions.Gameplay.TurnCameraL.performed += OnTurnCameraL;
        _playerInputActions.Gameplay.LockTarget.performed += OnLockTarget;
    }
    private void OnTurnCameraR(InputAction.CallbackContext context)
    {
        if (!_playerController.InCinematic && !_playerController.IsInMenu)
        {
            if (_activeCamera == _90DegCameras.Count - 1)
            {
                _activeCamera = 0;
            }
            else
            {
                _activeCamera += 1;
            }
            ActivateCamera(_90DegCameras, _activeCamera);
        }  
    }
    private void OnTurnCameraL(InputAction.CallbackContext context)
    {
        if (!_playerController.InCinematic && !_playerController.IsInMenu)
        {
            if (_activeCamera == 0)
            {
                _activeCamera = _90DegCameras.Count - 1;
            }
            else
            {
                _activeCamera -= 1;
            }

            ActivateCamera(_90DegCameras, _activeCamera);
        }
    }
    private void OnLockTarget(InputAction.CallbackContext context)
    {
        if (!_playerController.InCinematic && !_playerController.IsInMenu)
        {
            if (!_playerController.HasTarget)
            {
                LockTarget();
            }
            else
            {
                UnlockTarget();
            }
        } 
    }

    // Functions
    private void SetUpCameras()
    {
        _activeCamera = 0;
        _90DegCameras[0] = GameObject.FindGameObjectWithTag("VCamBack").GetComponent<CinemachineVirtualCamera>();
        _90DegCameras[1] = GameObject.FindGameObjectWithTag("VCamRight").GetComponent<CinemachineVirtualCamera>();
        _90DegCameras[2] = GameObject.FindGameObjectWithTag("VCamFront").GetComponent<CinemachineVirtualCamera>();
        _90DegCameras[3] = GameObject.FindGameObjectWithTag("VCamLeft").GetComponent<CinemachineVirtualCamera>();
        _cinematicCamera = GameObject.FindGameObjectWithTag("VCamCinematic").GetComponent<CinemachineVirtualCamera>();
        _zoomInCamera = GameObject.FindGameObjectWithTag("VCamZoomIn").GetComponent<CinemachineVirtualCamera>();
        _lockOnCamera = GameObject.FindGameObjectWithTag("VCamLockOn").GetComponent<CinemachineVirtualCamera>();

        _90DegCameras[0].Follow = _player.transform;
        _90DegCameras[1].Follow = _player.transform;
        _90DegCameras[2].Follow = _player.transform;
        _90DegCameras[3].Follow = _player.transform;
        _cinematicCamera.Follow = _player.transform;
        _zoomInCamera.Follow = _player.transform;
        _lockOnCamera.Follow = _player.transform;

        _90DegCameras[0].LookAt = _player.transform;
        _90DegCameras[1].LookAt = _player.transform;
        _90DegCameras[2].LookAt = _player.transform;
        _90DegCameras[3].LookAt = _player.transform;

        ActivateCamera(_90DegCameras, _activeCamera);
    }
    private void Normal3DCamera()
    {
        if (_currentTarget == null)
        {
            ResetTarget();
        }
        if (_currentTarget != null)
        {
            if (((_player.transform.position - _currentTarget.transform.position).sqrMagnitude) > _breakLockDistance)
            {
                BreakLock();
            }
        }
        if (!Physics.CheckSphere(_player.transform.localPosition + _playerCC.center, _sphereRadiusZoomIn, _buttonMask))
        {
            _zoomInCamera.LookAt = null;
        }
        if (_playerController.InCinematic)
        {
            ActivateCamera(_cinematicCamera);
        }
        else if (Physics.CheckSphere(_player.transform.localPosition + _playerCC.center, _sphereRadiusZoomIn, _buttonMask) && _activeZoomIn)
        {
            if (!_playerController.InCombat && !_playerController.HasTarget)
            {
                ActivateCamera(_zoomInCamera);
                TargetButton();
            }
            else if (_playerController.InCombat && !_playerController.HasTarget)
            {
                ActivateCamera(_90DegCameras, _activeCamera);
            }
            else if (_playerController.HasTarget)
            {
                ActivateCamera(_lockOnCamera);
            }
        }
        else if (_playerController.HasTarget)
        {
            ActivateCamera(_lockOnCamera);
        }
        else
        {
            ActivateCamera(_90DegCameras, _activeCamera);
        }
    }
    private void SplineCamera()
    {
        ActivateCamera(_90DegCameras, 1);
        
    }
    private void ActivateCamera(CinemachineVirtualCamera cam)
    {
        if (cam != null)
        {
            cam.MoveToTopOfPrioritySubqueue();
        } 
    }
    private void ActivateCamera(List<CinemachineVirtualCamera> camlist, int camIndex) 
    {
        if (camlist != null)
        {
            camlist[camIndex].MoveToTopOfPrioritySubqueue();
        }
    }
    private void LockTarget()
    {
        float distance;
        float _targetDistance = _sphereRadiusLockOn;
        _hitTargets = Physics.OverlapSphere(_player.transform.localPosition + _playerCC.center, _sphereRadiusLockOn, _targetMask);
        foreach (Collider target in _hitTargets)
        {
            if (_playerController.InCombat)
            {
                if (target.CompareTag("Enemy"))
                {
                    distance = (_player.transform.position - target.transform.position).sqrMagnitude;
                    if (distance < _targetDistance)
                    {
                        _targetDistance = distance;
                        ActivateLockOnCamera(target);
                    }
                }
            }
            else
            {
                distance = (_player.transform.position - target.transform.position).sqrMagnitude;
                if (distance < _targetDistance)
                {
                    _targetDistance = distance;
                    ActivateLockOnCamera(target);
                }
            } 
        }
        MarkTarget();
    }
    private void BreakLock()
    {
        ActivateCamera(_90DegCameras, _activeCamera);
        if (_currentTarget.CompareTag("Enemy"))
        {
            _markTarget.SetActive(false);
            _markTarget = null;
        }
        else
        {
            _currentTarget.GetComponent<MeshRenderer>().material.color = _currentTargetColor;
        }
        
        _currentTarget = null;
        _playerController.HasTarget = false;
    }
    private void ActivateLockOnCamera(Collider target)
    {
        _lockOnCamera.LookAt = target.GetComponent<Transform>();
        _playerController.HasTarget = true;
        _currentTarget = target;
        if (!_currentTarget.CompareTag("Enemy"))
        {
            _currentTargetColor = target.GetComponent<MeshRenderer>().material.color;
        }    
    }
    private void UnlockTarget()
    {
        ActivateCamera(_90DegCameras, _activeCamera);
        if (_currentTarget.CompareTag("Enemy") && _currentTarget != null)
        {
            _markTarget.SetActive(false);
            _markTarget = null;
        }
        else
        {
            _currentTarget.GetComponent<MeshRenderer>().material.color = _currentTargetColor;
        }
        _playerController.HasTarget = false;
        _currentTarget = null;
    }
    private void ResetTarget()
    {
        ActivateCamera(_90DegCameras, _activeCamera);
        _playerController.HasTarget = false;
        _currentTarget = null;
    }
    private void MarkTarget()
    {
        if (_currentTarget!= null)
        {
            if (_currentTarget.CompareTag("Enemy"))
            {
                _markTarget = _currentTarget.transform.Find("TargetMark").gameObject;
                _markTarget.SetActive(true);
            }
            else
            {
                MeshRenderer targetMeshRenderer;
                targetMeshRenderer = _currentTarget.GetComponent<MeshRenderer>();
                targetMeshRenderer.material.color = Color.blue;
            }
        }
    }
    private void TargetButton()
    {
        float distance;
        float targetDistance = _sphereRadiusZoomIn;
        _buttons = Physics.OverlapSphere(_player.transform.localPosition + _playerCC.center, _sphereRadiusZoomIn, _buttonMask);
        foreach (Collider button in _buttons)
        {
            distance = (_player.transform.position - button.transform.position).sqrMagnitude;
            if (distance < targetDistance)
            {
                targetDistance = distance;
                _zoomInCamera.LookAt = button.GetComponent<Transform>();
            }
        }
    }
}
