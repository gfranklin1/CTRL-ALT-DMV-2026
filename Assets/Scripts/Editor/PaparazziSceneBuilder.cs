// AUTO-GENERATED — runs once on compile, then removes itself.
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.AI;
using UnityEngine.UI;
using Unity.AI.Navigation;

[InitializeOnLoad]
public static class PaparazziSceneBuilder
{
    static PaparazziSceneBuilder()
    {
        EditorApplication.delayCall += TryAutoRun;
    }

    static void TryAutoRun()
    {
        // Only auto-run once per session if scene not yet built
        if (GameObject.Find("[Managers]") != null) return;
        BuildScene();
    }

    [MenuItem("Paparazzi/Rebuild Scene %#b")]
    public static void BuildScene()
    {
        Debug.Log("[SceneBuilder] Starting scene build...");

        // ── Cleanup existing Main Camera ──────────────────────────────────
        var oldCam = GameObject.Find("Main Camera");
        if (oldCam != null) Object.DestroyImmediate(oldCam);

        // ── MANAGERS ──────────────────────────────────────────────────────
        var managers = new GameObject("[Managers]");
        managers.AddComponent<GameManager>();
        var missionMgr  = managers.AddComponent<MissionManager>();
        managers.AddComponent<SuspicionSystem>();
        var scorer      = managers.AddComponent<PhotoScorer>();

        // ── LEVEL GEOMETRY ────────────────────────────────────────────────
        var levelRoot = new GameObject("[Level Geometry]");

        // Floor
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(levelRoot.transform);
        floor.transform.localScale = new Vector3(3f, 1f, 3f);
        GameObjectUtility.SetStaticEditorFlags(floor,
            StaticEditorFlags.NavigationStatic | StaticEditorFlags.ContributeGI);

        // Walls
        MakeWall("Wall_North", new Vector3(0,  2,  15), new Vector3(30, 4, 0.5f), levelRoot.transform);
        MakeWall("Wall_South", new Vector3(0,  2, -15), new Vector3(30, 4, 0.5f), levelRoot.transform);
        MakeWall("Wall_East",  new Vector3(15, 2,   0), new Vector3(0.5f, 4, 30), levelRoot.transform);
        MakeWall("Wall_West",  new Vector3(-15,2,   0), new Vector3(0.5f, 4, 30), levelRoot.transform);

        // Exit Zone
        var exitGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exitGO.name = "ExitZone";
        exitGO.transform.SetParent(levelRoot.transform);
        exitGO.transform.position = new Vector3(12f, 0.5f, 12f);
        exitGO.transform.localScale = new Vector3(2f, 1f, 2f);
        exitGO.GetComponent<BoxCollider>().isTrigger = true;
        var exitMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        exitMat.color = Color.green;
        exitGO.GetComponent<Renderer>().sharedMaterial = exitMat;
        var exitTrigger = exitGO.AddComponent<ExitTrigger>();
        SetField(exitTrigger, "rend", exitGO.GetComponent<Renderer>());

        // ── WAYPOINTS ─────────────────────────────────────────────────────
        var waypointsRoot = new GameObject("[Waypoints]");
        var wp0 = MakeWaypoint("WP0", new Vector3(-7f, 0f,  7f), waypointsRoot.transform);
        var wp1 = MakeWaypoint("WP1", new Vector3( 7f, 0f,  7f), waypointsRoot.transform);
        var wp2 = MakeWaypoint("WP2", new Vector3( 7f, 0f, -7f), waypointsRoot.transform);
        var wp3 = MakeWaypoint("WP3", new Vector3(-7f, 0f, -7f), waypointsRoot.transform);

        // ── CELEBRITY ─────────────────────────────────────────────────────
        var celebRoot = new GameObject("[Celebrity]");

        var celeb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        celeb.name = "Celebrity";
        celeb.transform.SetParent(celebRoot.transform);
        celeb.transform.position = new Vector3(5f, 1f, 5f);

        var celebMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        celebMat.color = Color.yellow;
        celeb.GetComponent<Renderer>().sharedMaterial = celebMat;

        var navAgent = celeb.AddComponent<NavMeshAgent>();
        navAgent.speed = 2f;
        navAgent.stoppingDistance = 0.5f;

        var waypointPath = celeb.AddComponent<WaypointPath>();
        SetArrayField(waypointPath, "waypoints", new Transform[] { wp0, wp1, wp2, wp3 });

        var celebCtrl = celeb.AddComponent<CelebrityController>();
        SetField(celebCtrl, "waypointPath", waypointPath);
        SetField(celebCtrl, "rend", celeb.GetComponent<Renderer>());

        // Detection Zone (child sphere trigger)
        var detZoneGO = new GameObject("DetectionZone");
        detZoneGO.transform.SetParent(celeb.transform);
        detZoneGO.transform.localPosition = Vector3.zero;
        var detCol = detZoneGO.AddComponent<SphereCollider>();
        detCol.radius  = 8f;
        detCol.isTrigger = true;
        detZoneGO.AddComponent<DetectionZone>();

        // ── CROWD ─────────────────────────────────────────────────────────
        var crowdRoot = new GameObject("[Crowd]");
        var crowdMat  = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        crowdMat.color = new Color(0.55f, 0.55f, 0.55f);
        Vector3[] crowdPos = {
            new Vector3(-3f, 1f,  3f),
            new Vector3( 3f, 1f, -3f),
            new Vector3(-5f, 1f, -5f),
            new Vector3( 5f, 1f,  3f),
        };
        foreach (var pos in crowdPos)
        {
            var npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = "CrowdNPC";
            npc.transform.SetParent(crowdRoot.transform);
            npc.transform.position = pos;
            npc.GetComponent<Renderer>().sharedMaterial = crowdMat;
            var npcAgent = npc.AddComponent<NavMeshAgent>();
            npcAgent.speed = 1.5f;
            npc.AddComponent<CrowdNPC>();
        }

        // ── PLAYER ────────────────────────────────────────────────────────
        var playerGO = new GameObject("[Player]");
        playerGO.tag = "Player";
        playerGO.transform.position = new Vector3(0f, 0f, -8f);

        var cc = playerGO.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.center = new Vector3(0f, 1f, 0f);
        playerGO.AddComponent<PlayerController>();

        // Camera child (eye height)
        var cameraGO = new GameObject("PlayerCamera");
        cameraGO.transform.SetParent(playerGO.transform);
        cameraGO.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        cameraGO.transform.localRotation = Quaternion.identity;

        var cam = cameraGO.AddComponent<Camera>();
        cam.tag  = "MainCamera";
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane  = 100f;
        cameraGO.AddComponent<AudioListener>();

        var camCtrl = cameraGO.AddComponent<CameraController>();
        SetField(camCtrl, "playerBody", playerGO.transform);
        cameraGO.AddComponent<PhotoCamera>();

        // ── WIRE MANAGER REFERENCES ───────────────────────────────────────
        // Create MissionData ScriptableObject
        var missionData = ScriptableObject.CreateInstance<MissionData>();
        missionData.missionTitle   = "HOT SHOT";
        missionData.briefText      = "The A-lister is out today.\nGet the shot — don't get busted.";
        missionData.targetAction   = CelebrityAction.WavingAtFan;
        missionData.payoutAmount   = 500;
        missionData.levelSceneName = "SampleScene";
        const string assetPath = "Assets/Scripts/Core/MissionData_Level1.asset";
        AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.CreateAsset(missionData, assetPath);
        AssetDatabase.SaveAssets();
        var savedMission = AssetDatabase.LoadAssetAtPath<MissionData>(assetPath);

        SetField(missionMgr, "missionData",  savedMission);
        SetField(missionMgr, "celebrity",    celebCtrl);
        SetField(scorer,     "celebrity",    celebCtrl);

        // ── NAV MESH ──────────────────────────────────────────────────────
        var surface = floor.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.BuildNavMesh();

        // ── UI ────────────────────────────────────────────────────────────
        BuildUI(canvasGO: BuildCanvas(out var canvas));

        // ── SAVE SCENE ────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(
            EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[SceneBuilder] ✅ Scene built! Press Play to test.");
    }

    // ── UI BUILDER ────────────────────────────────────────────────────────

    static GameObject BuildCanvas(out Canvas canvas)
    {
        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<UnityEngine.EventSystems.EventSystem>();
        es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        var canvasGO = new GameObject("[UI]");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode       = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        return canvasGO;
    }

    static void BuildUI(GameObject canvasGO)
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // ── HUD ──────────────────────────────────────────────────────────
        var hudGO = UIEmpty("HUD", canvasGO.transform, Vector2.zero, Vector2.one);
        var hud   = hudGO.AddComponent<HUD>();

        // Crosshair — small white dot, center
        var crosshair = UIImage("Crosshair", hudGO.transform,
            anchor: new Vector2(0.5f, 0.5f), size: new Vector2(16f, 16f),
            offset: Vector2.zero, color: Color.white);

        // Viewfinder border group
        var vf = UIEmpty("ViewfinderBorder", hudGO.transform, Vector2.zero, Vector2.one);
        Color wb = new Color(1f, 1f, 1f, 0.85f);
        // Top-left corner
        UICorner("TL", vf.transform, new Vector2(0f, 1f), new Vector2( 1f,-1f), wb);
        UICorner("TR", vf.transform, new Vector2(1f, 1f), new Vector2(-1f,-1f), wb);
        UICorner("BL", vf.transform, new Vector2(0f, 0f), new Vector2( 1f, 1f), wb);
        UICorner("BR", vf.transform, new Vector2(1f, 0f), new Vector2(-1f, 1f), wb);
        vf.SetActive(false);

        // Objective text (top-left)
        var objText = UIText("ObjectiveText", hudGO.transform,
            anchorMin: new Vector2(0f, 1f), anchorMax: new Vector2(0f, 1f),
            size: new Vector2(700f, 36f), offset: new Vector2(360f, -24f),
            text: "OBJECTIVE: ...", fontSize: 18, color: Color.white, font: font);

        // Shutter flash (full-screen white Image + CanvasGroup)
        var flashGO = UIStretch("FlashOverlay", hudGO.transform);
        var flashImg = flashGO.AddComponent<Image>();
        flashImg.color = Color.white;
        var flashCG = flashGO.AddComponent<CanvasGroup>();
        flashCG.alpha          = 0f;
        flashCG.blocksRaycasts = false;

        SetField(hud, "crosshair",       crosshair);
        SetField(hud, "viewfinderBorder", vf);
        SetField(hud, "objectiveText",   objText.GetComponent<Text>());
        SetField(hud, "flashOverlay",    flashCG);

        // ── SUSPICION METER (bottom-left) ─────────────────────────────────
        var meter = UIEmpty("SuspicionMeter", canvasGO.transform,
            new Vector2(0f, 0f), new Vector2(0f, 0f));
        var meterRT = meter.GetComponent<RectTransform>();
        meterRT.anchoredPosition = new Vector2(230f, 30f);
        meterRT.sizeDelta        = new Vector2(400f, 28f);

        // Label to the left
        UIText("Label", meter.transform,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(120f, 28f), new Vector2(-68f, 0f),
            "SUSPICION", 14, Color.white, font);

        // Background bar
        var bgBar = UIImage("MeterBG", meter.transform,
            anchor: new Vector2(0.5f, 0.5f), size: new Vector2(400f, 28f),
            offset: Vector2.zero, color: new Color(0.1f, 0.1f, 0.1f));

        // Fill bar
        var fillGO = UIStretch("MeterFill", meter.transform);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color      = new Color(0.2f, 0.8f, 0.2f);
        fillImg.type       = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0f;

        var meterUI = meter.AddComponent<SuspicionMeterUI>();
        SetField(meterUI, "fillBar", fillImg);

        // ── MISSION BRIEF ─────────────────────────────────────────────────
        var briefPanel = UIStretch("MissionBriefPanel", canvasGO.transform);
        UIImage(briefPanel, new Color(0f, 0f, 0f, 0.88f));
        var briefUI = briefPanel.AddComponent<MissionBriefUI>();

        UIText("Title",   briefPanel.transform, CX, CX, new Vector2(700f, 70f),  new Vector2(0f,  140f), "HOT SHOT",             36, Color.yellow, font);
        UIText("Brief",   briefPanel.transform, CX, CX, new Vector2(700f, 80f),  new Vector2(0f,   50f), "...",                  20, Color.white,  font);
        UIText("Action",  briefPanel.transform, CX, CX, new Vector2(700f, 40f),  new Vector2(0f,  -30f), "TARGET ACTION: ...",   22, Color.cyan,   font);
        UIText("Payout",  briefPanel.transform, CX, CX, new Vector2(700f, 40f),  new Vector2(0f,  -80f), "PAYOUT: $500",         22, Color.green,  font);
        UIText("Prompt",  briefPanel.transform, CX, CX, new Vector2(700f, 30f),  new Vector2(0f, -160f), "Press SPACE or E to begin", 16, new Color(0.6f, 0.6f, 0.6f), font);

        SetField(briefUI, "panel",      briefPanel);
        SetField(briefUI, "titleText",  briefPanel.transform.Find("Title").GetComponent<Text>());
        SetField(briefUI, "briefText",  briefPanel.transform.Find("Brief").GetComponent<Text>());
        SetField(briefUI, "actionText", briefPanel.transform.Find("Action").GetComponent<Text>());
        SetField(briefUI, "payoutText", briefPanel.transform.Find("Payout").GetComponent<Text>());
        SetField(briefUI, "promptText", briefPanel.transform.Find("Prompt").GetComponent<Text>());

        // ── PHOTO RESULT ──────────────────────────────────────────────────
        var resultPanel = UIImage("PhotoResultPanel", canvasGO.transform,
            anchor: CX, size: new Vector2(520f, 320f), offset: Vector2.zero,
            color: new Color(0f, 0f, 0f, 0.82f));
        var resultUI = resultPanel.AddComponent<PhotoResultUI>();
        resultPanel.SetActive(false);

        UIText("Grade",       resultPanel.transform, CX, CX, new Vector2(480f, 70f),  new Vector2(0f,  100f), "MONEY SHOT",    42, Color.yellow, font);
        UIText("Score",       resultPanel.transform, CX, CX, new Vector2(480f, 40f),  new Vector2(0f,   40f), "Score: 0/100",  24, Color.white,  font);
        UIText("Payout",      resultPanel.transform, CX, CX, new Vector2(480f, 40f),  new Vector2(0f,   -5f), "Payout: $0",    24, Color.green,  font);
        UIText("ActionMatch", resultPanel.transform, CX, CX, new Vector2(480f, 40f),  new Vector2(0f,  -55f), "",              22, Color.cyan,   font);

        SetField(resultUI, "panel",           resultPanel);
        SetField(resultUI, "gradeText",       resultPanel.transform.Find("Grade").GetComponent<Text>());
        SetField(resultUI, "scoreText",       resultPanel.transform.Find("Score").GetComponent<Text>());
        SetField(resultUI, "payoutText",      resultPanel.transform.Find("Payout").GetComponent<Text>());
        SetField(resultUI, "actionMatchText", resultPanel.transform.Find("ActionMatch").GetComponent<Text>());

        // ── WIN / FAIL ────────────────────────────────────────────────────
        var winFailGO  = UIEmpty("WinFailUI", canvasGO.transform, Vector2.zero, Vector2.one);
        var winFailComp = winFailGO.AddComponent<WinFailUI>();

        var winPanel = UIStretch("WinPanel", winFailGO.transform);
        UIImage(winPanel, new Color(0f, 0.15f, 0f, 0.92f));
        UIText("Title",  winPanel.transform, CX, CX, new Vector2(600f, 90f), new Vector2(0f, 100f), "PUBLISHED!", 52, Color.green, font);
        UIText("Payout", winPanel.transform, CX, CX, new Vector2(600f, 45f), new Vector2(0f,  20f), "Total: $0",  28, Color.white, font);
        var winBtn = UIButton("PlayAgain", winPanel.transform, new Vector2(0f, -80f), "PLAY AGAIN", font);
        winPanel.SetActive(false);

        var failPanel = UIStretch("FailPanel", winFailGO.transform);
        UIImage(failPanel, new Color(0.2f, 0f, 0f, 0.92f));
        UIText("Title", failPanel.transform, CX, CX, new Vector2(600f, 90f), new Vector2(0f, 100f), "BUSTED!",   52, Color.red,   font);
        UIText("Hint",  failPanel.transform, CX, CX, new Vector2(600f, 40f), new Vector2(0f,  20f), "They saw you coming.", 24, Color.white, font);
        var failBtn = UIButton("PlayAgain", failPanel.transform, new Vector2(0f, -80f), "TRY AGAIN", font);
        failPanel.SetActive(false);

        SetField(winFailComp, "winPanel",           winPanel);
        SetField(winFailComp, "failPanel",          failPanel);
        SetField(winFailComp, "winPlayAgainButton",  winBtn.GetComponent<Button>());
        SetField(winFailComp, "failPlayAgainButton", failBtn.GetComponent<Button>());
    }

    // ── UI HELPERS ────────────────────────────────────────────────────────

    static readonly Vector2 CX = new Vector2(0.5f, 0.5f); // center anchor shorthand

    static GameObject UIEmpty(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }

    static GameObject UIStretch(string name, Transform parent)
        => UIEmpty(name, parent, Vector2.zero, Vector2.one);

    static void UIImage(GameObject go, Color color)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = color;
    }

    static GameObject UIImage(string name, Transform parent, Vector2 anchor,
        Vector2 size, Vector2 offset, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.sizeDelta        = size;
        rt.anchoredPosition = offset;
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static void UICorner(string id, Transform parent, Vector2 anchor, Vector2 dir, Color color)
    {
        // Horizontal bar
        var h = new GameObject($"Corner_{id}_H");
        h.transform.SetParent(parent, false);
        var rh = h.AddComponent<RectTransform>();
        rh.anchorMin = rh.anchorMax = anchor;
        rh.pivot     = new Vector2(anchor.x == 0 ? 0f : 1f, anchor.y == 0 ? 0f : 1f);
        rh.sizeDelta = new Vector2(50f, 5f);
        rh.anchoredPosition = new Vector2(dir.x * 6f, dir.y * 4f);
        h.AddComponent<Image>().color = color;

        // Vertical bar
        var v = new GameObject($"Corner_{id}_V");
        v.transform.SetParent(parent, false);
        var rv = v.AddComponent<RectTransform>();
        rv.anchorMin = rv.anchorMax = anchor;
        rv.pivot     = new Vector2(anchor.x == 0 ? 0f : 1f, anchor.y == 0 ? 0f : 1f);
        rv.sizeDelta = new Vector2(5f, 50f);
        rv.anchoredPosition = new Vector2(dir.x * 4f, dir.y * 6f);
        v.AddComponent<Image>().color = color;
    }

    static GameObject UIText(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 size, Vector2 offset,
        string text, int fontSize, Color color, Font font)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.sizeDelta        = size;
        rt.anchoredPosition = offset;
        var t = go.AddComponent<Text>();
        t.text      = text;
        t.fontSize  = fontSize;
        t.color     = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.font      = font;
        t.resizeTextForBestFit = false;
        t.horizontalOverflow   = HorizontalWrapMode.Overflow;
        t.verticalOverflow     = VerticalWrapMode.Overflow;
        return go;
    }

    static GameObject UIButton(string name, Transform parent, Vector2 offset, string label, Font font)
    {
        var go = UIImage(name, parent, CX, new Vector2(220f, 55f), offset, new Color(0.18f, 0.18f, 0.18f));
        go.AddComponent<Button>();
        UIText("Label", go.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
            label, 22, Color.white, font);
        var lr = go.transform.Find("Label").GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = lr.offsetMax = Vector2.zero;
        return go;
    }

    // ── SCENE HELPERS ─────────────────────────────────────────────────────

    static void MakeWall(string name, Vector3 pos, Vector3 scale, Transform parent)
    {
        var w = GameObject.CreatePrimitive(PrimitiveType.Cube);
        w.name = name;
        w.transform.SetParent(parent);
        w.transform.position   = pos;
        w.transform.localScale = scale;
        GameObjectUtility.SetStaticEditorFlags(w,
            StaticEditorFlags.NavigationStatic | StaticEditorFlags.ContributeGI);
    }

    static Transform MakeWaypoint(string name, Vector3 pos, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        return go.transform;
    }

    // ── SERIALIZED FIELD HELPERS ──────────────────────────────────────────

    static void SetField(Object target, string fieldName, object value)
    {
        var so   = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogWarning($"[SceneBuilder] Field '{fieldName}' not found on {target.GetType().Name}");
            return;
        }
        switch (value)
        {
            case Object   o: prop.objectReferenceValue = o;          break;
            case bool     b: prop.boolValue            = b;          break;
            case int      i: prop.intValue             = i;          break;
            case float    f: prop.floatValue           = f;          break;
            case string   s: prop.stringValue          = s;          break;
        }
        so.ApplyModifiedProperties();
    }

    static void SetArrayField(Object target, string fieldName, Transform[] values)
    {
        var so   = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogWarning($"[SceneBuilder] Array field '{fieldName}' not found on {target.GetType().Name}");
            return;
        }
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedProperties();
    }
}
