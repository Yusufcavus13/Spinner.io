using SpinForward.Core;
using SpinForward.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SpinForward.EditorTools
{
    /// <summary>
    /// One-click builder for the playable prototype (ground + spinner + camera +
    /// joystick), assembled through the editor API so Unity owns the scene file.
    /// Safe to re-run: it wipes and rebuilds its own root object each time.
    /// </summary>
    public static class SceneBuilder
    {
        private const string RootName = "SpinForward_Playground";
        private const string MaterialDir = "Assets/_Project/Materials";

        [MenuItem("Tools/Spin Forward/Build Playground Scene")]
        public static void BuildPlayground()
        {
            // Start clean so repeated runs don't stack duplicates.
            var existing = GameObject.Find(RootName);
            if (existing != null)
                Object.DestroyImmediate(existing);

            var root = new GameObject(RootName);

            var ground = BuildGround(root.transform);
            var spinner = BuildSpinner(root.transform, out var visual, out var joystickTarget);
            var joystick = BuildJoystickUI(root.transform, out var background, out var handle);

            WireJoystick(joystick, background, handle);
            WireSpinner(spinner, joystick, visual);
            SetupCamera(joystickTarget);

            Selection.activeGameObject = spinner.gameObject;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Spin Forward] Playground built. Press Play and drag anywhere to steer the spinner.");
        }

        // ---- Ground -------------------------------------------------------

        private static GameObject BuildGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(parent);
            ground.transform.localScale = new Vector3(5f, 1f, 5f); // 50 x 50 units
            ApplyMaterial(ground, "M_Ground", new Color(0.16f, 0.18f, 0.22f));
            return ground;
        }

        // ---- Spinner ------------------------------------------------------

        private static Transform BuildSpinner(Transform parent, out Transform visual, out Transform followTarget)
        {
            var spinner = new GameObject("Spinner");
            spinner.transform.SetParent(parent);
            spinner.transform.position = new Vector3(0f, 0.6f, 0f);

            var body = spinner.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.constraints = RigidbodyConstraints.FreezePositionY
                             | RigidbodyConstraints.FreezeRotationX
                             | RigidbodyConstraints.FreezeRotationZ
                             | RigidbodyConstraints.FreezeRotationY;

            var col = spinner.AddComponent<SphereCollider>();
            col.radius = 0.6f;

            // Visual child: a flat cylinder that stands in for the spinner top.
            var visualGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visualGo.name = "Visual";
            Object.DestroyImmediate(visualGo.GetComponent<Collider>()); // physics lives on the root
            visualGo.transform.SetParent(spinner.transform);
            visualGo.transform.localPosition = Vector3.zero;
            visualGo.transform.localScale = new Vector3(1.2f, 0.25f, 1.2f);
            ApplyMaterial(visualGo, "M_Spinner", new Color(0.20f, 0.55f, 1f));

            visual = visualGo.transform;
            followTarget = spinner.transform;
            return spinner.transform;
        }

        // ---- Joystick UI --------------------------------------------------

        private static FloatingJoystick BuildJoystickUI(Transform parent, out RectTransform background, out RectTransform handle)
        {
            var canvasGo = new GameObject("UICanvas", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(parent);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            var joystick = canvasGo.AddComponent<FloatingJoystick>();

            background = CreateImage("JoystickBackground", canvasGo.transform,
                "UI/Skin/Background.psd", 300f, new Color(1f, 1f, 1f, 0.25f));
            handle = CreateImage("JoystickHandle", background,
                "UI/Skin/Knob.psd", 130f, new Color(1f, 1f, 1f, 0.6f));

            background.gameObject.SetActive(false); // hidden until touched
            return joystick;
        }

        private static RectTransform CreateImage(string name, Transform parent, string builtinSprite, float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(builtinSprite);
            img.color = color;
            img.raycastTarget = false;
            return rect;
        }

        // ---- Camera -------------------------------------------------------

        private static void SetupCamera(Transform target)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[Spin Forward] No Main Camera found; skipping camera follow setup.");
                return;
            }

            var follow = cam.GetComponent<CameraFollow>();
            if (follow == null)
                follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.SetTarget(target);
            cam.transform.position = target.position + new Vector3(0f, 14f, -9f);
        }

        // ---- Wiring & helpers --------------------------------------------

        private static void WireJoystick(FloatingJoystick joystick, RectTransform background, RectTransform handle)
        {
            SetRef(joystick, "background", background);
            SetRef(joystick, "handle", handle);
        }

        private static void WireSpinner(Transform spinner, FloatingJoystick joystick, Transform visual)
        {
            var controller = spinner.gameObject.AddComponent<SpinnerController>();
            SetRef(controller, "joystick", joystick);
            SetRef(controller, "visual", visual);
        }

        private static void SetRef(Object component, string field, Object value)
        {
            var so = new SerializedObject(component);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogWarning($"[Spin Forward] Field '{field}' not found on {component.GetType().Name}.");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ApplyMaterial(GameObject go, string assetName, Color color)
        {
            if (!AssetDatabase.IsValidFolder(MaterialDir))
                AssetDatabase.CreateFolder("Assets/_Project", "Materials");

            string path = $"{MaterialDir}/{assetName}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", color);

            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = mat;
        }
    }
}
