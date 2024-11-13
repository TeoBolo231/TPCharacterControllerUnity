using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] List<Switch> _switches;
    [SerializeField] int _triggeredSwitches = 0;

    [Header("Cinematic")]
    [SerializeField] float _delayToOpen = 3;
    [SerializeField] float _delayToEndCinematic = 6;
    Animator _doorAnimator;
    PlayerController _playerController;
    int _isOpenHash;
    bool _isOpening = false;
    
    private void Awake()
    {
        _playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        _doorAnimator = GetComponent<Animator>();
        IsOpenHash = Animator.StringToHash("isOpen");
    }
    private void Update()
    {
        if (TriggeredSwitches == _switches.Count)
        {
            if (!_isOpening)
            {
                _isOpening = true;
                _playerController.InCinematic = true;
                StartCoroutine(DelayOpen(_delayToOpen));
                StartCoroutine(EndCinematic(_delayToEndCinematic));
            }
        }
        else
        {
            if (_isOpening)
            {
                _isOpening = false;
            }
            AnimateDoor(false);
        }
    }

    public int TriggeredSwitches { get { return _triggeredSwitches; } set { _triggeredSwitches = value; } }
    public int IsOpenHash { get { return _isOpenHash; } set { _isOpenHash = value; } }

    private void AnimateDoor(bool value)
    {
        _doorAnimator.SetBool(IsOpenHash, value);
    }
    IEnumerator DelayOpen(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        AnimateDoor(true);
    }
    IEnumerator EndCinematic(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        _playerController.InCinematic = false;
    }
}
    

