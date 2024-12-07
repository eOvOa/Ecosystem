using UnityEngine;

public class Jellyfish_0 : MonoBehaviour
{
    public enum JellyfishState { Floating, Poisoning, Escaping, Dying }
    public JellyfishState currentState;

    [Header("State Durations")]
    public float floatingDuration = 10f;   // Time spent in the Floating state before transitioning
    public float poisoningDuration = 5f;  // Time spent in the Poisoning state
    public float respawnTime = 5f;        // Time before respawning after dying
    private float stateTimer;             // Timer to manage state transitions

    [Header("Movement")]
    public float normalSpeed = 1f;        // Normal speed during the Floating state
    public float escapeSpeedMultiplier = 2f; // Speed multiplier when escaping predators
    private Vector3 movementDirection;    // Current direction of movement

    [Header("Sprites")]
    public Sprite jellyfishNormal;        // Sprite for the Floating state
    public Sprite jellyfishPoisoning;     // Sprite for the Poisoning state
    public Sprite jellyfishRunning;       // Sprite for the Escaping state
    public Sprite jellyfishDead;          // Sprite for the Dying state
    private SpriteRenderer spriteRenderer;

    [Header("Poison Zone")]
    public CircleCollider2D poisonZone;  // Poison zone Collider2D

    [Header("Predator Interaction")]
    public LayerMask sharkLayer;          // Layer mask for detecting sharks
    public float sharkDetectionRange = 5f; // Distance at which the jellyfish detects sharks

    private Rigidbody2D rb;               // Rigidbody for movement
    private bool isRespawning = false;    // Flag to track if respawn is in progress

    private void Start()
    {
        // Initialization
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Start in the Floating state
        RespawnJellyfish();
    }

    private void Update()
    {
        if (isRespawning) return; // Skip logic during respawning

        HandleStateTransitions();
        HandleMovement();
    }

    /// Handles transitions between states.
    private void HandleStateTransitions()
    {
        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case JellyfishState.Floating:
                if (stateTimer <= 0)
                {
                    // Transition to Poisoning state
                    EnterPoisoningState();
                }
                else
                {
                    // Check for nearby predators (sharks)
                    Collider2D nearbyPredator = Physics2D.OverlapCircle(transform.position, sharkDetectionRange, sharkLayer);
                    if (nearbyPredator != null)
                    {
                        EnterEscapingState();
                    }
                }
                break;

            case JellyfishState.Poisoning:
                if (stateTimer <= 0)
                {
                    // Transition back to Floating state
                    ExitPoisoningState();
                }
                break;

            case JellyfishState.Escaping:
                // If the shark is no longer in range, return to Floating state
                Collider2D escapingPredator = Physics2D.OverlapCircle(transform.position, sharkDetectionRange, sharkLayer);
                if (escapingPredator == null)
                {
                    currentState = JellyfishState.Floating;
                    spriteRenderer.sprite = jellyfishNormal;
                    stateTimer = floatingDuration;
                }
                break;

            case JellyfishState.Dying:
                Die();
                break;
        }
    }

    /// Handles movement logic for the jellyfish
    private void HandleMovement()
    {
        if (currentState == JellyfishState.Dying || currentState == JellyfishState.Poisoning)
        {
            // No movement in these states
            rb.velocity = Vector2.zero;
            return;
        }

        // Movement logic based on state
        if (currentState == JellyfishState.Floating)
        {
            // Slow random movement
            Vector3 newPosition = transform.position + movementDirection * normalSpeed * Time.deltaTime;
            transform.position = ClampPositionToCameraBounds(newPosition);

            // Occasionally change direction
            if (Random.value < 0.01f) // 1% chance per frame
            {
                movementDirection = GetRandomDirection();
            }
        }
        else if (currentState == JellyfishState.Escaping)
        {
            // Move away from the predator at an accelerated speed
            Collider2D predator = Physics2D.OverlapCircle(transform.position, sharkDetectionRange, sharkLayer);
            if (predator != null)
            {
                Vector3 directionAwayFromShark = (transform.position - predator.transform.position).normalized;
                rb.velocity = directionAwayFromShark * normalSpeed * escapeSpeedMultiplier;
                transform.position = ClampPositionToCameraBounds(transform.position);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Shark"))
            return;
        else {
            currentState = JellyfishState.Dying;
        }
    }

    /// Activates the Poisoning state
    private void EnterPoisoningState()
    {
        currentState = JellyfishState.Poisoning;
        spriteRenderer.sprite = jellyfishPoisoning;
        stateTimer = poisoningDuration;

        // Enable and resize the poison zone
        if (poisonZone != null)
        {
            poisonZone.enabled = true;

            // Adjust the radius to match the jellyfishPoisoning sprite's dimensions
            float spriteWidth = spriteRenderer.sprite.bounds.size.x;
            float spriteHeight = spriteRenderer.sprite.bounds.size.y;
            float poisonRadius = Mathf.Max(spriteWidth, spriteHeight) / 2f; // Use the larger dimension as the radius
            poisonZone.radius = poisonRadius;
        }
    }

    /// Deactivates the Poisoning state
    private void ExitPoisoningState()
    {
        currentState = JellyfishState.Floating;
        spriteRenderer.sprite = jellyfishNormal;
        stateTimer = floatingDuration;

        // Deactivate the poison zone
        if (poisonZone != null)
            poisonZone.enabled = false;
    }

    /// Activates the Escaping state
    private void EnterEscapingState()
    {
        currentState = JellyfishState.Escaping;
        spriteRenderer.sprite = jellyfishRunning;
        rb.velocity = Vector2.zero; // Clear any previous velocity
    }

    /// Activates the Dying state when a shark successfully preys on the jellyfish
    public void Die()
    {
        spriteRenderer.sprite = jellyfishDead;
        rb.velocity = Vector2.zero; // Stop movement
        rb.isKinematic = true; // Prevent further physics interaction

        // Deactivate poison zone
        if (poisonZone != null)
            poisonZone.enabled = false;

        // Start respawn timer
        Invoke(nameof(RespawnJellyfish), respawnTime);
        gameObject.SetActive(false); // Temporarily hide the jellyfish
    }

    /// Respawns the jellyfish after death.
    private void RespawnJellyfish()
    {
        isRespawning = true;

        // Reset position within the camera's visible range
        transform.position = GetRandomPositionWithinCameraBounds();

        // Reset state and properties
        currentState = JellyfishState.Floating;
        spriteRenderer.sprite = jellyfishNormal;
        rb.isKinematic = false;
        stateTimer = floatingDuration;

        // Reactivate the jellyfish
        gameObject.SetActive(true);
        isRespawning = false;
    }

    /// Constrains the position to within the camera's visible boundaries.
    private Vector3 ClampPositionToCameraBounds(Vector3 position)
    {
        Camera mainCamera = Camera.main;

        // Get camera boundaries
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        float minX = mainCamera.transform.position.x - cameraWidth / 2f;
        float maxX = mainCamera.transform.position.x + cameraWidth / 2f;
        float minY = mainCamera.transform.position.y - cameraHeight / 2f;
        float maxY = mainCamera.transform.position.y + cameraHeight / 2f;

        // Clamp position
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        return position;
    }

    /// Gets a random position within the camera's visible boundaries
    private Vector3 GetRandomPositionWithinCameraBounds()
    {
        Camera mainCamera = Camera.main;

        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // Get the world boundaries of the camera
        float minX = mainCamera.transform.position.x - cameraWidth / 2f + 2f;
        float maxX = mainCamera.transform.position.x + cameraWidth / 2f - 2f;
        float minY = mainCamera.transform.position.y - cameraHeight / 2f + 2f;
        float maxY = mainCamera.transform.position.y + cameraHeight / 2f - 2f;

        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        return new Vector3(randomX, randomY, 0);
    }

    /// Gets a random direction for movement
    private Vector3 GetRandomDirection()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);
        return new Vector3(x, y).normalized;
    }

    // debug: shrak detection range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sharkDetectionRange);
    }
}
