using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{

    // public GameObject MainMenuContainer;
    // public GameObject CreditsContainer;
    public Camera cam;
    public Vector3 startMarker;
    public Vector3 endMarker;
    private bool startTransitionOut = false;
    private bool startTransitionIn = false;
    public float duration = 5.0f;
    private float startTime;
    private float camX;
    private float camY;

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
        camX = cam.transform.position.x;
        camY = cam.transform.position.y;

    }

    public void LevelTransitionIn() {
        startTransitionIn = true;
        // Make a note of the time the script started.
        startTime = Time.time;
        camX = cam.transform.position.x;
        camY = cam.transform.position.y;
    }

    public void Update() {

        if (startTransitionIn) {
            // Calculate the fraction of the total duration that has passed.
            float t = (Time.time - startTime) / duration;
            cam.transform.position = new Vector3(
                Mathf.SmoothStep(camX, startMarker.x,  t), 
                Mathf.SmoothStep(camY, startMarker.y, t), 
                cam.transform.position.z
            );
            
            if (Vector3.Distance(cam.transform.position, startMarker) < 0.1f) {
                startTransitionIn = false;
            }
        }

        if (startTransitionOut) {
            // Calculate the fraction of the total duration that has passed.
            float t = (Time.time - startTime) / duration;
            cam.transform.position = new Vector3(
                Mathf.SmoothStep(camX, endMarker.x, t), 
                Mathf.SmoothStep(camY, endMarker.y, t), 
                cam.transform.position.z
            );
            
            if (Vector3.Distance(cam.transform.position, endMarker) < 1f) {
                Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(currentScene.buildIndex + 1);
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