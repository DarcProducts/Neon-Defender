using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    public static UnityAction<Vector3, byte, byte, float> TextInfo;
    public static UnityAction<Vector3, float, byte> ProjectileExploded;
    public float currentDamage;
    [SerializeField] LayerMask hitLayers;
    [SerializeField] LayerMask ignoreLayers;
    TrailRenderer projectileTrail;
    Rigidbody pRigidbody;

    void Awake()
    {
        projectileTrail = GetComponentInChildren<TrailRenderer>();
        pRigidbody = GetComponent<Rigidbody>();
    }

    void OnDisable()
    {
        if (projectileTrail != null)
            projectileTrail.Clear();
        if (pRigidbody != null)
        {
            pRigidbody.velocity = Vector3.zero;
            pRigidbody.angularVelocity = Vector3.zero;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (Utilities.IsInLayerMask(other.gameObject, ignoreLayers))
            return;
        if (Utilities.IsInLayerMask(other.gameObject, hitLayers))
        {
            TryDamagingTarget(other.gameObject);
            ProjectileExploded?.Invoke(transform.position, .3f, 1);
        }
        gameObject.SetActive(false);
    }

    public void TryDamagingTarget(GameObject target)
    {
        IDamagable<float> d = target.GetComponent<IDamagable<float>>();
        if (d != null)
        {
            TextInfo?.Invoke(transform.position, 5, 0, currentDamage);
            d.ApplyDamage(currentDamage);
        }
    }

    public Rigidbody GetRigidbody() => pRigidbody;
}