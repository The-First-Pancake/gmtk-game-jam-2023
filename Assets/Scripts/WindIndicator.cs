using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindIndicator : MonoBehaviour
{
    void Update()
    {
        transform.localRotation = Quaternion.Euler(new Vector3(0, -GameManager.instance.wind.windAngle, 0));
    }
}
