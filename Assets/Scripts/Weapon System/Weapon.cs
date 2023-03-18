using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour {
    [Header("Throwing")]
    [SerializeField] private float throwForce;
    [SerializeField] private float throwExtraForce;
    [SerializeField] private float rotationForce;

    [Header("Pickup")]
    [SerializeField] private float animTime;

    [Header("Shooting")]
    [SerializeField] private int maxAmmo;
    [SerializeField] private int shotsPerSecond;
    [SerializeField] private float reloadSpeed;
    [SerializeField] private float hitForce;
    [SerializeField] private float range;
    [SerializeField] private bool tapable;
    [SerializeField] private float kickbackForce;
    [SerializeField] private float resetSmooth;
    [SerializeField] private Vector3 scopePos;

    [Header("Data")]
    [SerializeField] private int weaponGfxLayer;
    [SerializeField] private GameObject[] weaponGfxs;
    [SerializeField] private Collider[] gfxColliders;
    [SerializeField] private GameObject onHitFX;

    private float _rotationTime;
    private float _time;
    private bool _held;
    private bool _scoping;
    private bool _reloading;
    private bool _shooting;
    private int _ammo;
    private Rigidbody _rb;
    private Transform _playerCamera;
    private TMP_Text _ammoText;
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private void Start() {
        _rb = gameObject.AddComponent<Rigidbody>();
        _rb.mass = 0.1f;
        _ammo = maxAmmo;
    }

    private void Update() {
        if (!_held) return;

        if (_time < animTime) {
            _time += Time.deltaTime;
            _time = Mathf.Clamp(_time, 0f, animTime);
            var delta = -(Mathf.Cos(Mathf.PI * (_time / animTime)) - 1f) / 2f;
            transform.localPosition = Vector3.Lerp(_startPosition, Vector3.zero, delta);
            transform.localRotation = Quaternion.Lerp(_startRotation, Quaternion.identity, delta);
        }
        else {
            _scoping = Input.GetMouseButton(1) && !_reloading;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.Lerp(transform.localPosition, _scoping ? scopePos : Vector3.zero, resetSmooth * Time.deltaTime);
        }

        if (_reloading) {
            _rotationTime += Time.deltaTime;
            var spinDelta = -(Mathf.Cos(Mathf.PI * (_rotationTime / reloadSpeed)) - 1f) / 2f;
            transform.localRotation = Quaternion.Euler(new Vector3(spinDelta * 360f, 0, 0));
        }
        
        if (Input.GetKeyDown(KeyCode.R) && !_reloading && _ammo < maxAmmo) {
            StartCoroutine(ReloadCooldown());
        }

        if ((tapable ? Input.GetMouseButtonDown(0) : Input.GetMouseButton(0)) && !_shooting && !_reloading) {
            _ammo--;
            _ammoText.text = _ammo + " / " + maxAmmo;
            Shoot();
            StartCoroutine(_ammo <= 0 ? ReloadCooldown() : ShootingCooldown());
        }
    }

    private void Shoot() {
        transform.localPosition -= new Vector3(0, 0, kickbackForce);
        if (!Physics.Raycast(_playerCamera.position, _playerCamera.forward, out var hitInfo, range)) return;
        Instantiate(onHitFX, hitInfo.point + (hitInfo.normal * .01f), Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
        var rb = hitInfo.transform.GetComponent<Rigidbody>();
        if (rb == null) return;
        
        rb.velocity += _playerCamera.forward * hitForce;
    }

    private IEnumerator ShootingCooldown() {
        _shooting = true;
        yield return new WaitForSeconds(1f / shotsPerSecond);
        _shooting = false;
    }
    
    private IEnumerator ReloadCooldown() {
        _reloading = true;
        _ammoText.text = "RELOADING";
        _rotationTime = 0f;
        yield return new WaitForSeconds(reloadSpeed);
        _ammo = maxAmmo;
        _ammoText.text = _ammo + " / " + maxAmmo;
        _reloading = false;
    }

    public void Pickup(Transform weaponHolder, Transform playerCamera, TMP_Text ammoText) {
        if (_held) return;
        Destroy(_rb);
        _time = 0f;
        transform.parent = weaponHolder;
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;
        foreach (var col in gfxColliders) {
            col.enabled = false;
        }
        foreach (var gfx in weaponGfxs) {
            gfx.layer = weaponGfxLayer;
        }
        _held = true;
        _playerCamera = playerCamera;
        _ammoText = ammoText;
        _ammoText.text = _ammo + " / " + maxAmmo;
        _scoping = false;
    }

    public void Drop(Transform playerCamera) {
        if (!_held) return;
        _rb = gameObject.AddComponent<Rigidbody>();
        _rb.mass = 0.1f;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        var forward = playerCamera.forward;
        forward.y = 0f;
        _rb.velocity = forward * throwForce;
        _rb.velocity += Vector3.up * throwExtraForce;
        _rb.angularVelocity = Random.onUnitSphere * rotationForce;
        foreach (var col in gfxColliders) {
            col.enabled = true;
        }
        foreach (var gfx in weaponGfxs) {
            gfx.layer = 0;
        }
        _ammoText.text = "";
        transform.parent = null;
        _held = false;
    }

    public bool Scoping => _scoping;
}

