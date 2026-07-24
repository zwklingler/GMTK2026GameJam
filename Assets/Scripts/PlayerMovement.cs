using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float fuel;

    public float maxFuel;

    public float fuelUseSpeed = 1f;
    public float turnSpeed;
    public float moveSpeed;

    [Tooltip("Top speed in units per second. Set to 0 for no limit.")]
    public float maxSpeed = 30f;

    [Header("Gravity")]
    [Tooltip("Gravity strength at initial height")]
    public float baseGravity = 9.81f;

    [Tooltip("Gravity at height")]
    public float gravityFullHeight = 0f;

    [Tooltip("Height which gravity would be gravityFullHeight")]
    public float gravityAtHeight = 500f;

    [Header("Flame Trail")]
    [Tooltip("Engine flame/smoke systems. Uncheck 'Play On Awake' on each.")]
    [SerializeField] private ParticleSystem[] flameSystems;

    [Tooltip("On = particles vanish instantly. Off = they fade out naturally.")]
    [SerializeField] private bool clearFlameInstantly = false;

    [Header("Rocket Wobble")]
    [Tooltip("Transform for the mesh")]
    [SerializeField] private Transform shipModel;

    [Tooltip("Max wobble in degrees")]
    [SerializeField] private float wobbleAngle = 10f;


    [Tooltip("How much base rotation in the wobble")]
    [SerializeField] private float wobbleAngleAtRest = 0.6f;

    [Tooltip("How fast the base wobble happens")]
    [SerializeField] private float wobbleSpeed = 10f;

    [Tooltip("Wobble speed multiplier at max speed. 1 = no change with speed.")]
    [SerializeField] private float wobbleSpeedAtMaxSpeed = 3f;

    [Tooltip("How quickly the wobble fades in and out")]
    [SerializeField] private float wobbleBlendSpeed = 5f;

    [Header("Audio")]
    [SerializeField] private AudioClip boostSound;

    [Range(0f, 1f)]
    [SerializeField] private float boostVolume = 1f;

    InputAction upAction;
    InputAction turnAction;

    Rigidbody rigidbody;

    bool flameOn;

    bool thrusting;
    float wobbleAmount;
    float wobblePhase;
    Quaternion modelBaseRotation = Quaternion.identity;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();

        rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;

        rigidbody.useGravity = false;

        upAction = InputSystem.actions.FindAction("Up");
        turnAction = InputSystem.actions.FindAction("Turn");

        // Remember the model's rest pose so rocket rotation can go back to normal
        if (shipModel != null)
            modelBaseRotation = shipModel.localRotation;

        if (maxFuel <= 0f)
            maxFuel = fuel;

        SetFlame(false);
    }

    void OnDisable()
    {
        SetFlame(false);

        thrusting = false;
        wobbleAmount = 0f;

        if (shipModel != null)
            shipModel.localRotation = modelBaseRotation;
    }

    void FixedUpdate()
    {
        bool isThrusting = upAction.IsPressed() && fuel > 0;
        thrusting = isThrusting;

        if (isThrusting)
            Move();

        ApplyGravity();

        ClampSpeed();

        SetFlame(isThrusting);

        float turnDelta = turnAction.ReadValue<float>();

        if (turnDelta != 0)
            Turn(turnDelta);
    }

    void Update()
    {
        UpdateWobble();
    }

    void OnCollisionEnter(Collision collision)
    {
        TryCrash(collision.gameObject);
    }

    void OnCollisionStay(Collision collision)
    {
        TryCrash(collision.gameObject);
    }

    void TryCrash(GameObject other)
    {
        if (IsCrashSurface(other))
            GameManager.instance.CrashRocket();
    }

    bool IsCrashSurface(GameObject other)
    {
        if (other.CompareTag("Obstacle"))
            return true;

        bool isFloor = other.CompareTag("Floor")
            || other.name.IndexOf("floor", System.StringComparison.OrdinalIgnoreCase) >= 0;

        return isFloor && GameManager.instance.CanCrashOnFloor();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Powerup"))
            GameManager.instance.CollectPowerup(other.gameObject);
    }

    void Move()
    {
        fuel = Mathf.Max(0, fuel - fuelUseSpeed * Time.fixedDeltaTime);
        rigidbody.AddRelativeForce(Vector3.up * moveSpeed * Time.fixedDeltaTime, ForceMode.Acceleration);
    }
    void ApplyGravity()
    {
        float heightRatio = Mathf.InverseLerp(gravityFullHeight, gravityAtHeight, transform.position.y);
        float gravity = Mathf.Lerp(baseGravity, 0f, heightRatio);

        if (gravity <= 0f)
            return;

        rigidbody.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    public float GetGravityFraction()
    {
        return 1f - Mathf.InverseLerp(gravityFullHeight, gravityAtHeight, transform.position.y);
    }

    void ClampSpeed()
    {
        if (maxSpeed <= 0f)
            return;

        // sqrMagnitude to skip the square root on the common case
        if (rigidbody.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            rigidbody.linearVelocity = rigidbody.linearVelocity.normalized * maxSpeed;
    }

    void Turn(float turnDelta)
    {
        transform.Rotate(Vector3.right * turnDelta * turnSpeed * Time.fixedDeltaTime);
    }

    void UpdateWobble()
    {
        if (shipModel == null)
            return;

        wobbleAmount = Mathf.MoveTowards(wobbleAmount, thrusting ? 1f : 0f, wobbleBlendSpeed * Time.deltaTime);

        if (wobbleAmount <= 0f)
        {
            shipModel.localRotation = modelBaseRotation;
            return;
        }

        // Faster the quicker the rocket is going
        float speedRatio = maxSpeed > 0f ? Mathf.Clamp01(rigidbody.linearVelocity.magnitude / maxSpeed) : 0f;
        float currentWobbleSpeed = wobbleSpeed * Mathf.Lerp(1f, wobbleSpeedAtMaxSpeed, speedRatio);


        float currentWobbleAngle = wobbleAngle * Mathf.Lerp(wobbleAngleAtRest, 1f, speedRatio);

        wobblePhase += currentWobbleSpeed * Time.deltaTime;

        float pitch = (Mathf.PerlinNoise(wobblePhase, 0f) - 0.5f) * 2f;
        float yaw = (Mathf.PerlinNoise(0f, wobblePhase) - 0.5f) * 2f;
        float roll = (Mathf.PerlinNoise(wobblePhase, wobblePhase) - 0.5f) * 2f;

        Vector3 angles = new Vector3(pitch, yaw, roll) * currentWobbleAngle * wobbleAmount;

        shipModel.localRotation = modelBaseRotation * Quaternion.Euler(angles);
    }

    public void ResetShip(Vector3 position, Quaternion rotation)
    {
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        transform.SetPositionAndRotation(position, rotation);

        fuel = maxFuel;

        thrusting = false;
        wobbleAmount = 0f;

        if (shipModel != null)
            shipModel.localRotation = modelBaseRotation;

        SetFlame(false);
    }

    void SetFlame(bool on)
    {
        if (on == flameOn) return;
        flameOn = on;

        if (AudioManager.instance != null)
        {
            if (on)
                AudioManager.instance.PlayLayer(AudioManager.instance.thrustSource, boostSound, boostVolume);
            else
                AudioManager.instance.StopLayer(AudioManager.instance.thrustSource);
        }

        var stopMode = clearFlameInstantly ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting;

        foreach (var ps in flameSystems)
        {
            if (ps == null) continue;

            if (on) 
                ps.Play(true);
            else 
                ps.Stop(true, stopMode);
        }
    }
}
