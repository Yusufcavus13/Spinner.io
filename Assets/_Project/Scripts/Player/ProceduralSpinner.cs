using UnityEngine;

namespace SpinForward.Player
{
    public enum SpinnerShape { Disc, Saw, Star, Gear }

    /// <summary>
    /// A spinner built from Unity PRIMITIVES (always render correctly). Supports several
    /// shapes - a glossy disc, a circular SAW blade, a STAR/shuriken, and a GEAR - each
    /// with a bright "marker" element so the spin reads and a pin so the lean reads.
    /// Shop skins pick the shape + colors via ApplySkin. Right-click header > Rebuild.
    /// </summary>
    public class ProceduralSpinner : MonoBehaviour
    {
        [Header("Shape & Size")]
        [SerializeField] private SpinnerShape shape = SpinnerShape.Disc;
        [SerializeField] private float radius = 0.75f;
        [SerializeField] private float discThickness = 0.16f;
        [SerializeField] private float pinHeight = 0.55f;

        [Header("Colors")]
        [SerializeField] private Color bodyColor = new Color(0.11f, 0.42f, 0.95f);
        [SerializeField] private Color accentColor = new Color(1f, 0.82f, 0.2f);
        [SerializeField] private Color pointerColor = new Color(1f, 0.28f, 0.32f);
        [Range(0f, 1f)] [SerializeField] private float metallic = 0.85f;
        [Range(0f, 1f)] [SerializeField] private float smoothness = 0.8f;
        [SerializeField] private float emission = 2.5f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private void Awake() => Build();

        /// <summary>Applies a shop skin (shape + colors) and rebuilds the spinner.</summary>
        public void ApplySkin(SpinnerShape newShape, Color body, Color accent, Color pointer)
        {
            shape = newShape;
            bodyColor = body;
            accentColor = accent;
            pointerColor = pointer;
            Build();
        }

        [ContextMenu("Rebuild")]
        public void Build()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroySafe(transform.GetChild(i).gameObject);

            Material body = MakeMaterial(bodyColor, false);
            Material accent = MakeMaterial(accentColor, true);
            Material pointer = MakeMaterial(pointerColor, true);

            switch (shape)
            {
                case SpinnerShape.Saw: BuildSaw(body, accent, pointer); break;
                case SpinnerShape.Star: BuildStar(body, accent, pointer); break;
                case SpinnerShape.Gear: BuildGear(body, accent, pointer); break;
                default: BuildDisc(body, accent, pointer); break;
            }
        }

        // ---------- Shapes ----------

        private void BuildDisc(Material body, Material accent, Material pointer)
        {
            float y = discThickness * 0.5f;
            float top = discThickness;

            CreatePart(PrimitiveType.Cylinder, Vector3.up * y, new Vector3(radius * 2.15f, discThickness * 0.35f, radius * 2.15f), accent, "Rim");
            CreatePart(PrimitiveType.Cylinder, Vector3.up * y, new Vector3(radius * 2f, discThickness * 0.5f, radius * 2f), body, "Disc");
            CreatePart(PrimitiveType.Sphere, Vector3.up * top, new Vector3(radius * 1.7f, radius * 0.9f, radius * 1.7f), body, "Dome");
            CreatePart(PrimitiveType.Cube, new Vector3(radius * 0.55f, top, 0f), new Vector3(radius * 1.25f, discThickness * 0.6f, radius * 0.45f), pointer, "Pointer");
            CreatePart(PrimitiveType.Sphere, new Vector3(radius * 1.15f, top, 0f), Vector3.one * radius * 0.5f, pointer, "Nose");
            AddPin(accent);
        }

        private void BuildSaw(Material body, Material accent, Material pointer)
        {
            float y = discThickness * 0.5f;
            float top = discThickness;

            CreatePart(PrimitiveType.Cylinder, Vector3.up * y, new Vector3(radius * 1.7f, discThickness * 0.5f, radius * 1.7f), body, "Blade");

            const int teeth = 16;
            for (int i = 0; i < teeth; i++)
            {
                float ang = i * 360f / teeth;
                Vector3 dir = Quaternion.Euler(0f, ang, 0f) * Vector3.forward;
                Vector3 pos = dir * radius * 0.9f + Vector3.up * y;
                Material tm = (i == 0) ? pointer : body; // one bright tooth = spin marker
                CreatePart(PrimitiveType.Cube, pos, new Vector3(radius * 0.16f, discThickness * 0.5f, radius * 0.42f), tm, "Tooth", Quaternion.Euler(0f, ang + 20f, 0f));
            }

            CreatePart(PrimitiveType.Cylinder, Vector3.up * top, new Vector3(radius * 0.55f, discThickness * 0.7f, radius * 0.55f), accent, "Hub");
            CreatePart(PrimitiveType.Cylinder, Vector3.up * (top + 0.03f), new Vector3(radius * 0.22f, discThickness * 0.7f, radius * 0.22f), pointer, "Bolt");
            AddPin(accent);
        }

        private void BuildStar(Material body, Material accent, Material pointer)
        {
            float y = discThickness * 0.6f;
            float top = discThickness * 1.1f;

            const int points = 5;
            for (int i = 0; i < points; i++)
            {
                float ang = i * 360f / points;
                Vector3 dir = Quaternion.Euler(0f, ang, 0f) * Vector3.forward;
                Vector3 pos = dir * radius * 0.6f + Vector3.up * y;
                Material am = (i == 0) ? pointer : body; // one bright point = spin marker
                CreatePart(PrimitiveType.Cube, pos, new Vector3(radius * 0.34f, discThickness * 0.7f, radius * 1.5f), am, "Point", Quaternion.Euler(0f, ang, 0f));
            }

            CreatePart(PrimitiveType.Cylinder, Vector3.up * top, new Vector3(radius * 0.6f, discThickness * 0.8f, radius * 0.6f), accent, "Hub");
            CreatePart(PrimitiveType.Sphere, Vector3.up * (top + 0.05f), Vector3.one * radius * 0.34f, accent, "Knob");
            AddPin(accent);
        }

        private void BuildGear(Material body, Material accent, Material pointer)
        {
            float y = discThickness * 0.5f;
            float top = discThickness;

            CreatePart(PrimitiveType.Cylinder, Vector3.up * y, new Vector3(radius * 1.55f, discThickness * 0.5f, radius * 1.55f), body, "GearBody");

            const int teeth = 12;
            for (int i = 0; i < teeth; i++)
            {
                float ang = i * 360f / teeth;
                Vector3 dir = Quaternion.Euler(0f, ang, 0f) * Vector3.forward;
                Vector3 pos = dir * radius * 0.88f + Vector3.up * y;
                Material tm = (i == 0) ? pointer : body; // one bright tooth = spin marker
                CreatePart(PrimitiveType.Cube, pos, new Vector3(radius * 0.32f, discThickness * 0.55f, radius * 0.3f), tm, "GearTooth", Quaternion.Euler(0f, ang, 0f));
            }

            CreatePart(PrimitiveType.Cylinder, Vector3.up * top, new Vector3(radius * 0.85f, discThickness * 0.6f, radius * 0.85f), accent, "Ring");
            CreatePart(PrimitiveType.Cylinder, Vector3.up * (top + 0.03f), new Vector3(radius * 0.42f, discThickness * 0.7f, radius * 0.42f), body, "Hole");
            AddPin(accent);
        }

        // Vertical pin + knob so the lean/tilt is readable on any shape.
        private void AddPin(Material accent)
        {
            float pinBase = discThickness * 1.3f + radius * 0.25f;
            CreatePart(PrimitiveType.Cylinder, Vector3.up * (pinBase + pinHeight * 0.5f), new Vector3(radius * 0.14f, pinHeight * 0.5f, radius * 0.14f), accent, "Pin");
            CreatePart(PrimitiveType.Sphere, Vector3.up * (pinBase + pinHeight), Vector3.one * radius * 0.3f, accent, "Knob");
        }

        // ---------- Helpers ----------

        private GameObject CreatePart(PrimitiveType type, Vector3 localPos, Vector3 localScale, Material mat, string partName, Quaternion? localRot = null)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = partName;

            Collider col = go.GetComponent<Collider>();
            if (col != null)
                DestroySafe(col);

            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            if (localRot.HasValue)
                go.transform.localRotation = localRot.Value;
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
