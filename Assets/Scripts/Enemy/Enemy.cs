using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PathCreation;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Type")]
    [SerializeField] public EnemyType _type;
    public enum EnemyType
    {
        Humanoid = 1,
        Ooze = 2,
        Oozeling = 3
    }

    [Header("AI")]
    NavMeshAgent _navMeshAgent;

    [Header("Combat")]
    [SerializeField] Text _healthText;
    [SerializeField] float _MAX_HEALTH = 100.0f;
    [SerializeField] float _currentHealth;
    [SerializeField] float _damage = 10.0f;
    [SerializeField] float _attackDelay = 1.0f;
    [SerializeField] bool _canDamage = true;

    [Header("Chase")]
    [SerializeField] float _chasingSpeed = 5.0f;
    [SerializeField] float _chaseRadius = 5.0f;
    [SerializeField] float _aggroRadius = 15.0f;
    [SerializeField] LayerMask _aggroMask;

    [Header("Patrol")]
    [SerializeField] float _patrolSpeed = 3.0f;
    /*[SerializeField] List<Transform> _waypoints;
    [SerializeField] int _currentWaypointIndex;*/

    [Header("Spawned Children")]
    [SerializeField] bool _spawnChildrenActive;
    [SerializeField] GameObject _child;
    [SerializeField] int _numberOfChildren = 2;
    [SerializeField] PathCreator _spawnSpline;
    [SerializeField] EndOfPathInstruction _spawnSplineEnd;
    Vector3 pos;

    [Header("State")]
    [SerializeField] public States _currentState;
    public enum States
    {
        Patrolling = 1,
        Chasing = 2
    }

    Weapon _weapon;
    GameObject _player;
    PlayerController _playerController;
    GameObject _targetMark;

    public float Damage { get { return _damage;} set { _damage = value; } }
    public bool CanDamage { get { return _canDamage; } set { _canDamage = value; } }
    public States EnemyState { get { return _currentState; } set { _currentState = value; } }

    private void Awake()
    {
        LinkReferences();
        _currentState = States.Patrolling;
        
    }
    private void Update()
    {
        Move();
        ChangeState();
        PrintHealth();
        Die();
        /*Debug.Log("Length: " + _waypoints.Count);
        Debug.Log("Index: " + _currentWaypointIndex);*/
    }
    private void OnCollisionEnter(Collision collision)
    {
        TakeDamage(collision, _weapon.GetDamage());
        Attack(collision);
    }

    private void LinkReferences()
    {
        _currentHealth = _MAX_HEALTH;
        _player = GameObject.FindGameObjectWithTag("Player");
        _playerController = _player.GetComponent<PlayerController>();
        _weapon = _player.GetComponentInChildren<Weapon>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        /*_currentWaypointIndex = 0;
        _navMeshAgent.SetDestination(_waypoints[_currentWaypointIndex].position);*/
        _targetMark = transform.Find("TargetMark").gameObject;
        _targetMark.SetActive(false);
        if (_spawnChildrenActive)
        {
            _spawnSpline = GetComponentInChildren<PathCreator>();
        }
    }
    private void PrintHealth()
    {
        _healthText.text = "Health: " + _currentHealth.ToString();
    }
    private void TakeDamage(Collision collision, float dmg)
    {
        if (collision.collider == _weapon.GetComponent<Collider>())
        {
            if (_playerController.CanDealDamage)
            {
                _currentHealth -= dmg;
                Debug.Log("Ouch, that's mean! It Hurts!");
            }
        }
    }
    private void Die()
    {
        switch (_type)
        {
            case EnemyType.Ooze:
                if (_currentHealth <= _MAX_HEALTH/2)
                {
                    Debug.Log("I'm splitting");
                    SpawnChildren();
                    Destroy(gameObject);
                }
                break;
            case EnemyType.Oozeling:
                if (_currentHealth <= 0)
                {                    
                    if (_spawnChildrenActive)
                    {
                        SpawnChildren();
                        Debug.Log("I'm splitting again");
                    }
                    Destroy(gameObject);
                }
                break;
            default:
                if (_currentHealth <= 0)
                {
                    Debug.Log("Good bye cruel world");
                    Destroy(gameObject);
                }
                break;
        }
    }
    private void SpawnChildren()
    {
        if (_type == EnemyType.Ooze || _type == EnemyType.Oozeling)
        {
            float _spawnIncrement;

            _spawnIncrement = _spawnSpline.path.length / _numberOfChildren;

            for (int i = 0; i < _numberOfChildren; i++)
            {
                pos = _spawnSpline.path.GetPointAtDistance(_spawnIncrement * i, _spawnSplineEnd);
                Instantiate(_child, pos, Quaternion.identity);
            }
        }
    }
    private void ChangeState() 
    {
        if (Physics.CheckSphere(transform.position, _chaseRadius, _aggroMask))
        {
            _currentState = States.Chasing;
        }
        if (!Physics.CheckSphere(transform.position, _aggroRadius, _aggroMask))
        {
            _currentState = States.Patrolling;
        }
    }
    private void Move()
    {
        if (_currentState == States.Patrolling)
        {
            _navMeshAgent.speed = _patrolSpeed;
            Debug.Log("Patrolling");
/*
            if ((transform.position.x == _waypoints[0].position.x) && (transform.position.y == _waypoints[0].position.y))
            {
                MoveToNextWaypoint();
            }
            if ((transform.position.x == _waypoints[1].position.x) && (transform.position.y == _waypoints[1].position.y))
            {
                MoveToNextWaypoint();
            }
            if ((transform.position.x == _waypoints[2].position.x) && (transform.position.y == _waypoints[2].position.y))
            {
                _currentWaypointIndex = 0;
                _navMeshAgent.SetDestination(_waypoints[_currentWaypointIndex].position);
            }*/
        }
        else if (_currentState == States.Chasing)
        {
            _navMeshAgent.speed = _chasingSpeed;
            _navMeshAgent.SetDestination(_player.transform.position);
        }
    }
    /*private void MoveToNextWaypoint()
    {
        _currentWaypointIndex += 1;
        _navMeshAgent.SetDestination(_waypoints[_currentWaypointIndex].position);
    }*/
    private void Attack(Collision collision)
    {
        if (CanDamage && collision.transform.CompareTag("Player"))
        {
            CanDamage = false;
            _playerController.TakeDamage(_damage);
            StartCoroutine(DamageDelay(_attackDelay));
        }
    }
    IEnumerator DamageDelay(float delay) 
    {
        yield return new WaitForSecondsRealtime(delay);
        CanDamage = true;
    }
}
