using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraFollow : MonoBehaviour
{

    public Transform followingTarget;
    public float mapLeftBorderCoord;
    public float mapRightBorderCoord;
    public float mapTopBorderCoord;
    public float mapBottomBorderCoord;
    public float bestYDistanceToTarget = 1;
    public float bestXDistanceToTarget = 3;
    public float characterWidth;
    public float characterHeight;
    public bool affectedByFlip = true;

    private float maxYCoord = 4;
    private float minYCoord = -2;
    private float maxXCoord = 7;
    private float minXCoord = -7;
    private float maxYDistanceToTarget = 3; // positive value, how much camera can above target
    private float minYDistanceToTarget = -3; // negative value, how much camera can below target
    private float maxXDistanceToTarget = 7;
    private float minXDistanceToTarget = -7;

    private PlayerPlatformerController playerController;
    private float previousTargetX;

    private void Start()
    {
        float cameraVerticalSize   = (float)(Camera.main.orthographicSize * 2.0);
        float cameraHorizontalSize = cameraVerticalSize * Screen.width / Screen.height;
        maxXCoord = mapRightBorderCoord - cameraHorizontalSize / 2;
        minXCoord = mapLeftBorderCoord + cameraHorizontalSize / 2;
        maxYCoord = mapTopBorderCoord - cameraVerticalSize / 2;
        minYCoord = mapBottomBorderCoord + cameraVerticalSize / 2;
        maxXDistanceToTarget = (cameraHorizontalSize - characterWidth) / 2;
        minXDistanceToTarget = -maxXDistanceToTarget;
        maxYDistanceToTarget = (cameraVerticalSize - characterHeight) / 2;
        minYDistanceToTarget = -maxYDistanceToTarget;

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
