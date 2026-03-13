using UnityEngine;

public class PlayerMovementBlast : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Transform[] buttonPositions;
    private int currentIndex = 0;
    private bool canChoose = false;
    private bool walkingToStart = false;
    private BowserBlastMinigameManager manager;

    public int playerIndex; // used for reward system

    public void Initialize(BowserBlastMinigameManager mgr, Transform spawnPoint, Transform[] positions)
    {
        manager = mgr;
        buttonPositions = positions;

        transform.position = spawnPoint.position;

        currentIndex = 0;
        walkingToStart = true;
    }

    public void EnableInput(bool enable)
    {
        canChoose = enable;
    }

    void Update()
    {
        if (walkingToStart)
        {
            Vector3 target = buttonPositions[0].position;
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                walkingToStart = false;
                canChoose = true;
            }

            return;
        }

        if (!canChoose) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = buttonPositions.Length - 1;
            transform.position = buttonPositions[currentIndex].position;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            currentIndex++;
            if (currentIndex >= buttonPositions.Length) currentIndex = 0;
            transform.position = buttonPositions[currentIndex].position;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            canChoose = false;
            manager.OnPlayerSelectedButton(currentIndex);
        }
    }
}