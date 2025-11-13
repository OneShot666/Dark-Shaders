using UnityEngine;

/// <summary> Make an object rotate on itself and/or orbit around a target point.
/// Can be used for spinning tops, planets, solar systems, etc.</summary>
public class OrbitalRotator : MonoBehaviour {
    [Header("Self Rotation")]
    [Tooltip("Enable/disable rotation on itself")]
    [SerializeField] private bool rotateSelf = true;
    [Tooltip("Speed of rotation around itself (degrees per second)")]
    [SerializeField] private float selfRotationSpeed = 50f;
    [Tooltip("Local axis for self rotation")]
    [SerializeField] private Vector3 selfRotationAxis = Vector3.up;

    [Header("Orbital Rotation")]
    [Tooltip("Enable/disable orbit movement around a target")]
    [SerializeField] private bool orbitEnabled;
    [Tooltip("Target point (or object) to orbit around")]
    [SerializeField] private Transform orbitCenter;
    [Tooltip("Speed of revolution (degrees per second)")]
    [SerializeField] private float orbitSpeed = 20f;
    [Tooltip("Distance from the orbit center (radius)")]
    [SerializeField] private float orbitDistance = 5f;
    [Tooltip("Axis of the orbit (relative to the center)")]
    [SerializeField] private Vector3 orbitAxis = Vector3.up;
    [Tooltip("Initial angle offset around the orbit")]
    [SerializeField, Range(0f, 360f)] private float startAngle;

    private float _currentOrbitAngle;

    void Start() {
        _currentOrbitAngle = startAngle;

        if (!orbitCenter) {                                                     // Default orbit origin is world origin
            GameObject empty = new GameObject($"{name}_OrbitCenter") {
                transform = { position = Vector3.zero}};
            orbitCenter = empty.transform;
        }

        if (orbitEnabled) {                                                     // Check position is correct
            Vector3 offset = Quaternion.AngleAxis(_currentOrbitAngle, orbitAxis.normalized) * (Vector3.forward * orbitDistance);
            transform.position = orbitCenter.position + offset;
        }
    }

    void Update() {
        if (rotateSelf) {                                                       // Rotate on itself
            transform.Rotate(selfRotationAxis.normalized, selfRotationSpeed * Time.deltaTime, Space.Self);
        }

        if (orbitEnabled && orbitCenter) {                                      // Rotate around point
            _currentOrbitAngle += orbitSpeed * Time.deltaTime;
            if (_currentOrbitAngle >= 360f) _currentOrbitAngle -= 360f;

            Vector3 offset = Quaternion.AngleAxis(_currentOrbitAngle, orbitAxis.normalized) * (Vector3.forward * orbitDistance);
            transform.position = orbitCenter.position + offset;
        }
    }

    public void SetOrbitCenter(Transform newCenter) => orbitCenter = newCenter;

    public void SetOrbitDistance(float newDistance) => orbitDistance = newDistance;

    void OnDrawGizmosSelected() {                                               // Display trajectory on editor
        if (!orbitEnabled || !orbitCenter) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(orbitCenter.position, orbitDistance);
    }
}
