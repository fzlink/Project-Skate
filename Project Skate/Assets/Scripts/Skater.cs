using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skater : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private float forwardSpeed = 20f;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float spinAroundSpeed = 5f;
    [SerializeField] private float rampJumpFactor = 1000;

    private float Animation;
    private bool onRamp;
    private Vector3 tmpPos;
    private bool isFinishRamp;
    private Vector3 rampColliderExtentsMin;
    private bool onRampJump;
    private bool isGrounded;
    private Vector3 rampColliderExtentsMax;
    private bool isStartRamp;
    private bool goNeutral;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = true;
            rigidbody.detectCollisions = false;
        }
        rb.isKinematic = false;
        rb.detectCollisions = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (onRamp || onRampJump) {
            return;
        }
        Move();
        Rotate();
    }
    private void Update()
    {
        if (onRamp)
        {
            MoveToRampExtent();

        }
        else if (onRampJump)
        {
            SpinAround();
            Parabola();
        }
        else if (goNeutral)
        {
            //float step = 15 * Time.deltaTime;
            //Vector3.MoveTowards(transform.position, transform.position + new Vector3(-10, 0, 1), step);
            //Debug.Log("sad");
        }
    }

    private void MoveToRampExtent()
    {
        rb.freezeRotation = true;
        if (isStartRamp)
        {
            if (transform.position.y >= rampColliderExtentsMax.y - 0.1)
            {
                onRamp = false;
                tmpPos = transform.position;
                Parabola();
                //RampJump();
                return;
            }
            float step = 15 * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, rampColliderExtentsMax, step);
            transform.Rotate(-Vector3.right*5);

        }
        else if (isFinishRamp)
        {
            if(transform.position.y <= rampColliderExtentsMin.y + 0.4)
            {
                onRamp = false;
                isFinishRamp = false;
                rb.freezeRotation = false;

                rb.AddForce(new Vector3(-1.5f, 0, 3.5f)*225);
                rb.constraints = RigidbodyConstraints.None;
                transform.rotation = Quaternion.Euler(0, 0, 0);


                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                return;
            }
            float step = 15 * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, rampColliderExtentsMin + Vector3.forward*2, step);
        }
    }

    private void Parabola()
    {
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
        onRampJump = true;
        Animation += Time.deltaTime;
        Animation = Animation % 5f;
        transform.position = MathParabola.Parabola(tmpPos, tmpPos+(Vector3.forward*10)-(Vector3.right*0.5f), 5f, Animation / 1.25f);
    }

    private void RampJump()
    {
        rb.constraints = RigidbodyConstraints.FreezePositionX;
        rb.AddForce(Vector3.up * rampJumpFactor * Time.deltaTime, ForceMode.Impulse);
        onRampJump = true;
    }

    private void SpinAround()
    {
        rb.freezeRotation = true;
        //transform.rotation = Quaternion.Euler(-60, 0, 0);
        transform.Rotate(0, spinAroundSpeed*Time.deltaTime,0);
        rb.freezeRotation = false;
    }




    private void Move()
    {
        float force;
        if (rb.velocity.z < forwardSpeed)
            force = forwardSpeed;
        else
            force = 0;

        rb.AddRelativeForce(Vector3.forward * force* Time.deltaTime);
    }

    private void Rotate()
    {

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.up * rotationSpeed*Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(-Vector3.up * rotationSpeed * Time.deltaTime);
        }
        Vector3 clampedRot = transform.eulerAngles;
        clampedRot.y = ClampAngle(clampedRot.y, -60, 60);
        transform.eulerAngles = clampedRot;
    }

    float ClampAngle(float angle, float from, float to)
    {
        // accepts e.g. -80, 80
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Mathf.Max(angle, 360 + from);
        return Mathf.Min(angle, to);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("StartRamp"))
        {
            isStartRamp = true;
            rampColliderExtentsMax = collision.collider.bounds.max;
            onRamp = true;
        }
        else if (collision.collider.CompareTag("FinishRamp"))
        {
            isFinishRamp = true;
            onRamp = true;
            isStartRamp = false;
            rampColliderExtentsMin = collision.collider.bounds.min;
            onRampJump = false;
        }
    }
}
