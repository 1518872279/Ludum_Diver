using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The target to follow (should be the diver)")]
    public Transform target;

    [Tooltip("Offset from the target position")]
    public Vector3 offset = new Vector3(0, 0, -10);

    [Header("Follow Settings")]
    [Tooltip("How quickly the camera follows the target")]
    [Range(0.1f, 10f)]
    public float smoothSpeed = 3f;

    [Tooltip("How far ahead the camera looks based on target velocity")]
    [Range(0f, 2f)]
    public float lookAheadFactor = 0.5f;

    [Tooltip("The maximum distance the camera can be from the target")]
    public float maxFollowDistance = 5f;

    [Header("Depth Settings")]
    [Tooltip("Whether the camera should follow the target's vertical movement")]
    public bool followVertical = true;

    [Tooltip("Minimum Y position for the camera (maximum depth)")]
    public float minY = float.NegativeInfinity;

    [Tooltip("Maximum Y position for the camera (water surface)")]
    public float maxY = float.PositiveInfinity;

    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;
    private Rigidbody2D targetRigidbody;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow: No target assigned!");
            enabled = false;
            return;
        }

        // Try to get the target's Rigidbody2D for look-ahead movement
        targetRigidbody = target.GetComponent<Rigidbody2D>();
        
        // Initialize camera position
        transform.position = target.position + offset;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Calculate base target position
        targetPosition = target.position + offset;

        // Add look-ahead based on target velocity
        if (targetRigidbody != null)
        {
            Vector3 lookAheadPos = (Vector3)targetRigidbody.velocity * lookAheadFactor;
            targetPosition += lookAheadPos;
        }

        // Clamp the target position if we're not following vertical movement
        if (!followVertical)
        {
            targetPosition.y = transform.position.y;
        }

        // Clamp the Y position between min and max values
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        // Calculate the distance to target
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // If we're too far from the target, snap closer
        if (distanceToTarget > maxFollowDistance)
        {
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            Vector3 snapPosition = targetPosition - (directionToTarget * maxFollowDistance);
            transform.position = snapPosition;
        }
        else
        {
            // Smoothly move the camera towards the target position
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                1f / smoothSpeed
            );
        }
    }

    // Helper method to visualize the follow bounds in the editor
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(target.position + offset, maxFollowDistance);

        if (followVertical && (minY != float.NegativeInfinity || maxY != float.PositiveInfinity))
        {
            Gizmos.color = Color.cyan;
            Vector3 cameraPos = target.position + offset;
            float width = Camera.main != null ? Camera.main.orthographicSize * 2 * Camera.main.aspect : 10f;
            
            if (minY != float.NegativeInfinity)
            {
                Vector3 minPos = new Vector3(cameraPos.x - width/2, minY, cameraPos.z);
                Vector3 maxPos = new Vector3(cameraPos.x + width/2, minY, cameraPos.z);
                Gizmos.DrawLine(minPos, maxPos);
            }
            
            if (maxY != float.PositiveInfinity)
            {
                Vector3 minPos = new Vector3(cameraPos.x - width/2, maxY, cameraPos.z);
                Vector3 maxPos = new Vector3(cameraPos.x + width/2, maxY, cameraPos.z);
                Gizmos.DrawLine(minPos, maxPos);
            }
        }
    }
} 