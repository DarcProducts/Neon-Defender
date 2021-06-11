using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Vector3 cityLocation;

    [Header("Managers")]
    [SerializeField] private PlayerManager playerManager;

    [SerializeField] private BuildingManager BuildingManager;
    [SerializeField] private CityGenerator cityGenerator;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private SoundManager soundManager;

    private void Start()
    {
        if (cityGenerator != null)
            cityGenerator.CreateDarcGrid(Vector3.zero);
    }

    private void Update()
    {
        if (weaponManager != null)
            CheckWeapons();
        if (playerManager != null)
            CheckController();
    }

    private void CheckWeapons()
    {
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            weaponManager.ActivateLaser();
            if (soundManager != null)
                soundManager.PlayLaserShoot();
        }
        else
        {
            weaponManager.DeactivateLaser();
            if (soundManager != null)
                soundManager.StopLaserShoot();
        }
    }

    private void CheckController()
    {
        if (playerManager != null)
        {
            if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y > .1f)
                playerManager.TransformShip(Vector3.forward, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y);
            if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y < -.1f)
                playerManager.TransformShip(Vector3.forward, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y);

            if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x > .1f)
                playerManager.RotateShip(Vector3.up, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x);
            if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x < -.1f)
                playerManager.RotateShip(Vector3.down, -OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x);
            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
                playerManager.TransformShip(Vector3.up, 1);
            if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger))
                playerManager.TransformShip(Vector3.down, 1);
        }
    }
}