using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    public enum Consumer
    {
        Aux,
        Drive,
        Effector
    }

    private const int RPM_IDLE = 1100;          // rpm at idle
    private const int RPM_MAX = 2300;           // rpm at max power
    private const float RPM_IDLE_POWER = 2.5f;  // kW
    private const float RPM_MAX_POWER = 18.2f;  // kW

    private const float s = (RPM_MAX_POWER - RPM_IDLE_POWER) / (float)(RPM_MAX - RPM_IDLE);
    private const float c = RPM_MAX_POWER - s * RPM_MAX;

    [Space(10)]
    [SerializeField] private bool _powerOn = false;
    [Range(RPM_IDLE, RPM_MAX)]
    [SerializeField] private int _rpm = RPM_IDLE;

    private readonly List<IPowerConsumer> _consumers = new List<IPowerConsumer>();

    public void RegisterConsumer(IPowerConsumer consumer)
    {
        _consumers.Add(consumer);
        _consumers.Sort((a, b) => a.ConsumerType.CompareTo(b.ConsumerType));
    }

    private void FixedUpdate()
    {
        DistributePower();
    }

    private float GetCurrentPower()
    {
        if( _powerOn )
        {
            return _rpm * s + c;
        }
        else
            return 0.0f;
    }

    private void DistributePower()
    {
        float remainingPower = GetCurrentPower();

        foreach (IPowerConsumer consumer in _consumers)
        {
            float requested = consumer.RequestPower();
            float granted = Mathf.Min(requested, Mathf.Max(0f, remainingPower));
            consumer.ReceivePower(granted);
            remainingPower -= granted;
        }
    }
}

public interface IPowerConsumer
{
    Engine.Consumer ConsumerType { get; }
    float RequestPower();
    void ReceivePower(float power);
}