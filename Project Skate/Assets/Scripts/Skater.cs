using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skater : MonoBehaviour
{
    private Rigidbody rb;
    private BoxCollider boxCollider;

    [Header("Movement Properties")]
    [SerializeField] private float forwardSpeed = 20f;
    [SerializeField] private float rotationSpeed = 20f;

    [Header("Ramp Physics Properties")]
    [SerializeField] private float spinAroundSpeed = 5f;
    [SerializeField] private float toRampExtentSpeed = 15f;
    [SerializeField] private float startRampUpRotateSpeed = 300f;
    [SerializeField] private float finishRampUpRotateSpeed = 600f;
    [SerializeField] private float rampJumpSpeed = 1.25f;
    [SerializeField] private float rampJumpHeight = 5f;
    [SerializeField] private float rampDropOffForwardForce = 10f;
    [SerializeField] private float rampDropOffHorizontalForce = 5f;
    [SerializeField] private float moveToRampExtentThreshold = 0.1f;
    [SerializeField] private float parabolaFinishThreshold = 0.2f;
    [SerializeField] private float verticalDirectionThreshold = 2f;

    private Transform[] rampPoints;
    private float Animation;
    private bool onStartRamp;
    public bool onRamp { get; set; }
    private bool onRampJump;
    private bool onFinishRamp;
    private bool onStraightRampJump;
    private bool onAir;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
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
        if (!isGorunded())
        {
            if (onStraightRampJump)
            {
                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);
                onAir = true;
                //onStraightRampJump = false;
            }
                rb.AddForce(Physics.gravity  * (rb.mass * rb.mass*5));
        }
        else
        {
            if (onAir)
            {
                rb.AddRelativeForce(transform.forward * 1500 * Time.deltaTime);
                onStraightRampJump = false;
                onAir = false;
            }
            //if (onStraightRampJump)
            //{
            //    rb.AddRelativeForce(transform.forward * 500 * Time.deltaTime);
            //}
            Move();
            Rotate();
        }


        
    }
    private void Update()
    {

        if (onRamp)
        {
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
    }

    private bool isGorunded()
    {
        RaycastHit hit;
        int layerMaskRoad = 1 << 9;
        int layerMaskRamp = 1 << 8;
        int finalLayerMask = layerMaskRoad | layerMaskRamp;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, boxCollider.bounds.extents.y + 0.1f, finalLayerMask))
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.blue);
            return true;
        }
        else
        {
            Debug.DrawRay(transform.position, Vector3.down, Color.red);
        }
        return false;
    }

    private void MoveToRampJump()
    {
        if (Vector3.Distance(transform.position, rampPoints[0].position) <= moveToRampExtentThreshold)
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
        if (Vector3.Distance(transform.position, rampPoints[2].position) <= moveToRampExtentThreshold)
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
        rb.constraints = RigidbodyConstraints.None;
    }

    private void Parabola()
    {
        if(Vector3.Distance(transform.position,rampPoints[1].position) <= parabolaFinishThreshold)
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
        if(Vector3.Distance(transform.position,rampPoints[1].position) <= verticalDirectionThreshold)
        {
            if(transform.position.x>0)
                transform.rotation = Quaternion.Euler(90, -90, 0);
            else
                transform.rotation = Quaternion.Euler(90, 90, 0);
        }
        else
        {
            if (transform.position.x > 0)
                transform.Rotate(0, -spinAroundSpeed * Time.deltaTime,0);
            else
                transform.Rotate(0, spinAroundSpeed * Time.deltaTime, 0);
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
        if(onStraightRampJump)
            rb.AddRelativeForce(Vector3.forward * force *5* Time.deltaTime);
        else
            rb.AddRelativeForce(Vector3.forward * force* Time.deltaTime);
    }

    private void Rotate()
    {
        //transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);
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
        else if (collision.collider.CompareTag("StraightRamp"))
        {

            onStraightRampJump = true;
            //TurnDirection();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("StraightRamp"))
             rb.AddForce(new Vector3(0,1,1) * 15, ForceMode.Impulse);
    }

    private void TurnDirection()
    {
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("StraightRamp");

        if (Physics.Raycast(transform.position, Vector3.forward,out hit , 5.0f,mask)){
            if(hit.collider != null)
            {
                print("found ramp");
                Debug.DrawLine(hit.collider.transform.position, hit.normal * 10, Color.red);
                Debug.Log(Vector3.Angle(Vector3.up, hit.normal));
                transform.Rotate(-Vector3.right, Vector3.Angle(Vector3.up, hit.normal));
            }
        }
        
        
    }
}
