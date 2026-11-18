using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class HydraulicEffector : MonoBehaviour, IPowerConsumer
{
    private const float MIN_POWER = 0.1f;   // kW
    private const float MAX_POWER = 4.5f;   // kW
    private const float LOAD_MAX = 457.0f;  // kg

    [Space(10)]
    [SerializeField] private float _armSpeed = 10.0f;       // Grad/s
    [SerializeField] private float _bucketSpeed = 20.0f;    // Grad/s
    [SerializeField] private InputAction _effectorAction;
    [Space(10)]
    [SerializeField] private ArticulationBody _arm;
    [SerializeField] private ArticulationBody _bucket;
    [SerializeField] private LoadSensor _loadSensor;

    private Engine _engine;
    private float _armInput = 0.0f;
    private float _bucketInput = 0.0f;
    private float _requiredPower = 0.0f;

    public Engine.Consumer ConsumerType => Engine.Consumer.Effector;

    private void OnEnable() => _effectorAction.Enable();
    private void OnDisable() => _effectorAction.Disable();

    private void Awake()
    {
        _engine = GetComponent<Engine>();
        _engine.RegisterConsumer(this);
    }

    private void Update()
    {
        GetInput();
    }

    private void GetInput()
    {
        Vector2 input = _effectorAction.ReadValue<Vector2>();
        _armInput = -input.y;
        _bucketInput = -input.x;
    }

    public float RequestPower()
    {
        _requiredPower = 0.0f;
        if (_armInput != 0.0f || _bucketInput != 0.0f)
        {
            float loadPower = MAX_POWER * (_loadSensor.Load / LOAD_MAX);
            _requiredPower = Mathf.Max(MIN_POWER, loadPower);
        }
        return _requiredPower;
    }

    public void ReceivePower(float grantedPower)
    {
        OperateEffector(grantedPower);
    }

    private void OperateEffector(float grantedPower)
    {
        float powerRatio = _requiredPower > 0.0f ? grantedPower / _requiredPower : 0.0f;
        
        float armAngle = _arm.jointPosition[0] * Mathf.Rad2Deg;
        armAngle += _armInput * _armSpeed * Time.fixedDeltaTime * powerRatio;
        _arm.SetDriveTarget(ArticulationDriveAxis.X, armAngle);

        float bucketAngle = _bucket.jointPosition[0] * Mathf.Rad2Deg;
        bucketAngle += _bucketInput * _bucketSpeed * Time.fixedDeltaTime * powerRatio;
        _bucket.SetDriveTarget(ArticulationDriveAxis.X, bucketAngle);
    }
}