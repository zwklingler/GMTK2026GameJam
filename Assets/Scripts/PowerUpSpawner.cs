using System.Collections.Generic;
using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    enum PowerupType
    {
        Fuel, Points, Bomb
    }

    [System.Serializable]
    class PowerupDefinition
    {
        public GameObject prefab;

        [Tooltip("Relative chance against other powerups")]
        public float weight = 1f;

        [Tooltip("Points relative to the power up, if applicable")]
        public float value = 25f;
    }

    [SerializeField] PlayerMovement playerMovement;

    [Header("Spawning")]
    [Tooltip("Powerups start spawning at this height")]
    [SerializeField] float powerupStartHeight = 100f;

    [Tooltip("Seconds between spawn attempts")]
    [SerializeField] Vector2 powerupSpawnIntervalRange = new Vector2(6f, 12f);

    [Tooltip("Chance an attempt spawns something")]
    [Range(0f, 1f)]
    [SerializeField] float basePowerupSpawnChance = 0.35f;

    [Tooltip("Most powerups spawned at once")]
    [SerializeField] int maxPowerups = 3;

    [Tooltip("How far above the ship they spawn")]
    [SerializeField] float powerupSpawnHeightAboveShip = 45f;

    [Tooltip("How far left and right of the ship they spawn")]
    [SerializeField] float powerupSpawnSpread = 22f;

    [Tooltip("Downward fall speed")]
    [SerializeField] float powerupFallSpeed = 4f;

    [Tooltip("Spin speed for the powerup's rb")]
    [SerializeField] float powerupSpinSpeed = 60f;

    [Tooltip("Powerups range below ship for removing")]
    [SerializeField] float powerupDespawnDistanceBelowShip = 50f;

    [Header("Types")]
    [Tooltip("Gas can")]
    [SerializeField] PowerupDefinition fuelPowerup;

    [Tooltip("Points collectible")]
    [SerializeField] PowerupDefinition pointsPowerup;

    [Tooltip("Bomb obstacle removal")]
    [SerializeField] PowerupDefinition bombPowerup;

    class ActivePowerup
    {
        public GameObject gameObject;
        public PowerupType type;
        public float value;
    }

    readonly List<ActivePowerup> activePowerups = new List<ActivePowerup>();
    float powerupSpawnTimer;

    public float SpawnChanceBonus { get; set; }

    float CurrentSpawnChance
    {
        get { return Mathf.Clamp01(basePowerupSpawnChance + SpawnChanceBonus); }
    }

    public void Tick(Vector3 shipPosition)
    {
        if(shipPosition.y >= powerupStartHeight)
        {
            powerupSpawnTimer -= Time.deltaTime;

            if(powerupSpawnTimer <= 0f)
            {
                TrySpawn(shipPosition);
                powerupSpawnTimer = Random.Range(powerupSpawnIntervalRange.x, powerupSpawnIntervalRange.y);
            }
        }

        Cull(shipPosition.y);
    }

    public void ResetRun()
    {
        powerupSpawnTimer = 0f;
        ClearAll();
    }

    void TrySpawn(Vector3 shipPosition)
    {
        if(activePowerups.Count >= maxPowerups)
            return;

        if(Random.value > CurrentSpawnChance)
            return;

        PowerupType type;
        PowerupDefinition definition = Choose(out type);

        if(definition == null)
            return;

        Vector3 spawnPosition = new Vector3(
            shipPosition.x + Random.Range(-powerupSpawnSpread, powerupSpawnSpread),
            shipPosition.y + powerupSpawnHeightAboveShip,
            0f);

        GameObject powerup = Instantiate(definition.prefab, spawnPosition, definition.prefab.transform.rotation);

        if(powerup.TryGetComponent(out Rigidbody body))
        {
            body.useGravity = false;
            body.linearVelocity = Vector3.down * powerupFallSpeed;
            body.angularVelocity = new Vector3(0f, powerupSpinSpeed * Mathf.Deg2Rad, 0f);
            body.angularDamping = 0f;
        }

        activePowerups.Add(new ActivePowerup
        {
            gameObject = powerup,
            type = type,
            value = definition.value
        });
    }

    PowerupDefinition Choose(out PowerupType type)
    {
        float fuelWeight = Weight(fuelPowerup);
        float pointsWeight = Weight(pointsPowerup);
        float bombWeight = Weight(bombPowerup);

        float total = fuelWeight + pointsWeight + bombWeight;

        type = PowerupType.Fuel;

        if(total <= 0f)
            return null;

        float roll = Random.value * total;

        if(roll < fuelWeight)
        {
            type = PowerupType.Fuel;
            return fuelPowerup;
        }

        roll -= fuelWeight;

        if(roll < pointsWeight)
        {
            type = PowerupType.Points;
            return pointsPowerup;
        }

        type = PowerupType.Bomb;
        return bombPowerup;
    }

    float Weight(PowerupDefinition definition)
    {
        if(definition == null || definition.prefab == null)
            return 0f;

        return Mathf.Max(0f, definition.weight);
    }

    public void Collect(GameObject powerupObject)
    {
        for(int i = activePowerups.Count - 1; i >= 0; i--)
        {
            ActivePowerup powerup = activePowerups[i];

            if(powerup.gameObject == null)
            {
                activePowerups.RemoveAt(i);
                continue;
            }

            bool isMatch = powerup.gameObject == powerupObject || powerupObject.transform.IsChildOf(powerup.gameObject.transform);

            if(!isMatch)
                continue;

            Apply(powerup.type, powerup.value);

            Destroy(powerup.gameObject);
            activePowerups.RemoveAt(i);
            return;
        }
    }

    void Apply(PowerupType type, float value)
    {
        switch(type)
        {
            case PowerupType.Fuel:
                if(playerMovement != null)
                    playerMovement.fuel = Mathf.Min(playerMovement.fuel + value, playerMovement.maxFuel);
                break;

            case PowerupType.Points:
                GameManager.instance.AddPoints(Mathf.RoundToInt(value));
                break;

            case PowerupType.Bomb:
                GameManager.instance.ClearAllObstacles();
                break;
        }
    }

    void Cull(float shipHeight)
    {
        for(int i = activePowerups.Count - 1; i >= 0; i--)
        {
            ActivePowerup powerup = activePowerups[i];

            if(powerup.gameObject == null)
            {
                activePowerups.RemoveAt(i);
                continue;
            }

            if(powerup.gameObject.transform.position.y < shipHeight - powerupDespawnDistanceBelowShip)
            {
                Destroy(powerup.gameObject);
                activePowerups.RemoveAt(i);
            }
        }
    }

    void ClearAll()
    {
        foreach(ActivePowerup powerup in activePowerups)
        {
            if(powerup.gameObject != null)
                Destroy(powerup.gameObject);
        }

        activePowerups.Clear();
    }
}