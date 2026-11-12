using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class SelectionMenuSceneBuilder
{
    const string ScenePath = "Assets/Scenes/SelectionMenu.unity";

    static readonly Color Background = new Color(0.027f, 0.043f, 0.078f, 1f);
    static readonly Color Panel = new Color(0.07f, 0.1f, 0.15f, 1f);
    static readonly Color Row = new Color(0.09f, 0.13f, 0.19f, 1f);
    static readonly Color Amber = new Color(0.96f, 0.65f, 0.14f, 1f);
    static readonly Color Cyan = new Color(0.35f, 0.86f, 0.96f, 1f);
    static readonly Color Ink = new Color(0.93f, 0.95f, 0.98f, 1f);
    static readonly Color Muted = new Color(0.58f, 0.66f, 0.76f, 1f);

    static bool attempted;

    [InitializeOnLoadMethod]
    static void AutoBuildIfMissing()
    {
        EditorApplication.delayCall += () =>
        {
            if (attempted || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (File.Exists(ScenePath))
                return;
            attempted = true;
            Build();
        };
    }

    [MenuItem("T-60/Build Selection Menu Scene")]
    public static void Build()
    {
        bool dirty = false;
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            if (EditorSceneManager.GetSceneAt(i).isDirty)
                dirty = true;
        }

        var scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            dirty ? NewSceneMode.Additive : NewSceneMode.Single);
        EditorSceneManager.SetActiveScene(scene);

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        var cameraGo = new GameObject("Main Camera");
        cameraGo.tag = "MainCamera";
        var camera = cameraGo.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Background;
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        cameraGo.AddComponent<AudioListener>();
        cameraGo.transform.position = new Vector3(0f, 0f, -10f);

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        var input = eventSystem.AddComponent<InputSystemUIInputModule>();
        BindUiActions(input);

        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var background = CreateImage("Background", canvasGo.transform, sprite, Background);
        Stretch(background.rectTransform, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);

        var content = CreateRect("Content", canvasGo.transform);
        Stretch(content, 0f, 0f, 1f, 1f, 80f, 56f, 80f, 48f);

        var eyebrow = CreateText(content, "Eyebrow", "LAUNCH CONTROL", 18, FontStyle.Bold, Cyan, TextAnchor.UpperLeft, font);
        Stretch(eyebrow.rectTransform, 0f, 1f, 0.55f, 1f, 8f, -28f, 0f, 0f);

        var title = CreateText(content, "Title", "T−60", 92, FontStyle.Bold, Ink, TextAnchor.UpperLeft, font);
        Stretch(title.rectTransform, 0f, 1f, 0.55f, 1f, 0f, -132f, 0f, 36f);

        var countdown = CreateText(content, "Countdown", "T− 00:01:00", 28, FontStyle.Bold, Amber, TextAnchor.UpperRight, font);
        Stretch(countdown.rectTransform, 0.55f, 1f, 1f, 1f, 0f, -72f, 8f, 28f);

        var listLabel = CreateText(content, "List Label", "SELECT A CONSOLE", 16, FontStyle.Bold, Muted, TextAnchor.UpperLeft, font);
        Stretch(listLabel.rectTransform, 0f, 1f, 0.46f, 1f, 8f, -188f, 0f, 156f);

        var list = CreateRect("Options", content);
        Stretch(list, 0f, 0f, 0.46f, 1f, 0f, 72f, 0f, 204f);
        var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var card = CreateImage("Detail Card", content, sprite, Panel);
        Stretch(card.rectTransform, 0.5f, 0.12f, 1f, 0.78f, 0f, 0f, 0f, 0f);
        card.type = Image.Type.Sliced;

        var cardKicker = CreateText(card.transform, "Card Kicker", "CONSOLE", 16, FontStyle.Bold, Cyan, TextAnchor.UpperLeft, font);
        Stretch(cardKicker.rectTransform, 0f, 1f, 1f, 1f, 36f, -64f, 36f, 32f);

        var detailTitle = CreateText(card.transform, "Detail Title", "FLIGHT", 42, FontStyle.Bold, Ink, TextAnchor.UpperLeft, font);
        Stretch(detailTitle.rectTransform, 0f, 1f, 1f, 1f, 36f, -130f, 36f, 72f);

        var detailBody = CreateText(card.transform, "Detail Body", "", 24, FontStyle.Normal, Muted, TextAnchor.UpperLeft, font);
        Stretch(detailBody.rectTransform, 0f, 0.28f, 1f, 1f, 36f, 0f, 36f, 150f);

        var confirm = CreateButton("Lock In", card.transform, sprite, Amber, font, "LOCK IN", new Color(0.08f, 0.06f, 0.02f, 1f));
        Stretch(confirm.GetComponent<RectTransform>(), 0f, 0f, 0.48f, 0f, 36f, 36f, 0f, -100f);

        var exit = CreateButton("Exit", card.transform, sprite, new Color(0.14f, 0.18f, 0.24f, 1f), font, "EXIT", Ink);
        Stretch(exit.GetComponent<RectTransform>(), 0.52f, 0f, 1f, 0f, 0f, 36f, 36f, -100f);

        var status = CreateText(content, "Status", "SELECTED  ·  FLIGHT", 18, FontStyle.Bold, Ink, TextAnchor.MiddleLeft, font);
        Stretch(status.rectTransform, 0f, 0f, 0.7f, 0f, 8f, 8f, 0f, -40f);

        var hint = CreateText(content, "Hint", "ARROWS MOVE    ·    ENTER LOCKS IN", 16, FontStyle.Normal, Muted, TextAnchor.MiddleRight, font);
        Stretch(hint.rectTransform, 0.45f, 0f, 1f, 0f, 0f, 8f, 8f, -40f);

        string[] titles = { "FLIGHT", "GUIDANCE", "PROPULSION", "RANGE SAFETY" };
        string[] descriptions =
        {
            "Call the final go or no-go. You hold the count, the loop, and the last word before liftoff.",
            "Watch the trajectory. If the vehicle drifts in the final minute, you steer it back on course.",
            "Monitor engines and tanks from ignition through the moment the stack leaves the pad.",
            "Guard the range. If the path breaks the safety lines, you make the abort call."
        };

        var optionObjects = new GameObject[titles.Length];
        var rowBackgrounds = new Image[titles.Length];
        var selectionBars = new Image[titles.Length];
        var buttons = new Selectable[titles.Length];

        for (int i = 0; i < titles.Length; i++)
        {
            var row = CreateImage("Option " + titles[i], list, sprite, Row);
            row.type = Image.Type.Sliced;
            var rowButton = row.gameObject.AddComponent<Button>();
            rowButton.targetGraphic = row;
            rowButton.transition = Selectable.Transition.None;
            var element = row.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = 92f;
            element.minHeight = 92f;

            var bar = CreateImage("Bar", row.transform, sprite, new Color(Amber.r, Amber.g, Amber.b, 0f));
            Stretch(bar.rectTransform, 0f, 0f, 0f, 1f, 10f, 16f, -16f, 16f);

            var label = CreateText(row.transform, "Label", titles[i], 26, FontStyle.Bold, Ink, TextAnchor.MiddleLeft, font);
            Stretch(label.rectTransform, 0f, 0f, 1f, 1f, 36f, 0f, 16f, 0f);

            optionObjects[i] = row.gameObject;
            rowBackgrounds[i] = row;
            selectionBars[i] = bar;
            buttons[i] = rowButton;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            var navigation = buttons[i].navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = buttons[(i - 1 + buttons.Length) % buttons.Length];
            navigation.selectOnDown = buttons[(i + 1) % buttons.Length];
            buttons[i].navigation = navigation;
        }

        var menuGo = new GameObject("Selection Menu");
        var menu = menuGo.AddComponent<SelectionMenuController>();
        var serialized = new SerializedObject(menu);
        var options = serialized.FindProperty("options");
        options.arraySize = titles.Length;
        for (int i = 0; i < titles.Length; i++)
        {
            var element = options.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("title").stringValue = titles[i];
            element.FindPropertyRelative("description").stringValue = descriptions[i];
        }

        AssignObjectArray(serialized.FindProperty("optionObjects"), optionObjects);
        AssignObjectArray(serialized.FindProperty("rowBackgrounds"), rowBackgrounds);
        AssignObjectArray(serialized.FindProperty("selectionBars"), selectionBars);
        serialized.FindProperty("detailTitle").objectReferenceValue = detailTitle;
        serialized.FindProperty("detailBody").objectReferenceValue = detailBody;
        serialized.FindProperty("statusText").objectReferenceValue = status;
        serialized.FindProperty("countdownText").objectReferenceValue = countdown;
        serialized.FindProperty("rowColor").colorValue = Row;
        serialized.FindProperty("rowSelectedColor").colorValue = new Color(0.12f, 0.2f, 0.28f, 1f);
        serialized.FindProperty("barColor").colorValue = Amber;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        for (int i = 0; i < optionObjects.Length; i++)
            optionObjects[i].AddComponent<MenuOptionButton>().Configure(menu, i);

        confirm.onClick.AddListener(menu.Confirm);
        exit.onClick.AddListener(menu.ExitMenu);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings();
        Debug.Log("Created selection menu scene at " + ScenePath);
    }

    static void BindUiActions(InputSystemUIInputModule module)
    {
        var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Settings/InputSystem_Actions.inputactions");
        if (asset == null || module.actionsAsset != null)
            return;

        module.actionsAsset = asset;
        module.point = InputActionReference.Create(asset.FindAction("UI/Point", true));
        module.leftClick = InputActionReference.Create(asset.FindAction("UI/Click", true));
        module.rightClick = InputActionReference.Create(asset.FindAction("UI/RightClick", true));
        module.middleClick = InputActionReference.Create(asset.FindAction("UI/MiddleClick", true));
        module.scrollWheel = InputActionReference.Create(asset.FindAction("UI/ScrollWheel", true));
        module.move = InputActionReference.Create(asset.FindAction("UI/Navigate", true));
        module.submit = InputActionReference.Create(asset.FindAction("UI/Submit", true));
        module.cancel = InputActionReference.Create(asset.FindAction("UI/Cancel", true));
        module.trackedDevicePosition = InputActionReference.Create(asset.FindAction("UI/TrackedDevicePosition", true));
        module.trackedDeviceOrientation = InputActionReference.Create(asset.FindAction("UI/TrackedDeviceOrientation", true));
    }

    static void AddToBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(scene => scene.path == ScenePath))
            return;

        scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    static void AssignObjectArray<T>(SerializedProperty property, T[] values) where T : Object
    {
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
    {
        var rect = CreateRect(name, parent);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.type = Image.Type.Sliced;
        return image;
    }

    static Text CreateText(Transform parent, string name, string value, int size, FontStyle style, Color color, TextAnchor anchor, Font font)
    {
        var rect = CreateRect(name, parent);
        var text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    static Button CreateButton(string name, Transform parent, Sprite sprite, Color color, Font font, string label, Color labelColor)
    {
        var image = CreateImage(name, parent, sprite, color);
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var text = CreateText(image.transform, "Label", label, 22, FontStyle.Bold, labelColor, TextAnchor.MiddleCenter, font);
        Stretch(text.rectTransform, 0f, 0f, 1f, 1f, 12f, 8f, 12f, 8f);
        return button;
    }

    static void Stretch(RectTransform rect, float minX, float minY, float maxX, float maxY, float left, float bottom, float right, float top)
    {
        rect.anchorMin = new Vector2(minX, minY);
        rect.anchorMax = new Vector2(maxX, maxY);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }
}
