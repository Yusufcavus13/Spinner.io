using UnityEngine;

namespace SpinForward.Player
{
    /// <summary>
    /// A clean spinner built from Unity PRIMITIVES only (these always render
    /// correctly - unlike hand-rolled meshes). A glossy disc body with a bright
    /// pointer that makes the spin & facing obvious, a domed top, a decorative rim,
    /// and a short vertical pin so the lean reads. Everything sits above the pivot
    /// origin so leaning never clips the ground. Right-click header > Rebuild.
    /// </summary>
    public class ProceduralSpinner : MonoBehaviour
    {
        [Header("Size")]
        [SerializeField] private float radius = 0.75f;
        [SerializeField] private float discThickness = 0.16f;
        [SerializeField] private float pinHeight = 0.7f;

        [Header("Colors")]
        [SerializeField] private Color bodyColor = new Color(0.11f, 0.42f, 0.95f);
        [SerializeField] private Color accentColor = new Color(1f, 0.82f, 0.2f);
        [Tooltip("Bright pointer/nose - makes spin & direction obvious.")]
        [SerializeField] private Color pointerColor = new Color(1f, 0.28f, 0.32f);
        [Range(0f, 1f)] [SerializeField] private float metallic = 0.85f;
        [Range(0f, 1f)] [SerializeField] private float smoothness = 0.8f;
        [SerializeField] private float emission = 2.5f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private void Awake() => Build();

        [ContextMenu("Rebuild")]
        public void Build()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroySafe(transform.GetChild(i).gameObject);

            Material body = MakeMaterial(bodyColor, false);
            Material accent = MakeMaterial(accentColor, true);
            Material pointer = MakeMaterial(pointerColor, true);

            float discY = discThickness * 0.5f;
            float discTop = discThickness;

            // Decorative rim (slightly wider, thin, sits under the disc edge).
            CreatePart(PrimitiveType.Cylinder, Vector3.up * discY,
                new Vector3(radius * 2.15f, discThickness * 0.35f, radius * 2.15f), accent, "Rim");

            // Main disc body.
            CreatePart(PrimitiveType.Cylinder, Vector3.up * discY,
                new Vector3(radius * 2f, discThickness * 0.5f, radius * 2f), body, "Disc");

            // Domed top.
            CreatePart(PrimitiveType.Sphere, Vector3.up * discTop,
                new Vector3(radius * 1.7f, radius * 0.9f, radius * 1.7f), body, "Dome");

            // Bright pointer laid on the disc, reaching from center out to the rim.
            GameObject pointerGo = CreatePart(PrimitiveType.Cube, new Vector3(radius * 0.55f, discTop, 0f),
                new Vector3(radius * 1.25f, discThickness * 0.6f, radius * 0.45f), pointer, "Pointer");
            // little nose tip
            CreatePart(PrimitiveType.Sphere, new Vector3(radius * 1.15f, discTop, 0f),
                Vector3.one * radius * 0.5f, pointer, "Nose");

            // Vertical pin + knob so the lean is readable.
            float pinBase = discTop + radius * 0.45f;
            CreatePart(PrimitiveType.Cylinder, Vector3.up * (pinBase + pinHeight * 0.5f),
                new Vector3(radius * 0.16f, pinHeight * 0.5f, radius * 0.16f), accent, "Pin");
            CreatePart(PrimitiveType.Sphere, Vector3.up * (pinBase + pinHeight),
                Vector3.one * radius * 0.35f, accent, "Knob");
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
            m.SetColor(BaseColorId, color);
            m.SetFloat(MetallicId, metallic);
            m.SetFloat(SmoothnessId, smoothness);
            if (emissive)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor(EmissionColorId, color * emission);
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
