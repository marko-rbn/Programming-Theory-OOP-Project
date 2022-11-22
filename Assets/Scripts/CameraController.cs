using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float movementSpeed = 10;
    public float rotationSpeed = 90;
    public float autoRotationSpeed = 30;

    private float maxCameraHeight = 700;
    private float minCameraHeight = 50;
    private float maxCameraDownAngle = 60;
    private float minCameraDownAngle = 25;

    private Rigidbody rb;
    private GameObject cameraReference;
    private MainManager mainManager;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
        cameraReference = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //initialize vectors to zero
        Vector3 motionPlanar = Vector3.zero;  //no forward motion
        Vector3 motionUp = Vector3.zero;  //no vertical motion
        Vector3 angularVelocity = Vector3.zero;  //no rotation

        //TODO: constrain camera container location

        if (mainManager.selectedEntity == null)
        {
            //move camera container laterally using keyboard
            motionPlanar = transform.forward * Input.GetAxisRaw("Vertical") * movementSpeed;

            //rotate using keyboard
            angularVelocity = new Vector3(0, Input.GetAxis("Horizontal") * rotationSpeed, 0);
        } else
        {
            //automatically move camera container and rotate toward selected entity

            //rotate smoothly toward selected entity
            Vector3 targetDirection = (mainManager.selectedEntity.transform.position - transform.position);
            targetDirection.y = 0;  //cancel any vertical rotation component (pitch)
            float distance = targetDirection.magnitude;  //consider horizontal distance only
            targetDirection = targetDirection.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * autoRotationSpeed);

            //also move if too far (dependent on height also)
            if (distance > transform.position.y)
            {
                motionPlanar = targetDirection * (distance - 50) * Time.deltaTime;
            }
        }

        //move vertically using keyboard
        if (Input.GetKey(KeyCode.X) && transform.position.y <= maxCameraHeight)
        {
            motionUp = transform.up * movementSpeed;
        } else if (Input.GetKey(KeyCode.Z) && transform.position.y >= minCameraHeight)
        {
            motionUp = transform.up * (-movementSpeed);
        }
        rb.MovePosition(transform.position + motionPlanar + motionUp);

        //if camera container moved vertically, adjust downward angle of the camera itself
        if (motionUp != Vector3.zero)
        {
            float newAngleX = transform.position.y * (maxCameraDownAngle - minCameraDownAngle) / (maxCameraHeight - minCameraHeight) + minCameraDownAngle;
            cameraReference.transform.localRotation = Quaternion.AngleAxis(newAngleX, Vector3.right);
        }

        //rotate camera container if angular velocity is set
        if (angularVelocity != Vector3.zero)
        {
            Quaternion deltaRotation = Quaternion.Euler(angularVelocity * Time.fixedDeltaTime);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }
}
