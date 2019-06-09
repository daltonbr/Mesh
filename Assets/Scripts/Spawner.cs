using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabsToSpawn;
    [Range(0, 100)][SerializeField] private int amountToSpawn;
    [Range(0f, 100f)][SerializeField] private float timeBetweenSpawns = 0;
    [Range(0f, 100f)] [SerializeField] private float radiusToSpawn;
    [SerializeField] private Vector3Int minSizeToSpawn;
    [SerializeField] private Vector3Int maxSizeToSpawn;
    [SerializeField] private float minRoundness;
    [SerializeField] private float maxRoundness;
    private void Start()
    {
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0 || amountToSpawn == 0) yield break;
        
        var wait = new WaitForSeconds(timeBetweenSpawns);
        foreach (var prefab in prefabsToSpawn)
        {
            for (int i = 0; i < amountToSpawn; i++)
            {
                var positionToSpawn = transform.position + Random.insideUnitSphere * radiusToSpawn;
                var go = Instantiate(prefab, positionToSpawn, transform.rotation, transform);
                go.name = i.ToString();
                GenerateRoundedCubeRandom(go);
                yield return wait;
            }
        }
    }

    private void GenerateRoundedCubeRandom(GameObject go)
    {
        var size = new Vector3Int(
            Random.Range(minSizeToSpawn.x, maxSizeToSpawn.x),
            Random.Range(minSizeToSpawn.y, maxSizeToSpawn.y),
            Random.Range(minSizeToSpawn.z, maxSizeToSpawn.z));
        var roundness = Random.Range(minRoundness, maxRoundness);

        var minSide = Mathf.Min(size.x, size.y, size.z);
        if (roundness * 2f > minSide)
        {
            roundness = (float)minSide / 2;
        }
        
        go.GetComponent<RoundedCube>()?.Generate(size, roundness);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radiusToSpawn);
    }
}
