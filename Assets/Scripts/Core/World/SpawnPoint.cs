using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private static List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    public static Vector3 GetRandomSpawnPos()
    {
		if (spawnPoints.Count > 0)
		{
			return spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;
		}

		return Vector3.zero;    
	}

	private void OnEnable()
	{
		spawnPoints.Add(this);
	}

	private void OnDisable()
	{
		spawnPoints.Remove(this);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;

		Gizmos.DrawSphere(transform.position, 1f);
	}
}
