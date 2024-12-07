using System;
using System.Collections;
using System.Collections.Generic;
using Fries;
using Fries.TaskPerformer;
using UnityEngine;

public class Jellyfish : Organism
{
    public Collider2D poisonZone;
    private bool adultBool1 = false;
    private bool poisonBool = false;

    [Header("Poison Stats")]
    public bool isPoisoning = false;
    public float poisonCountDown = 15f; // Start poisoning every 15s
    public float poisonTimer = 3f; // Poison 3s everytime

    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite escapingSprite;
    public Sprite poisoningSprite;

    protected override void floating()
    {
        // Adultfish Period: change location in every 10s moving within the camera
        if (period == Period.adult) {
            if (!adultBool1) {
                TaskPerformer.inst().scheduleTask((Action)(() => {
                    speed = 1f;
                    dest = new Vector2(UnityEngine.Random.Range(SceneObjects.sceneObj.max.position.x, SceneObjects.sceneObj.min.position.x),
                    UnityEngine.Random.Range(SceneObjects.sceneObj.max.position.y, SceneObjects.sceneObj.min.position.y));
                    adultBool1 = false;
                }), 10);
                adultBool1 = true;
            }
        }
        // No movement while poisoning, poisoning for 3s
        if (!poisonBool){
            TaskPerformer.inst().scheduleTask((Action)(() => {
                speed = 0;
                spriteRenderer.sprite = poisoningSprite;
                isPoisoning = true;
                poisonZone.enabled = true;
                TaskPerformer.inst().scheduleTask((Action)(() => {
                    speed = 1;
                    spriteRenderer.sprite = normalSprite;
                    isPoisoning = false;
                    poisonBool = false;
                    poisonZone.enabled = false;
                }), poisonTimer);
            }), poisonCountDown);
            poisonBool = true;  
        }
    }   

    protected override void onPeriodChange(Period from) {}

    protected override void reproduction() {}

    // Start is called before the first frame update
    public override void resetValues()
    {
        // Start with adult state
        period = Period.adult;
        spriteRenderer.sprite = normalSprite;
        isPoisoning = false;
        // Set up var values
        visionRange = 0;
        speed = 0f;
        deadCountDown = 60f;
        spriteRenderer.sprite = normalSprite;
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
        if (other.CompareTag("Shark") && (other.gameObject.GetComponent<Shark>().period == Period.adult)) {
            spriteRenderer.sprite = escapingSprite;
            dest = -(other.gameObject.transform.position-transform.position).xy();
            speed = 2.5f;
        }
    }
}
