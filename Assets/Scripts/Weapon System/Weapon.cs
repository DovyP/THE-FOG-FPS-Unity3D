using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float throwForce;
    public float throwExtraForce;
    public float rotationForce;

    public int weaponGfxLayer;
    public GameObject[] weaponGfxs;
    public Collider[] gfxColliders;

    private bool _held;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = gameObject.AddComponent<Rigidbody>();
        _rb.mass = 0.1f;
    }

    public void Pickup(Transform weaponHolder)
    {
        if (_held) return;
        Destroy(_rb);
        transform.parent = weaponHolder;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        foreach (var col in gfxColliders)
        {
            col.enabled = false;
        }
        foreach (var gfx in weaponGfxs)
        {
            gfx.layer = weaponGfxLayer;
        }
        _held = true;
    }

    public void Drop(Transform playerCamera)
    {
        if (!_held) return;
        _rb = gameObject.AddComponent<Rigidbody>();
        _rb.mass = 0.1f;
        var forward = playerCamera.forward;
        forward.y = 0f;
        _rb.velocity = forward * throwForce;    // direction looking at
        _rb.velocity += Vector3.up * throwExtraForce;   // throw up as well
        _rb.angularVelocity = Random.onUnitSphere * rotationForce;  // spin to win
        foreach (var col in gfxColliders)
        {
            col.enabled = true;
        }
        foreach (var gfx in weaponGfxs)
        {
            gfx.layer = 0;
        }
        transform.parent = null;
        _held = false;
    }
}
