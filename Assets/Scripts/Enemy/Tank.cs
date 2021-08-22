using UnityEngine;
using UnityEngine.Events;

public enum TankState { moving, attacking }

public class Tank : AIUtilities, IDamagable<float>
{
    public static UnityAction<Vector3, byte, byte, float> TextInfo;
    [SerializeField] TankState state;
    [SerializeField] FloatVariable checkDistance;
    [SerializeField] IntVariable maxHealth;
    [SerializeField] FloatVariable tankSpeed;
    [SerializeField] FloatVariable rotateSpeed;
    [SerializeField] Transform barrelTip;
    [SerializeField] FloatVariable mortarDamage;
    [SerializeField] FloatVariable mortarRadius;
    [SerializeField] LayerMaskVariable targetLayers;
    [SerializeField] FloatVariable fireRate;
    [SerializeField] CityGenerator cityGenerator;
    [SerializeField] ObjectPooler explosionPool;
    [SerializeField] ObjectPooler rocketPool;
    [SerializeField] FloatVariable explosionSize;
    [SerializeField] FXInitializer shootFX;
    float currentFireRate = 0f;
    Vector3 targetLocation;
    GameObject currentTarget = null;
    float currentHealth;

    void Start() => InitialSetup();

    void OnEnable() => InitialSetup();

    void InitialSetup()
    {
        currentTarget = null;
        if (fireRate != null)
            currentFireRate = fireRate.Value;
        if (cityGenerator != null)
            targetLocation = cityGenerator.GetAttackVector();
        transform.LookAt(targetLocation);
        if (maxHealth != null)
            currentHealth = maxHealth.Value;
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case TankState.moving:
                if (currentTarget != null)
                    state = TankState.attacking;
                if (currentTarget == null)
                    FindRandomCloseTarget();
                if (transform.position == targetLocation)
                {
                    if (cityGenerator != null)
                        targetLocation = cityGenerator.GetAttackVector();
                }
                else
                {
                    if (tankSpeed != null && rotateSpeed != null)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, targetLocation, tankSpeed.Value * Time.fixedDeltaTime);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetLocation - transform.position), rotateSpeed.Value * Time.fixedDeltaTime);
                    }
                }
                break;

            case TankState.attacking:
                AttackTarget();
                break;

            default:
                break;
        }
    }

    void AttackTarget()
    {
        if (currentTarget != null)
        {
            if (currentTarget.activeSelf && rotateSpeed != null)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(currentTarget.transform.position - transform.position), rotateSpeed.Value * Time.fixedDeltaTime);
                targetLocation = currentTarget.transform.position;
                currentFireRate = currentFireRate < 0 ? 0 : currentFireRate -= Time.fixedDeltaTime;
                if (currentFireRate.Equals(0))
                {
                    LaunchMortar();
                    if (shootFX != null && barrelTip != null)
                        shootFX.PlayAllFX(barrelTip.position);
                    if (fireRate != null)
                        currentFireRate = fireRate.Value;
                }
            }
            if (!currentTarget.activeSelf)
                FindRandomCloseTarget();
        }
        if (currentTarget == null)
        {
            state = TankState.moving;
            return;
        }
    }

    void FindRandomCloseTarget()
    {
        if (checkDistance != null && targetLayers != null)
        {
            Collider[] closeObjects = Physics.OverlapSphere(transform.position, checkDistance.Value, targetLayers.value);
            if (closeObjects.Length > 0)
            {
                int randObj = Random.Range(0, closeObjects.Length);
                currentTarget = closeObjects[randObj].gameObject;
                return;
            }
        }
        currentTarget = null;
    }

    void LaunchMortar()
    {
        GameObject r = rocketPool.GetObject();
        Rocket rocket = r.GetComponent<Rocket>();
        if (r != null && rocket != null && barrelTip != null)
        {
            r.transform.position = barrelTip.position;
            rocket.type = RocketType.Mortar;
            rocket.currentTarget = currentTarget.transform.position + Vector3.up * 45f;
            rocket.rocketSpeed = 1200f;
            if (mortarDamage != null)
                rocket.rocketDamage = mortarDamage.Value;
            if (mortarRadius != null)
                rocket.explosionRadius = mortarRadius.Value;
            r.SetActive(true);
        }
    }

    public void ApplyDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0 && maxHealth != null)
        {
                GameObject e = explosionPool.GetObject();
                if (e != null && explosionSize != null)
                {
                    float eSize = explosionSize.Value;
                    e.transform.position = transform.position;
                    e.transform.localScale = new Vector3(eSize, eSize, eSize);
                    e.SetActive(true);
                }

            TextInfo?.Invoke(transform.position, 2, 1, maxHealth.Value * 2);
            gameObject.SetActive(false);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (targetLayers != null)
            if (Utilities.IsInLayerMask(other.gameObject, targetLayers.value))
                if (currentTarget == null)
                    FindRandomCloseTarget();
    }

    [ContextMenu("Kill Tank")]
    public void KillTank()
    {
        if (maxHealth != null)
            ApplyDamage(maxHealth.Value);
    }

    public float GetCurrentHealth() => currentHealth;
}