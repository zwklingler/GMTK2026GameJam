using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

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

        camera.transform.position = cameraShopFocus.transform.position;
    }

    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] CinemachineCamera camera;
    [SerializeField] Transform cameraShopFocus;
    [SerializeField] GameObject shopUI;
    [SerializeField] PowerupSpawner powerupSpawner;
    [SerializeField] BackgroundProps backgroundProps;

    [Header("Ending")]
    [Tooltip("Panel that fades in once the ship reaches the black hole")]
    [SerializeField] CanvasGroup endingPanel;

    [Tooltip("Seconds the ending panel takes to fade in")]
    [SerializeField] float endingFadeTime = 2f;

    [Tooltip("Initial pull speed once the black hole appears")]
    [SerializeField] float blackHolePullSpeed = 5f;

    [Tooltip("How much the pull speeds up each second")]
    [SerializeField] float blackHolePullAcceleration = 8f;

    [Tooltip("How close to the center counts as swallowed")]
    [SerializeField] float blackHoleCaptureRadius = 0.5f;

    [Tooltip("How fast the ship spins as it goes in")]
    [SerializeField] float blackHoleSpinSpeed = 320f;

    [Tooltip("Distance from the center where the ship shrinks")]
    [SerializeField] float blackHoleShrinkDistance = 12f;


    [Header("Fuel UI")]
    [SerializeField] GameObject fuelUI;

    [Tooltip("Filled image to display percentage of fuel remaining")]
    [SerializeField] Image fuelBarFill;

    [Header("Points UI")]
    [Tooltip("Text showing the player points")]
    [SerializeField] TMP_Text pointsText;

    [Tooltip("Points earned per unit of height")]
    [SerializeField] float pointsPerHeight = 1f;

    [Header("Countdown UI")]
    [Tooltip("Text for the launch countdown")]
    [SerializeField] TMP_Text countdownText;

    [Tooltip("Seconds the blastoff message stays on screen after the count ends")]
    [SerializeField] float blastoffMessageTime = 1f;

    [System.Serializable]
    class Upgrade
    {
        [Tooltip("Button label text")]
        public string displayName = "Upgrade";

        [Tooltip("Cost of the first buy")]
        public int baseCost = 100;

        [Tooltip("Cost is multiplied by this after each purchase")]
        public float costMultiplier = 1.6f;

        [Tooltip("How many times this can be bought")]
        public int maxLevel = 5;

        [Tooltip("How much one level changes the stat")]
        public float valuePerLevel = 1f;

        [Tooltip("Button inside the group")]
        public Button button;

        [Tooltip("Text showing the upgrade name")]
        public TMP_Text nameText;

        [Tooltip("Text showing the current level")]
        public TMP_Text levelText;

        [Tooltip("Text showing the cost of the next purchase")]
        public TMP_Text costText;

        public int level;
    }

    [Header("Upgrades")]
    [Tooltip("Adds to the rocket's turn speed each level")]
    [SerializeField] Upgrade turnSpeedUpgrade;

    [Tooltip("Cuts fuel use per level (linearly)")]
    [SerializeField] Upgrade fuelEfficiencyUpgrade;

    [Tooltip("Adds to the powerup spawn chance each level")]
    [SerializeField] Upgrade maxSpeedUpgrade;
    [Tooltip("Adds to the powerup spawn chance each level")]
    [SerializeField] Upgrade powerupChanceUpgrade;

    [Tooltip("Toggle to enable and disable obstacle spawning")]
    [SerializeField] bool spawnObstacles = true;

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

    [Tooltip("Max sideways speed")]
    [SerializeField] float asteroidHorizontalSpeed = 2f;

    [Tooltip("Max tumble speed on each axis")]
    [SerializeField] float asteroidSpinSpeed = 2f;

    [Tooltip("Asteroids this far below the ship get cleaned up")]
    [SerializeField] float despawnDistanceBelowShip = 50f;

    [Header("Satellites")]
    [SerializeField] GameObject satellitePrefab;

    [Tooltip("Satellites start spawning at this height")]
    [SerializeField] float satelliteStartHeight = 50f;

    [Tooltip("Most satellites allowed alive at once")]
    [SerializeField] int maxSatellites = 2;

    [Tooltip("Random seconds between satellite spawns")]
    [SerializeField] Vector2 satelliteSpawnIntervalRange = new Vector2(1f, 8f);
    [Tooltip("How far above the ship they cross")]
    [SerializeField] Vector2 satelliteSpawnHeightRange = new Vector2(30f, 70f);

    [Tooltip("How far off to the side they enter from")]
    [SerializeField] float satelliteSpawnSideOffset = 70f;

    [Tooltip("Horizontal travel speed")]
    [SerializeField] Vector2 satelliteSpeedRange = new Vector2(10f, 40f);

    [Tooltip("Max vertical speed")]
    [SerializeField] float satelliteVerticalSpeed = 2f;

    [Tooltip("Max Y axis spin")]
    [SerializeField] float satelliteSpinSpeedY = 1f;

    [Tooltip("Random tilt on X axis")]
    [SerializeField] float satelliteRotationRangeX = 20f;

    [Tooltip("Random tilt on Y axis")]
    [SerializeField] float satelliteRotationRangeY = 0f;

    [Tooltip("Random tilt on Z axis")]
    [SerializeField] float satelliteRotationRangeZ = 20f;

    [Tooltip("Satellites below the ship get cleaned up")]
    [SerializeField] float satelliteDespawnDistanceBelowShip = 60f;
    [Tooltip("Satellites sideways from the ship get cleaned up")]
    [SerializeField] float satelliteDespawnDistanceSideways = 140f;

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

    public int Points { get; private set; }

    // Highest point reached on this life
    float runMaxHeight;

    float pointsCarry;

    bool endingActive;
    bool endingFadeStarted;
    Vector3 blackHoleTarget;
    float currentPullSpeed;
    Rigidbody shipBody;
    Vector3 shipBaseScale = Vector3.one;

    float baseTurnSpeed;
    float baseFuelUseSpeed;
    float baseMoveSpeed;
    float baseMaxSpeed;

    readonly List<GameObject> activeAsteroids = new List<GameObject>();
    float spawnTimer;

    readonly List<GameObject> activeSatellites = new List<GameObject>();
    float satelliteSpawnTimer;

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

        runMaxHeight = shipStartPosition.y;

        baseTurnSpeed = playerMovement.turnSpeed;
        baseFuelUseSpeed = playerMovement.fuelUseSpeed;
        baseMoveSpeed = playerMovement.moveSpeed;
        baseMaxSpeed = playerMovement.maxSpeed;

        shipBaseScale = playerMovement.transform.localScale;

        HookUpgradeButton(turnSpeedUpgrade);
        HookUpgradeButton(fuelEfficiencyUpgrade);
        HookUpgradeButton(powerupChanceUpgrade);
        HookUpgradeButton(maxSpeedUpgrade);

        ApplyUpgrades();
        RefreshShopUI();

        playerMovement.enabled = false;
        shopping = true;

        if(countdownText != null)
            countdownText.gameObject.SetActive(false);

        if(endingPanel != null)
        {
            endingPanel.alpha = 0f;
            endingPanel.interactable = false;
            endingPanel.blocksRaycasts = false;
            endingPanel.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        UpdateFuelUI();
        UpdatePointsUI();

        if(endingActive)
        {
            UpdateBlackHoleCapture();
            return;
        }

        if(shopping)
            return;

        Vector3 shipPosition = playerMovement.transform.position;
        float shipHeight = shipPosition.y;

        TrackHeightPoints(shipHeight);

        if(shipHeight >= asteroidStartHeight)
        {
            spawnTimer -= Time.deltaTime;

            if(spawnTimer <= 0f)
            {
                if (spawnObstacles)
                {
                    SpawnAsteroid();
                }
                spawnTimer = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            }
        }

        if(shipHeight >= satelliteStartHeight)
        {
            satelliteSpawnTimer -= Time.deltaTime;

            if(satelliteSpawnTimer <= 0f)
            {
                if (spawnObstacles)
                {
                    SpawnSatellite(shipPosition);
                }
                satelliteSpawnTimer = Random.Range(satelliteSpawnIntervalRange.x, satelliteSpawnIntervalRange.y);
            }
        }

        if(shipHeight >= ufoStartHeight)
        {
            ufoSpawnTimer -= Time.deltaTime;

            if(ufoSpawnTimer <= 0f)
            {
                if (spawnObstacles)
                {
                    SpawnUfo(shipPosition);
                }
                ufoSpawnTimer = Random.Range(ufoSpawnIntervalRange.x, ufoSpawnIntervalRange.y);
            }
        }

        if(powerupSpawner != null)
            powerupSpawner.Tick(shipPosition);

        CullAsteroids(shipHeight);
        CullSatellites(shipPosition);
        CullUfos(shipHeight);
    }

    void FixedUpdate()
    {
        if(shopping || endingActive)
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
        if(countdownText != null)
            countdownText.gameObject.SetActive(true);

        for(int i = time; i > 0; i--)
        {
            if(countdownText != null)
                countdownText.text = i.ToString();

            Debug.Log(i);

            await Awaitable.WaitForSecondsAsync(1);
        }

        if(countdownText != null)
        {
            countdownText.text = "BLAST OFF!";

            HideCountdownAfter(blastoffMessageTime);
        }
    }

    async void HideCountdownAfter(float seconds)
    {
        await Awaitable.WaitForSecondsAsync(seconds);

        if(countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    public void CrashRocket()
    {
        if(shopping || endingActive)
            return;

        ReturnToShop();
    }

    void ReturnToShop()
    {
        shopping = true;
        spawnTimer = 0f;
        satelliteSpawnTimer = 0f;
        ufoSpawnTimer = 0f;

        runMaxHeight = shipStartPosition.y;
        pointsCarry = 0f;

        playerMovement.enabled = false;
        playerMovement.ResetShip(shipStartPosition, shipStartRotation);

        ClearAsteroids();
        ClearSatellites();
        ClearUfos();

        if(powerupSpawner != null)
            powerupSpawner.ResetRun();

        if (backgroundProps != null)
            backgroundProps.ResetRun();

        camera.Follow = cameraShopFocus;
        shopUI.SetActive(true);

        RefreshShopUI();
    }

    public void BeginBlackHoleCapture(Transform blackHole)
    {
        if(endingActive)
            return;

        endingActive = true;
        endingFadeStarted = false;

        ClearAllObstacles();

        if(powerupSpawner != null)
            powerupSpawner.ResetRun();

        playerMovement.enabled = false;

        shipBody = playerMovement.GetComponent<Rigidbody>();

        if(shipBody != null)
        {
            shipBody.linearVelocity = Vector3.zero;
            shipBody.angularVelocity = Vector3.zero;
            shipBody.isKinematic = true;
        }

        blackHoleTarget = blackHole.position;
        blackHoleTarget.z = playerMovement.transform.position.z;

        currentPullSpeed = blackHolePullSpeed;

        if(endingPanel != null)
        {
            endingPanel.alpha = 0f;
            endingPanel.gameObject.SetActive(true);
        }
    }

    void UpdateBlackHoleCapture()
    {
        Transform ship = playerMovement.transform;

        currentPullSpeed += blackHolePullAcceleration * Time.deltaTime;

        ship.position = Vector3.MoveTowards(ship.position, blackHoleTarget, currentPullSpeed * Time.deltaTime);
        ship.Rotate(Vector3.forward * blackHoleSpinSpeed * Time.deltaTime, Space.Self);

        float distance = Vector3.Distance(ship.position, blackHoleTarget);

        if(blackHoleShrinkDistance > 0f)
        {
            float shrink = Mathf.Clamp01(distance / blackHoleShrinkDistance);
            ship.localScale = shipBaseScale * shrink;
        }

        if(distance <= blackHoleCaptureRadius && !endingFadeStarted)
        {
            endingFadeStarted = true;
            ship.localScale = Vector3.zero;
            FadeInEnding();
        }
    }

    async void FadeInEnding()
    {
        if(endingPanel == null)
            return;

        float elapsed = 0f;

        while(elapsed < endingFadeTime)
        {
            elapsed += Time.deltaTime;
            endingPanel.alpha = Mathf.Clamp01(elapsed / endingFadeTime);

            await Awaitable.NextFrameAsync();
        }

        endingPanel.alpha = 1f;
        endingPanel.interactable = true;
        endingPanel.blocksRaycasts = true;
    }

    void TrackHeightPoints(float shipHeight)
    {
        if(shipHeight <= runMaxHeight)
            return;

        pointsCarry += (shipHeight - runMaxHeight) * pointsPerHeight;
        runMaxHeight = shipHeight;

        int earned = Mathf.FloorToInt(pointsCarry);

        if(earned > 0)
        {
            Points += earned;
            pointsCarry -= earned;
        }
    }

    public void CollectPowerup(GameObject powerupObject)
    {
        if(powerupSpawner != null)
            powerupSpawner.Collect(powerupObject);
    }

    public void ClearAllObstacles()
    {
        ClearAsteroids();
        ClearSatellites();
        ClearUfos();
    }

    public void AddPoints(int amount)
    {
        if(amount <= 0)
            return;

        Points += amount;
        UpdatePointsUI();
    }

    public bool SpendPoints(int amount)
    {
        if(amount <= 0 || Points < amount)
            return false;

        Points -= amount;
        return true;
    }

    void HookUpgradeButton(Upgrade upgrade)
    {
        if(upgrade == null || upgrade.button == null)
            return;
        upgrade.button.onClick.AddListener(() => TryPurchase(upgrade));
    }

    int GetUpgradeCost(Upgrade upgrade)
    {
        return Mathf.RoundToInt(upgrade.baseCost * Mathf.Pow(upgrade.costMultiplier, upgrade.level));
    }

    void TryPurchase(Upgrade upgrade)
    {
        if(upgrade == null || upgrade.level >= upgrade.maxLevel)
            return;

        // SpendPoints only deducts if the player can afford it
        if(!SpendPoints(GetUpgradeCost(upgrade)))
            return;

        upgrade.level++;

        ApplyUpgrades();
        UpdatePointsUI();
        RefreshShopUI();
    }

    void ApplyUpgrades()
    {
        //linear
        playerMovement.turnSpeed = baseTurnSpeed + turnSpeedUpgrade.level * turnSpeedUpgrade.valuePerLevel;

        //linear
        playerMovement.fuelUseSpeed = baseFuelUseSpeed - Mathf.Min(fuelEfficiencyUpgrade.valuePerLevel * fuelEfficiencyUpgrade.level, baseFuelUseSpeed);

        //linear
        playerMovement.maxSpeed = baseMaxSpeed + maxSpeedUpgrade.level * maxSpeedUpgrade.valuePerLevel;

        if(powerupSpawner != null && powerupChanceUpgrade != null)
            powerupSpawner.SpawnChanceBonus = powerupChanceUpgrade.level * powerupChanceUpgrade.valuePerLevel;
    }

    void RefreshShopUI()
    {
        RefreshUpgradeUI(turnSpeedUpgrade);
        RefreshUpgradeUI(fuelEfficiencyUpgrade);
        RefreshUpgradeUI(powerupChanceUpgrade);
        RefreshUpgradeUI(maxSpeedUpgrade);
    }

    void RefreshUpgradeUI(Upgrade upgrade)
    {
        if(upgrade == null)
            return;

        bool maxed = upgrade.level >= upgrade.maxLevel;
        int cost = GetUpgradeCost(upgrade);

        if(upgrade.nameText != null)
            upgrade.nameText.text = upgrade.displayName;

        if(upgrade.levelText != null)
            upgrade.levelText.text = "Lv " + upgrade.level;

        if(upgrade.costText != null)
            upgrade.costText.text = maxed ? "MAX" : cost + " pts";

        // Greys the button out when it's maxed or unaffordable
        if(upgrade.button != null)
            upgrade.button.interactable = !maxed && Points >= cost;
    }

    void UpdatePointsUI()
    {
        if(pointsText != null)
            pointsText.text = Points.ToString() + "pts";
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
            body.linearVelocity = new Vector3(Random.Range(-asteroidHorizontalSpeed, asteroidHorizontalSpeed), -Random.Range(asteroidFallSpeed.x, asteroidFallSpeed.y), 0f);
            body.angularVelocity = new Vector3(Random.Range(-asteroidSpinSpeed, asteroidSpinSpeed), Random.Range(-asteroidSpinSpeed, asteroidSpinSpeed), Random.Range(-asteroidSpinSpeed, asteroidSpinSpeed));
        }

        activeAsteroids.Add(asteroid);
    }

    void SpawnSatellite(Vector3 shipPosition)
    {
        if(satellitePrefab == null)
            return;

        if(activeSatellites.Count >= maxSatellites)
            return;
        // Randomly decide which side to spawn on
        float side = Random.value < 0.5f ? -1f : 1f;

        Vector3 spawnPosition = new Vector3(shipPosition.x + side * satelliteSpawnSideOffset, shipPosition.y + Random.Range(satelliteSpawnHeightRange.x, satelliteSpawnHeightRange.y), 0f);

        Vector3 tilt = new Vector3(Random.Range(-satelliteRotationRangeX, satelliteRotationRangeX), Random.Range(-satelliteRotationRangeY, satelliteRotationRangeY), Random.Range(-satelliteRotationRangeZ, satelliteRotationRangeZ));

        Quaternion rotation = satellitePrefab.transform.rotation * Quaternion.Euler(tilt);

        GameObject satellite = Instantiate(satellitePrefab, spawnPosition, rotation);

        if(satellite.TryGetComponent(out Rigidbody body))
        {
            float speed = Random.Range(satelliteSpeedRange.x, satelliteSpeedRange.y);
            body.linearVelocity = new Vector3(-side * speed, Random.Range(-satelliteVerticalSpeed, satelliteVerticalSpeed), 0f);
            body.angularVelocity = new Vector3(0f, Random.Range(-satelliteSpinSpeedY, satelliteSpinSpeedY) * Mathf.Deg2Rad, 0f);
            body.angularDamping = 0f;
        }


        activeSatellites.Add(satellite);
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

    void CullSatellites(Vector3 shipPosition)
    {
        for(int i = activeSatellites.Count - 1; i >= 0; i--)
        {
            GameObject satellite =  activeSatellites[i];

            if(satellite == null)
            {
                activeSatellites.RemoveAt(i);
                continue;
            }

            Vector3 position = satellite.transform.position;

            bool tooLow = position.y < shipPosition.y - satelliteDespawnDistanceBelowShip;
            bool tooFarSideways = Mathf.Abs(position.x - shipPosition.x) > satelliteDespawnDistanceSideways;

            if(tooLow || tooFarSideways)
            {
                Destroy(satellite);
                activeSatellites.RemoveAt(i);
            }
        }
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

    void ClearSatellites()
    {
        foreach(GameObject satellite in activeSatellites)
        {
            if(satellite != null)
                Destroy(satellite);
        }

        activeSatellites.Clear();
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