using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;


public class PlayerWeapon : MonoBehaviour
{
    public int maxAmmo = 3;
    public int ammo;

    [SerializeField] float bulletLifetime = 5f;    
    [SerializeField] GameObject bullet;
    [SerializeField] float coolDown;
    [SerializeField] TextMeshProUGUI ammoCounter;

    InputAction shootAction;

    List<GameObject> activeBullets = new List<GameObject>();

    bool canShoot = true;

    [Header("Audio")]
    [SerializeField] AudioClip fireSound;

    [Range(0f, 1f)]
    [SerializeField] float fireVolume = 1f;

    void Awake()
    {
        shootAction = InputSystem.actions.FindAction("Shoot");
        ammo = maxAmmo;
    }

    void Update()
    {
        //I am aware this is a cheap solution
        ammoCounter.text = ammo.ToString();

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

        if(AudioManager.instance != null)
            AudioManager.instance.PlaySFX(fireSound, fireVolume);

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
