using System.Collections;
using System.Collections.Generic;
using MobileWebControl.NetworkData.InputData;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speedStep = 100f;
    private float speed = 0f;
    private float steeringAngle = 0f;

    private Rigidbody _rigidbody;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            speed += speedStep;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            speed -= speedStep;
        }
        Debug.Log($"new speed {speed}");
        transform.position = Vector3.Lerp(transform.position, transform.position + (Vector3.forward * speed) + (Vector3.left * steeringAngle * 0.1f), Time.deltaTime);
    }

    public void StartMoving()
    {
        _rigidbody.isKinematic = false;
    }

    public void ReceiveInput(InputDataType type, object inputData)
    {
        Debug.Log("received input in player");
        switch (type)
        {
            case InputDataType.accelerometer:
                Vector3 deviceOrientation = (Vector3)inputData;

                //z (c) = x; x (a) = y; y (b) = z;
                transform.rotation = Quaternion.Euler(180 + transform.rotation.x, 90 + deviceOrientation.x, transform.rotation.z);

                steeringAngle += deviceOrientation.x - 90;

                //add or reduce speed with y (a).
                //should be -1 to 1 now.
                float multiplier = deviceOrientation.y / 180;
                speed += speedStep * multiplier;
                if (speed < 0)
                {
                    speed = 0;
                }
                //roll x (b) is not needed by now.
                break;
            default:
                break;
        }
    }
}
