using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coral : MonoBehaviour
{
    private static int count = 0;
    public Sprite coral1;
    public Sprite coral2;
    public Sprite coral3;

    private CircleCollider2D c2d;
    private SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        var spriteList = new List<Sprite>
        { coral1, coral2, coral3 };

        c2d = GetComponent<CircleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = spriteList[count];
        count++;
    }

    public Vector2 getRandomPos() {
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();

        // 获取世界坐标下的半径，考虑对象的缩放
        float worldRadius = circleCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);

        // 计算碰撞器的中心位置，考虑偏移
        Vector2 center = (Vector2)transform.position + circleCollider.offset;

        // 在单位圆内获取随机点，并调整到碰撞器的范围和位置
        Vector2 randomPoint = Random.insideUnitCircle * worldRadius + center;

        return randomPoint;
    }
}
