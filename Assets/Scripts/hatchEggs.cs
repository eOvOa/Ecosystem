using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hatchEggs : MonoBehaviour
{
    [Header("Hatching Settings")]
    public float hatchTime = 10f; // Time for the egg to hatch
    public GameObject babyFishPrefab;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Start the hatching timer
        Invoke(nameof(Hatch), hatchTime);
        
    }
    private void Hatch()
    {
        for (int i = 0; i < 3; i++)
        {
            // Spawn eggs slightly offset around the fish's position
            Vector3 babyFishPosition = transform.position + new Vector3 (Random.Range(-0.5f, 0.5f), 0);
            Fish fish = (Fish)SceneObjects.sceneObj.fishes.activate();
            fish.pool = SceneObjects.sceneObj.fishes;
            fish.transform.position = babyFishPosition;
            fish.onActivate();
            
            // Destroy the egg after hatching
            Destroy(gameObject);
        }
    }
}
