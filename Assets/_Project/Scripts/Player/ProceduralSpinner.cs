using UnityEngine;

namespace SpinForward.Player
{
    /// <summary>
    /// Builds a chunky, 3D fidget-spinner from primitives - no downloads. A tall
    /// central spindle makes the lean visible, sphere bearings give it volume, and
    /// one brightly colored "nose" bearing makes the spin unmistakable. Put it on
    /// the spinning "visual" object. Right-click the header > Rebuild to preview.
    /// </summary>
    public class ProceduralSpinner : MonoBehaviour
    {
        [Header("Shape")]
        [Range(2, 6)] [SerializeField] private int arms = 3;
        [SerializeField] private float armRadius = 0.8f;
        [SerializeField] private float bearingSize = 0.55f;
        [SerializeField] private float hubSize = 0.55f;
        [SerializeField] private float armThickness = 0.32f;
        [Tooltip("Height of the vertical spindle. Taller = lean is more visible.")]
        [SerializeField] private float spindleHeight = 1.6f;

        [Header("Colors")]
        [SerializeField] private Color bodyColor = new Color(0.13f, 0.4f, 1f);
        [SerializeField] private Color accentColor = new Color(0.2f, 0.95f, 1f);
        [Tooltip("The one bright bearing that makes spinning obvious.")]
        [SerializeField] private Color noseColor = new Color(1f, 0.4f, 0.05f);
        [Range(0f, 1f)] [SerializeField] private float metallic = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float smoothness = 0.75f;
        [SerializeField] private float emission = 2.5f;

        private void Awake() => Build();

        [ContextMenu("Rebuild")]
        public void Build()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroySafe(transform.GetChild(i).gameObject);

            Material body = MakeMaterial(bodyColor, false);
            Material accent = MakeMaterial(accentColor, true);
            Material nose = MakeMaterial(noseColor, true);

            // Center hub.
            CreatePart(PrimitiveType.Cylinder, Vector3.zero, new Vector3(hubSize, armThickness, hubSize), body, "Hub");

            // Vertical spindle + glowing top knob - this is what makes the LEAN readable.
            CreatePart(PrimitiveType.Cylinder, Vector3.up * spindleHeight * 0.5f,
                new Vector3(hubSize * 0.35f, spindleHeight * 0.5f, hubSize * 0.35f), accent, "Spindle");
            CreatePart(PrimitiveType.Sphere, Vector3.up * spindleHeight,
                Vector3.one * hubSize * 0.6f, accent, "SpindleTop");

            for (int i = 0; i < arms; i++)
            {
                float ang = (360f / arms) * i * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));
                Vector3 bearingPos = dir * armRadius;

                // Arm bar reaching out to the bearing.
                GameObject arm = CreatePart(PrimitiveType.Cube, dir * armRadius * 0.5f,
                    new Vector3(armRadius, armThickness, bearingSize * 0.5f), body, "Arm");
                arm.transform.localRotation = Quaternion.FromToRotation(Vector3.right, dir);

                // Outer bearing as a sphere (3D volume). The first one is the bright nose.
                Material bearingMat = (i == 0) ? nose : body;
                CreatePart(PrimitiveType.Sphere, bearingPos, Vector3.one * bearingSize, bearingMat, "Bearing");
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
