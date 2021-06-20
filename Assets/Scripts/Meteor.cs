using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Meteor : MonoBehaviour, IDamagable<float>
{
    public static UnityAction<Vector3, float> MeteorExploded;
    public float currentHealth;
    public float explosionRadius;
    private float currentSpeed;
    [SerializeField] private Vector2 meteorMinMaxSize;

    private void OnEnable()
    {
        float newScale = Random.Range(meteorMinMaxSize.x, meteorMinMaxSize.y);
        transform.localScale = new Vector3(newScale, newScale, newScale);
        currentSpeed = newScale * 3;
    }

    private void FixedUpdate() => transform.Translate(currentSpeed * Time.fixedDeltaTime * Vector3.down, Space.Self);

    public void ApplyDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            MeteorExploded?.Invoke(transform.position, explosionRadius);
            gameObject.SetActive(false);
        }
    }

    public float GetCurrentHealth() => currentHealth;

    private void OnCollisionEnter(Collision collision)
    {
        MeteorExploded?.Invoke(transform.position, explosionRadius);
        TryDamagingNearTargets();
        gameObject.SetActive(false);
    }

    private void TryDamagingNearTargets()
    {
        float damage = transform.localScale.x + transform.localScale.y + transform.localScale.z * currentHealth;
        Collider[] closeObjects = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in closeObjects)
        {
            IDamagable<float> d = hit.gameObject.GetComponent<IDamagable<float>>();
            if (d != null)
                d.ApplyDamage(damage);
        }
    }
}