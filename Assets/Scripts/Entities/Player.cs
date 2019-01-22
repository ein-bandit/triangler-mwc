using System.Collections;
using System.Collections.Generic;
using MobileWebControl.NetworkData.InputData;
using UnityEngine;

public class Player : MonoBehaviour
{
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
                Vector3 rotation = (Vector3)inputData;
                transform.rotation = Quaternion.Euler(rotation);
                break;
            default:
                break;
        }
    }
}
