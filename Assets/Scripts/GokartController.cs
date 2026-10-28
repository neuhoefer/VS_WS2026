using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GokartController : MonoBehaviour
{
    [SerializeField] private float _motorTorque;
    [SerializeField] private float _brakeTorque;
    [SerializeField] private float _maxSteeringAngle;

    [SerializeField] private WheelCollider _frontLeftCollider;
    [SerializeField] private WheelCollider _frontRightCollider;
    [SerializeField] private WheelCollider _rearLeftCollider;
    [SerializeField] private WheelCollider _rearRightCollider;

    [SerializeField] private Transform _frontLeftWheel;
    [SerializeField] private Transform _frontRightWheel;
    [SerializeField] private Transform _rearLeftWheel;
    [SerializeField] private Transform _rearRightWheel;

    [SerializeField] private InputAction _moveAction;
    [SerializeField] private InputAction _brakeAction;

    private Vector2 _movement;
    private bool _isBraking;

    private void OnEnable()
    {
        _moveAction.Enable();
        _brakeAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _brakeAction.Disable();
    }

    private void FixedUpdate()
    {
        GetInput();
        ApplyForces();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        _movement = _moveAction.ReadValue<Vector2>();
        _isBraking = _brakeAction.IsPressed();
    }

    private void ApplyForces()
    {
        _frontLeftCollider.motorTorque = _motorTorque * _movement.y;
        _frontRightCollider.motorTorque = _motorTorque * _movement.y;

        float currentBrakeTorque = _isBraking ? _brakeTorque : 0.0f;
        _frontLeftCollider.brakeTorque = currentBrakeTorque;
        _frontRightCollider.brakeTorque = currentBrakeTorque;
        _rearLeftCollider.brakeTorque = currentBrakeTorque;
        _rearRightCollider.brakeTorque = currentBrakeTorque;
    }

    private void HandleSteering()
    {
        _frontLeftCollider.steerAngle = _maxSteeringAngle * _movement.x;
        _frontRightCollider.steerAngle = _maxSteeringAngle * _movement.x;
    }

    private void UpdateWheels()
    {
        UpdateWheel(_frontLeftCollider, _frontLeftWheel);
        UpdateWheel(_frontRightCollider, _frontRightWheel);
        UpdateWheel(_rearLeftCollider, _rearLeftWheel);
        UpdateWheel(_rearRightCollider, _rearRightWheel);
    }

    private void UpdateWheel(WheelCollider collider, Transform wheel)
    {
        collider.GetWorldPose(out var pos, out var rot);
        wheel.position = pos;
        wheel.rotation = rot;
    }
}
