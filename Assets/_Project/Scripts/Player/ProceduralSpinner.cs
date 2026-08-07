using System.Collections.Generic;
using UnityEngine;

namespace SpinForward.Player
{
    /// <summary>
    /// Builds a proper spinning TOP (topaç): a glossy cone body with its point at
    /// the pivot origin, a rounded dome, a stem + knob, a decorative band, and a
    /// bright jewel so the spin reads. Because the tip sits AT this object's origin
    /// - which is what the lean rotates around - the top wobbles on its point and
    /// never dips into the ground, no matter how hard it leans.
    /// Put it on the spinning "visual" object. Right-click header > Rebuild.
    /// </summary>
    public class ProceduralSpinner : MonoBehaviour
    {
        [Header("Shape")]
        [SerializeField] private float bodyRadius = 0.6f;
        [SerializeField] private float bodyHeight = 1.2f;
        [SerializeField] private float stemHeight = 0.5f;
        [Range(8, 48)] [SerializeField] private int segments = 28;

        [Header("Colors")]
        [SerializeField] private Color bodyColor = new Color(0.12f, 0.35f, 0.95f);
        [SerializeField] private Color accentColor = new Color(1f, 0.82f, 0.2f);
        [Tooltip("Bright jewel that makes the spin obvious.")]
        [SerializeField] private Color jewelColor = new Color(1f, 0.25f, 0.35f);
        [Range(0f, 1f)] [SerializeField] private float metallic = 0.85f;
        [Range(0f, 1f)] [SerializeField] private float smoothness = 0.82f;
        [SerializeField] private float emission = 2.5f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int MetallicId = Shader.PropertyToID("_Metallic");
        private static readonly int SmoothnessId = Shader.PropertyToID("_Smoothness");
        private static readonly int CullId = Shader.PropertyToID("_Cull");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private void Awake() => Build();

        [ContextMenu("Rebuild")]
        public void Build()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroySafe(transform.GetChild(i).gameObject);

            Material body = MakeMaterial(bodyColor, false);
            Material accent = MakeMaterial(accentColor, true);
            Material jewel = MakeMaterial(jewelColor, true);

            // Cone body: tip at the origin (the pivot), widening upward.
            CreateMeshPart(BuildCone(bodyRadius, bodyHeight, segments), Vector3.zero, body, "Body");

            // Rounded dome sitting on the wide top of the cone.
            CreatePart(PrimitiveType.Sphere, Vector3.up * bodyHeight,
                new Vector3(bodyRadius * 2f, bodyRadius * 1.1f, bodyRadius * 2f), body, "Dome");

            // Decorative band around the widest part.
            CreatePart(PrimitiveType.Cylinder, Vector3.up * bodyHeight * 0.9f,
                new Vector3(bodyRadius * 1.95f, bodyRadius * 0.08f, bodyRadius * 1.95f), accent, "Band");

            // Stem + knob on top (the bit you'd flick to spin it).
            CreatePart(PrimitiveType.Cylinder, Vector3.up * (bodyHeight + stemHeight * 0.5f),
                new Vector3(bodyRadius * 0.22f, stemHeight * 0.5f, bodyRadius * 0.22f), accent, "Stem");
            CreatePart(PrimitiveType.Sphere, Vector3.up * (bodyHeight + stemHeight),
                Vector3.one * bodyRadius * 0.45f, accent, "Knob");

            // Jewel on the side of the cone - the asymmetry that makes spin visible.
            float jy = bodyHeight * 0.55f;
            float rAt = bodyRadius * (jy / bodyHeight); // cone radius at that height
            CreatePart(PrimitiveType.Sphere, new Vector3(rAt, jy, 0f),
                Vector3.one * bodyRadius * 0.4f, jewel, "Jewel");
        }

        // ---- Cone mesh: tip at (0,0,0), circular base at y = height ----
        private static Mesh BuildCone(float radius, float height, int seg)
        {
            var verts = new List<Vector3> { Vector3.zero };        // 0 = tip
            var tris = new List<int>();

            int ringStart = verts.Count;
            for (int i = 0; i < seg; i++)
            {
                float a = (float)i / seg * Mathf.PI * 2f;
                verts.Add(new Vector3(Mathf.Cos(a) * radius, height, Mathf.Sin(a) * radius));
            }

            for (int i = 0; i < seg; i++)
            {
                int a = ringStart + i;
                int b = ringStart + (i + 1) % seg;
                tris.Add(0); tris.Add(a); tris.Add(b); // sides
            }

            int baseCenter = verts.Count;
            verts.Add(new Vector3(0f, height, 0f));
            for (int i = 0; i < seg; i++)
            {
                int a = ringStart + i;
                int b = ringStart + (i + 1) % seg;
                tris.Add(baseCenter); tris.Add(b); tris.Add(a); // base cap
            }

            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private GameObject CreateMeshPart(Mesh mesh, Vector3 localPos, Material mat, string partName)
        {
            var go = new GameObject(partName, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
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
            m.SetFloat(CullId, 0f); // draw both sides so the generated cone never looks inside-out
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
