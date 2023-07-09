using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindIndicator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start(){

    }

    // Update is called once per frame
    void Update()
    {
        float angle = -Vector3.SignedAngle(WorldMap.instance.grid.WorldToCell(Vector3.down), GameManager.instance.wind.GetIsoWindDir(), WorldMap.instance.grid.WorldToCell(Vector3.back));
        transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
    }
}
