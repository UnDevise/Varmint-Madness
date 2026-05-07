using UnityEngine;

/// <summary>
/// QuestionData — ScriptableObject that represents one game-show question.
///
/// HOW TO CREATE:
///   Right-click in the Project window → Create → GameShow → Question
///
/// Fill in the question text, up to four answers, and mark exactly one as correct.
/// Leave overridePoints at 0 to use the GameShowManager's default points value.
/// </summary>
[CreateAssetMenu(fileName = "New Question", menuName = "GameShow/Question", order = 1)]
public class QuestionData : ScriptableObject
{
    [Header("── Question ──")]
    [TextArea(2, 6)]
    [Tooltip("The full question text that will be read aloud (typed) by the host.")]
    public string questionText = "What is the capital of France?";

    [Header("── Answers ──")]
    [Tooltip("Provide 2–4 answer choices. Exactly ONE should have isCorrect = true.")]
    public AnswerEntry[] answers = new AnswerEntry[4]
    {
        new AnswerEntry { answerText = "Paris",  isCorrect = true  },
        new AnswerEntry { answerText = "London", isCorrect = false },
        new AnswerEntry { answerText = "Berlin", isCorrect = false },
        new AnswerEntry { answerText = "Rome",   isCorrect = false }
    };

    [Header("── Scoring ──")]
    [Tooltip("Point value for a correct answer. Set to 0 to use the global default " +
             "defined in GameShowManager.defaultPointsPerQuestion.")]
    public int overridePoints = 0;

    // ─────────────────────────────────────────────────────────────────────────
    //  EDITOR VALIDATION
    // ─────────────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnValidate()
    {
        int correctCount = 0;
        foreach (var a in answers)
            if (a.isCorrect) correctCount++;

        if (correctCount == 0)
            Debug.LogWarning($"[QuestionData] \"{name}\": No answer is marked as correct!");
        else if (correctCount > 1)
            Debug.LogWarning($"[QuestionData] \"{name}\": More than one answer is marked correct. " +
                             "Only the first correct answer will be used.");
    }
#endif
}

// ─────────────────────────────────────────────────────────────────────────────
//  ANSWER ENTRY  (nested struct shown inline in the Inspector)
// ─────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public class AnswerEntry
{
    [Tooltip("The text shown on the answer button.")]
    public string answerText = "Answer";

    [Tooltip("Check this box if this answer is the correct one. " +
             "Only check ONE answer per question.")]
    public bool isCorrect = false;
}
