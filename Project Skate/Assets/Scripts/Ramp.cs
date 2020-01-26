using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ramp : MonoBehaviour
{
    [SerializeField] private Transform jumpingOffPoint;
    [SerializeField] private Transform fallingOffPoint;
    [SerializeField] private Transform dropOffPoint;
    public Transform[] GetRampPoints()
    {
        Transform[] points = new Transform[3];
        points[0] = jumpingOffPoint;
        points[1] = fallingOffPoint;
        points[2] = dropOffPoint;
        return points;
    } 

}
