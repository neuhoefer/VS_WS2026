using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    [SerializeField] private Vector3 _offset;

    private void Update()
    {
        transform.position = _target.transform.TransformPoint(_offset);
        Vector3 direction = _target.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
