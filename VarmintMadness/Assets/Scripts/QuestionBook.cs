#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// QuestionBook — Custom Editor Window for managing all QuestionData assets.
///
/// Open via the Unity menu:  GameShow → Question Book
///
/// Features:
///   • Lists every QuestionData asset in the project
///   • Inline editing: question text, all 4 answer texts, correct-answer toggle
///   • Per-question point override
///   • "New Question" button creates a named asset in Assets/Questions/
///   • "Delete" button removes the asset after confirmation
///   • Highlights questions that have no correct answer set (red) or multiple (yellow)
/// </summary>
public class QuestionBook : EditorWindow
{
    // ─────────────────────────────────────────────────────────────────────────
    //  STATE
    // ─────────────────────────────────────────────────────────────────────────

    private List<QuestionData> questions = new();
    private Vector2 scrollPos;
    private int expandedIndex = -1;

    private const string SAVE_FOLDER = "Assets/Questions";

    // ─────────────────────────────────────────────────────────────────────────
    //  MENU ITEM
    // ─────────────────────────────────────────────────────────────────────────

    [MenuItem("GameShow/Question Book")]
    public static void OpenWindow()
    {
        var window = GetWindow<QuestionBook>("Question Book");
        window.minSize = new Vector2(480, 400);
        window.Refresh();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  GUI
    // ─────────────────────────────────────────────────────────────────────────

    private void OnGUI()
    {
        DrawToolbar();
        DrawQuestionList();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("＋ New Question", EditorStyles.toolbarButton, GUILayout.Width(130)))
            CreateNewQuestion();

        if (GUILayout.Button("⟳ Refresh", EditorStyles.toolbarButton, GUILayout.Width(80)))
            Refresh();

        GUILayout.FlexibleSpace();
        GUILayout.Label($"{questions.Count} question(s)", EditorStyles.miniLabel);

        EditorGUILayout.EndHorizontal();
    }

    private void DrawQuestionList()
    {
        if (questions.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "No QuestionData assets found in this project.\n" +
                "Click '＋ New Question' to create your first one, or right-click in the " +
                "Project window → Create → GameShow → Question.",
                MessageType.Info);
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int i = 0; i < questions.Count; i++)
            DrawQuestion(i);

        EditorGUILayout.EndScrollView();
    }

    private void DrawQuestion(int index)
    {
        QuestionData q = questions[index];
        if (q == null) return;

        // ── Validation colour ────────────────────────────────────────────────
        int correctCount = 0;
        foreach (var a in q.answers)
            if (a.isCorrect) correctCount++;

        Color rowBg = correctCount == 0 ? new Color(0.9f, 0.3f, 0.3f, 0.25f)
                    : correctCount > 1  ? new Color(0.9f, 0.8f, 0.1f, 0.25f)
                    : Color.clear;

        Rect rowRect = EditorGUILayout.BeginVertical();

        // Tint background
        if (rowBg != Color.clear)
            EditorGUI.DrawRect(rowRect, rowBg);

        // ── Header / foldout ─────────────────────────────────────────────────
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        bool isExpanded = (expandedIndex == index);
        string arrow = isExpanded ? "▾" : "▸";
        string label = string.IsNullOrWhiteSpace(q.questionText)
            ? $"[{index + 1}] (no question text)"
            : $"[{index + 1}] {q.questionText.Substring(0, Mathf.Min(q.questionText.Length, 60))}";

        if (GUILayout.Button($"{arrow} {label}", EditorStyles.foldout, GUILayout.ExpandWidth(true)))
            expandedIndex = isExpanded ? -1 : index;

        // Warn badge
        if (correctCount == 0)
            GUILayout.Label("⚠ No correct answer", EditorStyles.miniLabel, GUILayout.Width(130));
        else if (correctCount > 1)
            GUILayout.Label("⚠ Multiple correct", EditorStyles.miniLabel, GUILayout.Width(130));

        // Delete button
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("✕", GUILayout.Width(26)))
        {
            if (EditorUtility.DisplayDialog("Delete Question",
                    $"Delete \"{q.name}\"? This cannot be undone.", "Delete", "Cancel"))
            {
                string path = AssetDatabase.GetAssetPath(q);
                AssetDatabase.DeleteAsset(path);
                Refresh();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        // ── Expanded editor ──────────────────────────────────────────────────
        if (isExpanded)
        {
            EditorGUI.indentLevel++;
            SerializedObject so = new SerializedObject(q);
            so.Update();

            // Asset name
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Asset Name");
            string newName = EditorGUILayout.DelayedTextField(q.name);
            if (newName != q.name)
            {
                string assetPath = AssetDatabase.GetAssetPath(q);
                AssetDatabase.RenameAsset(assetPath, newName);
                AssetDatabase.SaveAssets();
            }
            EditorGUILayout.EndHorizontal();

            // Question text
            EditorGUILayout.PropertyField(so.FindProperty("questionText"),
                new GUIContent("Question Text"));

            // Override points
            EditorGUILayout.PropertyField(so.FindProperty("overridePoints"),
                new GUIContent("Points Override (0 = use default)"));

            // Answers
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Answers", EditorStyles.boldLabel);

            SerializedProperty answersProp = so.FindProperty("answers");
            for (int ai = 0; ai < answersProp.arraySize; ai++)
            {
                SerializedProperty entry = answersProp.GetArrayElementAtIndex(ai);
                SerializedProperty textProp    = entry.FindPropertyRelative("answerText");
                SerializedProperty correctProp = entry.FindPropertyRelative("isCorrect");

                EditorGUILayout.BeginHorizontal();

                // Correct toggle as a styled checkbox
                GUIContent checkLabel = new GUIContent(
                    correctProp.boolValue ? "✔ Correct" : "    Wrong",
                    "Toggle whether this is the correct answer");

                Color origColor = GUI.color;
                GUI.color = correctProp.boolValue ? Color.green : Color.white;
                correctProp.boolValue = EditorGUILayout.Toggle(correctProp.boolValue, GUILayout.Width(16));
                EditorGUILayout.LabelField(correctProp.boolValue ? "Correct ✔" : "Wrong", GUILayout.Width(72));
                GUI.color = origColor;

                // Answer text
                EditorGUILayout.PropertyField(textProp, GUIContent.none);

                EditorGUILayout.EndHorizontal();
            }

            so.ApplyModifiedProperties();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(6);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    private void Refresh()
    {
        questions.Clear();
        string[] guids = AssetDatabase.FindAssets("t:QuestionData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var q = AssetDatabase.LoadAssetAtPath<QuestionData>(path);
            if (q != null) questions.Add(q);
        }
        Repaint();
    }

    private void CreateNewQuestion()
    {
        if (!AssetDatabase.IsValidFolder(SAVE_FOLDER))
        {
            System.IO.Directory.CreateDirectory(SAVE_FOLDER);
            AssetDatabase.Refresh();
        }

        QuestionData q = ScriptableObject.CreateInstance<QuestionData>();
        string path = AssetDatabase.GenerateUniqueAssetPath($"{SAVE_FOLDER}/New Question.asset");
        AssetDatabase.CreateAsset(q, path);
        AssetDatabase.SaveAssets();
        Refresh();

        // Auto-expand the new entry
        expandedIndex = questions.IndexOf(AssetDatabase.LoadAssetAtPath<QuestionData>(path));
        Repaint();
    }
}
#endif
