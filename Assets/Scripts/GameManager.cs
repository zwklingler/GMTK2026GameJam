using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Fuel UI")]
    [SerializeField] GameObject fuelUI;
 
    [Tooltip("Filled image to display percentage of fuel remaining")]
    [SerializeField] Image fuelBarFill;

    [Header("Asteroids")]
    [SerializeField] GameObject asteroidPrefab;

    [Tooltip("Asteroids only start spawning at this height")]
    [SerializeField] float asteroidStartHeight = 200f;

    [Tooltip("Most asteroids allowed alive at once")]
    [SerializeField] int maxAsteroids = 30;

    [Tooltip("Random seconds between spawns (min, max)")]
    [SerializeField] Vector2 spawnIntervalRange = new Vector2(0.3f, 1.2f);

    [Tooltip("How far above the ship asteroids appear")]
    [SerializeField] float spawnHeightAboveShip = 40f;

    [Tooltip("How far left and right of the ship they can spawn")]
    [SerializeField] float spawnSpread = 25f;

    [Tooltip("Random size multiplier applied to the prefab (min, max)")]
    [SerializeField] Vector2 asteroidSizeRange = new Vector2(0.6f, 2f);

    [Tooltip("Random downward speed range")]
    [SerializeField] Vector2 asteroidFallSpeed = new Vector2(3f, 10f);

    [Tooltip("Max tumble speed on each axis")]
    [SerializeField] float asteroidSpinSpeed = 2f;

    [Tooltip("Asteroids this far below the ship get cleaned up")]
    [SerializeField] float despawnDistanceBelowShip = 50f;

    [Header("UFOs")]
    [SerializeField] GameObject ufoPrefab;

    [Tooltip("UFOs start spawning at this height")]
    [SerializeField] float ufoStartHeight = 400f;

    [Tooltip("Most UFOs allowed alive at once")]
    [SerializeField] int maxUfos = 2;

    [Tooltip("Random seconds between UFO spawns")]
    [SerializeField] Vector2 ufoSpawnIntervalRange = new Vector2(8f, 16f);

    [Tooltip("How far above the rocket a UFO tries to sit")]
    [SerializeField] float ufoHoverHeight = 25f;

    [Tooltip("How far to either side UFOs stagger themselves")]
    [SerializeField] float ufoFormationSpread = 8f;

    [Tooltip("UFO chase speed toward the ship")]
    [SerializeField] float ufoFollowSpeed = 6f;

    [Tooltip("Vertical speed while lining up")]
    [SerializeField] float ufoVerticalSpeed = 7f;

    [Tooltip("How closely a UFO must be lined up above the ship before it commits to a dive")]
    [SerializeField] float ufoAlignThreshold = 2.5f;

    [Tooltip("Telegraph time for UFO dive")]
    [SerializeField] float ufoDiveDelay = 0.7f;

    [Tooltip("How fast the UFO drops during a dive")]
    [SerializeField] float ufoDiveSpeed = 20f;

    [Tooltip("How hard the UFO steers towards the rocket mid-dive")]
    [SerializeField] float ufoDiveTrackSpeed = 4f;

    [Tooltip("How far past the ship a dive continues before the UFO gives up")]
    [SerializeField] float ufoDiveDepthBelowShip = 15f;

    [Tooltip("Max UFO dive duration before it stops")]
    [SerializeField] float ufoDiveMaxDuration = 3f;

    [Tooltip("Speed while climbing back up after a dive")]
    [SerializeField] float ufoRecoverSpeed = 14f;

    [Tooltip("How far off to the side UFOs enter from")]
    [SerializeField] float ufoSpawnSideOffset = 45f;

    [Tooltip("UFOs this far below the ship get cleaned up")]
    [SerializeField] float ufoDespawnDistanceBelowShip = 80f;

    [Tooltip("Possible tilt angle for the UFO")]
    [SerializeField] float ufoTiltAngle = 8f;

    [Tooltip("Movement speed at which the UFO reaches its full bank angle")]
    [SerializeField] float ufoTiltReferenceSpeed = 10f;

    [Tooltip("How quickly the UFO changes to a new tilt")]
    [SerializeField] float ufoTiltSpeed = 120f;

    [Tooltip("Speed of the base hover wobble")]
    [SerializeField] float ufoWobbleSpeed = 1.5f;

    bool shopping = true;
    InputAction upAction;

    readonly List<GameObject> activeAsteroids = new List<GameObject>();
    float spawnTimer;

    enum UfoState
    {
        Chase, WindUp, Dive, Recover
    }

    class ActiveUfo
    {
        public GameObject gameObject;
        public Rigidbody body;
        public float offsetX;
        public UfoState state;
        public float timer;
        public Quaternion baseRotation;
        public float noiseSeed;
    }

    readonly List<ActiveUfo> activeUfos = new List<ActiveUfo>();
    float ufoSpawnTimer;

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
        UpdateFuelUI();

        if(shopping)
            return;

        Vector3 shipPosition = playerMovement.transform.position;
        float shipHeight = shipPosition.y;

        if(shipHeight >= asteroidStartHeight)
        {
            spawnTimer -= Time.deltaTime;

            if(spawnTimer <= 0f)
            {
                SpawnAsteroid();
                spawnTimer = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            }
        }

        if(shipHeight >= ufoStartHeight)
        {
            ufoSpawnTimer -= Time.deltaTime;

            if(ufoSpawnTimer <= 0f)
            {
                SpawnUfo(shipPosition);
                ufoSpawnTimer = Random.Range(ufoSpawnIntervalRange.x, ufoSpawnIntervalRange.y);
            }
        }

        CullAsteroids(shipHeight);
        CullUfos(shipHeight);
    }

    void FixedUpdate()
    {
        if(shopping)
            return;

        UpdateUfos(playerMovement.transform.position);
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
        ufoSpawnTimer = 0f;

        playerMovement.enabled = false;
        playerMovement.ResetShip(shipStartPosition, shipStartRotation);

        ClearAsteroids();
        ClearUfos();

        camera.Follow = cameraShopFocus;
        shopUI.SetActive(true);
    }

    void UpdateFuelUI()
    {
        float ratio = playerMovement.maxFuel > 0f ? Mathf.Clamp01(playerMovement.fuel / playerMovement.maxFuel) : 0f;
 
        if(fuelBarFill != null)
        {
            fuelBarFill.fillAmount = ratio;
        }
 
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

    void SpawnUfo(Vector3 shipPosition)
    {
        if(ufoPrefab == null)
            return;

        if(activeUfos.Count >= maxUfos)
            return;

        // Enter from one side
        float side = Random.value < 0.5f ? -1f : 1f;

        Vector3 spawnPosition = new Vector3(
            shipPosition.x + side * ufoSpawnSideOffset,
            shipPosition.y + ufoHoverHeight + Random.Range(10f, 30f),
            0f);

        GameObject ufo = Instantiate(ufoPrefab, spawnPosition, ufoPrefab.transform.rotation);

        ufo.TryGetComponent(out Rigidbody ufoBody);

        activeUfos.Add(new ActiveUfo
        {
            gameObject = ufo,
            body = ufoBody,
            offsetX = Random.Range(-ufoFormationSpread, ufoFormationSpread),
            state = UfoState.Chase,
            baseRotation = ufoPrefab.transform.rotation,
            noiseSeed = Random.Range(0f, 100f)
        });
    }

    void UpdateUfos(Vector3 shipPosition)
    {
        float delta = Time.fixedDeltaTime;

        for(int i = activeUfos.Count - 1; i >= 0; i--)
        {
            ActiveUfo ufo = activeUfos[i];

            if(ufo.gameObject == null)
            {
                activeUfos.RemoveAt(i);
                continue;
            }

            Vector3 position = ufo.gameObject.transform.position;

            switch(ufo.state)
            {
                case UfoState.Chase:
                    position = MoveTowardHoverPoint(position, shipPosition, ufo.offsetX, delta);

                    // Commit to dive the rocket if it is aligned
                    bool linedUp = Mathf.Abs(position.x - (shipPosition.x + ufo.offsetX)) < ufoAlignThreshold && Mathf.Abs(position.y - (shipPosition.y + ufoHoverHeight)) < ufoAlignThreshold && position.y > shipPosition.y;

                    if(linedUp)
                    {
                        ufo.state = UfoState.WindUp;
                        ufo.timer = ufoDiveDelay;
                    }
                    break;

                case UfoState.WindUp:
                    position = MoveTowardHoverPoint(position, shipPosition, ufo.offsetX, delta);

                    ufo.timer -= delta;

                    if(ufo.timer <= 0f)
                    {
                        ufo.state = UfoState.Dive;
                        ufo.timer = ufoDiveMaxDuration;
                    }
                    break;

                case UfoState.Dive:
                    // Dive straight down
                    position.y -= ufoDiveSpeed * delta;
                    position.x = Mathf.MoveTowards(position.x, shipPosition.x, ufoDiveTrackSpeed * delta);

                    ufo.timer -= delta;

                    bool missed = position.y < shipPosition.y - ufoDiveDepthBelowShip;

                    if(missed || ufo.timer <= 0f)
                    {
                        ufo.state = UfoState.Recover;

                        // Pick a side to move towards
                        float side = position.x < shipPosition.x ? -1f : 1f;
                        ufo.offsetX = side * Random.Range(ufoFormationSpread, ufoFormationSpread * 2f);
                    }
                    break;

                case UfoState.Recover:
                    position.x = Mathf.MoveTowards(position.x, shipPosition.x + ufo.offsetX, ufoRecoverSpeed * delta);

                    bool clearOfShip = Mathf.Abs(position.x - shipPosition.x) > ufoAlignThreshold * 2f;

                    if(clearOfShip)
                        position.y = Mathf.MoveTowards(position.y, shipPosition.y + ufoHoverHeight, ufoRecoverSpeed * delta);

                    bool backInPosition = clearOfShip
                                       && Mathf.Abs(position.y - (shipPosition.y + ufoHoverHeight)) < ufoAlignThreshold;

                    if(backInPosition)
                    {
                        ufo.state = UfoState.Chase;
                        ufo.offsetX = Random.Range(-ufoFormationSpread, ufoFormationSpread);
                    }
                    break;
            }

            position.z = 0f;

            Quaternion rotation = CalculateUfoRotation(ufo, position, delta);

            if(ufo.body != null)
            {
                ufo.body.MovePosition(position);
                ufo.body.MoveRotation(rotation);
            }
            else
            {
                ufo.gameObject.transform.SetPositionAndRotation(position, rotation);
            }
        }
    }
    Quaternion CalculateUfoRotation(ActiveUfo ufo, Vector3 nextPosition, float delta)
    {
        Transform ufoTransform = ufo.gameObject.transform;

        float bankBudget = ufoTiltAngle * 0.6f;
        float wobbleBudget = ufoTiltAngle * 0.4f;

        Vector3 velocity = (nextPosition - ufoTransform.position) / delta;

        float roll = Mathf.Clamp(-velocity.x / ufoTiltReferenceSpeed, -1f, 1f) * bankBudget;
        float pitch = Mathf.Clamp(velocity.y / ufoTiltReferenceSpeed, -1f, 1f) * bankBudget;

        float noiseTime = Time.time * ufoWobbleSpeed + ufo.noiseSeed;

        float wobblePitch = (Mathf.PerlinNoise(noiseTime, 0f) - 0.5f) * 2f * wobbleBudget;
        float wobbleRoll = (Mathf.PerlinNoise(0f, noiseTime) - 0.5f) * 2f * wobbleBudget;

        Quaternion target = ufo.baseRotation * Quaternion.Euler(pitch + wobblePitch, 0f, roll + wobbleRoll);

        return Quaternion.RotateTowards(ufoTransform.rotation, target, ufoTiltSpeed * delta);
    }

    Vector3 MoveTowardHoverPoint(Vector3 position, Vector3 shipPosition, float offsetX, float delta)
    {
        float targetX = shipPosition.x + offsetX;
        float targetY = shipPosition.y + ufoHoverHeight;

        position.x = Mathf.MoveTowards(position.x, targetX, ufoFollowSpeed * delta);
        position.y = Mathf.MoveTowards(position.y, targetY, ufoVerticalSpeed * delta);

        return position;
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

    void CullUfos(float shipHeight)
    {
        for(int i = activeUfos.Count - 1; i >= 0; i--)
        {
            ActiveUfo ufo = activeUfos[i];

            if(ufo.gameObject == null)
            {
                activeUfos.RemoveAt(i);
                continue;
            }

            if(ufo.gameObject.transform.position.y < shipHeight - ufoDespawnDistanceBelowShip)
            {
                Destroy(ufo.gameObject);
                activeUfos.RemoveAt(i);
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

    void ClearUfos()
    {
        foreach(ActiveUfo ufo in activeUfos)
        {
            if(ufo.gameObject != null)
                Destroy(ufo.gameObject);
        }

        activeUfos.Clear();
    }
}