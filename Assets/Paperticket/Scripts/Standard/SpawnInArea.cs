using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnInArea : MonoBehaviour {
    [SerializeField] List<Transform> objects = new List<Transform>();
    [Space(5)]
    [SerializeField] bool autoSpawn = true;
    [SerializeField] [Min(0)] float spawnDelay;
    [Space(5)]
    [SerializeField] SpawningBox[] spawningAreas = new SpawningBox[1];
    [SerializeField] AnimationCurve spawningAreaChance;

    [Space(10)]
    [SerializeField] Color GizmosColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);
    [SerializeField] float GizmosAlpha = 0.5f;

    int spawnInt;

    Coroutine spawningCo;

    // Start is called before the first frame update
    void OnEnable() {
        if (objects.Count == 0) {
            Debug.LogError("[SpawnInArea] ERROR -> No objects set! Disabling.");
            enabled = false;
        }
        if (autoSpawn) Activate();
    }

    public void Activate() {
        if (spawningCo != null) StopCoroutine(spawningCo);
        spawningCo = StartCoroutine(Spawning());
    }

    public void Deactivate() {
        if (spawningCo != null) StopCoroutine(spawningCo);
    }



    IEnumerator Spawning() {

        SpawningBox chosenArea = null;

        while (enabled) {

            objects[spawnInt].gameObject.SetActive(false);


            chosenArea = spawningAreas[spawningAreas.Length * (int)spawningAreaChance.Evaluate(Random.value)];

            objects[spawnInt].localPosition = new Vector3(Random.Range(chosenArea.position.x + chosenArea.scale.x, chosenArea.position.x - chosenArea.scale.x),
                                                    Random.Range(chosenArea.position.y + chosenArea.scale.y, chosenArea.position.y - chosenArea.scale.y),
                                                     Random.Range(chosenArea.position.z + chosenArea.scale.z, chosenArea.position.z - chosenArea.scale.z));
            objects[spawnInt].rotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
            yield return null;


            objects[spawnInt].gameObject.SetActive(true);


            spawnInt = (spawnInt + 1) % objects.Count;

            if (spawnDelay > 0) yield return new WaitForSeconds(spawnDelay);
            else yield return null;
        }

    }


    void OnDrawGizmosSelected()  {
        foreach (SpawningBox area in spawningAreas) {
            Gizmos.color = GizmosColor;
            Gizmos.DrawWireCube(transform.position + area.position, area.scale);
            Gizmos.color = new Color(GizmosColor.r, GizmosColor.g, GizmosColor.b, GizmosAlpha);
            Gizmos.DrawCube(transform.position + area.position, area.scale);
        }
    }
}

[System.Serializable]
public class SpawningBox {

    public Vector3 position = Vector3.zero;
    public Vector3 scale = Vector3.zero;

    public SpawningBox() {
        position = Vector3.zero;
        scale = Vector3.zero;
    }

    public SpawningBox(Vector3 relativePosition, Vector3 relativeScale, float spawningWeight) {
        position = relativePosition;
        scale = relativeScale;
    }

}