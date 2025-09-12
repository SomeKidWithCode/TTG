using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEditor;

using UnityEngine;

public class UniControllerScript : MonoBehaviour
{
    public bool ExpAttackSys = true;
    public GameObject RangedEnemyPrefab;
    public float DistM = 2f;
    public int SpawnCount = 10;

    private void Start()
    {
        for (float i = 0; i < SpawnCount / 2; i++)
        {
            GameObject GO = PrefabUtility.InstantiatePrefab(RangedEnemyPrefab).GameObject();
            GO.transform.position = transform.position + new Vector3(0, 0, i * DistM);
        }
        for (float i = 0; i < SpawnCount / 2; i++)
        {
            GameObject GO = PrefabUtility.InstantiatePrefab(RangedEnemyPrefab).GameObject();
            GO.transform.position = transform.position - new Vector3(0, 0, i * DistM);
        }
    }
}
