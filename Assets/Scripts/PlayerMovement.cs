using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float fuel;
    public float fuelUseSpeed;
    public float turnSpeed;
    public float moveSpeed;

    [Header("Flame Trail")]
    [Tooltip("Engine flame/smoke systems")]
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

        upAction = InputSystem.actions.FindAction("Up");
        turnAction = InputSystem.actions.FindAction("Turn");

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

    void Move()
    {
        fuel = Mathf.Max(0, fuel - fuelUseSpeed * Time.fixedDeltaTime);
        rigidbody.AddRelativeForce(Vector3.up * moveSpeed * Time.fixedDeltaTime, ForceMode.Acceleration);
    }

    void Turn(float turnDelta)
    {
        transform.Rotate(Vector3.right * turnDelta * turnSpeed * Time.fixedDeltaTime);
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