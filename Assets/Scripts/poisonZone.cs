using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class poisonZone : MonoBehaviour
{
    public Jellyfish fish;
    public CapsuleCollider2D poisonRange;
    void Start()
    {
        poisonRange = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        
    }
}
