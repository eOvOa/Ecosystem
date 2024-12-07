using System;
using Fries;
using Fries.TaskPerformer;
using UnityEngine;
using UnityEngine.AI;

public class Shark : Organism
{
    // private bool babyBool1 = false;
    private bool youngBool1 = false;
    private bool adultBool1 = false;

    //[Header("Prefab")]
    //public GameObject babySharkPrefab;

    [Header("Sprites")]
    public Sprite babySprite;
    public Sprite youngSprite;
    public Sprite adultSprite;
    public Sprite reproductingSprite;

    // Floating movement for different states
    protected override void floating()
    {
        // Youngfish Period: change location in every 5s moving within the camera
        if (period == Period.young) {
            if (!youngBool1) {
                TaskPerformer.inst().scheduleTask((Action)(() => {
                    dest = new Vector2(UnityEngine.Random.Range(SceneObjects.sceneObj.max.position.x, SceneObjects.sceneObj.min.position.x),
                    UnityEngine.Random.Range(SceneObjects.sceneObj.max.position.y, SceneObjects.sceneObj.min.position.y));
                    youngBool1 = false;
                }), 5);
                youngBool1 = true;
            }
        }
        // Adultfish Period: change location in every 8s moving within the camera
        if (period == Period.adult) {
            if (!adultBool1) {
                TaskPerformer.inst().scheduleTask((Action)(() => {
                    dest = new Vector2(UnityEngine.Random.Range(SceneObjects.sceneObj.max.position.x, SceneObjects.sceneObj.min.position.x),
                    UnityEngine.Random.Range(SceneObjects.sceneObj.max.position.y, SceneObjects.sceneObj.min.position.y));
                    adultBool1 = false;
                }), 8);
                adultBool1 = true;
            }
        }
    }

    protected override void reproduction()
    {
        spriteRenderer.sprite = reproductingSprite;
        // Spaw Babyshark slightly offset around the Adultshark's position
        Vector3 babySharkPosition = transform.position + new Vector3 (UnityEngine.Random.Range(-0.5f, 0.5f), 0);
        Shark shark = (Shark)SceneObjects.sceneObj.sharks.activate();
        shark.onActivate();
        shark.setPool(SceneObjects.sceneObj.sharks);
        // Change back to adult sprite after 2s
        TaskPerformer.inst().scheduleTask((Action)(() => {
            spriteRenderer.sprite = adultSprite;
        }), 2);
        
    }

    // Speed change for each state
    protected override void onPeriodChange(Period from)
    {
        if (period == Period.baby)
        {
            spriteRenderer.sprite = babySprite;
        }
        if (period == Period.young)
        {
            spriteRenderer.sprite = youngSprite;
            speed = 1.5f;
        }  
        if (period == Period.adult)
        {
            spriteRenderer.sprite = adultSprite;
            speed = 2.5f;
        }       
    }

    // Start is called before the first frame update
    public override void resetValues() {
        // Start with Babyfish state
        period = Period.baby;
        spriteRenderer.sprite = babySprite;
        // Set up var values
        isPoisoned = false;
        youngCountDown = 2f;
        adultCountDown = 25f;
        deadCountDown = 40f;
        reproductionCountDown = 28f;
        visionRange = 8.5f; // HuntPrey Range
        speed = 1f;
    }

    void Update()
    {
        this.floating();

        // Movement
        this.MoveTo();
        
    }

    void OnTriggerStay2D(Collider2D other) {
        // Hunt after prey enter the HuntPrey range
        if (period != Period.adult)
            return;
        if (other.CompareTag("Fish") || other.CompareTag("Jellyfish")) {
            dest = other.gameObject.transform.position.xy();
            speed = 3f;
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        // If entered Jellyfish's PoisonZone, is poisoned
        if (other.gameObject.CompareTag("PoisonZone")) {
            if (!other.gameObject.GetComponent<poisonZone>().fish.isPoisoning) return;
            isPoisoned = true;
            return;
        }

        if (isPoisoned) return;
        if (period != Period.adult)
            return;
        if (other.gameObject.CompareTag("Shark"))
            return;

        Organism organism = other.gameObject.GetComponent<Organism>();
        if (organism is Fish fish) { if (fish.period == Period.baby) return; }
        if (!organism) return;
        organism.hasEaten = true;
    }
}
