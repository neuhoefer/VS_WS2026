using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HydraulicDrive : MonoBehaviour, IPowerConsumer
{
    private const float MAX_POWER = 12.4f;          // kW
    private const float SPEED_MAX = 8.2f;           // km/h
    private const float BRAKE_TORQUE = 500.0f;      // km/h

    [Space(10)]
    [Range(-1, 1)]
    [SerializeField] private float _leftWheelsInput = 0.0f;
    [Range(-1, 1)]
    [SerializeField] private float _rightWheelsInput = 0.0f;
    [SerializeField] private bool _isoControl = true;
    [SerializeField] private float _inputSpeed = 2.0f;
    [SerializeField] private InputAction _driveAction;

    [Space(10)]
    [SerializeField] private WheelCollider[] _leftWheelColliders = new WheelCollider[2];
    [SerializeField] private WheelCollider[] _rightWheelColliders = new WheelCollider[2];

    private Engine _engine;
    private float _wheelRadius;

    public Engine.Consumer ConsumerType => Engine.Consumer.Drive;

    private void OnEnable() => _driveAction.Enable();
    private void OnDisable() => _driveAction.Disable();

    private void Awake()
    {
        _engine = GetComponent<Engine>();
        _engine.RegisterConsumer(this);
        _wheelRadius = _leftWheelColliders[0].radius;
    }

    private void Update()
    {
        GetInput();
    }

    private void GetInput()
    {
        if(_isoControl)
        {
            Vector2 input = _driveAction.ReadValue<Vector2>();
            float leverLeft = Mathf.Clamp(input.y + input.x, -1f, 1f);
            float leverRight = Mathf.Clamp(input.y - input.x, -1f, 1f);

            _leftWheelsInput = Mathf.MoveTowards(_leftWheelsInput, leverLeft, _inputSpeed * Time.deltaTime);
            _rightWheelsInput = Mathf.MoveTowards(_rightWheelsInput, leverRight, _inputSpeed * Time.deltaTime);
        }
    }

    private float GetCurrentSpeedMS()
    {
        float rpm = Mathf.Abs(_leftWheelColliders[0].rpm + _rightWheelColliders[0].rpm) / 2.0f;
        return rpm * _wheelRadius * Mathf.PI / 30.0f;
    }

    private float GetTargetSpeedMS()
    {
        float input = (Mathf.Abs(_leftWheelsInput) + Mathf.Abs(_rightWheelsInput)) / 2.0f;
        return input * SPEED_MAX / 3.6f;
    }

    public float RequestPower()
    {
        float input = (Mathf.Abs(_leftWheelsInput) + Mathf.Abs(_rightWheelsInput)) / 2.0f;
        return input * MAX_POWER;
    }

    public void ReceivePower(float grantedPower)
    {
        ApplyTorque(grantedPower);
    }

    private void ApplyTorque(float grantedPower)
    {
        float currentSpeed = GetCurrentSpeedMS();
        float targetSpeed = GetTargetSpeedMS();

        float torque = grantedPower * 50.0f;

        ApplyTorqueToWheelColliders(_leftWheelColliders, _leftWheelsInput, torque);
        ApplyTorqueToWheelColliders(_rightWheelColliders, _rightWheelsInput, torque);
    }

    private void ApplyTorqueToWheelColliders(WheelCollider[] colliders, float input, float torque)
    {
        foreach(WheelCollider wc in colliders)
        {
            wc.motorTorque = Mathf.Sign(input) * torque;
            wc.brakeTorque = 0.0f;
        }
    }
}