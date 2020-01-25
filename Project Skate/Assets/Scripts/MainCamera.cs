using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    private float offsetZ;
    // Start is called before the first frame update
    void Start()
    {
        offsetZ = transform.position.z - target.position.z;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = new Vector3(transform.position.x,transform.position.y, target.position.z + offsetZ);
    }
}
