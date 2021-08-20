using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Turret : AIUtilities
{
    public static UnityAction TurretFiredGun;
    [SerializeField] LayerMaskVariable targetLayers;
    MultiPooler objectPooler;
    [SerializeField] float projectileForce;
    [SerializeField] float projectileSize;
    [SerializeField] FloatVariable turretDamage;
    [SerializeField] FloatVariable turretFireRate;
    [SerializeField] FloatVariable turretRadius;
    SphereCollider detector;
    GameObject timerText;
    TMP_Text text;
    [SerializeField] float timerDistanceAboveObject;
    [SerializeField] FloatVariable turretDuration;
    float currentTime = 0;
    float currentRate = 0;
    [SerializeField] List<GameObject> targets = new List<GameObject>();
    GameObject currentTarget = null;

    void OnEnable()
    {
        InitializeTurret();
    }

    void OnDisable()
    {
        if (timerText != null)
            timerText.SetActive(false);
    }

    void Start()
    {
        objectPooler = FindObjectOfType<MultiPooler>();
        detector = GetComponent<SphereCollider>();
    }

    void InitializeTurret()
    {
        currentTime = turretDuration.value;
        if (objectPooler != null)
            timerText = objectPooler.GetObject(0);
        if (timerText != null)
        {
            text = timerText.GetComponentInChildren<TMP_Text>();
            timerText.SetActive(true);
        }
        if (detector != null && turretRadius != null)
            detector.radius = turretRadius.value;
        if (text != null)
        {
            text.fontSize = 56;
            text.gameObject.transform.position = transform.position + Vector3.up * timerDistanceAboveObject;
            text.text = Mathf.RoundToInt(currentTime).ToString();
        }
    }

    void FixedUpdate()
    {
        currentTime = currentTime < 0 ? 0 : currentTime -= Time.fixedDeltaTime;
        if (currentTime <= 0)
            gameObject.SetActive(false);
        if (text != null)
            text.text = Mathf.RoundToInt(currentTime).ToString();

        if (currentTarget != null)
        {
            if (!currentTarget.activeSelf)
                currentTarget = null;
            else
            {
                if (turretRadius != null)
                {
                    if (CheckIfPathClear(gameObject, currentTarget, turretRadius.value))
                        FireProjectile();
                }
                else currentTarget = null;
            }
        }
        if (currentTarget == null)
            currentTarget = GetTarget();
    }

    void FireProjectile()
    {
        if (objectPooler != null)
        {
            currentRate = currentRate <= 0 ? 0 : currentRate -= Time.fixedDeltaTime;
            if (currentRate <= 0 && currentTarget != null)
            {
                Rigidbody targetRigid = currentTarget.GetComponent<Rigidbody>();
                GameObject p = objectPooler.GetObject(1);
                if (p != null)
                {
                    p.layer = 0;
                    p.transform.localScale = Vector3.one * projectileSize;
                    p.transform.position = transform.position;
                    Projectile j = p.GetComponent<Projectile>();
                    Rigidbody r = p.GetComponent<Rigidbody>();
                    if (j != null)
                        j.currentDamage = turretDamage.value;
                    p.SetActive(true);
                    Vector3 dir = (currentTarget.transform.position + (targetRigid.velocity.normalized * targetRigid.velocity.magnitude) - transform.position).normalized;
                    if (r != null && targetRigid != null)
                        r.AddForce(projectileForce * dir, ForceMode.Impulse);
                    TurretFiredGun?.Invoke();
                }
                currentRate = turretFireRate.value;
            }
        }
    }

    GameObject GetTarget()
    {
        GameObject closest = null;
        if (targets.Count > 0)
        {
            closest = targets[0];
            for (int i = 0; i < targets.Count; i++)
            {
                if (Vector3.Distance(transform.position, targets[i].transform.position) < Vector3.Distance(transform.position, closest.transform.position))
                    closest = targets[i];
                if (!targets[i].activeSelf)
                    RemoveTarget(targets[i]);
            }
        }        
        return closest;
    }

    void OnTriggerEnter(Collider other)
    {
        if (Utilities.IsInLayerMask(other.gameObject, targetLayers.value))
            targets.Add(other.gameObject);
    }

    void RemoveTarget(GameObject target) => targets.Remove(target);
}