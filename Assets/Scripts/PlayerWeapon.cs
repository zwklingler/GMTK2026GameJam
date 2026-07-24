using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviour
{
    public int ammo = 3;
    [SerializeField] GameObject bullet;
    [SerializeField] float coolDown;

    InputAction shootAction;
    bool canShoot = true;

    void Awake()
    {
        shootAction = InputSystem.actions.FindAction("Shoot");
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

        Instantiate(bullet, transform.position, transform.rotation);

        await Awaitable.WaitForSecondsAsync(coolDown);

        canShoot = true;
    }
}
