using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLoseText : MonoBehaviour
{

    List<string> winPhrases = new List<string>();
    List<string> losePhrases = new List<string>();
    GameManager gameManager;

    public TextMesh winLoseText;

    public void Start() {

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

    public string ChooseContent() {
        int randPhrase = Random.Range(0, 3);
        if (gameManager.DidWin()) {
            return winPhrases[randPhrase];
        } else if (!gameManager.DidWin()) {
            return losePhrases[randPhrase];
        }
        return "Oops";
    }
}
