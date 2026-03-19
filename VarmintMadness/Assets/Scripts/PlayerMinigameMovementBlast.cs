using UnityEngine;

public class PlayerMovementBlast : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Transform[] buttonPositions;
    private Transform spawnPoint;

    private int currentIndex = 0;

    private bool walkingToStart = false;
    private bool walkingBack = false;
    private bool canChoose = false;

    private BowserBlastMinigameManager manager;

    public int playerIndex;

    public void Initialize(BowserBlastMinigameManager mgr, Transform spawn, Transform[] positions)
    {
        manager = mgr;
        spawnPoint = spawn;
        buttonPositions = positions;

        transform.position = spawn.position;

        walkingToStart = false;
        walkingBack = false;
        canChoose = false;
    }

    public void UpdateButtonPositions(Transform[] newPositions)
    {
        buttonPositions = newPositions;

        if (currentIndex >= buttonPositions.Length)
            currentIndex = buttonPositions.Length - 1;
    }

    public void BeginTurn()
    {
        currentIndex = 0;
        walkingToStart = true;
        walkingBack = false;
        canChoose = false;
    }

    public void EndTurn()
    {
        walkingToStart = false;
        walkingBack = false;
        canChoose = false;
    }

    public void StartWalkingBack()
    {
        walkingBack = true;
        walkingToStart = false;
        canChoose = false;
    }

    public void ReturnToSpawnInstant()
    {
        transform.position = spawnPoint.position;
        walkingBack = false;
        walkingToStart = false;
        canChoose = false;
    }

    public void ExplodeAndRemove()
    {
        gameObject.SetActive(false);
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

        if (walkingBack)
        {
            transform.position = Vector3.MoveTowards(transform.position, spawnPoint.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, spawnPoint.position) < 0.05f)
            {
                walkingBack = false;
                manager.StartPlayerTurn();
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
            walkingBack = true;
            manager.OnPlayerSelectedButton(currentIndex);
        }
    }
}