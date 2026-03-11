using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrainController : MonoBehaviour
{
    public Transform[] trackPoints;
    public GameObject trainObject;
    public float trainSpeed = 8f;

    public DiceController diceController;
    public int sendBackTileIndex = 0;

    private bool eventRunning = false;

    public void StartTrainEvent(PlayerMovement triggeringPlayer)
    {
        if (!eventRunning)
            StartCoroutine(TrainSequence(triggeringPlayer));
    }

    private IEnumerator TrainSequence(PlayerMovement triggeringPlayer)
    {
        eventRunning = true;

        List<PlayerMovement> playersAhead = GetPlayersAhead(triggeringPlayer);

        yield return MoveTrainAlongTrack(playersAhead);

        eventRunning = false;
    }

    private List<PlayerMovement> GetPlayersAhead(PlayerMovement triggeringPlayer)
    {
        List<PlayerMovement> result = new List<PlayerMovement>();

        int triggerIndex = triggeringPlayer.GetCurrentTileIndex();

        foreach (var p in diceController.playersToMove)
        {
            if (p != triggeringPlayer && p.GetCurrentTileIndex() > triggerIndex)
                result.Add(p);
        }

        return result;
    }

    private IEnumerator MoveTrainAlongTrack(List<PlayerMovement> playersAhead)
    {
        trainObject.SetActive(true);

        for (int i = 0; i < trackPoints.Length; i++)
        {
            Vector3 start = trainObject.transform.position;
            Vector3 end = trackPoints[i].position;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * trainSpeed;
                trainObject.transform.position = Vector3.Lerp(start, end, t);

                CheckTrainCollisions(playersAhead);

                yield return null;
            }
        }

        trainObject.SetActive(false);
    }

    private void CheckTrainCollisions(List<PlayerMovement> playersAhead)
    {
        foreach (var p in playersAhead)
        {
            float dist = Vector3.Distance(trainObject.transform.position, p.transform.position);

            if (dist < 1.5f)
                SendPlayerBack(p);
        }
    }

    private void SendPlayerBack(PlayerMovement player)
    {
        player.ForceMoveToTile(sendBackTileIndex);
    }
}
