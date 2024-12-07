using System;
using System.Collections;
using System.Collections.Generic;
using Fries;
using Fries.Pool;
using Fries.TaskPerformer;
using Unity.VisualScripting;
using UnityEngine;

public class SceneObjects : MonoBehaviour {
    public static SceneObjects sceneObj;

    public Transform max;
    public Transform min;

    public GameObject coralPrefab;
    public Transform coralRoot;
    public GameObject fishPrefab;
    public Transform fishRoot;
    public GameObject sharkPrefab;
    public Transform sharkRoot;
    public GameObject jellyfishPrefab;
    public Transform jellyfishRoot;

    public CompPool<Coral> corals;
    public CompPool<Organism> fishes;
    public CompPool<Organism> sharks;
    public CompPool<Organism> jellyfishes;

    // Start is called before the first frame update
    void Start() {
        sceneObj = this;
        
        // Place three corals at random position at the bottom of the scene
        corals = coralPrefab.toPool<Coral>(coralRoot, 3);
        for (int i = 0; i < 3; i++) {
            Coral coral = corals.activate();
            coral.transform.position =
            new Vector3(UnityEngine.Random.Range(min.position.x, max.position.x), min.position.y, 0);
        }
        // Spwan 3 fishes when game starts
        fishes = fishPrefab.toPool<Organism>(fish => fish.init(), fish => fish.resetValues(), fishRoot, 6);
        for (int i = 0; i < 6; i++) {
            Fish fish = (Fish)fishes.activate();
            fish.pool = fishes;
            fish.transform.position = new Vector3(UnityEngine.Random.Range(min.position.x, max.position.x),
            UnityEngine.Random.Range(min.position.y, max.position.y), 0);
            fish.onActivate();
        }
        // Spwan 2 shark when game starts
        sharks = sharkPrefab.toPool<Organism>(shark => shark.init(), shark => shark.resetValues(), sharkRoot, 2);
        for (int i = 0; i < 2; i++) {
            Shark shark = (Shark)sharks.activate();
            shark.pool = sharks;
            shark.transform.position = new Vector3(UnityEngine.Random.Range(min.position.x, max.position.x),
            UnityEngine.Random.Range(min.position.y, max.position.y), 0);
            shark.onActivate();
        }
        // Spwan 1 jellyfish when game starts
        jellyfishes = jellyfishPrefab.toPool<Organism>(jelly => jelly.init(), jelly => jelly.resetValues(), jellyfishRoot, 1);
        for (int i = 0; i < 1; i++) {
            Jellyfish jellyfish = (Jellyfish)jellyfishes.activate();
            jellyfish.pool = jellyfishes;
            jellyfish.transform.position = new Vector3(UnityEngine.Random.Range(min.position.x, max.position.x),
            UnityEngine.Random.Range(min.position.y, max.position.y), 0);
            jellyfish.onActivate();
        }
    }

    void FixedUpdate() {
        if (jellyfishes.activeSize() == 0) respawnJellyfish();

        if (fishes.activeSize() == 0) respawnFish();

        if (sharks.activeSize() == 0) respawnShark();
    }

    bool temp = false;
    void respawnJellyfish() {
        if (temp) return;

        TaskPerformer.inst().scheduleTask((Action)(() => {
            Jellyfish jellyfish = (Jellyfish)jellyfishes.activate();
            jellyfish.setPool(jellyfishes);
            jellyfish.transform.position = new Vector3(UnityEngine.Random.Range(min.position.x, max.position.x),
            UnityEngine.Random.Range(min.position.y, max.position.y), 0);
            jellyfish.onActivate();
            temp = false;
        }), 3f);

        temp = true;
    }

    bool temp1 = false;
    void respawnShark() {
        if (temp1) return;

        TaskPerformer.inst().scheduleTask((Action)(() => {
            Shark shark = (Shark)sharks.activate();
            shark.setPool(sharks);
            shark.transform.position = new Vector3(UnityEngine.Random.Range(min.position.x, max.position.x),
            UnityEngine.Random.Range(min.position.y, max.position.y), 0);
            shark.onActivate();
            temp1 = false;
        }), 3f);

        temp1 = true;
    }

    bool temp2 = false;
    void respawnFish() {
        if (temp2) return;

        TaskPerformer.inst().scheduleTask((Action)(() => {
            Fish fish = (Fish)fishes.activate();
            fish.setPool(fishes);
            fish.transform.position = new Vector3(UnityEngine.Random.Range(min.position.x, max.position.x),
            UnityEngine.Random.Range(min.position.y, max.position.y), 0);
            fish.onActivate();
            temp2 = false;
        }), 3f);

        temp2 = true;
    }
}
