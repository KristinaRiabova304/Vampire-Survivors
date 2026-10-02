using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class GameBuilder
{
    const string Root = "Assets/Game";
    const string SpritesDir = Root + "/Sprites";
    const string PrefabsDir = Root + "/Prefabs";
    const string ScenePath = Root + "/Scenes/Game.unity";

    static Font font;

    [MenuItem("Tools/Survivors/Build Game")]
    public static void Build()
    {
        foreach (var dir in new[] { SpritesDir, PrefabsDir, Root + "/Scenes" })
            Directory.CreateDirectory(dir);

        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Sprite square = MakeSprite("Square", 32, (x, y, s) => 1f);
        Sprite circle = MakeSprite("Circle", 64, CircleAlpha);
        Sprite ground = MakeSprite("GroundTile", 64, (x, y, s) => 1f, grid: true);

        Projectile projectile = MakeProjectile(circle);
        XPPickup xp = MakeXP(square);
        ParticleSystem deathFx = MakeDeathParticles();

        Enemy normal = MakeEnemy("Enemy_Normal", circle, new Color(0.9f, 0.25f, 0.25f), 0.8f, speed: 2f, hp: 3, dmg: 1, score: 1, xpValue: 1);
        Enemy fast = MakeEnemy("Enemy_Fast", circle, new Color(1f, 0.85f, 0.2f), 0.55f, speed: 3.6f, hp: 1, dmg: 1, score: 2, xpValue: 1);
        Enemy tank = MakeEnemy("Enemy_Tank", circle, new Color(0.6f, 0.3f, 0.9f), 1.4f, speed: 1.1f, hp: 12, dmg: 2, score: 5, xpValue: 3);

        var groundGo = new GameObject("Ground");
        var gsr = groundGo.AddComponent<SpriteRenderer>();
        gsr.sprite = ground;
        gsr.drawMode = SpriteDrawMode.Tiled;
        gsr.size = new Vector2(64, 64);
        gsr.color = new Color(0.16f, 0.2f, 0.18f);
        gsr.sortingOrder = -10;

        var player = new GameObject("Player");
        player.tag = "Player";
        var psr = player.AddComponent<SpriteRenderer>();
        psr.sprite = square;
        psr.color = new Color(0.3f, 0.75f, 1f);
        psr.sortingOrder = 5;
        var prb = player.AddComponent<Rigidbody2D>();
        prb.gravityScale = 0f;
        prb.freezeRotation = true;
        prb.mass = 5f;
        prb.interpolation = RigidbodyInterpolation2D.Interpolate;
        prb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        player.AddComponent<BoxCollider2D>();
        var playerHealth = player.AddComponent<Health>();
        SetInt(playerHealth, "maxHP", 10);
        SetColor(playerHealth, "flashColor", new Color(1f, 0.2f, 0.2f));
        var controller = player.AddComponent<PlayerController>();
        var level = player.AddComponent<PlayerLevel>();
        var weapon = player.AddComponent<AutoWeapon>();

        var poolGo = new GameObject("ProjectilePool");
        var pool = poolGo.AddComponent<ProjectilePool>();
        SetRef(pool, "prefab", projectile);
        SetRef(weapon, "pool", pool);

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(0, 0, -10);
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.09f, 0.1f);
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<AudioSource>();
        camGo.AddComponent<SoundFX>();
        SetRef(camGo.AddComponent<CameraFollow>(), "target", player.transform);

        var spawner = new GameObject("EnemySpawner").AddComponent<EnemySpawner>();
        var so = new SerializedObject(spawner);
        var types = so.FindProperty("enemyTypes");
        types.arraySize = 3;
        SetType(types.GetArrayElementAtIndex(0), normal, 1f, 1f);
        SetType(types.GetArrayElementAtIndex(1), fast, 0.15f, 0.9f);
        SetType(types.GetArrayElementAtIndex(2), tank, 0.05f, 0.6f);
        so.ApplyModifiedPropertiesWithoutUndo();

        var systems = new GameObject("Systems");
        SetRef(systems.AddComponent<EnemyDeathEffects>(), "deathParticlesPrefab", deathFx);
        SetRef(systems.AddComponent<XPDropper>(), "xpPrefab", xp);

        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        var canvas = canvasGo.transform;

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var hpBar = MakeBar(canvas, "HP Bar", new Vector2(30, -30), new Vector2(420, 32), new Color(0.85f, 0.2f, 0.2f));
        MakeText(hpBar.transform, "Label", "HP", 22, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, stretch: true);
        var xpBar = MakeBar(canvas, "XP Bar", new Vector2(30, -72), new Vector2(420, 20), new Color(0.3f, 0.6f, 1f));
        var levelText = MakeText(canvas, "Level", "LV 1", 30, TextAnchor.MiddleLeft, new Vector2(465, -82), new Vector2(150, 40), anchor: new Vector2(0, 1));
        var timerText = MakeText(canvas, "Timer", "00:00", 52, TextAnchor.MiddleCenter, new Vector2(0, -50), new Vector2(300, 70), anchor: new Vector2(0.5f, 1));
        var scoreText = MakeText(canvas, "Score", "Score: 0", 30, TextAnchor.UpperRight, new Vector2(-30, -25), new Vector2(400, 90), anchor: new Vector2(1, 1));
        MakeText(canvas, "Hint", "WASD / Arrows - move. The weapon fires automatically.", 22, TextAnchor.MiddleCenter, new Vector2(0, 30), new Vector2(900, 40), anchor: new Vector2(0.5f, 0));

        var levelUpPanel = MakePanel(canvas, "LevelUpPanel");
        MakeText(levelUpPanel.transform, "Title", "LEVEL UP!", 72, TextAnchor.MiddleCenter, new Vector2(0, 180), new Vector2(800, 100));
        MakeText(levelUpPanel.transform, "Subtitle", "Choose an upgrade", 30, TextAnchor.MiddleCenter, new Vector2(0, 100), new Vector2(800, 50));
        var dmgBtn = MakeButton(levelUpPanel.transform, "+Damage", new Vector2(-340, -40), new Color(0.85f, 0.3f, 0.3f));
        var atkBtn = MakeButton(levelUpPanel.transform, "+Attack Speed", new Vector2(0, -40), new Color(0.95f, 0.7f, 0.2f));
        var movBtn = MakeButton(levelUpPanel.transform, "+Movement Speed", new Vector2(340, -40), new Color(0.3f, 0.7f, 0.95f));

        var gameOverPanel = MakePanel(canvas, "GameOverPanel");
        MakeText(gameOverPanel.transform, "Title", "GAME OVER", 90, TextAnchor.MiddleCenter, new Vector2(0, 170), new Vector2(900, 120));
        var statsText = MakeText(gameOverPanel.transform, "Stats", "", 36, TextAnchor.MiddleCenter, new Vector2(0, 40), new Vector2(900, 120));
        var restartBtn = MakeButton(gameOverPanel.transform, "Restart", new Vector2(0, -110), new Color(0.35f, 0.8f, 0.4f));
        MakeText(gameOverPanel.transform, "Hint", "or press R", 24, TextAnchor.MiddleCenter, new Vector2(0, -200), new Vector2(400, 40));

        var gm = new GameObject("GameManager").AddComponent<GameManager>();
        SetRef(gm, "playerHealth", playerHealth);
        SetRef(gm, "playerController", controller);
        SetRef(gm, "playerWeapon", weapon);
        SetRef(gm, "playerLevel", level);
        SetRef(gm, "hpBar", hpBar);
        SetRef(gm, "xpBar", xpBar);
        SetRef(gm, "scoreText", scoreText);
        SetRef(gm, "timerText", timerText);
        SetRef(gm, "levelText", levelText);
        SetRef(gm, "gameOverPanel", gameOverPanel);
        SetRef(gm, "gameOverStatsText", statsText);
        SetRef(gm, "restartButton", restartBtn);
        SetRef(gm, "levelUpPanel", levelUpPanel);
        SetRef(gm, "damageButton", dmgBtn);
        SetRef(gm, "attackSpeedButton", atkBtn);
        SetRef(gm, "moveSpeedButton", movBtn);

        levelUpPanel.SetActive(false);
        gameOverPanel.SetActive(false);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("[GameBuilder] Done: " + ScenePath);
    }

    static Enemy MakeEnemy(string name, Sprite sprite, Color color, float scale, float speed, int hp, int dmg, int score, int xpValue)
    {
        var go = new GameObject(name);
        go.transform.localScale = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = 3;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        go.AddComponent<CircleCollider2D>().radius = 0.5f;
        var health = go.AddComponent<Health>();
        SetInt(health, "maxHP", hp);
        var enemy = go.AddComponent<Enemy>();
        SetFloat(enemy, "moveSpeed", speed);
        SetInt(enemy, "contactDamage", dmg);
        SetInt(enemy, "scoreValue", score);
        SetInt(enemy, "xpValue", xpValue);
        return SavePrefab(go).GetComponent<Enemy>();
    }

    static Projectile MakeProjectile(Sprite sprite)
    {
        var go = new GameObject("Projectile");
        go.transform.localScale = Vector3.one * 0.3f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1f, 0.95f, 0.6f);
        sr.sortingOrder = 6;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;
        var trail = go.AddComponent<TrailRenderer>();
        trail.time = 0.12f;
        trail.startWidth = 0.15f;
        trail.endWidth = 0f;
        trail.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        trail.startColor = new Color(1f, 0.9f, 0.5f, 0.8f);
        trail.endColor = new Color(1f, 0.9f, 0.5f, 0f);
        trail.sortingOrder = 5;
        go.AddComponent<Projectile>();
        return SavePrefab(go).GetComponent<Projectile>();
    }

    static XPPickup MakeXP(Sprite sprite)
    {
        var go = new GameObject("XPPickup");
        go.transform.localScale = Vector3.one * 0.3f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.3f, 1f, 0.5f);
        sr.sortingOrder = 2;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.8f;
        go.AddComponent<XPPickup>();
        return SavePrefab(go).GetComponent<XPPickup>();
    }

    static ParticleSystem MakeDeathParticles()
    {
        var go = new GameObject("EnemyDeathFX");
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.6f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        var r = go.GetComponent<ParticleSystemRenderer>();
        var mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
        r.sharedMaterial = mat != null ? mat : AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        r.sortingOrder = 10;

        return SavePrefab(go).GetComponent<ParticleSystem>();
    }

    static GameObject SavePrefab(GameObject go)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabsDir}/{go.name}.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static float CircleAlpha(int x, int y, int size)
    {
        float r = size / 2f;
        float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
        return Mathf.Clamp01(r - d);
    }

    static Sprite MakeSprite(string name, int size, System.Func<int, int, int, float> alpha, bool grid = false)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float v = grid && x >= 2 && y >= 2 ? 0.7f : 1f;
            tex.SetPixel(x, y, new Color(v, v, v, alpha(x, y, size)));
        }

        string path = $"{SpritesDir}/{name}.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = size;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = grid ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Text MakeText(Transform parent, string name, string text, int fontSize, TextAnchor align,
                         Vector2 pos, Vector2 size, Vector2? anchor = null, bool stretch = false)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        if (stretch)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
        else
        {
            Vector2 a = anchor ?? new Vector2(0.5f, 0.5f);
            rt.anchorMin = rt.anchorMax = rt.pivot = a;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }
        var t = go.AddComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = fontSize;
        t.alignment = align;
        t.color = Color.white;
        t.raycastTarget = false;
        go.AddComponent<Outline>().effectColor = new Color(0, 0, 0, 0.8f);
        return t;
    }

    static UIBar MakeBar(Transform parent, string name, Vector2 pos, Vector2 size, Color fillColor)
    {
        var bg = new GameObject(name, typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(parent, false);
        var rt = (RectTransform)bg.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(bg.transform, false);
        var frt = (RectTransform)fill.transform;
        frt.anchorMin = Vector2.zero;
        frt.anchorMax = Vector2.one;
        frt.offsetMin = new Vector2(3, 3);
        frt.offsetMax = new Vector2(-3, -3);
        fill.GetComponent<Image>().color = fillColor;

        var bar = bg.AddComponent<UIBar>();
        SetRef(bar, "fill", frt);
        return bar;
    }

    static GameObject MakePanel(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);
        return go;
    }

    static Button MakeButton(Transform parent, string label, Vector2 pos, Color color)
    {
        var go = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(300, 110);
        go.GetComponent<Image>().color = color;
        MakeText(go.transform, "Text", label, 34, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, stretch: true);
        return go.GetComponent<Button>();
    }

    static void SetRef(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetInt(Object target, string field, int value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).intValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetFloat(Object target, string field, float value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetColor(Object target, string field, Color value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).colorValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetType(SerializedProperty element, Enemy prefab, float startWeight, float endWeight)
    {
        element.FindPropertyRelative("prefab").objectReferenceValue = prefab;
        element.FindPropertyRelative("startWeight").floatValue = startWeight;
        element.FindPropertyRelative("endWeight").floatValue = endWeight;
    }
}
