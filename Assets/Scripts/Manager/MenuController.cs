using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Text playersCountText;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdatePlayerCount(int newPlayerCount)
    {
        playersCountText.text = newPlayerCount.ToString();
    }
}
