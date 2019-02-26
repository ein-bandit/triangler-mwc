using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPlayer : MonoBehaviour
{
    private Text readyText;

    private Color playerColor;
    private Material _material;

    private float startingPlayerPosition = -7.5f;
    private float startingPlayerPositionStep = 2.5f;

    private int playerIndex;

    private void Start()
    {
        _material = GetComponentInChildren<Renderer>().material;

        _material.color = this.playerColor;
    }

    private void OnEnable()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            readyText = FindObjectOfType<Canvas>().transform.Find("PlayersReady").Find(this.playerIndex.ToString()).GetComponent<Text>();
        }
    }

    public void SetReady()
    {
        readyText.enabled = true;
    }

    public void Init(Color color, int playerIndex)
    {
        this.playerColor = color;
        float startingPos = startingPlayerPosition + startingPlayerPositionStep * playerIndex;

        this.playerIndex = playerIndex;

        transform.position = new Vector3(startingPos, transform.position.y, transform.position.z);
    }
}
