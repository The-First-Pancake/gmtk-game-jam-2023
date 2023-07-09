using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{

    // public GameObject MainMenuContainer;
    // public GameObject CreditsContainer;
    public Camera cam;
    public Vector3 inCameraPosition;
    public Vector3 outCameraPosition;
    private Vector3 levelCameraPosition = new Vector3(0, 0, -10);
    private bool startTransitionOut = false;
    private bool startTransitionIn = false;
    public float duration = 5.0f;
    private float startTime;

    public void Start() {
        LevelTransitionIn();
    }

    public void NextLevel(){
        LevelTransitionOut();
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

    public void Update() {

        Debug.Log("startTransitionIn: " + startTransitionIn);
        Debug.Log("startTransitionOut: " + startTransitionOut);


        Debug.Log("cam.transform.position: " + cam.transform.position);
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
            Debug.Log("t: " + t);
            Debug.Log("duration: " + duration);
            Debug.Log("Easing function calculated Y Position: " + EaseInBack(levelCameraPosition.y, outCameraPosition.y, t));
            cam.transform.position = new Vector3(
                EaseInBack(levelCameraPosition.x / 4, outCameraPosition.x, t), 
                EaseInBack(levelCameraPosition.y / 4, outCameraPosition.y, t), 
                levelCameraPosition.z
            );
            
            if (Vector3.Distance(cam.transform.position, outCameraPosition) < 1f) {
                Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.LoadSceneAsync(currentScene.buildIndex + 1);
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