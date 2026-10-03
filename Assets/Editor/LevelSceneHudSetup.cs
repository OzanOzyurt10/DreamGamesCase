using DreamCase.Meta;
using DreamCase.View;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace DreamCase.Editor
{
    /// <summary>
    /// One-time tool: builds LevelScene's background and top panel (goals and moves) through Unity's API.
    /// Delete this file after running it once.
    /// </summary>
    public static class LevelSceneHudSetup
    {
        private const string LevelScenePath = "Assets/Scenes/LevelScene.unity";
        private const string BackgroundPath = "Assets/Art/Menu/background.png";
        private const string TopPanelPath = "Assets/Art/UI/Gameplay/Top/top_ui.png";
        private const string GoalCheckPath = "Assets/Art/UI/Gameplay/Top/goal_check.png";
        private const string VaseIconPath = "Assets/Art/Obstacles/Vase/vase_01.png";
        private const string StoneIconPath = "Assets/Art/Obstacles/Stone/stone.png";
        private const string ChaliceBoxIconPath = "Assets/Art/Obstacles/ChaliceBox/ChaliceBoxDoors.png";

        // top_ui.png is 1125x650. The rectangles below are measured on it, in pixels from its top-left corner.
        private static readonly Vector2 TopPanelArtSize = new Vector2(1125f, 650f);
        private static readonly Rect GoalTabArt = Rect.MinMaxRect(18f, 82f, 343f, 180f);
        private static readonly Rect MoveTabArt = Rect.MinMaxRect(782f, 82f, 1107f, 180f);
        private static readonly Rect GoalBoxArt = Rect.MinMaxRect(45f, 225f, 310f, 490f);
        private static readonly Rect MoveBoxArt = Rect.MinMaxRect(818f, 225f, 1085f, 490f);

        /// <summary>The panel's plain top strip (about 80 art px) is pushed above the screen edge.</summary>
        private const float TopPanelRaise = 77f;

        /// <summary>The part of the screen the board leaves free for the panel.</summary>
        private const float BoardTopReservedFraction = 0.26f;

        private static readonly Color BackgroundTint = new Color(0.7f, 0.7f, 0.7f);
        private static readonly Color CounterColor = new Color(0.33f, 0.12f, 0.55f);

        [MenuItem("Dream Games/One-time Setup/Build Level Scene HUD")]
        private static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.OpenScene(LevelScenePath);
            if (Object.FindFirstObjectByType<Canvas>() != null)
            {
                EditorUtility.DisplayDialog("Level Scene HUD Setup", "LevelScene already has a Canvas. Nothing was changed.", "OK");
                return;
            }

            var camera = Object.FindFirstObjectByType<Camera>();
            CreateBackground(camera);
            var hud = CreateHud();

            SetReference(Object.FindFirstObjectByType<LevelSceneBootstrap>(), "hud", hud);

            var boardView = new SerializedObject(Object.FindFirstObjectByType<BoardView>());
            boardView.FindProperty("topReservedFraction").floatValue = BoardTopReservedFraction;
            boardView.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene);
            EditorUtility.DisplayDialog("Level Scene HUD Setup", "LevelScene's background and top panel are ready.", "OK");
        }

        /// <summary>A camera-space canvas sorted behind every sprite, so the picture sits behind the board.</summary>
        private static void CreateBackground(Camera camera)
        {
            var canvas = CreateCanvas("BackgroundCanvas");
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 50f;
            canvas.sortingOrder = -100;

            var background = CreateRect("Background", canvas.transform);
            Stretch(background, Vector2.zero, Vector2.one);
            var image = background.gameObject.AddComponent<Image>();
            image.sprite = LoadSprite(BackgroundPath);
            image.color = BackgroundTint;
            image.raycastTarget = false;

            var fitter = background.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = image.sprite.rect.width / image.sprite.rect.height;
        }

        private static LevelHud CreateHud()
        {
            var canvas = CreateCanvas("HudCanvas");
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var panel = CreateRect("TopPanel", canvas.transform);
            panel.anchorMin = new Vector2(0f, 1f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.pivot = new Vector2(0.5f, 1f);
            panel.anchoredPosition = new Vector2(0f, TopPanelRaise);
            var panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.sprite = LoadSprite(TopPanelPath);
            panelImage.raycastTarget = false;
            var panelFitter = panel.gameObject.AddComponent<AspectRatioFitter>();
            panelFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            panelFitter.aspectRatio = TopPanelArtSize.x / TopPanelArtSize.y;

            CreateLabel("GoalLabel", panel, GoalTabArt, "Goal", 56f, Color.white);
            CreateLabel("MoveLabel", panel, MoveTabArt, "Move", 56f, Color.white);

            var goals = CreateGoals(panel);

            var movesText = CreateLabel("MovesCount", panel, MoveBoxArt, "0", 130f, CounterColor);
            var moves = movesText.gameObject.AddComponent<MovesView>();
            SetReference(moves, "count", movesText);

            var hud = panel.gameObject.AddComponent<LevelHud>();
            SetReference(hud, "goals", goals);
            SetReference(hud, "moves", moves);
            return hud;
        }

        private static GoalsView CreateGoals(RectTransform panel)
        {
            var box = CreateRect("Goals", panel);
            PlaceOnArt(box, GoalBoxArt);

            var grid = box.gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.spacing = new Vector2(6f, 6f);
            grid.childAlignment = TextAnchor.MiddleCenter;

            var goals = box.gameObject.AddComponent<GoalsView>();
            SetReference(goals, "entryTemplate", CreateGoalEntryTemplate(box));
            SetReference(goals, "grid", grid);
            SetReference(goals, "vaseIcon", LoadSprite(VaseIconPath));
            SetReference(goals, "stoneIcon", LoadSprite(StoneIconPath));
            SetReference(goals, "chaliceBoxIcon", LoadSprite(ChaliceBoxIconPath));
            return goals;
        }

        /// <summary>An inactive entry that GoalsView copies once per goal: icon, count in the corner, check mark.</summary>
        private static GoalEntryView CreateGoalEntryTemplate(RectTransform parent)
        {
            var entry = CreateRect("GoalEntryTemplate", parent);

            var icon = CreateRect("Icon", entry);
            Stretch(icon, Vector2.zero, Vector2.one);
            var iconImage = icon.gameObject.AddComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            var count = CreateRect("Count", entry);
            Stretch(count, new Vector2(0.4f, 0f), new Vector2(1f, 0.45f));
            var countText = count.gameObject.AddComponent<TextMeshProUGUI>();
            countText.text = "0";
            countText.fontSize = 44f;
            countText.fontStyle = FontStyles.Bold;
            countText.alignment = TextAlignmentOptions.BottomRight;
            countText.color = CounterColor;
            countText.raycastTarget = false;

            var check = CreateRect("Check", entry);
            Stretch(check, new Vector2(0.55f, 0f), new Vector2(1f, 0.45f));
            var checkImage = check.gameObject.AddComponent<Image>();
            checkImage.sprite = LoadSprite(GoalCheckPath);
            checkImage.preserveAspect = true;
            checkImage.raycastTarget = false;

            var view = entry.gameObject.AddComponent<GoalEntryView>();
            SetReference(view, "icon", iconImage);
            SetReference(view, "count", countText);
            SetReference(view, "check", checkImage);

            entry.gameObject.SetActive(false);
            return view;
        }

        private static TextMeshProUGUI CreateLabel(string name, RectTransform panel, Rect artRect, string text, float size, Color color)
        {
            var rect = CreateRect(name, panel);
            PlaceOnArt(rect, artRect);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }

        /// <summary>Anchors a child to a rectangle measured on top_ui.png, so it scales with the panel.</summary>
        private static void PlaceOnArt(RectTransform rect, Rect artRect)
        {
            rect.anchorMin = new Vector2(artRect.xMin / TopPanelArtSize.x, 1f - artRect.yMax / TopPanelArtSize.y);
            rect.anchorMax = new Vector2(artRect.xMax / TopPanelArtSize.x, 1f - artRect.yMin / TopPanelArtSize.y);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Canvas CreateCanvas(string name)
        {
            var canvasObject = new GameObject(name, typeof(Canvas), typeof(CanvasScaler));
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvasObject.GetComponent<Canvas>();
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
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
