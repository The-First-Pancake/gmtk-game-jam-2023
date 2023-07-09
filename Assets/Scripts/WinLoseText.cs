using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLoseText : MonoBehaviour
{

    List<string> winPhrases = new List<string>();
    List<string> losePhrases = new List<string>();
    GameManager gameManager;
    float startTime;
    float duration = 0.5f;

    bool transitionIn = false;
    bool transitionOut = false;

    public TMPro.TextMeshProUGUI winLoseText;

    public void Start() {

        winLoseText.text = "";
        winLoseText.transform.localScale = new Vector3(0, 0, 0);
        winLoseText.transform.eulerAngles = Vector3.forward * Random.Range(15, -15);

        gameManager = gameObject.GetComponent<GameManager>();

        winPhrases.Add("Level Complete!");
        winPhrases.Add("Nice One!");
        winPhrases.Add("Freakin' Sweet");
        winPhrases.Add("Gabbagool!");

        losePhrases.Add("So Close!");
        losePhrases.Add("Try Again!");
        losePhrases.Add("Needs More Fire!");
        losePhrases.Add("Almost!!");
    }

    private void TransitionIn() {
        transitionIn = true;
        startTime = Time.time;
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
        }
    }

    public void SetWinLoseText(string levelOutcome) {

        if (levelOutcome == "WIN") {
            winLoseText.text = winPhrases[Random.Range(0, winPhrases.Count - 1)];
            TransitionIn();
        } else if (levelOutcome == "LOSE") {
            winLoseText.text = losePhrases[Random.Range(0, losePhrases.Count - 1)];
            TransitionIn();
        }
    }
}
