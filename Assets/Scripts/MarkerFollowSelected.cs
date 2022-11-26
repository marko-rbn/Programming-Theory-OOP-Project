using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerFollowSelected : MonoBehaviour
{
    public GameObject target;

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            transform.position = target.transform.position;
            transform.Rotate(Vector3.up * 90 * Time.deltaTime);
        }
    }
}
