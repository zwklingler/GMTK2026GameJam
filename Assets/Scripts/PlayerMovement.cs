using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float fuel;
    public float fuelUseSpeed;
    public float turnSpeed;
    public float moveSpeed;

    InputAction upAction;
    InputAction turnAction;

    Rigidbody2D rigidbody;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();

        upAction = InputSystem.actions.FindAction("Up");
        turnAction = InputSystem.actions.FindAction("Turn");
    }

    void FixedUpdate()
    {
        if (upAction.IsPressed() && fuel > 0)
            Move();

        float turnDelta = turnAction.ReadValue<float>();

        if (turnDelta != 0)
            Turn(turnDelta);
    }

    void Move()
    {
        fuel = Mathf.Max(0, fuel - fuelUseSpeed * Time.fixedDeltaTime);
        rigidbody.AddRelativeForce(Vector3.up * moveSpeed * Time.fixedDeltaTime, ForceMode2D.Force);
    }

    void Turn(float turnDelta)
    {
        transform.Rotate(Vector3.forward * -turnDelta * turnSpeed * Time.fixedDeltaTime);
    }
}