using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float movementSpeed = 10;
    public float rotationSpeed = 90;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Move
        Vector3 motionForward = transform.forward * Input.GetAxisRaw("Vertical") * movementSpeed;
        Vector3 motionUp = Vector3.zero;  //no vertical motion, unless X or Z are pressed and height is within constraints
        if (Input.GetKey(KeyCode.X) && transform.position.y <= 500)  //max height 500
        {
            motionUp = transform.up * movementSpeed;
        } else if (Input.GetKey(KeyCode.Z) && transform.position.y >= 50)  //min height 50
        {
            motionUp = transform.up * (-movementSpeed);
        }
        rb.MovePosition(transform.position + motionForward + motionUp);

        //Rotate
        Vector3 angularVelocity = new Vector3(0, Input.GetAxis("Horizontal") * rotationSpeed, 0);
        Quaternion deltaRotation = Quaternion.Euler(angularVelocity * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * deltaRotation);

        //TODO: 3rd person cam movement: https://youtu.be/sNmeK3qK7oA
    }
}
