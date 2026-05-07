using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GameShowManager — Master controller for the 4-player local-multiplayer game show.
/// Attach this to a dedicated "GameManager" GameObject in the scene.
/// </summary>
public class GameShowManager : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    //  INSPECTOR-EXPOSED SETTINGS  (all host-adjustable in the Inspector)
    // ─────────────────────────────────────────────────────────────────────────

    [Header("── Text Speed ──")]
    [Tooltip("Characters revealed per second in the host dialogue box.")]
    [Range(5f, 200f)]
    public float textRevealSpeed = 40f;

    [Header("── Player Settings ──")]
    [Tooltip("Walking speed of every player (units per second).")]
    [Range(1f, 20f)]
    public float playerWalkSpeed = 4f;

    [Header("── Scoring ──")]
    [Tooltip("Base points awarded for a correct answer. " +
             "Individual questions can override this via QuestionData.")]
    public int defaultPointsPerQuestion = 100;

    [Header("── Scene References ──")]
    public PlayerController[] players;          // Drag the 4 player GameObjects here
    public Transform[] playerWaypoints;         // One waypoint per player (question spot)
    public Transform waitingArea;               // Where players stand before their turn

    [Header("── UI References ──")]
    public TextMeshProUGUI hostDialogueText;    // The label where the host speaks
    public GameObject answerPanel;              // Parent panel that holds the 4 answer buttons
    public Button[] answerButtons;              // Exactly 4 buttons
    public TextMeshProUGUI[] answerButtonTexts; // TMP text on each button
    public GameObject introPanel;               // Optional "Welcome" overlay
    public TextMeshProUGUI introText;

    [Header("── Question Bank ──")]
    public QuestionData[] questions;            // Assign via Inspector or QuestionBook

    [Header("── Greeting ──")]
    [TextArea(2, 6)]
    public string greetingMessage = "Welcome to the Ultimate Game Show! " +
                                    "Let's see who has what it takes to win!";

    // ─────────────────────────────────────────────────────────────────────────
    //  PRIVATE STATE
    // ─────────────────────────────────────────────────────────────────────────

    private int currentPlayerIndex = 0;
    private int currentQuestionIndex = 0;
    private bool waitingForAnswer = false;
    private int correctAnswerIndex = -1;

    // ─────────────────────────────────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        ValidateSetup();
        answerPanel.SetActive(false);
        StartCoroutine(RunShow());
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  MAIN SHOW FLOW
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator RunShow()
    {
        // ── 1. Greeting ──────────────────────────────────────────────────────
        yield return StartCoroutine(TypeText(hostDialogueText, greetingMessage));
        yield return new WaitForSeconds(2f);

        // ── 2. Player rounds ─────────────────────────────────────────────────
        for (currentPlayerIndex = 0; currentPlayerIndex < players.Length; currentPlayerIndex++)
        {
            // Skip if we've run out of questions
            if (currentQuestionIndex >= questions.Length)
            {
                yield return StartCoroutine(TypeText(hostDialogueText,
                    "We've run out of questions — let's tally the final scores!"));
                break;
            }

            PlayerController player = players[currentPlayerIndex];
            QuestionData qData = questions[currentQuestionIndex];

            // ── Announce player ──────────────────────────────────────────────
            yield return StartCoroutine(TypeText(hostDialogueText,
                $"It's your turn, {player.playerName}! Walk up to the podium!"));

            // ── Player walks to waypoint ─────────────────────────────────────
            yield return StartCoroutine(WalkPlayerToWaypoint(player, playerWaypoints[currentPlayerIndex]));

            // ── Ask the question ─────────────────────────────────────────────
            yield return StartCoroutine(AskQuestion(player, qData));

            currentQuestionIndex++;
        }

        // ── 3. Final results ─────────────────────────────────────────────────
        yield return StartCoroutine(AnnounceWinner());
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  WALKING
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator WalkPlayerToWaypoint(PlayerController player, Transform waypoint)
    {
        player.StartWalking(waypoint.position, playerWalkSpeed);

        // Wait until the player arrives
        while (!player.HasReachedDestination())
            yield return null;

        player.StopWalking();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  QUESTION / ANSWER
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator AskQuestion(PlayerController player, QuestionData qData)
    {
        // Type the question
        yield return StartCoroutine(TypeText(hostDialogueText, qData.questionText));
        yield return new WaitForSeconds(0.5f);

        // Populate answer buttons
        int pointsThisRound = qData.overridePoints > 0 ? qData.overridePoints : defaultPointsPerQuestion;
        correctAnswerIndex = -1;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < qData.answers.Length)
            {
                answerButtonTexts[i].text = qData.answers[i].answerText;
                answerButtons[i].gameObject.SetActive(true);

                if (qData.answers[i].isCorrect && correctAnswerIndex == -1)
                    correctAnswerIndex = i;

                int capturedIndex = i; // closure capture
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(capturedIndex, player, pointsThisRound));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        answerPanel.SetActive(true);
        waitingForAnswer = true;

        // Pause show until an answer is chosen
        while (waitingForAnswer)
            yield return null;

        answerPanel.SetActive(false);
        yield return new WaitForSeconds(1.5f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ANSWER CALLBACK
    // ─────────────────────────────────────────────────────────────────────────

    private void OnAnswerSelected(int index, PlayerController player, int pointsThisRound)
    {
        if (!waitingForAnswer) return;
        waitingForAnswer = false;

        if (index == correctAnswerIndex)
        {
            player.AddPoints(pointsThisRound);
            StartCoroutine(TypeText(hostDialogueText,
                $"Correct! {player.playerName} earns {pointsThisRound} points! 🎉"));
        }
        else
        {
            string correctText = questions[currentQuestionIndex < questions.Length
                ? currentQuestionIndex : questions.Length - 1].answers[correctAnswerIndex].answerText;
            StartCoroutine(TypeText(hostDialogueText,
                $"Oh no! That's wrong. The correct answer was: \"{correctText}\"."));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  WINNER ANNOUNCEMENT
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator AnnounceWinner()
    {
        PlayerController winner = players[0];
        bool isTie = false;

        foreach (var p in players)
        {
            if (p.Points > winner.Points)
            {
                winner = p;
                isTie = false;
            }
            else if (p != winner && p.Points == winner.Points)
            {
                isTie = true;
            }
        }

        string endMessage = isTie
            ? $"It's a tie! What an incredible show! Everyone gave it their all!"
            : $"And the winner is... {winner.playerName} with {winner.Points} points! Congratulations!";

        yield return StartCoroutine(TypeText(hostDialogueText, endMessage));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  TEXT TYPEWRITER EFFECT
    // ─────────────────────────────────────────────────────────────────────────

    public IEnumerator TypeText(TextMeshProUGUI label, string message)
    {
        label.text = "";
        foreach (char c in message)
        {
            label.text += c;
            yield return new WaitForSeconds(1f / textRevealSpeed);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  VALIDATION
    // ─────────────────────────────────────────────────────────────────────────

    private void ValidateSetup()
    {
        if (players.Length != 4)
            Debug.LogWarning("[GameShowManager] Expected 4 players in the players array.");
        if (playerWaypoints.Length != players.Length)
            Debug.LogError("[GameShowManager] playerWaypoints count must match players count.");
        if (answerButtons.Length != 4 || answerButtonTexts.Length != 4)
            Debug.LogError("[GameShowManager] answerButtons and answerButtonTexts must each have exactly 4 elements.");
        if (questions.Length == 0)
            Debug.LogWarning("[GameShowManager] No questions assigned! Add QuestionData assets to the questions array.");
    }
}
