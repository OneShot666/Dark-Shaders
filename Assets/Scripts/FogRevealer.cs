using UnityEngine;

// Add this component to any unit, building, or player that should reveal the fog.
public class FogRevealer : MonoBehaviour
{
    [Tooltip("How far this object can see.")]
    public float visionRadius = 10f;

    void Start()
    {
        if (FogOfWarManager.Instance != null)
        {
            FogOfWarManager.Instance.RegisterRevealer(transform, visionRadius);
        }
    }

    void OnDestroy()
    {
        if (FogOfWarManager.Instance != null)
        {
            FogOfWarManager.Instance.UnregisterRevealer(transform);
        }
    }

    // Optional: Draw a gizmo to see the vision radius in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }
}