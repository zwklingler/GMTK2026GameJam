using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BackgroundProps : MonoBehaviour
{
    [System.Serializable]
    public class BackgroundProp
    {
        public string name = "Prop";

        public Sprite sprite;

        [Tooltip("Relative chance against at this height")]
        public float weight = 1f;

        [Tooltip("Lowest rocket height this can appear at")]
        public float minHeight = 0f;

        [Tooltip("Highest rocket height this can appear at")]
        public float maxHeight = 100000f;

        [Tooltip("Random prop size")]
        public Vector2 sizeRange = new Vector2(8f, 16f);

        [Tooltip("How many can be on screen at once")]
        public int maxAlive = 1;

        [Tooltip("This is for the speed of background (parallax effect)")]
        [Range(0f, 1f)]
        public float parallax = 0.85f;

        [Tooltip("Use this prop's spawn instead of the generic default")]
        public bool overrideSpawnArea;


        [Tooltip("Min and max X relative to the ship")]
        public Vector2 horizontalRange = new Vector2(-40f, 40f);

        [Tooltip("Min and max height relative to the camera")]
        public Vector2 verticalRange = new Vector2(35f, 80f);

        [Tooltip("Spawning this prop ends the game")]
        public bool triggersEnding;

    }

    [SerializeField] private Transform rocket;

    [Header("Placement")]
    [Tooltip("Distance from the camera")]
    [SerializeField] private float propDistance = 100f;

    [Tooltip("How far left and right they can spawn")]
    [SerializeField] private float horizontalRange = 40f;

    [Tooltip("How far above they can spawn")]
    [SerializeField] private Vector2 verticalSpawnRange = new Vector2(35f, 80f);

    [Tooltip("Distance when the prop is removed")]
    [SerializeField] private float despawnBelow = 90f;

    [Header("Spawning")]
    [Tooltip("Random seconds between spawn attempts")]
    [SerializeField] private Vector2 spawnIntervalRange = new Vector2(8f, 20f);

    [Tooltip("Total props allowed on screen at once")]
    [SerializeField] private int maxPropsAlive = 3;

    [Tooltip("Extra vertical speed")]
    [SerializeField] private float driftSpeed = 0.3f;

    [Header("Ending Prop")]
    [Tooltip("World Z position the ending sits at")]
    [SerializeField] private float endingPropDepth = 5f;

    [Header("Props")]
    [SerializeField] private List<BackgroundProp> props = new List<BackgroundProp>();

    private class ActiveProp
    {
        public GameObject gameObject;
        public BackgroundProp definition;
        public float spawnLocalY;
        public float spawnCameraY;
        public float spawnTime;
        public bool frozen;
    }

    private readonly List<ActiveProp> activeProps = new List<ActiveProp>();
    private float spawnTimer;

    // Set once the ending prop appears
    private bool endingStarted;

    void Update()
    {
        if (!endingStarted)
        {
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0f)
            {
                TrySpawn();
                spawnTimer = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            }
        }

        MoveProps();
    }

    public void ResetRun()
    {
        foreach (ActiveProp prop in activeProps)
        {
            if (prop.gameObject != null)
                Destroy(prop.gameObject);
        }
 
        activeProps.Clear();
        spawnTimer = 0f;
        endingStarted = false;
    }

    void TrySpawn()
    {
        if (activeProps.Count >= maxPropsAlive)
            return;

        BackgroundProp definition = Choose();

        if (definition == null)
            return;

        if (definition.triggersEnding)
        {
            SpawnEndingProp(definition);
            return;
        }

        GameObject prop = new GameObject("BackgroundProp_" + definition.name);
        prop.transform.SetParent(transform, false);

        SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
        renderer.sprite = definition.sprite;

        float size = Random.Range(definition.sizeRange.x, definition.sizeRange.y);
        float scale = size / definition.sprite.bounds.size.y;
        prop.transform.localScale = new Vector3(scale, scale, 1f);

         Vector2 horizontal = definition.overrideSpawnArea ? definition.horizontalRange : new Vector2(-horizontalRange, horizontalRange);

        Vector2 vertical = definition.overrideSpawnArea ? definition.verticalRange : verticalSpawnRange;

        float localX = Random.Range(horizontal.x, horizontal.y);
        float localY = Random.Range(vertical.x, vertical.y);

        prop.transform.localPosition = new Vector3(localX, localY, propDistance);
        prop.transform.localRotation = Quaternion.identity;

        activeProps.Add(new ActiveProp
        {
            gameObject = prop,
            definition = definition,
            spawnLocalY = localY,
            spawnCameraY = transform.position.y,
            spawnTime = Time.time,
            frozen = false
        });
    }

    void SpawnEndingProp(BackgroundProp definition)
    {
        GameObject prop = new GameObject("BackgroundProp_" + definition.name);

        SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
        renderer.sprite = definition.sprite;

        float size = Random.Range(definition.sizeRange.x, definition.sizeRange.y);
        float scale = size / definition.sprite.bounds.size.y;
        prop.transform.localScale = new Vector3(scale, scale, 1f);

        Vector2 horizontal = definition.overrideSpawnArea ? definition.horizontalRange : new Vector2(-horizontalRange, horizontalRange);
        Vector2 vertical = definition.overrideSpawnArea ? definition.verticalRange : verticalSpawnRange;

        Vector3 shipPosition = rocket != null ? rocket.position : transform.position;

        prop.transform.position = new Vector3(
            shipPosition.x + Random.Range(horizontal.x, horizontal.y),
            shipPosition.y + Random.Range(vertical.x, vertical.y),
            endingPropDepth);

        prop.transform.rotation = Quaternion.identity;

        activeProps.Add(new ActiveProp
        {
            gameObject = prop,
            definition = definition,
            spawnLocalY = prop.transform.position.y,
            spawnCameraY = transform.position.y,
            spawnTime = Time.time,
            frozen = true
        });

        endingStarted = true;
        GameManager.instance.BeginBlackHoleCapture(prop.transform);
    }

    BackgroundProp Choose()
    {
        float height = rocket != null ? rocket.position.y : transform.position.y;

        float total = 0f;

        foreach (BackgroundProp definition in props)
        {
            if (IsEligible(definition, height))
                total += Mathf.Max(0f, definition.weight);
        }

        if (total <= 0f)
            return null;

        float roll = Random.value * total;

        foreach (BackgroundProp definition in props)
        {
            if (!IsEligible(definition, height))
                continue;

            float weight = Mathf.Max(0f, definition.weight);

            if (roll < weight)
                return definition;

            roll -= weight;
        }

        return null;
    }

    bool IsEligible(BackgroundProp definition, float height)
    {
        if (definition == null || definition.sprite == null)
            return false;

        if (height < definition.minHeight || height > definition.maxHeight)
            return false;

        return CountAlive(definition) < definition.maxAlive;
    }

    int CountAlive(BackgroundProp definition)
    {
        int count = 0;

        foreach (ActiveProp prop in activeProps)
        {
            if (prop.definition == definition)
                count++;
        }

        return count;
    }

    void MoveProps()
    {
        float cameraY = transform.position.y;

        for (int i = activeProps.Count - 1; i >= 0; i--)
        {
            ActiveProp prop = activeProps[i];

            if (prop.gameObject == null)
            {
                activeProps.RemoveAt(i);
                continue;
            }

            // The ending prop is never culled
            if (prop.frozen)
                continue;

            float cameraDelta = cameraY - prop.spawnCameraY;
            float drift = driftSpeed * (Time.time - prop.spawnTime);

            float localY = prop.spawnLocalY - cameraDelta * (1f - prop.definition.parallax) + drift;

            Vector3 position = prop.gameObject.transform.localPosition;
            position.y = localY;
            prop.gameObject.transform.localPosition = position;

            if (localY < -despawnBelow)
            {
                Destroy(prop.gameObject);
                activeProps.RemoveAt(i);
            }
        }
    }
}