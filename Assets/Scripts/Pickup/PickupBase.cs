using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PickupBase : MonoBehaviour
{
    [Header("Pick Up Parameters")]
    [SerializeField] public bool _respawnable;
    [SerializeField] public float _respawnTimer = 2;
    public enum PowerUps
    {
        MultiJump = 1,
        Sprint = 2,
        Dash = 3
    }
    [SerializeField] public PowerUps _powerType;
    [SerializeField] public float _duration = 2f;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem[] _onSpawnParticles = null;
    [SerializeField] private ParticleSystem[] _alwaysActiveParicles = null;
    [SerializeField] private ParticleSystem[] _onDespawnParticles = null;

    [Header("Sound Effects")]
    [SerializeField] AudioClip _spawnClip;
    [SerializeField] AudioClip _despawnClip;

    [HideInInspector]
    [SerializeField] private List<ParticleSystem> _instantiatedOnSpawn = null;
    [HideInInspector]
    [SerializeField] private List<ParticleSystem> _instantiatedAlwaysActive = null;
    [HideInInspector]
    [SerializeField] private List<ParticleSystem> _instantiatedOnDespawn = null;

    private PowerUps _power;
    private float _pickUpTime;
    private MeshRenderer _meshRenderer;
    private Collider _collider;
    private AudioSource _audioSource;

    private void Awake()
    {
        ActivePower = _powerType;
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        _collider = GetComponent<Collider>();
        _audioSource = GetComponent<AudioSource>();
        SetUpParticles(_onSpawnParticles, _instantiatedOnSpawn);
        SetUpParticles(_alwaysActiveParicles, _instantiatedAlwaysActive);
        SetUpParticles(_onDespawnParticles, _instantiatedOnDespawn);
    }
    private void Start()
    {
        PlayParticles(_instantiatedOnSpawn);
        PlayParticles(_instantiatedAlwaysActive);
    }
    private void Update()
    {
        if (_respawnable && _meshRenderer.enabled == false && Time.time >= _pickUpTime + _respawnTimer)
        {
           Respawn();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Despawn();
        }
    }
    private void OnDestroy()
    {
        //PlayAudioClip(_despawnClip);
        /*StopParticles(_instantiatedOnSpawn);
        StopParticles(_instantiatedAlwaysActive);
        PlayParticles(_instantiatedOnDespawn);
        gameObject.transform.parent = null;*/
    }

    // Getters & Setters
    public PowerUps ActivePower { get { return _power; } set { _power = value; } }
    public float PowerDuration { get { return _duration; } }

    // Functions
    private void Despawn()
    {
        if (_respawnable)
        {
            _pickUpTime = Time.time;
            _collider.enabled = false;
            _meshRenderer.enabled = false;
            PlayAudioClip(_despawnClip);
            StopParticles(_instantiatedOnSpawn);
            StopParticles(_instantiatedAlwaysActive);
            PlayParticles(_instantiatedOnDespawn);
        }
        else
        {
            /*StopParticles(_instantiatedOnSpawn);
            StopParticles(_instantiatedAlwaysActive);
            PlayParticles(_instantiatedOnDespawn);*/
            Destroy(gameObject);
        }
    }
    private void Respawn()
    {
        _collider.enabled = true;
        _meshRenderer.enabled = true;
        PlayAudioClip(_spawnClip);

        if (_instantiatedOnSpawn != null)
        {
            PlayParticles(_instantiatedOnSpawn);
            PlayParticles(_instantiatedAlwaysActive);
        }
    }
    private void PlayParticles(List<ParticleSystem> list)
    {
        if (list != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Play();

            }
        }
    }
    private void StopParticles(List<ParticleSystem> list)
    {
        if (list != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Stop();
            }
        }
    }
    private void SetUpParticles(ParticleSystem[] array, List<ParticleSystem> list)
    {
        if (array != null)
        {
            for (int i = 0; i < array.Length; i++)
            {
                ParticleSystem temp;
                temp = Instantiate(array[i].GetComponent<ParticleSystem>(), gameObject.transform);
                temp.Stop();
                list.Add(temp);
            }
        }
    }
    private void PlayAudioClip(AudioClip clip)
    {
        if (clip != null)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
        }
    }
}
