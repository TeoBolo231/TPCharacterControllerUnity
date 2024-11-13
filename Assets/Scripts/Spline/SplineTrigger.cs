using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class SplineTrigger : MonoBehaviour
{
    [SerializeField] PlayerController _player;
    [SerializeField] PathCreator _splinePath;

    [SerializeField] bool _enterTrigger;
    [SerializeField] bool _exitTrigger;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        _splinePath = GetComponentInParent<PathCreator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other == _player.GetComponent<Collider>())
        {
            if (_enterTrigger)
            {
                _player.DistanceTraveled = 0;
            }
            if (_exitTrigger)
            {
                _player.DistanceTraveled = _splinePath.path.length;
            }
            
            _player.IsOnSpline = !_player.IsOnSpline;

            if (_player.IsOnSpline)
            {
                _player.transform.rotation = new Quaternion(0, _player.transform.rotation.y, transform.rotation.z, 1);
            }
        }
        
    }
}
