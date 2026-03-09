using UnityEngine;
using System.Collections.Generic;

// This script should be placed on a single, persistent GameObject in your scene.
// It will manage the fog of war texture data.
[ExecuteAlways]
public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance;
    private static readonly int FogOfWarTexture = Shader.PropertyToID("_FogOfWarTexture");
    private static readonly int FogWorldScaleOffset = Shader.PropertyToID("_FogWorldScaleOffset");

    [Header("Fog Texture Settings")]
    [Tooltip("The resolution of the fog texture. Higher is more detailed but costs more performance.")]
    public int textureResolution = 256;
    
    [Header("World Mapping")]
    [Tooltip("The size (width and height) of your playable world area.")]
    public float worldSize = 100f;
    [Tooltip("The center of your playable world area.")]
    public Vector2 worldCenter = Vector2.zero;

    [Header("Fog Colors")]
    public Color unexploredColor = new Color(0, 0, 0, 1);
    public Color exploredColor = new Color(0.5f, 0.5f, 0.5f, 1);
    public Color visibleColor = new Color(1, 1, 1, 1);

    private Texture2D _fogTexture;
    private Color[] _fogColors;
    private bool _needsTextureUpdate = false;
    private Vector4 _fogWorldScaleOffset;

    // A list of all active "revealers" (units, buildings, etc.)
    private List<Transform> _revealers = new List<Transform>();
    // We cache their vision radius to avoid GetComponent calls
    private Dictionary<Transform, float> _revealerRadius = new Dictionary<Transform, float>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeFogTexture();
    }

    void OnEnable()
    {
        // Set the global shader properties that the Shader Graph will read
        UpdateShaderGlobals();
    }

    void Update()
    {
        // This is our main fog update loop
        // 1. Age the fog: Turn all "Visible" (white) areas into "Explored" (grey)
        AgeFog();

        // 2. Reveal new fog: Turn areas around revealers into "Visible" (white)
        UpdateVisibleAreas();

        // 3. If any changes were made, apply them to the texture
        if (_needsTextureUpdate)
        {
            _fogTexture.SetPixels(_fogColors);
            _fogTexture.Apply();
            _needsTextureUpdate = false;
        }
    }

    void InitializeFogTexture()
    {
        if (_fogTexture == null)
        {
            _fogTexture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGB24, false);
            _fogTexture.wrapMode = TextureWrapMode.Clamp;
            _fogTexture.filterMode = FilterMode.Bilinear; // Bilinear gives a softer edge
        }

        _fogColors = new Color[textureResolution * textureResolution];
        for (int i = 0; i < _fogColors.Length; i++)
        {
            _fogColors[i] = unexploredColor;
        }

        _fogTexture.SetPixels(_fogColors);
        _fogTexture.Apply();

        UpdateShaderGlobals();
    }

    void UpdateShaderGlobals()
    {
        // This Vector4 tells the shader how to map world XZ coordinates to texture UVs
        float scale = 1.0f / worldSize;
        float offsetX = -worldCenter.x / worldSize + 0.5f;
        float offsetY = -worldCenter.y / worldSize + 0.5f;
        _fogWorldScaleOffset = new Vector4(scale, scale, offsetX, offsetY);

        // Set the global properties that our shader will read
        Shader.SetGlobalTexture(FogOfWarTexture, _fogTexture);
        Shader.SetGlobalVector(FogWorldScaleOffset, _fogWorldScaleOffset);
    }
    
    // Turns currently visible areas (white) into explored areas (grey)
    void AgeFog()
    {
        for (int i = 0; i < _fogColors.Length; i++)
        {
            if (_fogColors[i] == visibleColor)
            {
                _fogColors[i] = exploredColor;
                _needsTextureUpdate = true;
            }
        }
    }

    // Updates all areas around revealers to be visible
    void UpdateVisibleAreas()
    {
        foreach (var revealer in _revealers)
        {
            if (revealer == null) continue; // Should probably remove from list, but this is safer for now
            
            Vector3 worldPos = revealer.position;
            float radius = _revealerRadius[revealer];

            // Convert world position and radius to texture-space
            Vector2Int texCenter = WorldToTextureCoords(new Vector2(worldPos.x, worldPos.z));
            int radiusInPixels = (int)((radius / worldSize) * textureResolution);

            // Loop through a square bounding box around the radius
            for (int y = -radiusInPixels; y <= radiusInPixels; y++)
            {
                for (int x = -radiusInPixels; x <= radiusInPixels; x++)
                {
                    // Check if we're inside the circle
                    if (x * x + y * y <= radiusInPixels * radiusInPixels)
                    {
                        int texX = texCenter.x + x;
                        int texY = texCenter.y + y;

                        // Check if we're within the texture bounds
                        if (texX >= 0 && texX < textureResolution && texY >= 0 && texY < textureResolution)
                        {
                            int index = texY * textureResolution + texX;
                            
                            // If this pixel isn't already visible, mark it as visible
                            if (_fogColors[index] != visibleColor)
                            {
                                _fogColors[index] = visibleColor;
                                _needsTextureUpdate = true;
                            }
                        }
                    }
                }
            }
        }
    }

    // Helper function to convert a world XZ position to a texture pixel coordinate
    Vector2Int WorldToTextureCoords(Vector2 worldPos)
    {
        int x = (int)((worldPos.x * _fogWorldScaleOffset.x + _fogWorldScaleOffset.z) * textureResolution);
        int y = (int)((worldPos.y * _fogWorldScaleOffset.y + _fogWorldScaleOffset.w) * textureResolution);
        return new Vector2Int(x, y);
    }

    // --- Public Functions for other scripts to call ---

    public void RegisterRevealer(Transform revealer, float visionRadius)
    {
        if (!_revealers.Contains(revealer))
        {
            _revealers.Add(revealer);
            _revealerRadius[revealer] = visionRadius;
        }
    }

    public void UnregisterRevealer(Transform revealer)
    {
        if (_revealers.Contains(revealer))
        {
            _revealers.Remove(revealer);
            _revealerRadius.Remove(revealer);
        }
    }

    // This is useful if you change world size/center at runtime
    void OnValidate()
    {
        UpdateShaderGlobals();
    }
}