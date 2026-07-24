using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


public class PlayerWeapon : MonoBehaviour
{
    public int maxAmmo = 3;
    public int ammo;

    [SerializeField] float bulletLifetime = 5f;    
    [SerializeField] GameObject bullet;
    [SerializeField] float coolDown;

    InputAction shootAction;

    List<GameObject> activeBullets = new List<GameObject>();

    bool canShoot = true;

    void Awake()
    {
        shootAction = InputSystem.actions.FindAction("Shoot");
        ammo = maxAmmo;
    }

    void Update()
    {
        if(shootAction.WasPressedThisFrame())
        {
            TryShoot();
        }
    }

    async void TryShoot()
    {
        if(!canShoot || ammo < 1)
        {
            return;
        }

        ammo--;
        canShoot = false;

        // Use the prefab's rotation
        Quaternion rotation = transform.rotation * bullet.transform.rotation;
        GameObject spawnedBullet = Instantiate(bullet, transform.position, rotation);
        activeBullets.Add(spawnedBullet);

        Destroy(spawnedBullet, bulletLifetime);


        await Awaitable.WaitForSecondsAsync(coolDown);

        canShoot = true;
    }

    public void ResetWeapon()
    {
        ammo = maxAmmo;

        canShoot = true;

        foreach(GameObject spawned in activeBullets)
        {
            if(spawned != null)
                Destroy(spawned);
        }

        activeBullets.Clear();
    }
}
