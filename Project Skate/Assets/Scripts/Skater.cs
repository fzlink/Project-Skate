using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skater : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Movement Properties")]
    [SerializeField] private float forwardSpeed = 20f;
    [SerializeField] private float rotationSpeed = 20f;

    [Header("Ramp Properties")]
    [SerializeField] private float spinAroundSpeed = 5f;
    [SerializeField] private float toRampExtentSpeed = 15f;
    [SerializeField] private float startRampUpRotateSpeed = 300f;
    [SerializeField] private float finishRampUpRotateSpeed = 600f;
    [SerializeField] private float rampJumpSpeed = 1.25f;
    [SerializeField] private float rampJumpHeight = 5f;
    [SerializeField] private float rampDropOffForwardForce = 10f;
    [SerializeField] private float rampDropOffHorizontalForce = 5f;

    private Transform[] rampPoints;
    private float Animation;
    private bool onStartRamp;
    private bool onRamp;
    private bool onRampJump;
    private bool isGrounded;
    private bool onFinishRamp;

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
        if (onRamp) { return; }
        Move();
        Rotate();
    }
    private void Update()
    {
        if (!onRamp) { return; }

        if (onStartRamp)
        {
            MoveToRampJump();

        }
        else if (onRampJump)
        {
            Parabola();
            SpinAround();
        }
        else if (onFinishRamp)
        {
            MoveToRampFinish();
        }
    }

    private void MoveToRampJump()
    {
        if (Vector3.Distance(transform.position, rampPoints[0].position) <= 0.1f)
        {
            onStartRamp = false;
            onRampJump = true;
        }
        else
        {
            float step = toRampExtentSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, rampPoints[0].position, step);
            rb.freezeRotation = true;
            transform.Rotate(-Vector3.right * startRampUpRotateSpeed * Time.deltaTime);
            rb.freezeRotation = false;
        }
    }

    private void MoveToRampFinish()
    {
        if (Vector3.Distance(transform.position, rampPoints[2].position) <= 0.1f)
        {
            onFinishRamp = false;
            Straighten();
            onRamp = false;
        }
        else
        {
            float step = toRampExtentSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, rampPoints[2].position, step);
            rb.freezeRotation = true;
            transform.Rotate(-Vector3.right * finishRampUpRotateSpeed * Time.deltaTime);
            rb.freezeRotation = false;
        }
    }


    private void Straighten()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        if (transform.position.x < 0) rb.AddForce(Vector3.forward * rampDropOffForwardForce + Vector3.right * rampDropOffHorizontalForce, ForceMode.Impulse);
        else rb.AddForce(Vector3.forward * rampDropOffForwardForce + Vector3.left * rampDropOffHorizontalForce, ForceMode.Impulse);
    }

    private void Parabola()
    {
        if(Vector3.Distance(transform.position,rampPoints[1].position) <= 0.2f)
        {
            onRampJump = false;
            onFinishRamp = true;
        }
        else
        {
            Animation += Time.deltaTime;
            Animation = Animation % 5f;
            transform.position = MathParabola.Parabola(rampPoints[0].position, rampPoints[1].position, rampJumpHeight, Animation / rampJumpSpeed);
        }
    }

    private void SpinAround()
    {
        rb.freezeRotation = true;
        if(Vector3.Distance(transform.position,rampPoints[1].position) <= 2f)
        {
            if(transform.position.x>0)
                transform.rotation = Quaternion.Euler(90, -90, 0);
            else
                transform.rotation = Quaternion.Euler(90, 90, 0);
        }
        else
        {
            transform.Rotate(0, spinAroundSpeed * Time.deltaTime,0);
        }
        
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
            onStartRamp = true;
            onRamp = true;
            rampPoints = collision.gameObject.GetComponentInParent<Ramp>().GetRampPoints();
        }
    }
}
