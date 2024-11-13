using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    Animator _playerAnimator;
   
    int _isWalkingHash;
    int _isRunningHash;
    int _isAttackingHash;
    int _isJumpingHash;
    int _walkingSpeedHash;
    int _forwardHash;

    float _playerAnimationDuration;

    private void Awake()
    {
        _playerAnimator = GetComponent<Animator>();

        SetStringsToHash();
    }

    private void Update()
    {
        SetAnimationDuration(_playerAnimator);
    }

    // Getters & Setters
    public int IsWalkingHash { get { return _isWalkingHash; } set { _isWalkingHash = value; } }
    public int IsRunningHash { get { return _isRunningHash; } set { _isRunningHash = value; } }
    public int ForwardHash { get { return _forwardHash; } set { _forwardHash = value; } }
    public int IsAttackingHash { get { return _isAttackingHash; } set { _isAttackingHash = value; } }
    public int IsJumpingHash { get { return _isJumpingHash; } set { _isJumpingHash = value; } }
    public int WalkingSpeedHash { get { return _walkingSpeedHash; } set { _walkingSpeedHash = value; } }
    public float PlayerAnimationDuration { get { return _playerAnimationDuration; } set { _playerAnimationDuration = value; } }

    // Functions
    private void SetStringsToHash()
    {
        // Player
        IsWalkingHash = Animator.StringToHash("isWalking");
        IsRunningHash =  Animator.StringToHash("isRunning");
        ForwardHash = Animator.StringToHash("forward");
        IsAttackingHash = Animator.StringToHash("isAttacking");
        IsJumpingHash = Animator.StringToHash("isJumping");
        WalkingSpeedHash = Animator.StringToHash("walkingSpeed");
    }
    public void SetAnimationState(Animator animator, int hashID, bool state)
    {
        animator.SetBool(hashID, state);
    }
    public void SetAnimationState(Animator animator, int hashID, float value)
    {
        animator.SetFloat(hashID, value);
    }
    public void SetAnimationDuration(Animator animator)
    {
        PlayerAnimationDuration = animator.GetCurrentAnimatorStateInfo(0).length;
    }

}
