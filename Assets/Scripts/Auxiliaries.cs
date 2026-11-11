using UnityEngine;

public class Auxiliaries : MonoBehaviour, IPowerConsumer
{
    private const float MAX_POWER = 1.0f;  // kW

    private Engine _engine;

    public Engine.Consumer ConsumerType => Engine.Consumer.Aux;

    private void Awake()
    {
        _engine = GetComponent<Engine>();
        _engine.RegisterConsumer(this);
    }

    public float RequestPower()
    {
        return MAX_POWER;
    }

    public void ReceivePower(float grantedPower)
    {
        //Debug.Log("Auxiliaries running with " + grantedPower + " kW.");
    }
}