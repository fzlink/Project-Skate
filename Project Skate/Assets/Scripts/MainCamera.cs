using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float defaultY = 6f;
    [Tooltip("Big number = Less follow")] [SerializeField] private float rampFollowXFactor = 2f;
    [Tooltip("Big number = Less follow")] [SerializeField] private float rampFollowYFactor = 2f;
    private Vector3 offset;
    private bool isBroken;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, 6, target.position.z - 7);
        offset = transform.position - target.position;
    }



    // Update is called once per frame
    void LateUpdate()
    {
        if (target.GetComponentInParent<Skater>().onRamp)
        {
            FollowRamp();
        }
        else if (isBroken)
        {
            FixCamera();
        }
        else // Default Follow
            transform.position = new Vector3(0, defaultY, target.position.z + offset.z);
    }

    private void FixCamera()
    {
        Vector3 defaultPosition = new Vector3(0, defaultY, target.position.z + offset.z);
        if (Vector3.Distance(transform.position, defaultPosition) <= 0.1f)
        {
            isBroken = false;
            return;
        }
        float step = 15 * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, defaultPosition, step);
    }

    private void FollowRamp()
    {
        float step = 15 * Time.deltaTime;
        Vector3 rampPosition = new Vector3(target.position.x + offset.x / rampFollowXFactor, target.position.y + offset.y / rampFollowYFactor, target.position.z + offset.z);
        transform.position = Vector3.MoveTowards(transform.position, rampPosition, step);
        isBroken = true;
    }
}
