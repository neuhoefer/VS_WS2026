using UnityEngine;
using System.Collections.Generic;

public class LoadSensor : MonoBehaviour
{
    public float Load { get; private set; }

    private readonly HashSet<Rigidbody> _rigidbodies = new HashSet<Rigidbody>();

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null) _rigidbodies.Add(rb);
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null) _rigidbodies.Remove(rb);
    }

    private void Update()
    {
        Load = 0f;
        foreach (Rigidbody rb in _rigidbodies)
            Load += rb.mass;
    }
}