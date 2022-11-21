using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float movementSpeed = 10;
    public float rotationSpeed = 90;

    private float maxCameraHeight = 700;
    private float minCameraHeight = 50;
    private float maxCameraDownAngle = 60;
    private float minCameraDownAngle = 25;

    private Rigidbody rb;
    private GameObject cameraReference;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraReference = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //TODO: constrain camera location

        //Move
        Vector3 motionForward = transform.forward * Input.GetAxisRaw("Vertical") * movementSpeed;
        Vector3 motionUp = Vector3.zero;  //no vertical motion, unless X or Z are pressed and height is within constraints
        if (Input.GetKey(KeyCode.X) && transform.position.y <= maxCameraHeight)
        {
            motionUp = transform.up * movementSpeed;
        } else if (Input.GetKey(KeyCode.Z) && transform.position.y >= minCameraHeight)
        {
            motionUp = transform.up * (-movementSpeed);
        }
        rb.MovePosition(transform.position + motionForward + motionUp);
        if (motionUp != Vector3.zero)
        {
            //if container moved vertically, adjust downward angle of the camera itself
            float newAngleX = transform.position.y * (maxCameraDownAngle - minCameraDownAngle) / (maxCameraHeight - minCameraHeight) + minCameraDownAngle;
            cameraReference.transform.localRotation = Quaternion.AngleAxis(newAngleX, Vector3.right);
        }

        //Rotate
        Vector3 angularVelocity = new Vector3(0, Input.GetAxis("Horizontal") * rotationSpeed, 0);
        Quaternion deltaRotation = Quaternion.Euler(angularVelocity * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * deltaRotation);

        //TODO: 3rd person cam movement: https://youtu.be/sNmeK3qK7oA
    }
}
