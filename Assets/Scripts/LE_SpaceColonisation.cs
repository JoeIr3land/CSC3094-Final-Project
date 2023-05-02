using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LE_SpaceColonisation : LightningEffect
{

    Vector3 prevSegmentPos;
    List<Vector3> attractors;


    // Start is called before the first frame update
    void Start()
    {
        transform.position = sourcePos;
        branchesRemaining = maxBranchCount;
        currentStage = Stage.SteppedLeaderGrow;
        currentSegmentPos = sourcePos;
        prevSegmentPos = sourcePos;
        segmentPointer = 0;
        error = 0.0f;

        lightSource = GetComponent<Light>();
        lightSource.intensity = preFlashIntensity;
        numFlashesCompleted = 0;

        lr = GetComponent<LineRenderer>();
        lr.positionCount = 1;
        lr.widthMultiplier = lineWidth;
        lineMaterial = lr.material;
        lr.SetPosition(0, sourcePos);

    }

    /*
    Vector3 GenerateSegment(int segmentNumber)
    {
        float distanceToTarget = Vector3.Distance(prevSegmentPos, targetPos);
        if (distanceToTarget < segmentSize) { return targetPos; } //If distance to target is less than the length of a node, then return the target node to prevent overshooting and circling back

        Vector3 directionPreviousSegment;
        List<Vector3> directionsToAttractors;
        Vector3 newDirection;

        //Direction to target
        Vector3 directionToTarget = (targetPos - prevSegmentPos).normalized;
        //Direction of previous segment (if first segment after start, then direction = direct vector)
        if (segmentNumber > 1) { directionPreviousSegment = (prevSegmentPos - lr.GetPosition(segmentNumber - 2)).normalized; }
        else { directionPreviousSegment = directionToTarget; }
    }*/
}
