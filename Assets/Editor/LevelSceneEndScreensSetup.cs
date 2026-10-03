using DreamCase.Meta;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace DreamCase.Editor
{
    /// <summary>
    /// One-time tool: builds LevelScene's win celebration and fail popup through Unity's API, and moves the HUD
    /// to a camera-space canvas so the celebration particles can draw above all UI. Delete this file after running it.
    /// </summary>
    public static class LevelSceneEndScreensSetup
    {
        private const string LevelScenePath = "Assets/Scenes/LevelScene.unity";
        private const string MaterialsFolder = "Assets/Materials";
        private const string StarPath = "Assets/Art/UI/Gameplay/Celebration/star.png";
        private const string SparklePath = "Assets/Art/UI/Gameplay/Celebration/Particles/additive_particle_star.png";
        private const string PopupBasePath = "Assets/Art/UI/Gameplay/Popup/popup_base.png";
        private const string PopupRibbonPath = "Assets/Art/UI/Gameplay/Popup/popup_ribbon.png";
        private const string CloseButtonPath = "Assets/Art/UI/Gameplay/Popup/close_button.png";
        private const string ButtonFacePath = "Assets/Art/UI/Menu/button.png";
        private const string ButtonFramePath = "Assets/Art/UI/Menu/button_frame.png";

        // Sorting: background -100, board 0..100, HUD 200, end screens 300, sparkles above everything.
        private const int HudSortingOrder = 200;
        private const int EndScreenSortingOrder = 300;
        private const int SparkleSortingOrder = 310;

        private static readonly Color TitleColor = new Color(1f, 0.93f, 0.55f);
        private static readonly Color OutlineColor = new Color(0.27f, 0.07f, 0.45f);

        [MenuItem("Dream Games/One-time Setup/Build Level End Screens")]
        private static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.OpenScene(LevelScenePath);
            if (GameObject.Find("EndScreenCanvas") != null)
            {
                EditorUtility.DisplayDialog("Level End Screens Setup", "LevelScene already has end screens. Nothing was changed.", "OK");
                return;
            }

            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
                AssetDatabase.CreateFolder("Assets", "Materials");
            var outlinedText = CreateOutlinedTextMaterial();
            var sparkleMaterial = CreateSparkleMaterial();

            var camera = Object.FindFirstObjectByType<Camera>();
            MoveHudToCameraSpace(camera);

            var canvas = CreateEndScreenCanvas(camera);
            var celebration = CreateCelebration(canvas, outlinedText, sparkleMaterial);
            var failPopup = CreateFailPopup(canvas, outlinedText);
            CreateEventSystem();

            var bootstrap = Object.FindFirstObjectByType<LevelSceneBootstrap>();
            SetReference(bootstrap, "celebration", celebration);
            SetReference(bootstrap, "failPopup", failPopup);

            EditorSceneManager.SaveScene(scene);
            EditorUtility.DisplayDialog("Level End Screens Setup", "The celebration and fail popup are ready.", "OK");
        }

        /// <summary>A copy of the default TMP font material with an outline, so titles stay readable over any background.</summary>
        private static Material CreateOutlinedTextMaterial()
        {
            var material = new Material(TMP_Settings.defaultFontAsset.material);
            material.EnableKeyword(ShaderUtilities.Keyword_Outline);
            material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.25f);
            material.SetColor(ShaderUtilities.ID_OutlineColor, OutlineColor);
            material.SetFloat(ShaderUtilities.ID_FaceDilate, 0.15f);
            AssetDatabase.CreateAsset(material, MaterialsFolder + "/OutlinedText.mat");
            return material;
        }

        /// <summary>Additive blending makes overlapping sparkles add up to bright light instead of covering each other.</summary>
        private static Material CreateSparkleMaterial()
        {
            var material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
            material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(SparklePath);
            AssetDatabase.CreateAsset(material, MaterialsFolder + "/CelebrationSparkle.mat");
            return material;
        }

        private static void MoveHudToCameraSpace(Camera camera)
        {
            var hud = GameObject.Find("HudCanvas").GetComponent<Canvas>();
            hud.renderMode = RenderMode.ScreenSpaceCamera;
            hud.worldCamera = camera;
            hud.planeDistance = 5f;
            hud.sortingOrder = HudSortingOrder;
        }

        private static RectTransform CreateEndScreenCanvas(Camera camera)
        {
            var canvasObject = new GameObject("EndScreenCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 4f;
            canvas.sortingOrder = EndScreenSortingOrder;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return (RectTransform)canvasObject.transform;
        }

        private static CelebrationView CreateCelebration(RectTransform canvas, Material outlinedText, Material sparkleMaterial)
        {
            var root = CreateRect("Celebration", canvas);
            Stretch(root, Vector2.zero, Vector2.one);

            // The dim covers the whole screen and doubles as the "tap anywhere" button.
            var dim = CreateDim(root);
            var continueButton = root.gameObject.AddComponent<Button>();
            continueButton.transition = Selectable.Transition.None;
            continueButton.targetGraphic = dim;

            var title = CreateText("Title", root, "Perfect!", 150f, TitleColor, outlinedText);
            Place(title.rectTransform, new Vector2(0f, 520f), new Vector2(1000f, 220f));

            var star = CreateRect("Star", root);
            Place(star, new Vector2(0f, 60f), new Vector2(520f, 490f));
            var starImage = star.gameObject.AddComponent<Image>();
            starImage.sprite = LoadSprite(StarPath);
            starImage.preserveAspect = true;
            starImage.raycastTarget = false;

            var tapToContinue = CreateText("TapToContinue", root, "Tap to Continue", 70f, Color.white, outlinedText);
            Place(tapToContinue.rectTransform, new Vector2(0f, -620f), new Vector2(900f, 120f));

            var sparkles = CreateSparkles(root, sparkleMaterial);
            ((RectTransform)sparkles.transform).anchoredPosition = new Vector2(0f, 60f);

            var view = root.gameObject.AddComponent<CelebrationView>();
            SetReference(view, "dim", dim);
            SetReference(view, "title", title.rectTransform);
            SetReference(view, "star", star);
            SetReference(view, "tapToContinue", tapToContinue);
            SetReference(view, "sparkles", sparkles);
            SetReference(view, "continueButton", continueButton);

            root.gameObject.SetActive(false);
            return view;
        }

        /// <summary>
        /// A burst of sparkles plus a gentle stream around the star. Sizes and speeds are in canvas units: with
        /// Hierarchy scaling the system follows the canvas scale, like the UI around it.
        /// </summary>
        private static ParticleSystem CreateSparkles(RectTransform parent, Material material)
        {
            var rect = CreateRect("Sparkles", parent);
            var particles = rect.gameObject.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = particles.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 3f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(300f, 900f);
            main.startSize = new ParticleSystem.MinMaxCurve(40f, 110f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new ParticleSystem.MinMaxGradient(TitleColor, Color.white);
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 300;

            var emission = particles.emission;
            emission.rateOverTime = 12f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 120f;

            var drag = particles.limitVelocityOverLifetime;
            drag.enabled = true;
            drag.drag = 2.5f;
            drag.multiplyDragByParticleSize = false;
            drag.multiplyDragByParticleVelocity = false;

            var rotation = particles.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-3f, 3f);

            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            var color = particles.colorOverLifetime;
            color.enabled = true;
            color.color = fade;

            var size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.3f));

            var renderer = rect.GetComponent<ParticleSystemRenderer>();
            renderer.material = material;
            renderer.sortingOrder = SparkleSortingOrder;
            return particles;
        }

        private static FailPopupView CreateFailPopup(RectTransform canvas, Material outlinedText)
        {
            var root = CreateRect("FailPopup", canvas);
            Stretch(root, Vector2.zero, Vector2.one);
            var dim = CreateDim(root);

            var popup = CreateRect("Popup", root);
            Place(popup, Vector2.zero, new Vector2(820f, 1096f));
            AddImage(popup, PopupBasePath);

            var ribbon = CreateRect("Ribbon", popup);
            ribbon.anchorMin = ribbon.anchorMax = new Vector2(0.5f, 1f);
            ribbon.sizeDelta = new Vector2(760f, 258f);
            ribbon.anchoredPosition = new Vector2(0f, -60f);
            AddImage(ribbon, PopupRibbonPath);

            var title = CreateText("Title", ribbon, "Level 1", 90f, Color.white, outlinedText);
            Stretch(title.rectTransform, new Vector2(0f, 0.2f), Vector2.one);

            var message = CreateText("Message", popup, "Out of moves!", 80f, Color.white, outlinedText);
            Place(message.rectTransform, new Vector2(0f, 80f), new Vector2(700f, 200f));

            var tryAgain = CreateTryAgainButton(popup, outlinedText);

            var close = CreateRect("CloseButton", popup);
            close.anchorMin = close.anchorMax = Vector2.one;
            close.sizeDelta = new Vector2(150f, 150f);
            close.anchoredPosition = new Vector2(-30f, -30f);
            var closeButton = close.gameObject.AddComponent<Button>();
            closeButton.targetGraphic = AddImage(close, CloseButtonPath);

            var view = root.gameObject.AddComponent<FailPopupView>();
            SetReference(view, "dim", dim);
            SetReference(view, "popup", popup);
            SetReference(view, "title", title);
            SetReference(view, "closeButton", closeButton);
            SetReference(view, "tryAgainButton", tryAgain);

            root.gameObject.SetActive(false);
            return view;
        }

        /// <summary>The menu's green button art: a sliced left half and its mirror, inside a mirrored frame.</summary>
        private static Button CreateTryAgainButton(RectTransform popup, Material outlinedText)
        {
            const float frameThickness = 14f;

            var root = CreateRect("TryAgainButton", popup);
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0f);
            root.sizeDelta = new Vector2(480f, 190f);
            root.anchoredPosition = new Vector2(0f, 220f);

            CreateMirroredHalves(root, "Frame", LoadSprite(ButtonFramePath), 0f);
            CreateMirroredHalves(root, "Face", LoadSprite(ButtonFacePath), frameThickness);

            var label = CreateText("Label", root, "Try Again", 70f, Color.white, outlinedText);
            Stretch(label.rectTransform, Vector2.zero, Vector2.one);

            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = root.Find("FaceLeft").GetComponent<Image>();
            return button;
        }

        private static void CreateMirroredHalves(RectTransform parent, string name, Sprite sprite, float inset)
        {
            var left = CreateRect(name + "Left", parent);
            Stretch(left, Vector2.zero, new Vector2(0.5f, 1f));
            left.offsetMin = new Vector2(inset, inset);
            left.offsetMax = new Vector2(0f, -inset);
            AddSlicedImage(left, sprite);

            var right = CreateRect(name + "Right", parent);
            Stretch(right, new Vector2(0.5f, 0f), Vector2.one);
            right.offsetMin = new Vector2(0f, inset);
            right.offsetMax = new Vector2(-inset, -inset);
            right.localScale = new Vector3(-1f, 1f, 1f);
            AddSlicedImage(right, sprite);
        }

        private static void AddSlicedImage(RectTransform rect, Sprite sprite)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
        }

        /// <summary>A full-screen black image, transparent until the view fades it in. It also blocks taps behind it.</summary>
        private static Image CreateDim(RectTransform parent)
        {
            var rect = CreateRect("Dim", parent);
            Stretch(rect, Vector2.zero, Vector2.one);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            return image;
        }

        private static TextMeshProUGUI CreateText(string name, RectTransform parent, string text, float size, Color color, Material material)
        {
            var rect = CreateRect(name, parent);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            label.fontSharedMaterial = material;
            label.raycastTarget = false;
            return label;
        }

        private static Image AddImage(RectTransform rect, string spritePath)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = LoadSprite(spritePath);
            image.preserveAspect = true;
            return image;
        }

        private static void CreateEventSystem()
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        /// <summary>Centered on the parent, at an offset and size in canvas units.</summary>
        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetReference(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite LoadSprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                throw new System.InvalidOperationException($"Sprite not found: {path}");
            return sprite;
        }
    }
}
