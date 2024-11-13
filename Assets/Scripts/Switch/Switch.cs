using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    [Header("Doors")]
    [SerializeField] bool _activeOnDoors;
    [SerializeField] List<Door> _doorsList;

    [Header("Moving Platforms")]
    [SerializeField] bool _activeOnPlatforms;
    [SerializeField] List<MovePlatform> _movingPlatformList;

    [Header("SwitchState")]
    [SerializeField] bool _isTriggered;
    [SerializeField] bool _activeInCombat;

    GameObject _player;
    PlayerController _playerController;
    Collider _hand;
    Collider _feet;

    Animator _switchAnimator;
    MeshRenderer _switchMeshRenderer;
    int _isPressedHash;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _playerController = _player.GetComponent<PlayerController>();
        _hand = GameObject.FindGameObjectWithTag("Hand").GetComponent<Collider>();
        _feet = GameObject.FindGameObjectWithTag("Feet").GetComponent<Collider>();
        _isTriggered = false;
        _switchAnimator = GetComponent<Animator>();
        IsPressedHash = Animator.StringToHash("isPressed");
        _switchMeshRenderer = GetComponent<MeshRenderer>();
        _switchMeshRenderer.material.color = Color.red;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_activeInCombat)
        {
            CheckCollision(collision);
        }
        else
        {
            if (!_playerController.InCombat)
            {
                CheckCollision(collision);
            }
        }
    }

    public bool IsSwitchActive { get { return _isTriggered; } set { _isTriggered = value; } }
    public int IsPressedHash { get { return _isPressedHash; } set { _isPressedHash = value; } }

    private void TriggerSwitch()
    {
        if (_isTriggered)
        {
            IsSwitchActive = false;

            if (_activeOnDoors)
            {
                TriggeredSwitchesDoor(_doorsList, -1);
            }
            if (_activeOnPlatforms)
            {
                TriggeredSwitchesPlatforms(_movingPlatformList, -1);
            }
            
            AnimateButton(false);
            _switchMeshRenderer.material.color = Color.red;
        }
        else
        {
            IsSwitchActive = true;
            if (_activeOnDoors)
            {
                TriggeredSwitchesDoor(_doorsList, 1);
            }
            if (_activeOnPlatforms)
            {
                TriggeredSwitchesPlatforms(_movingPlatformList, 1);
            }
            AnimateButton(true);
            _switchMeshRenderer.material.color = Color.green;
        }
    }

    private void TriggeredSwitchesDoor(List<Door> list, int value)
    {
        for (int i = 0; i < list.Count; i++)
        {
            _doorsList[i].TriggeredSwitches += value;
        }
    }
    private void TriggeredSwitchesPlatforms(List<MovePlatform> list, int value)
    {
        for (int i = 0; i < list.Count; i++)
        {
            _movingPlatformList[i].TriggeredSwitches += value;
        }
    }

    private void CheckCollision(Collision collision)
    {
        if (collision.collider == _hand || collision.collider == _feet)
        {
            TriggerSwitch();
        }
    }
    
    private void AnimateButton(bool value) 
    {
        _switchAnimator.SetBool(IsPressedHash, value);
    }
}
