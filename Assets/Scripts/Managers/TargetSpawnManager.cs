using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpawnManager : MonoBehaviour
{
    public Vector3 GetRandomTargetSpawn()
    {
        return this.transform.GetChild(Random.Range(0, this.transform.childCount)).position;
    }
}
