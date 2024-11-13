using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class MovePlatform : MonoBehaviour
{
    [Header("Platform")]
    [SerializeField] PathCreator _splinePath;
    [SerializeField] EndOfPathInstruction _splineEnd;
    [SerializeField] float _platformSpeed = 10;
    [SerializeField] List<Switch> _switches;
    [SerializeField] int _triggeredSwitches = 0;

    float _distance = 0f;
    GameObject _player;
    bool _isMoving;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _isMoving = false;

        transform.position = _splinePath.path.GetPointAtDistance(_distance, _splineEnd);
        transform.rotation = _splinePath.path.GetRotationAtDistance(_distance, _splineEnd);
    }
    private void Update()
    {
        if (TriggeredSwitches == _switches.Count)
        {
            if (!_isMoving)
            {
                _isMoving = true;
                Debug.Log("platform started");
            }
        }
        else
        {
            if (_isMoving)
            {
                _isMoving = false;
                Debug.Log("platform stopped");
            }
        }

        Move();
    }

    public int TriggeredSwitches { get { return _triggeredSwitches; } set { _triggeredSwitches = value; } }

    private void Move()
    {
        if (_isMoving)
        {
            _distance += _platformSpeed * Time.deltaTime;
            transform.position = _splinePath.path.GetPointAtDistance(_distance, _splineEnd);
            transform.rotation = _splinePath.path.GetRotationAtDistance(_distance, _splineEnd);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other == _player.GetComponent<Collider>())
        {
            _player.transform.parent = transform;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other == _player.GetComponent<Collider>())
        {
            _player.transform.parent = null;
        }
    }
}
