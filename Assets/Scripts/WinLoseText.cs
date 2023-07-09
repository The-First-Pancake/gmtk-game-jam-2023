using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLoseText : MonoBehaviour
{

    List<string> winPhrases = new List<string>();
    List<string> losePhrases = new List<string>();
    GameManager gameManager;
    SceneHandler sceneHandler;
    float startTime;
    float duration = 0.5f;

    bool transitionIn = false;
    bool creditsScroll = false;

    public TMPro.TextMeshProUGUI winLoseText;
    public TMPro.TextMeshProUGUI credits;

    public void Start() {

        winLoseText.text = "";
        winLoseText.transform.localScale = new Vector3(0, 0, 0);

        winLoseText.transform.eulerAngles = Vector3.forward * Random.Range(15, -15);


        gameManager = gameObject.GetComponent<GameManager>();
        sceneHandler = gameObject.GetComponent<SceneHandler>();

        winPhrases.Add("Level Complete!");
        winPhrases.Add("Nice One!");
        winPhrases.Add("Freakin' Sweet");
        winPhrases.Add("Gabbagool!");

        losePhrases.Add("So Close!");
        losePhrases.Add("Try Again!");
        losePhrases.Add("Needs More Fire!");
        losePhrases.Add("Almost!!");

        if (sceneHandler.getCurrentLevel() == "Level_EndScreen") {
            winLoseText.text = "Thanks for Playing!";
            Invoke("TransitionIn", 1);
            Invoke("CreditsScroll", 3);
            sceneHandler.Invoke("LevelTransitionBackToMenu", 20);
            Invoke("DestroyText", 20);
        }

    }

    private void TransitionIn() {
        transitionIn = true;
        startTime = Time.time;
    }

    private void CreditsScroll() {
        creditsScroll = true;
        startTime = Time.time;
    }

    private void DestroyText() {
        winLoseText.text = "";
        credits.text = "";
    }

    public void Update() {
        if (transitionIn) {
            // Calculate the fraction of the total duration that has passed.
            float t = (Time.time - startTime) / duration;
            winLoseText.transform.localScale = new Vector3(
                Mathf.SmoothStep(0, 1, t), 
                Mathf.SmoothStep(0, 1, t), 
                1
            );

            if (Vector3.Distance(winLoseText.transform.localScale, new Vector3(1, 1, 1)) < 0.1f) {
                transitionIn = false;
            }

        }

        if (creditsScroll) {
            winLoseText.transform.position = new Vector3(
                winLoseText.transform.position.x, 
                winLoseText.transform.position.y + 0.01f, 
                winLoseText.transform.position.z
            );

            credits.transform.position = new Vector3(
                credits.transform.position.x,
                credits.transform.position.y + 0.01f,
                credits.transform.position.z
            );
        }

    }



    public void SetWinLoseText(string levelOutcome) {

        if (levelOutcome == "WIN") {
            winLoseText.text = winPhrases[Random.Range(0, winPhrases.Count - 1)];
        } else if (levelOutcome == "LOSE") {
            winLoseText.text = losePhrases[Random.Range(0, losePhrases.Count - 1)];
        }
        TransitionIn();
    }
}
