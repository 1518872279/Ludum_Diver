using UnityEngine;

public class DiverMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Force applied when pressing a flipper button")]
    public float forwardForce = 5f;
    
    [Tooltip("Torque force applied for rotation")]
    public float torqueForce = 2f;
    
    [Tooltip("Maximum velocity the diver can move at")]
    public float maxVelocity = 10f;
    
    [Tooltip("Maximum angular velocity for rotation")]
    public float maxAngularVelocity = 180f;
    
    [Header("Water Physics")]
    [Tooltip("Drag applied when moving through water")]
    public float waterDrag = 2f;
    
    [Tooltip("Angular drag for rotation in water")]
    public float waterAngularDrag = 2f;
    
    private Rigidbody2D rb;
    private bool isLeftFlipperActive;
    private bool isRightFlipperActive;
    private float lastLeftFlipperTime;
    private float lastRightFlipperTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("DiverMovement requires a Rigidbody2D component!");
            enabled = false;
            return;
        }

        // Set up water-like physics
        rb.drag = waterDrag;
        rb.angularDrag = waterAngularDrag;
        rb.gravityScale = 0.1f; // Slight downward force to simulate sinking
    }

    private void Update()
    {
        // Check for flipper input
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            isLeftFlipperActive = true;
            lastLeftFlipperTime = Time.time;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isLeftFlipperActive = false;
        }

        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            isRightFlipperActive = true;
            lastRightFlipperTime = Time.time;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isRightFlipperActive = false;
        }
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ClampVelocities();
    }

    private void ApplyMovement()
    {
        Vector2 force = Vector2.zero;
        float torque = 0f;

        // Apply forces based on flipper state
        if (isLeftFlipperActive)
        {
            force += (Vector2)(transform.up * forwardForce);
            torque -= torqueForce;
        }

        if (isRightFlipperActive)
        {
            force += (Vector2)(transform.up * forwardForce);
            torque += torqueForce;
        }

        // Apply the forces
        rb.AddForce(force, ForceMode2D.Force);
        rb.AddTorque(torque, ForceMode2D.Force);
    }

    private void ClampVelocities()
    {
        // Clamp linear velocity
        if (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }

        // Clamp angular velocity
        rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxAngularVelocity, maxAngularVelocity);
    }

    // Helper method to visualize the movement direction in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.up);
    }
} 