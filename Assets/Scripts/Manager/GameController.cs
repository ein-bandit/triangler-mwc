using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public int startGameDelayInSeconds = 3;
    public float endGameDelay = 3f;

    private Text countdown;
    private Text gameEnd;

    private PlayerManager playerManager;

    void Start()
    {
        countdown = FindObjectOfType<Canvas>().transform.Find("Countdown").GetComponent<Text>();
        gameEnd = FindObjectOfType<Canvas>().transform.Find("End").gameObject.GetComponent<Text>();
        gameEnd.gameObject.SetActive(false);
        playerManager = GameManager.instance.GetComponent<PlayerManager>();
    }

    public IEnumerator ShowEnd(string winner)
    {
        gameEnd.text = "Game over. <" + (winner == null ? "AI" : winner) + " won!>";
        gameEnd.gameObject.SetActive(true);
        yield return new WaitForSeconds(endGameDelay);
        GameManager.instance.AdvanceToMenu();
    }

    public IEnumerator StartCountdown()
    {
        countdown = FindObjectOfType<Canvas>().transform.Find("Countdown").GetComponent<Text>();
        int count = startGameDelayInSeconds;
        do
        {
            countdown.text = count.ToString();
            count--;
            yield return new WaitForSeconds(1f);
        } while (count > 0);

        playerManager.StartGame();

        countdown.text = "GO!";
        yield return new WaitForSeconds(.75f);
        countdown.gameObject.SetActive(false);
    }
}
