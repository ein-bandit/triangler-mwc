using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityWebRTCControl.QRCode;

public class MenuController : MonoBehaviour
{
    public int gameCountdownInSeconds = 5;
    private Text playersCountText;

    private Text gameCountdownText;

    void Start()
    {
        playersCountText = FindObjectOfType<Canvas>().transform.Find("PlayersCount").GetComponent<Text>();
        gameCountdownText = FindObjectOfType<Canvas>().transform.Find("GameCountdown").GetComponent<Text>();
        gameCountdownText.gameObject.SetActive(false);
        playersCountText.text = "0";

        FindObjectOfType<QRCodeGenerator>().GenerateQRCode();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Debug.Log("Forcing Game Countdown.");
            GameManager.instance.ForceGameStartCountdown();
        }
    }

    public void UpdatePlayerCount(int newPlayerCount)
    {
        playersCountText.text = newPlayerCount.ToString();
    }

    public IEnumerator StartGameCountdown()
    {
        int count = gameCountdownInSeconds;
        gameCountdownText.gameObject.SetActive(true);
        do
        {
            gameCountdownText.text = count.ToString();
            count--;
            yield return new WaitForSeconds(1f);

        } while (count >= 0);

        GameManager.instance.AdvanceToGame();
    }
}
