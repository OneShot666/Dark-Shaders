using UnityEngine;

public class CameraManager : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float offSet = 50f;
    [SerializeField] private float speed = 50f;
    [SerializeField] private float rotation = 100f;
    [SerializeField] private LayerMask clickableLayer;                          // Optional layer to filter clickable objects

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2000f;
    [SerializeField] private float maxOffset = 500f;
    [SerializeField, Range(1, 10)] private float safetyMargin = 1.2f;

    private bool _isPaused;
    private Camera _cam;
    private float _minOffset = 5f;

    void Start() {
        _cam = GetComponent<Camera>();
        if (target) {
            UpdateTargetBounds();
            transform.LookAt(target);
        }
    }

    void Update() {
        HandleSelection();
        HandleZoom();

        if (target) FollowTarget();
        else MoveCamera();

        RotateCamera();
    }

    private void UpdateTargetBounds() {
        if (!target) return;

        Collider col = target.GetComponent<Collider>();
        if (col) {
            float objectSize = col.bounds.extents.magnitude;                    // Get highest object size
            _minOffset = objectSize * safetyMargin;
            
            if (offSet < _minOffset) offSet = _minOffset + 5f;                  // Check isn't already in object
        }
    }

    private void HandleSelection() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit)) target = hit.transform;   // Check if click on something
        }

        if (target && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.A))) {  // Exit follow mode
            target = null;
        }
    }

    private void HandleZoom() {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollInput) > 0.01f) {
            offSet -= scrollInput * zoomSpeed * Time.unscaledDeltaTime;
            offSet = Mathf.Clamp(offSet, _minOffset, maxOffset);
        }
    }

    private void FollowTarget() {
        Vector3 desiredPosition = target.position - transform.forward * offSet;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, speed * Time.unscaledDeltaTime);
    }

    private void MoveCamera() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            _isPaused = !_isPaused;
            Time.timeScale = _isPaused ? 0f : 1f;
        }

        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) dir += transform.forward;
        if (Input.GetKey(KeyCode.S)) dir -= transform.forward;
        if (Input.GetKey(KeyCode.A)) dir -= transform.right;
        if (Input.GetKey(KeyCode.D)) dir += transform.right;
        if (Input.GetKey(KeyCode.Q)) dir += Vector3.up;
        if (Input.GetKey(KeyCode.E)) dir -= Vector3.up;

        transform.position += dir.normalized * (speed * Time.unscaledDeltaTime);
    }

    private void RotateCamera() {
        int reverse = target ? -1 : 1;
        float yaw = 0f;
        float pitch = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) yaw = -1f * reverse;
        if (Input.GetKey(KeyCode.RightArrow)) yaw = 1f * reverse;
        if (Input.GetKey(KeyCode.UpArrow)) pitch = -1f * reverse;
        if (Input.GetKey(KeyCode.DownArrow)) pitch = 1f * reverse;

        transform.Rotate(Vector3.up, yaw * rotation * Time.unscaledDeltaTime, Space.World);
        transform.Rotate(Vector3.right, pitch * rotation * Time.unscaledDeltaTime, Space.Self);
    }
}
