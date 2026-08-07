using UnityEngine;

namespace SpinForward.Player
{
    /// <summary>
    /// Builds a clean fidget-spinner look from primitives - no downloads, no ugly
    /// imported models. Put it on the spinning "visual" object; its parts spin (and
    /// now lean) with it. Right-click the component header > Rebuild to preview in
    /// the editor. Colliders are stripped so it stays purely visual.
    /// </summary>
    public class ProceduralSpinner : MonoBehaviour
    {
        [Header("Shape")]
        [Range(2, 6)] [SerializeField] private int arms = 3;
        [SerializeField] private float armRadius = 0.6f;
        [SerializeField] private float bearingSize = 0.35f;
        [SerializeField] private float hubSize = 0.35f;
        [SerializeField] private float thickness = 0.18f;

        [Header("Colors")]
        [SerializeField] private Color bodyColor = new Color(0.14f, 0.45f, 1f);
        [SerializeField] private Color accentColor = new Color(1f, 0.85f, 0.2f);
        [Range(0f, 1f)] [SerializeField] private float metallic = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float smoothness = 0.75f;
        [SerializeField] private float emission = 2f;

        private void Awake() => Build();

        [ContextMenu("Rebuild")]
        public void Build()
        {
            // Clear any previous parts (editor preview or old build).
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroySafe(transform.GetChild(i).gameObject);

            Material body = MakeMaterial(bodyColor, false);
            Material accent = MakeMaterial(accentColor, true);

            // Center hub + glowing cap.
            CreatePart(PrimitiveType.Cylinder, Vector3.zero, new Vector3(hubSize, thickness, hubSize), body, "Hub");
            CreatePart(PrimitiveType.Cylinder, Vector3.up * thickness * 0.6f, new Vector3(hubSize * 0.55f, thickness, hubSize * 0.55f), accent, "HubCap");

            for (int i = 0; i < arms; i++)
            {
                float ang = (360f / arms) * i * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));
                Vector3 bearingPos = dir * armRadius;

                // Arm: a bar reaching from the hub out to the bearing.
                GameObject arm = CreatePart(PrimitiveType.Cube, dir * armRadius * 0.5f,
                    new Vector3(armRadius, thickness * 0.7f, bearingSize * 0.5f), body, "Arm");
                arm.transform.localRotation = Quaternion.FromToRotation(Vector3.right, dir);

                // Outer bearing ring + glowing cap.
                CreatePart(PrimitiveType.Cylinder, bearingPos, new Vector3(bearingSize, thickness, bearingSize), body, "Bearing");
                CreatePart(PrimitiveType.Cylinder, bearingPos + Vector3.up * thickness * 0.6f, new Vector3(bearingSize * 0.5f, thickness, bearingSize * 0.5f), accent, "BearingCap");
            }
        }

        private GameObject CreatePart(PrimitiveType type, Vector3 localPos, Vector3 localScale, Material mat, string partName)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = partName;

            Collider col = go.GetComponent<Collider>();
            if (col != null)
                DestroySafe(col);

            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        private Material MakeMaterial(Color color, bool emissive)
        {
            Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smoothness);
            if (emissive)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", color * emission);
            }
            return m;
        }

        private static void DestroySafe(Object o)
        {
            if (Application.isPlaying)
                Destroy(o);
            else
                DestroyImmediate(o);
        }
    }
}
