using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindIndicator : MonoBehaviour
{
    RectTransform rectTransform;
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        float angle = -Vector3.SignedAngle(WorldMap.instance.grid.WorldToCell(Vector3.up), GameManager.instance.wind.GetIsoWindDir(), WorldMap.instance.grid.WorldToCell(Vector3.back));
        rectTransform.rotation = Quaternion.Euler(new Vector3(60, 0, angle));
    }
}
