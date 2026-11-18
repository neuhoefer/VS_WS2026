using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HydraulicDrive : MonoBehaviour, IPowerConsumer
{
    private const float MAX_POWER = 12.4f;          // kW
    private const float SPEED_MAX = 8.2f;           // km/h
    private const float MAX_TORQUE = 500.0f;        // Nm

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
    [SerializeField] private Transform[] _leftWheelMeshes = new Transform[2];
    [SerializeField] private Transform[] _rightWheelMeshes = new Transform[2];

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
        UpdateWheels();
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
        return (_leftWheelsInput != 0f || _rightWheelsInput != 0f) ? MAX_POWER : 0.0f;
    }

    public void ReceivePower(float grantedPower)
    {
        ApplyTorque(grantedPower);
    }

    private void ApplyTorque(float grantedPower)
    {
        float currentSpeed = GetCurrentSpeedMS();
        //Debug.Log("Current speed: " + Mathf.Round(currentSpeed * 3.6f * 100f) / 100f);
        float targetSpeed = GetTargetSpeedMS();

        float targetOmega = targetSpeed / _wheelRadius;
        float torque = targetOmega != 0.0f ? Mathf.Min((grantedPower * 1000.0f) / (targetOmega * 4.0f), MAX_TORQUE) : 0.0f;

        ApplyTorqueToWheelColliders(_leftWheelColliders, _leftWheelsInput, torque, targetSpeed - currentSpeed);
        ApplyTorqueToWheelColliders(_rightWheelColliders, _rightWheelsInput, torque, targetSpeed - currentSpeed);
    }

    private void ApplyTorqueToWheelColliders(WheelCollider[] colliders, float input, float torque, float speedDelta)
    {
        foreach(WheelCollider wc in colliders)
        {
            if (speedDelta > 0.05f && input != 0f)
            {
                wc.motorTorque = Mathf.Sign(input) * torque;
                wc.brakeTorque = 0.0f;
            }
            else
            {
                wc.motorTorque = 0.0f;
                wc.brakeTorque = MAX_TORQUE;
            }
        }
    }

    private void UpdateWheels()
    {
        for (int i = 0; i < _leftWheelColliders.Length; i++)
            UpdateWheel(_leftWheelColliders[i], _leftWheelMeshes[i]);
        for (int i = 0; i < _rightWheelColliders.Length; i++)
            UpdateWheel(_rightWheelColliders[i], _rightWheelMeshes[i]);
    }

    private void UpdateWheel(WheelCollider collider, Transform wheel)
    {
        collider.GetWorldPose(out var pos, out var rot);
        wheel.position = pos;
        wheel.rotation = rot;
    }
}