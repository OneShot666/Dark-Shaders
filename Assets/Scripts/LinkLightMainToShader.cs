using UnityEngine;

[ExecuteInEditMode]
public class LinkLightMainToShader : MonoBehaviour {
    [SerializeField] private Material skyboxMaterial;

    private static readonly int MainLightDirection = Shader.PropertyToID("_MainLightDirection");

    private void Update() {
        skyboxMaterial.SetVector(MainLightDirection, transform.forward);
    }
}
