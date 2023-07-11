using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class childrenReporter : MonoBehaviour
{
    float interval = .2f;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("reportChildren", 0, interval);
    }

    void reportChildren()
    {
        int num = GetComponentsInChildren<Transform>().Length;
        GameManager.instance.spawnDebugMSG(num.ToString(), Vector3.zero, 0, interval);
    }
}
