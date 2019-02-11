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

    private float resetMatrialColorDelay = 3f;

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

    private bool started = false;

    public float startGameDelay = 3f;

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

    public IEnumerator StartPlayerMovement()
    {
        yield return new WaitForSeconds(startGameDelay);
        started = true;
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
                    case "tap-area-mask":
                        //choose random color (from other players? get from playerManager)
                        _material.color = playerManager.GetRandomPlayerColor(this.playerColor);
                        Invoke("ResetMaterialColor", resetMatrialColorDelay);
                        break;
                    case "tap-area-boost":
                        if (canBoost)
                        {
                            boostActivated = true;
                        }
                        break;
                    case "tap-area-stealth":
                        EnableStealth();
                        break;
                    case "ready":
                        Debug.Log("should already be handled in playermanager");
                        //to be implemented, playermanager set ready.                  
                        break;
                    case "reset-orientation":
                        //reset position if something went wrong?
                        initalRotationX = ((Vector3)inputData).x;
                        break;
                    default:
                        //do nothing, tap not recognized.
                        break;
                }
                break;
            case InputDataType.proximity:
                Debug.Log($"received proximity {activateStealthDelay}");
                //false indicates sensor is blocked / covered.
                if ((bool)inputData == false)
                {
                    EnableStealth();
                }
                break;
            default:
                break;
        }
    }

    private void EnableStealth()
    {
        if (canActivateStealth)
        {
            _renderer.enabled = false;
            canActivateStealth = false;
            Invoke("ResetInvisible", 1f);
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
