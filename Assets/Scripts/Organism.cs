using UnityEngine;
using Fries;
using Fries.TaskPerformer;
using System;
using DG.Tweening;
using Fries.Pool;

public abstract class Organism : MonoBehaviour
{
    public CompPool<Organism> pool {set; private get;}
    public void setPool(CompPool<Organism> pool) {
        this.pool = pool;
    }
    public Period period;
    protected float youngCountDown;
    protected float adultCountDown;
    protected float deadCountDown;
    protected float reproductionCountDown;
    private float _visionRange;
    protected float visionRange {
        get => _visionRange;
        set {
            _visionRange = value;
            if (collider2d) collider2d.radius = value;
        }
    }
    protected float speed;
    public bool _isPoisoned = false;

    // If isPoisoned, change to deathSprite for 2s, then destroy
    public bool isPoisoned {
        get => _isPoisoned;
        set {
            _isPoisoned = value;
            if (_isPoisoned) {
                TaskPerformer.inst().scheduleTask((Action)(() => {
                    spriteRenderer.sprite = deadSprite;
                }), 2f);
                pool.deactivate(this);
                cancelTasks();
            }
        }
    }

    // If hasEaten, instant destroy
    public bool _hasEaten = false;
    public bool hasEaten {
        get => _hasEaten;
        set {
            _hasEaten = value;
            if (_hasEaten) {
                pool.deactivate(this);
                cancelTasks();
            }
        }
    }
    protected abstract void floating();
    public abstract void resetValues();
    protected abstract void reproduction();
    protected abstract void onPeriodChange(Period from);
    public Sprite deadSprite;
    
    // Turn the game object to the direction is moving
    private Vector2 _dest;
    protected Vector2 dest {
        get => _dest;
        set {
            if (this == null) return;
            _dest = value;
            if (_dest == -10000f.ff()) return;
            Vector2 direction = (dest - transform.position.xy()).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion toRotation = Quaternion.Euler(0f, 0f, angle);
            transform.DORotateQuaternion(toRotation, 0.75f);
        }
    }

    // Movement
    protected void MoveTo()
    {
        if (dest == -10000f.ff()) return;
        float dist = (transform.position.xy() - dest).magnitude;
        if (dist < 0.2f) {
            dest = -10000f.ff();
            return;
        }
        float speedFactor = Mathf.Clamp01(dist);
        Debug.Log(speedFactor + " " + speed);
        Vector2 delta = transform.right * speed * Time.deltaTime * speedFactor;
        Debug.Log(delta);
        transform.position += delta.xy_(0f);
    }

    private TaskHandle youngTask;
    private TaskHandle adultTask;
    private TaskHandle reproductionTask;
    private TaskHandle deadTask;
    public void cancelTasks() {
        youngTask.cancel();
        adultTask.cancel();
        deadTask.cancel();
        reproductionTask.cancel();
    }

    // Initialization
    protected Rigidbody2D rb;
    protected CircleCollider2D collider2d;
    protected SpriteRenderer spriteRenderer;
    public void init()
    {
        // Initialization
        rb = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void onActivate() {
        if (collider2d) collider2d.radius = visionRange;

        youngTask = TaskPerformer.inst().scheduleTask((Action)(() => {
            Period previous = period;
            period = Period.young;
            onPeriodChange(period);
        }), youngCountDown);

        adultTask = TaskPerformer.inst().scheduleTask((Action)(() => {
            Period previous = period;
            period = Period.adult;
            onPeriodChange(period);
        }), adultCountDown);

        // Swtich to reproduction state after count down
        reproductionTask = TaskPerformer.inst().scheduleTask((Action)(() => {
            reproduction();}
        ), reproductionCountDown);

        // Organism Death after reaching a specific time period: change to deadSprite for 2s, then destroy
        deadTask = TaskPerformer.inst().scheduleTask((Action)(() => {
            spriteRenderer.sprite = deadSprite;
            speed = 0;
            TaskPerformer.inst().scheduleTask((Action)(() => {
                cancelTasks();
                pool.deactivate(this);
            }), 2f);
        }), deadCountDown);  
    }
}
