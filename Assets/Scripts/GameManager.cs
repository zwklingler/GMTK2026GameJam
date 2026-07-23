using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    static public GameManager instance;

    void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }

    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] CinemachineCamera camera;
    [SerializeField] Transform cameraShopFocus;
    [SerializeField] GameObject shopUI;

    [Header("Asteroids")]
    [SerializeField] GameObject asteroidPrefab;

    [Tooltip("Asteroids only start spawning at this height")]
    [SerializeField] float asteroidStartHeight = 200f;

    [Tooltip("Most asteroids allowed alive at once")]
    [SerializeField] int maxAsteroids = 30;

    [Tooltip("Random seconds between spawns")]
    [SerializeField] Vector2 spawnIntervalRange = new Vector2(0.3f, 1.2f);

    [Tooltip("How far above the ship asteroids appear")]
    [SerializeField] float spawnHeightAboveShip = 40f;

    [Tooltip("How far left and right of the ship they can spawn")]
    [SerializeField] float spawnSpread = 25f;

    [Tooltip("Random size multiplier applied to the prefab")]
    [SerializeField] Vector2 asteroidSizeRange = new Vector2(1f, 2f);

    [Tooltip("Random downward speed range")]
    [SerializeField] Vector2 asteroidFallSpeed = new Vector2(3f, 10f);

    [Tooltip("Max tumble speed on each axis")]
    [SerializeField] float asteroidSpinSpeed = 2f;

    [Tooltip("Asteroids this far below the ship get cleaned up")]
    [SerializeField] float despawnDistanceBelowShip = 50f;

    bool shopping = true;
    InputAction upAction;

    readonly List<GameObject> activeAsteroids = new List<GameObject>();
    float spawnTimer;

    Vector3 shipStartPosition;
    Quaternion shipStartRotation;

    void Start()
    {
        upAction = InputSystem.actions.FindAction("Up");
        upAction.performed += StartRocket;

        // Remember where the ship started to put it back after a crash
        shipStartPosition = playerMovement.transform.position;
        shipStartRotation = playerMovement.transform.rotation;

        playerMovement.enabled = false;
        shopping = true;
    }

    void Update()
    {
        if(shopping)
            return;

        float shipHeight = playerMovement.transform.position.y;

        if(shipHeight >= asteroidStartHeight)
        {
            spawnTimer -= Time.deltaTime;

            if(spawnTimer <= 0f)
            {
                SpawnAsteroid();
                spawnTimer = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            }
        }

        CullAsteroids(shipHeight);
    }

    async void StartRocket(InputAction.CallbackContext context)
    {
        if(!shopping)
            return;

        shopping = false;
        shopUI.SetActive(false);
        camera.Follow = playerMovement.transform;

        await Countdown(3);

        playerMovement.enabled = true;
    }

    async Awaitable Countdown(int time)
    {
        for(int i = time; i > 0; i--)
        {
            //TODO display numbers
            Debug.Log(i);

            await Awaitable.WaitForSecondsAsync(1);
        }

        //TODO display blastoff or something
    }

    public void CrashRocket()
    {
        if(shopping)
            return;

        ReturnToShop();
    }

    void ReturnToShop()
    {
        shopping = true;
        spawnTimer = 0f;

        playerMovement.enabled = false;
        playerMovement.ResetShip(shipStartPosition, shipStartRotation);

        ClearAsteroids();

        camera.Follow = cameraShopFocus;
        shopUI.SetActive(true);
    }

    void SpawnAsteroid()
    {
        if(asteroidPrefab == null)
            return;

        if(activeAsteroids.Count >= maxAsteroids)
            return;

        Vector3 shipPosition = playerMovement.transform.position;

        Vector3 spawnPosition = new Vector3(
            shipPosition.x + Random.Range(-spawnSpread, spawnSpread),
            shipPosition.y + spawnHeightAboveShip,
            0f);

        GameObject prefab = asteroidPrefab;
        GameObject asteroid = Instantiate(prefab, spawnPosition, Random.rotation);

        float size = Random.Range(asteroidSizeRange.x, asteroidSizeRange.y);
        asteroid.transform.localScale *= size;

        if(asteroid.TryGetComponent(out Rigidbody body))
        {
            body.linearVelocity = Vector3.down * Random.Range(asteroidFallSpeed.x, asteroidFallSpeed.y);
            body.angularVelocity = new Vector3(Random.Range(-asteroidSpinSpeed, asteroidSpinSpeed), Random.Range(-asteroidSpinSpeed, asteroidSpinSpeed), Random.Range(-asteroidSpinSpeed, asteroidSpinSpeed));
        }

        activeAsteroids.Add(asteroid);
    }

    void CullAsteroids(float shipHeight)
    {
        for(int i = activeAsteroids.Count - 1; i >= 0; i--)
        {
            GameObject asteroid = activeAsteroids[i];

            if(asteroid == null)
            {
                activeAsteroids.RemoveAt(i);
                continue;
            }

            if(asteroid.transform.position.y < shipHeight - despawnDistanceBelowShip)
            {
                Destroy(asteroid);
                activeAsteroids.RemoveAt(i);
            }
        }
    }

    void ClearAsteroids()
    {
        foreach(GameObject asteroid in activeAsteroids)
        {
            if(asteroid != null)
                Destroy(asteroid);
        }

        activeAsteroids.Clear();
    }
}