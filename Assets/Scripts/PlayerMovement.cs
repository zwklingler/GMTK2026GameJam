using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float fuel;

    public float maxFuel;

    public float fuelUseSpeed;
    public float turnSpeed;
    public float moveSpeed;

    [Header("Flame Trail")]
    [Tooltip("Engine flame/smoke systems. Uncheck 'Play On Awake' on each.")]
    [SerializeField] private ParticleSystem[] flameSystems;

    [Tooltip("On = particles vanish instantly. Off = they fade out naturally.")]
    [SerializeField] private bool clearFlameInstantly = false;

    InputAction upAction;
    InputAction turnAction;

    Rigidbody rigidbody;

    bool flameOn;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();

        rigidbody.constraints |= RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;

        upAction = InputSystem.actions.FindAction("Up");
        turnAction = InputSystem.actions.FindAction("Turn");

        if (maxFuel <= 0f)
            maxFuel = fuel;

        SetFlame(false);
    }

    void OnDisable()
    {
        SetFlame(false);
    }

    void FixedUpdate()
    {
        bool isThrusting = upAction.IsPressed() && fuel > 0;

        if (isThrusting)
            Move();

        SetFlame(isThrusting);

        float turnDelta = turnAction.ReadValue<float>();

        if (turnDelta != 0)
            Turn(turnDelta);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
            GameManager.instance.CrashRocket();
    }

    void Move()
    {
        fuel = Mathf.Max(0, fuel - fuelUseSpeed * Time.fixedDeltaTime);
        rigidbody.AddRelativeForce(Vector3.up * moveSpeed * Time.fixedDeltaTime, ForceMode.Acceleration);
    }

    void Turn(float turnDelta)
    {
        transform.Rotate(Vector3.right * turnDelta * turnSpeed * Time.fixedDeltaTime);
    }

    public void ResetShip(Vector3 position, Quaternion rotation)
    {
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        transform.SetPositionAndRotation(position, rotation);

        fuel = maxFuel;

        SetFlame(false);
    }

    void SetFlame(bool on)
    {
        if (on == flameOn) return;
        flameOn = on;

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