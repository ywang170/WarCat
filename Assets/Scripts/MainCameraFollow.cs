using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraFollow : MonoBehaviour
{

    public Transform followingTarget;
    public float maxYCoord;
    public float minYCoord;
    public float maxXCoord;
    public float minXCoord;
    public float bestYDistanceToTarget;
    public float maxYDistanceToTarget; // positive value, how much camera can above target
    public float minYDistanceToTarget; // negative value, how much camera can below target
    public float bestXDistanceToTarget;
    public float maxXDistanceToTarget;
    public float minXDistanceToTarget;
    public bool affectedByFlip;

    private PlayerPlatformerController playerController;
    private float previousTargetX;

    private void Start()
    {
        if (affectedByFlip)
        {
            playerController = followingTarget.GetComponent<PlayerPlatformerController>();
        }
        previousTargetX = followingTarget.position.x;
    }

    void FixedUpdate()
    {
        float currentYDistanceBetweenCameraAndTarget = transform.position.y - followingTarget.position.y;
        float currentXDistanceBetweenCameraAndTarget = transform.position.x - followingTarget.position.x;
        Vector3 newPosition = transform.position;
        bool targetFlipped = affectedByFlip ? playerController.IsFlipped() : false;

        if (currentYDistanceBetweenCameraAndTarget > maxYDistanceToTarget)
        {
            newPosition.y = followingTarget.position.y + maxYDistanceToTarget;
        } else if (currentYDistanceBetweenCameraAndTarget < minYDistanceToTarget)
        {
            newPosition.y = followingTarget.position.y + minYDistanceToTarget;
        } else
        {
            newPosition.y -= (currentYDistanceBetweenCameraAndTarget - bestYDistanceToTarget) * 0.2f;
        }
        float flipAffectedMaxXDistanceToTarget = targetFlipped ? 0 - minXDistanceToTarget : maxXDistanceToTarget;
        float flipAffectedMinXDistanceToTarget = targetFlipped ? 0 - maxXDistanceToTarget : minXDistanceToTarget;
        float flipAffectedBestXDistanceToTarget = targetFlipped ? 0 - bestXDistanceToTarget : bestXDistanceToTarget;
        if (currentXDistanceBetweenCameraAndTarget > flipAffectedMaxXDistanceToTarget)
        {
            newPosition.x = followingTarget.position.x + flipAffectedMaxXDistanceToTarget;
        } else if (currentXDistanceBetweenCameraAndTarget < flipAffectedMinXDistanceToTarget)
        {
            newPosition.x = followingTarget.position.x + flipAffectedMinXDistanceToTarget;
        } else
        {
            newPosition.x -= 
                (currentXDistanceBetweenCameraAndTarget - flipAffectedBestXDistanceToTarget) * 
                1f * 
                (Mathf.Abs(followingTarget.position.x - previousTargetX) + 0.02f);
        }

        newPosition.y = Mathf.Min(maxYCoord, Mathf.Max(newPosition.y, minYCoord));
        newPosition.x = Mathf.Min(maxXCoord, Mathf.Max(newPosition.x, minXCoord));

        transform.position = newPosition;
        previousTargetX = followingTarget.position.x;
    }
}
