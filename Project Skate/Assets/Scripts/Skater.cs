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
    private int isLeftRamp;

    // Start is called before the first frame update
    void Start()
    {
        InitializeRigidBodies();
    }

    private void InitializeRigidBodies()
    {
        rb = GetComponent<Rigidbody>();
        DisableRagdoll();
        rb.isKinematic = false; // Re-enable parent rigidbody
        rb.detectCollisions = true; //
    }

    private void DisableRagdoll()
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = true;
            rigidbody.detectCollisions = false; // To maintain center of mass
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (onRamp || onRampJump) { return; }
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
            Parabola();
            SpinAround();
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
                onRampJump = true;
                tmpPos = transform.position;
                return;
            }
            float step = 15 * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, rampColliderExtentsMax, step);
            transform.Rotate(-Vector3.right * 500 * Time.deltaTime);

        }
        else if (isFinishRamp)
        {
            if(transform.position.y <= rampColliderExtentsMin.y + 0.1)
            {
                onRamp = false;
                isFinishRamp = false;
                Straighten();
                return;
            }
            float step = 15 * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, rampColliderExtentsMin + Vector3.forward*2, step);
            transform.Rotate(-Vector3.right * 1000 * Time.deltaTime);
        }
    }

    private void Straighten()
    {
        rb.freezeRotation = false;
        rb.AddForce(new Vector3(1.5f * isLeftRamp, 0, 3.5f) * 225);
        rb.constraints = RigidbodyConstraints.None;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Parabola()
    {
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
        Animation += Time.deltaTime;
        Animation = Animation % 5f;
        transform.position = MathParabola.Parabola(tmpPos, tmpPos+(Vector3.forward*10), 5f, Animation / 1.25f);
    }

    //private void RampJump()
    //{
    //    rb.constraints = RigidbodyConstraints.FreezePositionX;
    //    rb.AddForce(Vector3.up * rampJumpFactor * Time.deltaTime, ForceMode.Impulse);
    //    onRampJump = true;
    //}

    private void SpinAround()
    {
        rb.freezeRotation = true;
        transform.Rotate(0, spinAroundSpeed * Time.deltaTime * -isLeftRamp,0);
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

        //Clamp Rotation
        Vector3 clampedRot = transform.eulerAngles;
        clampedRot.y = ClampAngle(clampedRot.y, -60, 60);
        transform.eulerAngles = clampedRot;
        
    }

    float ClampAngle(float angle, float from, float to)
    {
        if (angle < 0f) angle = 360 + angle;
        if (angle > 180f) return Mathf.Max(angle, 360 + from);
        return Mathf.Min(angle, to);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("StartRamp"))
        {
            isStartRamp = true;
            onRamp = true;
            if (collision.collider.GetComponent<Ramp>().isLeftRamp) { isLeftRamp = 1; }
            else { isLeftRamp = -1; }
            rampColliderExtentsMax = collision.collider.bounds.max;
        }
        else if (collision.collider.CompareTag("FinishRamp"))
        {
            isFinishRamp = true;
            onRamp = true;
            isStartRamp = false;
            onRampJump = false;
            rampColliderExtentsMin = collision.collider.bounds.min;
            Debug.Log(rampColliderExtentsMin);
        }
    }
}
