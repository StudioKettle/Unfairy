using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TossingCoins : MonoBehaviour {

    [SerializeField] List<Transform> coins = new List<Transform>();

    [SerializeField] [Min(0)] float spawnDelay;

    [SerializeField] Vector3 minPosition;
    [SerializeField] Vector3 maxPosition;

    int spawnInt;

    Coroutine spawningCo;

    // Start is called before the first frame update
    void OnEnable() {
        if (coins.Count == 0) {
            Debug.LogError("[TossingCoins] ERROR -> No coins set! Disabling.");
            enabled = false;
        }
        StartCoins();
    }

    public void StartCoins() {
        if (spawningCo != null) StopCoroutine(spawningCo);
        spawningCo = StartCoroutine(SpawningCoins());
    }

    public void StopCoins() {
        if (spawningCo != null) StopCoroutine(spawningCo);
    }



    IEnumerator SpawningCoins() {

        while (enabled) {

            coins[spawnInt].gameObject.SetActive(false);


            coins[spawnInt].localPosition = new Vector3(Random.Range(minPosition.x, maxPosition.x),
                                                    Random.Range(minPosition.y, maxPosition.y),
                                                     Random.Range(minPosition.z, maxPosition.z));
            coins[spawnInt].rotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
            yield return null;


            coins[spawnInt].gameObject.SetActive(true);


            spawnInt = (spawnInt + 1) % coins.Count;

            if (spawnDelay > 0) yield return new WaitForSeconds(spawnDelay);
            else yield return null;
        }

    }

}
