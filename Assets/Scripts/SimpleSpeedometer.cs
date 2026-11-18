using UnityEngine;

public class SimpleSpeedometer : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float speedKMH = Mathf.Round(_rigidbody.linearVelocity.magnitude * 3.6f * 100f) / 100f;
        Debug.Log(speedKMH);
    }
}