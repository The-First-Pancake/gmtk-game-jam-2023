using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    // public GameObject MainMenuContainer;
    // public GameObject CreditsContainer;
    public int gameSceneIndex;

    public void StartGame(){
        SceneManager.LoadScene(gameSceneIndex);
    }

    // public void ShowCredits() {
    //     MainMenuContainer.SetActive(false);
    //     CreditsContainer.SetActive(true);
    // }

    // public void BackToMainMenu () {
    //     MainMenuContainer.SetActive(true);
    //     CreditsContainer.SetActive(false);
    // }
}