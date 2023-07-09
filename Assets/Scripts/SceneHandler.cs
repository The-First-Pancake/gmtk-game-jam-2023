using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{

    // public GameObject MainMenuContainer;
    // public GameObject CreditsContainer;
    private Camera cam;
    public Vector3 inCameraPosition;
    public Vector3 outCameraPosition;
    private Vector3 levelCameraPosition = new Vector3(0, 0, -10);
    private bool startTransitionOut = false;
    private bool startTransitionIn = false;
    private bool startTransitionRestart = false;
    private bool startTransitionBackToMenu = false;
    private bool isRestarting = false;
    private float duration = 0.75f;
    private float startTime;

    public void Start() {
        isRestarting = false;
        startTransitionOut = false;
        startTransitionIn = false;
        startTransitionRestart = false;
        startTransitionBackToMenu = false;
        LevelTransitionIn();
        cam = Camera.main;
    }

    public void NextLevel(){
        LevelTransitionOut();
    }

    public void Restart(){
        LevelTransitionRestart();
    }

    public bool IsTransitioning(){
        return startTransitionOut || startTransitionIn || startTransitionRestart || startTransitionBackToMenu || isRestarting || (SceneManager.GetActiveScene().name == "Main_Menu");
    }

    public void LevelTransitionOut() {
        startTransitionOut = true;
        // Make a note of the time the script started.
        startTime = Time.time;
    }

    public void LevelTransitionIn() {
        startTransitionIn = true;
        // Make a note of the time the script started.
        startTime = Time.time;
    }

    public void LevelTransitionRestart() {
        startTransitionRestart = true;
        // Make a note of the time the script started.
        startTime = Time.time;
    }
    public void LevelTransitionBackToMenu() {
        startTransitionBackToMenu = true;
        // Make a note of the time the script started.
        startTime = Time.time;
    }

    public string getCurrentLevel() {
        return SceneManager.GetActiveScene().name;
    }

    public void Update() {

        if (startTransitionIn) {
            // Calculate the fraction of the total duration that has passed.
            float t = (Time.time - startTime) / duration;
            cam.transform.position = new Vector3(
                EaseOutQuartD(inCameraPosition.x / 4, levelCameraPosition.x, t), 
                EaseOutQuartD(-inCameraPosition.y / 4, levelCameraPosition.y, t), 
                levelCameraPosition.z
            );
            
            if (Vector3.Distance(cam.transform.position, levelCameraPosition) < 0.001f) {
                startTransitionIn = false;
            }
        }

        if (startTransitionOut) {
            // Calculate the fraction of the total duration that has passed.
            float t = (Time.time - startTime) / duration;
            cam.transform.position = new Vector3(
                EaseInBack(levelCameraPosition.x, outCameraPosition.x / 4, t), 
                EaseInBack(levelCameraPosition.y, outCameraPosition.y / 4, t), 
                levelCameraPosition.z
            );
            
            if (Vector3.Distance(cam.transform.position, outCameraPosition) < 1f) {
                startTransitionOut = false;
                Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.LoadSceneAsync(currentScene.buildIndex + 1);
            }
        }

        if (startTransitionRestart) {
            isRestarting = true;
            // Calculate the fraction of the total duration that has passed.
            float t = (Time.time - startTime) / duration;
            cam.transform.position = new Vector3(
                EaseInBack(levelCameraPosition.x, inCameraPosition.x / 4, t), 
                EaseInBack(levelCameraPosition.y, inCameraPosition.y / 4, t), 
                levelCameraPosition.z
            );
            
            if (Vector3.Distance(cam.transform.position, inCameraPosition) < 1f) {
                Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.LoadSceneAsync(currentScene.buildIndex);
                startTransitionRestart = false;
            }
        }

        if (startTransitionBackToMenu) {
            isRestarting = true;
            // Calculate the fraction of the total duration that has passed.
            float t = (Time.time - startTime) / duration;
            cam.transform.position = new Vector3(
                EaseInBack(levelCameraPosition.x, inCameraPosition.x / 4, t), 
                EaseInBack(levelCameraPosition.y, inCameraPosition.y / 4, t), 
                levelCameraPosition.z
            );
            
            if (Vector3.Distance(cam.transform.position, inCameraPosition) < 1f) {
                SceneManager.LoadSceneAsync(0);
                startTransitionBackToMenu = false;
            }
        }

    }

    public static float EaseInBack(float start, float end, float value)
    {
        end -= start;
        value /= 1;
        float s = 1.70158f;
        return end * (value) * value * ((s + 1) * value - s) + start;
    }

    public static float EaseOutQuartD(float start, float end, float value)
    {
        value--;
        end -= start;
        return -4f * end * value * value * value;
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