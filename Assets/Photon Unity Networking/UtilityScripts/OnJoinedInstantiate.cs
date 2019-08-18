using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OnJoinedInstantiate : MonoBehaviour
{
    public List<Transform> SpawnPosition;
    public float PositionOffset = 2.0f;
    public GameObject[] PrefabsToInstantiate;   // set in inspector

    public void OnJoinedRoom()
    {
        if (this.PrefabsToInstantiate != null)
        {
            GameObject o = PrefabsToInstantiate[PhotonNetwork.player.ID - 1 % PrefabsToInstantiate.Length];
            Debug.Log("Instantiating: " + o.name);

            Vector3 spawnPos = Vector3.up;
            if (this.SpawnPosition != null)
            {
                spawnPos = this.SpawnPosition[(PhotonNetwork.player.ID - 1) % SpawnPosition.Count].position;
            }

            Vector3 random = Random.insideUnitSphere;
            random.y = 0;
            random = random.normalized;
            Vector3 itempos = spawnPos + this.PositionOffset * random;

            PhotonNetwork.Instantiate(o.name, itempos, o.transform.rotation, 0);
        }
    }
}
