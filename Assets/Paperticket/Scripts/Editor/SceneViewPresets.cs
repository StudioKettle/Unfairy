using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class SceneViewPresets {
    [System.Serializable]
    public class Preset {

        public string name;
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 pivot;
        public float size;
        public bool twoD;
        public bool ortho;
        public bool isEditing { get; set; }
        public Preset Save( SceneView aView ) {
            pos = aView.camera.transform.position;
            pivot = aView.pivot;
            rot = aView.rotation.eulerAngles;
            size = aView.size;
            twoD = aView.in2DMode;
            ortho = aView.orthographic;
            return this;
        }
        public void Restore( SceneView aView ) {
            aView.in2DMode = twoD;
            aView.LookAt(pivot, Quaternion.Euler(rot), size, ortho);
        }
    }
    [System.Serializable]
    public class Settings
    {

        public List<Preset> presets = new List<Preset>();
        public bool showPresets = true;
        public bool expanded = false;
        public Rect winPos = new Rect(10, 10, 70, 46);
        public bool showCones = true;
        public bool enableMoving { get; set; }
    }
    static Color[] _colors = new Color[] { Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow };
    static SceneViewPresets _instance;
    static SceneViewPresets() {
        _instance = new SceneViewPresets();
    }
    [MenuItem("Tools/Toggle SceneView Presets")]
    public static void ToggleControls() {
        _instance.settings.showPresets = !_instance.settings.showPresets;
        SceneView.RepaintAll();
    }

    public SceneViewPresets() {
#if UNITY_2019_1_OR_NEWER
        SceneView.duringSceneGui += OnSceneView;
#else
    // In the past we had to use the "onSceneGUIDelegate" event
    SceneView.onSceneGUIDelegate += OnSceneView;
#endif
    }

    bool _initialized = false;
    Vector2 _scrollPos;
    SceneView _sceneView;
    GUIStyle _buttonWordWrap = null;
    public Settings settings = new Settings();

    public void Initialize() {
        var data = EditorPrefs.GetString("B83.SceneViewPresets", "");
        if (data != "")
            settings = JsonUtility.FromJson<Settings>(data);
        _initialized = true;
    }
    public void SaveSettings() {
        EditorPrefs.SetString("B83.SceneViewPresets", JsonUtility.ToJson(settings));
    }

    void OnSceneView( SceneView aView ) {
        if (!_initialized)
            Initialize();
        _sceneView = aView;
        Event e = Event.current;
        GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);

        if (e.type == EventType.KeyUp && e.alt && e.control && e.keyCode == KeyCode.P) {
            settings.showPresets = !settings.showPresets;
            e.Use();
        }
        if (settings.showPresets) {
            var tmpRect = settings.winPos;
            if (settings.expanded && settings.enableMoving) {
                tmpRect.width += 100;
            }
            var title = new GUIContent(" Presets", EditorGUIUtility.IconContent("d_SceneViewCamera").image);
            var newPos = GUILayout.Window(8301, tmpRect, OnWindow, title);
            if (newPos != tmpRect)
                SaveSettings();
            settings.winPos.x = Mathf.Clamp(newPos.x, 15 - newPos.width, aView.position.width - 15);
            settings.winPos.y = Mathf.Clamp(newPos.y, 21, aView.position.height - 15);
            if (settings.showCones && e.keyCode == KeyCode.PageUp) {
                int colorCount = 0;
                foreach (var p in settings.presets) {
                    var c = _colors[colorCount++];
                    colorCount = colorCount % _colors.Length;
                    c.a = 1f;
                    Handles.color = c;
                    var q = Quaternion.Euler(p.rot);
                    var dir = q * Vector3.forward;

                    q = Quaternion.LookRotation(-dir, Vector3.up);
                    bool enableCone = !p.ortho && !p.twoD && (_sceneView.camera.transform.position - p.pos).sqrMagnitude > 2;
                    if (enableCone && Handles.Button(p.pos - q * Vector3.forward * 0.7f, q, 1, 0.5f, Handles.ConeHandleCap))
                        p.Restore(_sceneView);
                    if (Handles.Button(p.pivot, Quaternion.identity, HandleUtility.GetHandleSize(p.pivot) * 0.1f, 0.25f, Handles.DotHandleCap))
                        p.Restore(_sceneView);
                }
            }
        }
    }

    void OnWindow( int id ) {
        if (_buttonWordWrap == null) {
            _buttonWordWrap = new GUIStyle("button") {
                wordWrap = true
            };
        }
        Event e = Event.current;

        GUILayout.BeginHorizontal();
        for (int i = 0; i < Mathf.Min(settings.presets.Count, 5); i++) {
            var p = settings.presets[i];
            var buttonStyle = new GUIStyle("button");
            if (p.pos == _sceneView.camera.transform.position) {
                buttonStyle.normal.textColor = Color.green;
            }
            if (!p.isEditing && GUILayout.Button((i + 1).ToString(), buttonStyle, GUILayout.Width(24))) {
                if (e.button == 0) {
                    p.Restore(_sceneView);
                } else if (e.button == 1) {
                    ShowContextMenu(p);
                }
            }
        }

        var toggle = GUILayout.Toggle(settings.expanded, settings.expanded ? "Collapse" : "Expand", "Button");

        if (toggle != settings.expanded && toggle) {
            settings.expanded = true;
            settings.winPos.size = new Vector2(200, 46);
            SaveSettings();
        } else if (toggle != settings.expanded && !toggle) {
            settings.expanded = false;
            settings.winPos.size = new Vector2(80, 46);
            SaveSettings();
        }
        GUILayout.EndHorizontal();

        if (settings.expanded) {
            _scrollPos = GUILayout.BeginScrollView(
                  _scrollPos,
                  GUIStyle.none,
                  GUIStyle.none,
                  GUILayout.MinHeight(settings.presets.Count * 24),
                  GUILayout.MaxHeight(300)
              );
            int colorCount = 0;
            for (int i = 0; i < settings.presets.Count; i++) {
                var p = settings.presets[i];
                GUI.color = Color.white;
                GUILayout.BeginHorizontal();
                if (p.isEditing) {
                    p.name = EditorGUILayout.TextArea(p.name);
                    if (GUILayout.Button("Done", GUILayout.Width(50))) {
                        p.isEditing = false;
                        SaveSettings();
                    }
                    GUI.color = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(20))) {
                        settings.presets.Remove(p);
                        SaveSettings();
                        GUIUtility.ExitGUI();
                    }
                    GUI.color = Color.white;
                } else {
                    if (e.keyCode == KeyCode.PageUp) {
                        GUI.color = _colors[colorCount++];
                        colorCount = colorCount % _colors.Length;
                    }

                    var buttonStyle = new GUIStyle(_buttonWordWrap);
                    if (p.pos == _sceneView.camera.transform.position) {
                        buttonStyle.normal.textColor = Color.green;
                    }
                    if (GUILayout.Button(p.name, buttonStyle)) {
                        if (e.button == 0)
                            p.Restore(_sceneView);
                        else if (e.button == 1)
                            ShowContextMenu(p);
                    }
                }
                if (settings.enableMoving) {
                    GUI.enabled = i > 0;
                    if (GUILayout.Button("Up", GUILayout.Width(30))) {
                        var temp = settings.presets[i - 1];
                        settings.presets[i - 1] = p;
                        settings.presets[i] = temp;
                        SaveSettings();
                    }
                    GUI.enabled = i < settings.presets.Count - 1;
                    if (GUILayout.Button("Down", GUILayout.Width(50))) {
                        var temp = settings.presets[i + 1];
                        settings.presets[i + 1] = p;
                        settings.presets[i] = temp;
                        SaveSettings();
                    }
                    GUI.enabled = true;
                    GUI.color = Color.red;
                    if (GUILayout.Button("X", GUILayout.Width(20))) {
                        settings.presets.Remove(p);
                        SaveSettings();
                        GUIUtility.ExitGUI();
                    }
                    GUI.color = Color.white;

                }
                GUILayout.EndHorizontal();
            }
            GUI.color = Color.white;
            GUILayout.EndScrollView();
            if (settings.showCones) {
                GUILayout.Label("Hold CTRL to vizualize");
            }
            GUILayout.BeginHorizontal();
            var buttonLabel = new GUIContent("New Preset", EditorGUIUtility.IconContent("d_Toolbar Plus").image);
            if (GUILayout.Button(buttonLabel) && e.button == 0) {
                settings.presets.Add(new Preset { name = $"Position {settings.presets.Count + 1}", isEditing = true }.Save(_sceneView));
                SaveSettings();
            }
            if (settings.enableMoving && GUILayout.Button("Done editing", GUILayout.Width(120))) {
                settings.enableMoving = false;
            }
            GUILayout.EndHorizontal();
        } else {

        }
        GUI.DragWindow();
    }

    void ShowContextMenu( Preset aPreset ) {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Save current position"), false, () => {
            aPreset.Save(_sceneView);
            SaveSettings();
        });
        menu.AddItem(new GUIContent("Rename preset"), false, () => aPreset.isEditing = true);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Edit all presets"), false, () => settings.enableMoving = true);
        menu.AddItem(new GUIContent("Show camera cones"), settings.showCones, () => { settings.showCones = !settings.showCones; SaveSettings(); });
        menu.AddItem(new GUIContent("Settings/Export to file"), false, () => {
            var file = EditorUtility.SaveFilePanel("SceneViewPresetSettings", "", "SceneViewPresets", "json");
            if (!string.IsNullOrEmpty(file))
                System.IO.File.WriteAllText(file, JsonUtility.ToJson(settings));
        });
        menu.AddItem(new GUIContent("Settings/Import from file"), false, () => {
            var file = EditorUtility.OpenFilePanel("SceneViewPresetSettings", "", "json");
            if (string.IsNullOrEmpty(file) || !System.IO.File.Exists(file))
                return;
            var newSettings = JsonUtility.FromJson<Settings>(System.IO.File.ReadAllText(file));
            if (newSettings == null) {
                Debug.LogError("Settings file could not be loaded. Error in the file");
                return;
            }
            settings = newSettings;
            SaveSettings();
        });
        menu.AddItem(new GUIContent("Settings/Import Presets from file (keep current presets)"), false, () => {
            var file = EditorUtility.OpenFilePanel("SceneViewPresetSettings", "", "json");
            if (string.IsNullOrEmpty(file) || !System.IO.File.Exists(file))
                return;
            var newSettings = JsonUtility.FromJson<Settings>(System.IO.File.ReadAllText(file));
            if (newSettings == null) {
                Debug.LogError("Settings file could not be loaded. Error in the file");
                return;
            }
            settings.presets.AddRange(newSettings.presets);
            SaveSettings();
        });
        menu.AddItem(new GUIContent("About/Created by Bunny83"), false, null);
        menu.AddSeparator("About/");
        menu.AddItem(new GUIContent("About/For more information see"), false, null);
        menu.AddItem(new GUIContent("About/UnityAnswers"), false, () => Application.OpenURL("https://answers.unity.com/questions/1515748/how-to-save-scene-view-camera-perspectives.html"));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Preset"), false, () => {
            settings.presets.Remove(aPreset);
            SaveSettings();
        });
        menu.ShowAsContext();
    }
}