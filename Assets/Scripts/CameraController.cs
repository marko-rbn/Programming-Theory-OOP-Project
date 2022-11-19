using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float movementSpeed = 10;
    public float rotationSpeed = 90;
    private Vector3 motion;
    private Vector3 rotation;
    private Rigidbody rb;
    Vector3 m_EulerAngleVelocity;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //motion = new Vector3(0, 0, Input.GetAxisRaw("Vertical")) * movementSpeed;
        motion = transform.forward * Input.GetAxisRaw("Vertical") * movementSpeed;
        //rb.velocity = motion;

        rb.MovePosition(transform.position + motion);

        //float rotationAmount = Input.GetAxis("Horizontal");
        //rotation = new Vector3(0, rotationAmount, 0);
        //transform.eulerAngles = rotation;

        //rb.MoveRotation(new Quaternion(1, 0, 0, 0));

        m_EulerAngleVelocity = new Vector3(0, Input.GetAxis("Horizontal") * rotationSpeed, 0);
        Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * deltaRotation);

        //TODO: try attaching this to camera directly!
        //TODO: 3rd person cam movement: https://youtu.be/sNmeK3qK7oA
    }
}
