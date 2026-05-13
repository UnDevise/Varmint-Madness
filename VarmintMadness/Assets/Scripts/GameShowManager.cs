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

    [Header("── Rounds ──")]
    [Tooltip("How many questions each player must answer before the game ends.")]
    [Range(1, 20)]
    public int questionsPerPlayer = 3;

    [Tooltip("Shuffle the question order randomly every time the game starts.")]
    public bool randomizeQuestions = true;

    [Header("── Scoring ──")]
    [Tooltip("Base points awarded for a correct answer. " +
             "Individual questions can override this via QuestionData.")]
    public int defaultPointsPerQuestion = 100;

    [Header("── Scene References ──")]
    public PlayerController[] players;          // Drag the 4 player GameObjects here
    public Transform[] playerWaypoints;         // One podium waypoint per player
    public Transform[] playerWaitingSpots;      // One waiting-area spot per player

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
    private bool waitingForAnswer = false;
    private int correctAnswerIndex = -1;
    private Coroutine activeTypeCoroutine = null;

    // Shuffled working copy of the question list
    private List<QuestionData> questionPool = new List<QuestionData>();
    // Advances each time any player is asked a question
    private int poolIndex = 0;

    // ─────────────────────────────────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        // Get ordered players from MinigameCharacterApplier
        MinigameCharacterApplier applier = Object.FindFirstObjectByType<MinigameCharacterApplier>();

        List<PlayerController> activePlayers = new List<PlayerController>();
        List<Transform> activeWaypoints = new List<Transform>();

        if (applier != null && applier.orderedActivePlayers != null)
        {
            for (int i = 0; i < applier.orderedActivePlayers.Length; i++)
            {
                GameObject obj = applier.orderedActivePlayers[i];
                if (obj != null)
                {
                    PlayerController p = obj.GetComponent<PlayerController>();
                    if (p != null)
                    {
                        activePlayers.Add(p);
                        if (i < playerWaypoints.Length)
                            activeWaypoints.Add(playerWaypoints[i]);
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null && players[i].gameObject.activeSelf)
                {
                    activePlayers.Add(players[i]);
                    if (i < playerWaypoints.Length)
                        activeWaypoints.Add(playerWaypoints[i]);
                }
            }
        }

        players = activePlayers.ToArray();
        playerWaypoints = activeWaypoints.ToArray();

        ValidateSetup();
        BuildQuestionPool();

        answerPanel.SetActive(false);

        // Snap every player to their waiting spot instantly at scene start
        for (int i = 0; i < players.Length; i++)
        {
            if (i < playerWaitingSpots.Length && playerWaitingSpots[i] != null)
                players[i].transform.position = playerWaitingSpots[i].position;
        }

        StartCoroutine(RunShow());
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  QUESTION POOL BUILDER
    // ─────────────────────────────────────────────────────────────────────────

    private void BuildQuestionPool()
    {
        int totalNeeded = players.Length * questionsPerPlayer;

        // Fill the pool, looping the question list if there are fewer questions than needed
        questionPool.Clear();
        while (questionPool.Count < totalNeeded)
        {
            foreach (QuestionData q in questions)
            {
                questionPool.Add(q);
                if (questionPool.Count >= totalNeeded) break;
            }
        }

        // Fisher-Yates shuffle
        if (randomizeQuestions)
        {
            for (int i = questionPool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                QuestionData tmp = questionPool[i];
                questionPool[i] = questionPool[j];
                questionPool[j] = tmp;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  MAIN SHOW FLOW
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator RunShow()
    {
        // ── 1. Greeting ──────────────────────────────────────────────────────
        yield return StartCoroutine(TypeText(hostDialogueText, greetingMessage));
        yield return new WaitForSeconds(2f);

        // ── 2. Rounds: questionsPerPlayer rounds, each round covers all players
        for (int round = 0; round < questionsPerPlayer; round++)
        {
            for (currentPlayerIndex = 0; currentPlayerIndex < players.Length; currentPlayerIndex++)
            {
                // Safety: stop if the pool is somehow exhausted
                if (poolIndex >= questionPool.Count)
                {
                    yield return StartCoroutine(TypeText(hostDialogueText,
                        "We've run out of questions - let's tally the final scores!"));
                    goto ShowEnd;
                }

                PlayerController player = players[currentPlayerIndex];
                QuestionData qData = questionPool[poolIndex];
                poolIndex++;

                // ── Announce player ──────────────────────────────────────────
                yield return StartCoroutine(TypeText(hostDialogueText,
                    $"It's your turn, {player.playerName}! Walk up to the podium!"));

                // ── Player walks from waiting spot to podium ─────────────────
                yield return StartCoroutine(
                    WalkPlayerTo(player, playerWaypoints[currentPlayerIndex].position));

                // ── Ask the question ─────────────────────────────────────────
                yield return StartCoroutine(AskQuestion(player, qData));

                // ── Player walks back to their waiting spot ──────────────────
                if (currentPlayerIndex < playerWaitingSpots.Length &&
                    playerWaitingSpots[currentPlayerIndex] != null)
                {
                    yield return StartCoroutine(TypeText(hostDialogueText,
                        $"Head back to your spot, {player.playerName}!"));

                    yield return StartCoroutine(
                        WalkPlayerTo(player, playerWaitingSpots[currentPlayerIndex].position));
                }
            }
        }

    ShowEnd:
        // ── 3. Final results ─────────────────────────────────────────────────
        yield return StartCoroutine(AnnounceWinner());
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  WALKING
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator WalkPlayerTo(PlayerController player, Vector2 destination)
    {
        player.StartWalking(destination, playerWalkSpeed);

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

                int capturedIndex = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(
                    () => OnAnswerSelected(capturedIndex, player, pointsThisRound, qData));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        answerPanel.SetActive(true);
        waitingForAnswer = true;

        while (waitingForAnswer)
            yield return null;

        answerPanel.SetActive(false);
        yield return new WaitForSeconds(1.5f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ANSWER CALLBACK
    // ─────────────────────────────────────────────────────────────────────────

    private void OnAnswerSelected(int index, PlayerController player,
                                  int pointsThisRound, QuestionData qData)
    {
        if (!waitingForAnswer) return;
        waitingForAnswer = false;

        if (index == correctAnswerIndex)
        {
            player.AddPoints(pointsThisRound);
            StartCoroutine(TypeText(hostDialogueText,
                $"Correct! {player.playerName} earns {pointsThisRound} points!"));
        }
        else
        {
            string correctText = correctAnswerIndex >= 0
                ? qData.answers[correctAnswerIndex].answerText
                : "unknown";
            // Stop any running TypeText first, then show the correct answer
            if (activeTypeCoroutine != null)
            {
                StopCoroutine(activeTypeCoroutine);
                activeTypeCoroutine = null;
            }
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

        foreach (PlayerController p in players)
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
            ? "It's a tie! What an incredible show! Everyone gave it their all!"
            : $"And the winner is... {winner.playerName} with {winner.Points} points! Congratulations!";

        yield return StartCoroutine(TypeText(hostDialogueText, endMessage));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  TEXT TYPEWRITER EFFECT
    // ─────────────────────────────────────────────────────────────────────────

    public IEnumerator TypeText(TextMeshProUGUI label, string message)
    {
        // Stop any currently running TypeText before starting a new one
        if (activeTypeCoroutine != null)
        {
            StopCoroutine(activeTypeCoroutine);
            activeTypeCoroutine = null;
        }

        activeTypeCoroutine = StartCoroutine(TypeTextInternal(label, message));
        yield return activeTypeCoroutine;
    }

    private IEnumerator TypeTextInternal(TextMeshProUGUI label, string message)
    {
        label.text = "";
        foreach (char c in message)
        {
            label.text += c;
            yield return new WaitForSeconds(1f / textRevealSpeed);
        }
        activeTypeCoroutine = null;
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
        if (playerWaitingSpots.Length != players.Length)
            Debug.LogError("[GameShowManager] playerWaitingSpots count must match players count.");
        if (answerButtons.Length != 4 || answerButtonTexts.Length != 4)
            Debug.LogError("[GameShowManager] answerButtons and answerButtonTexts must each have exactly 4 elements.");
        if (questions.Length == 0)
            Debug.LogWarning("[GameShowManager] No questions assigned! Add QuestionData assets to the questions array.");
    }
}
