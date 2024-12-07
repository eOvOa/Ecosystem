using System;
using Fries;
using Fries.TaskPerformer;
using UnityEngine;

public class Fish : Organism
{
    private bool babyBool1 = false;
    private bool youndBool1 = false;
    private bool adultBool1 = false;

    [Header("Prefab")]
    public GameObject eggPrefab;

    [Header("Sprites")]
    public Sprite babySprite;
    public Sprite youngSprite;
    public Sprite adultSprite;
    
    // Floating movement for different states
    protected override void floating()
    {
        // Babyfish Period: change location in every 3s between corals
        if (period == Period.baby) {
            if (!babyBool1) {
                TaskPerformer.inst().scheduleTask((Action)(() => {
                    var corals = SceneObjects.sceneObj.corals.getActives();
                    Coral c = corals[UnityEngine.Random.Range(0, corals.Count)];
                    dest = c.getRandomPos();
                    babyBool1 = false;
                }), 3);
                babyBool1 = true;
            }
        } 
        // Youngfish Period: change location in every 5s moving within the middle area
        if (period == Period.young) {
            if (!youndBool1) {
                TaskPerformer.inst().scheduleTask((Action)(() => {
                    dest = new Vector2(UnityEngine.Random.Range(SceneObjects.sceneObj.max.position.x, SceneObjects.sceneObj.min.position.x), 
                    UnityEngine.Random.Range(SceneObjects.sceneObj.min.position.y, (SceneObjects.sceneObj.max.position.y
                     - SceneObjects.sceneObj.min.position.y) / 1.75f + SceneObjects.sceneObj.min.position.y));
                    youndBool1 = false;
                }), 5);
                youndBool1 = true;
            }
        }
        // Adultfish Period: change location in every 8s moving within the camera
        if (period == Period.adult) {
            if (!adultBool1) {
                TaskPerformer.inst().scheduleTask((Action)(() => {
                    dest = new Vector2(UnityEngine.Random.Range(SceneObjects.sceneObj.max.position.x, SceneObjects.sceneObj.min.position.x + 0.5f),
                    UnityEngine.Random.Range(SceneObjects.sceneObj.max.position.y, SceneObjects.sceneObj.min.position.y));
                    adultBool1 = false;
                }), 8);
                adultBool1 = true;
            }
        }
    }

    // Reproduction Period: stop movment and lay eggs
    protected override void reproduction()
    {
        // Spawn eggs slightly offset around the fish's position
        Vector3 eggPosition = transform.position + new Vector3 (UnityEngine.Random.Range(-0.5f, 0.5f), 0);
        
        float newY = eggPosition.y;
        if (newY < SceneObjects.sceneObj.transform.position.y) newY = SceneObjects.sceneObj.transform.position.y;
        eggPosition = eggPosition.x_z(newY);

        Instantiate(eggPrefab, eggPosition, Quaternion.identity);
    }

    // Speed change for each state
    protected override void onPeriodChange(Period from)
    {
        if (period == Period.baby)
        {
            spriteRenderer.sprite = babySprite;
            speed = 0.5f;
        }
            
        if (period == Period.young)
        {
            spriteRenderer.sprite = youngSprite;
            speed = 1f;
        }
            
        if (period == Period.adult)
        {
            spriteRenderer.sprite = adultSprite;
            speed = 1.5f;
        }          
    }

    public override void resetValues()
    {
        // Start with Babyfish state
        period = Period.baby;
        spriteRenderer.sprite = babySprite;
        // Set up var values
        isPoisoned = false;
        youngCountDown = 15f;
        adultCountDown = 20f;
        deadCountDown = 35f;
        reproductionCountDown = 22f;
        visionRange = 3.5f;
        speed = 0.5f;
    }

    void Update()
    {
        // Handle movment 
        this.floating();
        this.MoveTo();
    }

    // Escape if enters Shark's HuntPrey range
    void OnTriggerStay2D(Collider2D other) {
        if (period != Period.adult)
            return;
        if (other.CompareTag("Shark")) {
            dest = -(other.gameObject.transform.position-transform.position).xy();
            speed = 2f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision2D) {
        // If entered Jellyfish's PoisonZone, is poisoned
        if (collision2D.gameObject.CompareTag("PoisonZone")) {
            if (!collision2D.gameObject.GetComponent<poisonZone>().fish.isPoisoning) return;
            isPoisoned = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision2D) {
        // If entered Jellyfish's PoisonZone, is poisoned
        if (collision2D.gameObject.CompareTag("PoisonZone")) {
            if (!collision2D.gameObject.GetComponent<poisonZone>().fish.isPoisoning) return;
            isPoisoned = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision2D) {
        // If entered Jellyfish's PoisonZone, is poisoned
        if (collision2D.gameObject.CompareTag("PoisonZone")) {
            if (!collision2D.gameObject.GetComponent<poisonZone>().fish.isPoisoning) return;
            isPoisoned = true;
        }
    }
}
