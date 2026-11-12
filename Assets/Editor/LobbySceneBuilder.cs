using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class LobbySceneBuilder
{
    const string ScenePath = "Assets/Scenes/Lobby.unity";

    static readonly Color Background = new Color(0.025f, 0.04f, 0.07f, 1f);
    static readonly Color Panel = new Color(0.07f, 0.1f, 0.15f, 1f);
    static readonly Color Row = new Color(0.09f, 0.13f, 0.19f, 1f);
    static readonly Color Accent = new Color(0.35f, 0.86f, 0.96f, 1f);
    static readonly Color Green = new Color(0.35f, 0.82f, 0.52f, 1f);
    static readonly Color Ink = new Color(0.93f, 0.95f, 0.98f, 1f);
    static readonly Color Muted = new Color(0.58f, 0.66f, 0.76f, 1f);

    static bool attempted;

    [InitializeOnLoadMethod]
    static void AutoBuildIfMissing()
    {
        EditorApplication.delayCall += () =>
        {
            if (attempted || EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath))
                return;

            attempted = true;
            Build();
        };
    }

    [MenuItem("T-60/Build Lobby Scene")]
    public static void Build()
    {
        bool dirty = false;
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            dirty |= EditorSceneManager.GetSceneAt(i).isDirty;

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
        cameraGo.AddComponent<AudioListener>();
        cameraGo.transform.position = new Vector3(0f, 0f, -10f);

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        var input = eventSystem.AddComponent<InputSystemUIInputModule>();
        BindUiActions(input);

        var canvasGo = new GameObject("Canvas");
        canvasGo.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var background = CreateImage("Background", canvasGo.transform, sprite, Background);
        Stretch(background.rectTransform, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 0f);

        var content = CreateRect("Content", canvasGo.transform);
        Stretch(content, 0f, 0f, 1f, 1f, 96f, 56f, 96f, 48f);

        var eyebrow = CreateText(content, "Eyebrow", "MULTIPLAYER  /  LAUNCH CONTROL", 18, FontStyle.Bold, Accent, TextAnchor.UpperLeft, font);
        Stretch(eyebrow.rectTransform, 0f, 1f, 0.7f, 1f, 8f, -28f, 0f, 0f);

        var title = CreateText(content, "Title", "MISSION LOBBY", 58, FontStyle.Bold, Ink, TextAnchor.UpperLeft, font);
        Stretch(title.rectTransform, 0f, 1f, 0.7f, 1f, 4f, -112f, 0f, 44f);

        var subtitle = CreateText(content, "Subtitle", "Rally your crew. Launch when everyone is ready.", 22, FontStyle.Normal, Muted, TextAnchor.UpperLeft, font);
        Stretch(subtitle.rectTransform, 0f, 1f, 0.8f, 1f, 8f, -152f, 0f, 0f);

        var card = CreateImage("Lobby Card", content, sprite, Panel);
        Stretch(card.rectTransform, 0f, 0f, 1f, 0.74f, 0f, 0f, 0f, 0f);

        var roomLabel = CreateText(card.transform, "Room Label", "ROOM CODE", 16, FontStyle.Bold, Muted, TextAnchor.UpperLeft, font);
        Stretch(roomLabel.rectTransform, 0f, 0.82f, 0.48f, 1f, 32f, 0f, 0f, 16f);

        var roomCode = CreateText(card.transform, "Room Code", "T60-042", 36, FontStyle.Bold, Accent, TextAnchor.UpperLeft, font);
        Stretch(roomCode.rectTransform, 0f, 0.72f, 0.48f, 0.88f, 32f, 0f, 0f, 0f);

        var crewLabel = CreateText(card.transform, "Crew Label", "CREW  ·  1/4 CONNECTED", 16, FontStyle.Bold, Muted, TextAnchor.UpperLeft, font);
        Stretch(crewLabel.rectTransform, 0f, 0.58f, 0.62f, 0.7f, 32f, 0f, 0f, 0f);

        var crew = CreateRect("Crew", card.transform);
        Stretch(crew, 0f, 0.1f, 0.62f, 0.58f, 24f, 0f, 16f, 8f);
        var layout = crew.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        AddPlayerRow(crew, sprite, font, "PLAYER 01  ·  YOU", "HOST", Green);
        AddPlayerRow(crew, sprite, font, "WAITING FOR PLAYER", "EMPTY SLOT", Muted);
        AddPlayerRow(crew, sprite, font, "WAITING FOR PLAYER", "EMPTY SLOT", Muted);
        AddPlayerRow(crew, sprite, font, "WAITING FOR PLAYER", "EMPTY SLOT", Muted);

        var statusCard = CreateImage("Status Card", card.transform, sprite, Row);
        Stretch(statusCard.rectTransform, 0.67f, 0.25f, 0.96f, 0.88f, 0f, 0f, 0f, 0f);

        var statusLabel = CreateText(statusCard.transform, "Status Label", "LOBBY STATUS", 16, FontStyle.Bold, Muted, TextAnchor.UpperLeft, font);
        Stretch(statusLabel.rectTransform, 0f, 0.75f, 1f, 1f, 24f, 0f, 24f, 16f);

        var status = CreateText(statusCard.transform, "Readiness", "NOT READY", 30, FontStyle.Bold, Ink, TextAnchor.UpperLeft, font);
        Stretch(status.rectTransform, 0f, 0.47f, 1f, 0.78f, 24f, 0f, 24f, 0f);

        var statusHint = CreateText(statusCard.transform, "Status Hint", "Waiting for your crew to join.", 18, FontStyle.Normal, Muted, TextAnchor.UpperLeft, font);
        Stretch(statusHint.rectTransform, 0f, 0.1f, 1f, 0.48f, 24f, 0f, 24f, 0f);

        var readyButton = CreateButton("Ready Up", card.transform, sprite, Green, font, "READY UP", new Color(0.035f, 0.08f, 0.05f, 1f));
        Stretch(readyButton.GetComponent<RectTransform>(), 0.67f, 0.08f, 0.96f, 0.2f, 0f, 0f, 0f, 0f);

        var leaveButton = CreateButton("Leave Lobby", card.transform, sprite, new Color(0.14f, 0.18f, 0.24f, 1f), font, "LEAVE LOBBY", Ink);
        Stretch(leaveButton.GetComponent<RectTransform>(), 0.67f, 0.02f, 0.96f, 0.08f, 0f, 0f, 0f, -8f);

        var hint = CreateText(content, "Footer Hint", "SHARE THE ROOM CODE TO INVITE YOUR CREW", 16, FontStyle.Normal, Muted, TextAnchor.MiddleRight, font);
        Stretch(hint.rectTransform, 0.45f, 0f, 1f, 0f, 0f, 8f, 8f, -40f);

        var controller = new GameObject("Lobby Controller").AddComponent<LobbySceneController>();
        controller.Configure(status, readyButton.GetComponentInChildren<Text>());
        readyButton.onClick.AddListener(controller.ToggleReady);
        leaveButton.onClick.AddListener(controller.LeaveLobby);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Debug.Log("Created lobby scene at " + ScenePath);
    }

    static void AddPlayerRow(Transform parent, Sprite sprite, Font font, string player, string role, Color roleColor)
    {
        var row = CreateImage(player, parent, sprite, Row);
        var element = row.gameObject.AddComponent<LayoutElement>();
        element.preferredHeight = 72f;
        element.minHeight = 72f;

        var playerName = CreateText(row.transform, "Player", player, 18, FontStyle.Bold, Ink, TextAnchor.MiddleLeft, font);
        Stretch(playerName.rectTransform, 0f, 0f, 0.72f, 1f, 20f, 0f, 0f, 0f);

        var roleLabel = CreateText(row.transform, "Role", role, 14, FontStyle.Bold, roleColor, TextAnchor.MiddleRight, font);
        Stretch(roleLabel.rectTransform, 0.72f, 0f, 1f, 1f, 0f, 0f, 20f, 0f);
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
        var text = CreateText(image.transform, "Label", label, 18, FontStyle.Bold, labelColor, TextAnchor.MiddleCenter, font);
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
