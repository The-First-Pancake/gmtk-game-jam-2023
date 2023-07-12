using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionBehavior : MonoBehaviour
{
    public float boomRadius = 1.2f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(boomSequence());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator boomSequence(){
        yield return new WaitForSeconds(.1f);
        int maxCellDistance = Mathf.CeilToInt(boomRadius);
        TileBehavior thisTile = WorldMap.instance.GetTopTileFromWorldPoint(transform.position);
        for(int i = -maxCellDistance; i < maxCellDistance;i++){
            for(int j = -maxCellDistance; j < maxCellDistance;j++){
                TileBehavior checkTile = WorldMap.instance.GetTopTile(thisTile.IsoCoordinates + new Vector3Int(i,j,0));
                if (Vector3Int.Distance(checkTile.IsoCoordinates, thisTile.IsoCoordinates) < boomRadius){
                    checkTile.Fire.ignite();
                    checkTile.Fire.sustain *= 1/2;
                    if(checkTile.IsUpper && checkTile.Fire.flambilityScore == 0){
                        checkTile.DeleteTile();
                    }
                    
                }
                
            }
        }
        yield return null;
    }
}
