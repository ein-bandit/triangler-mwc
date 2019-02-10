using System.Collections;
using System.Collections.Generic;
using MobileWebControl.NetworkData.InputData;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speedForce = 10f;
    public float rotationSpeed = 5f;
    public float boostForce = 15f;
    public float reactivateBoostTime = 2f;
    public float activateStealthDelay = 10f;


    private Rigidbody _rigidbody;
    private Renderer _renderer;
    private Material _material;
    private PlayerManager playerManager;


    private bool boostActivated = false;
    private bool canBoost = true;

    private bool canActivateStealth = true;

    private float currentRotation = 0f;

    private Color playerColor;
    private float initalRotationX = float.MinValue;
    private float maxRotationX = 50;

    public bool started = false;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponentInChildren<Renderer>();
        _material = _renderer.material;
        _material.color = this.playerColor;
    }

    void FixedUpdate()
    {
        if (started)
        {
            _rigidbody.AddForce(transform.forward * speedForce);

            if (canBoost && boostActivated)
            {
                boostActivated = false;
                canBoost = false;
                Invoke("ReactivateBoost", reactivateBoostTime);
                _rigidbody.AddForce(transform.forward * boostForce, ForceMode.Impulse);
            }

            _rigidbody.AddRelativeTorque(transform.up * rotationSpeed * currentRotation, ForceMode.Force);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall")
        {
            transform.rotation = Quaternion.LookRotation(Vector3.Reflect(transform.forward, other.transform.forward));
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.AddForce(transform.forward * speedForce, ForceMode.Impulse);
        }
    }

    public void StartMoving()
    {
        _rigidbody.isKinematic = false;
    }

    public void ReceiveInput(InputDataType type, object inputData)
    {
        Debug.Log($"received input in player: {type},{inputData}");
        switch (type)
        {
            case InputDataType.orientation:
                if (initalRotationX == float.MinValue)
                {
                    initalRotationX = ((Vector3)inputData).x;
                    //phone was initialized in default position - game can start.
                    started = true;
                }
                float rotationX = ((Vector3)inputData).x - initalRotationX;
                //get float from -1 to 1. (lerp to new rotation ?)
                currentRotation = Mathf.Clamp(rotationX, -maxRotationX, maxRotationX) / maxRotationX;
                //Debug.Log($"calculated deviceorientation {rotationX}, {currentRotation}");
                break;
            case InputDataType.tap:
                //Debug.Log($"received string {(string)inputData}");
                switch ((string)inputData)
                {
                    case "tap-area-1":
                        //choose random color (from other players? get from playerManager)
                        _material.color = Color.green;
                        Invoke("ResetMaterialColor", 1f);
                        break;
                    case "tap-area-2":
                        if (canBoost)
                        {
                            boostActivated = true;
                        }
                        break;
                    case "start":
                        //to be implemented, playermanager set ready.
                        break;
                    default:
                        //do nothing, tap not recognized.
                        break;
                }
                break;
            case InputDataType.proximity:
                Debug.Log($"received proximity {activateStealthDelay}");
                if (canActivateStealth && (bool)inputData == false)
                {
                    _renderer.enabled = false;
                    canActivateStealth = false;
                    Invoke("ResetInvisible", 1f);
                }
                break;
            default:
                break;
        }
    }

    public void SetPlayerColor(Color color)
    {
        this.playerColor = color;
    }
    private void ResetMaterialColor()
    {
        _material.color = this.playerColor;
    }

    private void ResetInvisible()
    {
        _renderer.enabled = true;
        Invoke("ResetCanActivateStealth", activateStealthDelay);
    }
    private void ResetCanActivateStealth()
    {
        canActivateStealth = true;
    }

    private void ReactivateBoost()
    {
        canBoost = true;
    }
}
