using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LE_SpaceColonisation : LightningEffect
{
    GameObject attractors;
    Attractors att;


    // Start is called before the first frame update
    void Start()
    {
        transform.position = sourcePos;
        branchesRemaining = maxBranchCount;
        currentStage = Stage.SteppedLeaderGrow;
        currentSegmentPos = sourcePos;
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

        att = attractorObj.GetComponent<Attractors>();
    }

    // Update is called once per frame
    void Update()
    {
        if (parentBranch && currentStage == Stage.SteppedLeaderGrow) //If this instance is a branch of a bigger lightning section, then stop growing when parent reaches destination
        {
            if (parentBranch.currentStage != Stage.SteppedLeaderGrow)
            {
                currentStage = Stage.SteppedLeaderFlash;
            }
        }

        if (currentStage == Stage.SteppedLeaderGrow)
        {
            Grow();
        }

        else if (currentStage == Stage.SteppedLeaderFlash)
        {
            SteppedLeaderFlash();
        }

        else if (currentStage == Stage.SteppedLeaderFade)
        {
            SteppedLeaderFade();
        }

        else if (currentStage == Stage.DartLeaderFlash)
        {
            DartLeaderFlash();
        }

        else if (currentStage == Stage.DartLeaderFade)
        {
            DartLeaderFade();
        }

        else if (currentStage == Stage.Ended)
        {
            End();
        }
    }

    void Grow()
    {
        float distanceTravelled = Time.deltaTime * drawSpeed;
        if (distanceTravelled < segmentSize) //If distance travelled in a frame is less than the length of a segment, smoothly animate segment growth
        {
            if (lr.GetPosition(segmentPointer) == currentSegmentPos) //Calculate new segment when previous segment finishes animating
            {
                if (CheckDistanceToTarget())
                {
                    currentStage = Stage.SteppedLeaderFlash;
                }
                else //First create the next point on the path, then check if branching can occur at previous segment
                {
                    segmentPointer++;
                    currentSegmentPos = GenerateSegment(segmentPointer);
                    CalculateIfBranch(segmentPointer - 1, lr.GetPosition(segmentPointer - 1));

                    lr.positionCount++;
                    lr.SetPosition(segmentPointer, lr.GetPosition(segmentPointer - 1)); //Set position to beginning of new segment
                    timeSinceSegmentCreation = 0.0f;
                }
            }
            else
            {
                MoveLinePos();
            }
        }
        else //If distance travelled in a frame is more than the length of one segment, then generate and render segments in their entirety (>1 per frame if needed)
        {
            error += distanceTravelled;
            while (error >= 0.5f)
            {
                segmentPointer++;
                currentSegmentPos = GenerateSegment(segmentPointer);
                CalculateIfBranch(segmentPointer - 1, lr.GetPosition(segmentPointer - 1));

                lr.positionCount++;
                lr.SetPosition(segmentPointer, currentSegmentPos); //Set position to end of new segment
                transform.position = currentSegmentPos;
                error--;

                if (CheckDistanceToTarget())
                {
                    currentStage = Stage.DartLeaderFlash;
                    error = 0.0f;
                }

            }
        }
    }

    void End()
    {
        Destroy(lineMaterial);
        Destroy(attractorObj);
        Destroy(gameObject);
    }


    Vector3 GenerateSegment(int segmentNumber)
    {
        Vector3 directionPreviousSegment;
        List<Vector3> attractorsInRange;
        Vector3 directionToAttractors;
        Vector3 newDirection;
        Vector3 prevSegment = lr.GetPosition(segmentNumber - 1);
        float distanceToTarget = Vector3.Distance(prevSegment, targetPos);
        if (distanceToTarget < segmentSize) { return targetPos; } //If distance to target is less than the length of a node, then return the target node to prevent overshooting and circling back

        //Random direction
        Vector3 directionRandom = new Vector3(Random.Range(-1.0f, 1.0f),
                                      Random.Range(-1.0f, 1.0f),
                                      Random.Range(-1.0f, 1.0f));

        //Direction to target
        Vector3 directionToTarget = (targetPos - prevSegment).normalized;

        //Direction of previous segment (if first segment after start, then direction = direct vector)
        if (segmentNumber > 1) { directionPreviousSegment = (prevSegment - lr.GetPosition(segmentNumber - 2)).normalized; }
        else { directionPreviousSegment = directionToTarget; }

        //Direction towards attractors in range
        attractorsInRange = att.GetAttractorsInRange(prevSegment, attractorOuterBound);
        if (attractorsInRange.Count == 0) 
        {
            directionToAttractors = directionPreviousSegment;//If no attractors remaining, keep going in straight line
        }
        else
        {
            directionToAttractors = (attractorsInRange[0] - prevSegment).normalized;
            Debug.DrawLine(prevSegment, attractorsInRange[0]);
            for (int i = 1; i < attractorsInRange.Count; i++)
            {
                directionToAttractors += (attractorsInRange[i] - prevSegment); //Add directions from prev segment towards each attractor together
                Debug.DrawLine(prevSegment, attractorsInRange[i]);
            }
            directionToAttractors = directionToAttractors.normalized;
        }

        //Adjust direction to attractors by random amount to prevent branches from following a similar path
        directionToAttractors = Vector3.Lerp(directionToAttractors, directionRandom, randomInfluenceWeight);

        //Interpolate direction between moving directly to target, and moving towards attractors (when near target, move directly to target)
        if(distanceToTarget > targetOuterThreshold) 
        { 
            newDirection = (Vector3.Lerp(directionToTarget, directionToAttractors, attractorInfluenceWeight)).normalized;
        }
        else { newDirection = directionToTarget; }

        //Calculate new node position
        Vector3 newPosition = (prevSegment + (newDirection * segmentSize));

        return newPosition;
    }


    void CalculateIfBranch(int i, Vector3 position)
    {
        bool deletionOccurred = att.KillAttractorsInRange(currentSegmentPos, attractorInnerBound);
        if ((maxBranchDepth > 1) && (branchesRemaining > 0) && deletionOccurred && (i - pointOfLastBranch > minSegmentsBetweenBranching) && att.GetAttractorsInRange(position, attractorOuterBound).Count > 0)
        {
            branchesRemaining--;
            CreateBranch(i, position);
            pointOfLastBranch = i;
        }
    }


    void CreateBranch(int pointOfDivergence, Vector3 branchPoint)
    {
        GameObject branch = Instantiate(lightningObj) as GameObject;
        LE_SpaceColonisation le = branch.GetComponent<LE_SpaceColonisation>();
        le.parentBranch = this;
        le.pointOfDivergenceFromParent = pointOfDivergence;

        le.sourcePos = branchPoint;
        le.targetPos = targetPos;
        le.segmentSize = segmentSize;
        le.targetOuterThreshold = targetOuterThreshold;
        le.targetInnerThreshold = targetInnerThreshold;

        le.maxBranchCount = branchesRemaining;
        le.chanceOfBranchAtPosition = chanceOfBranchAtPosition * chanceOfBranchScaleMult;
        le.chanceOfBranchScaleMult = chanceOfBranchScaleMult;
        le.minSegmentsBetweenBranching = minSegmentsBetweenBranching;
        le.maxBranchesAtPosition = maxBranchesAtPosition;
        le.branchLineWidthMult = branchLineWidthMult;

        le.maxBranchDepth = maxBranchDepth - 1;

        le.preFlashIntensity = preFlashIntensity * branchPreFlashIntensityMult;
        le.flashIntensity = flashIntensity * branchPreFlashIntensityMult;
        le.branchPreFlashIntensityMult = branchPreFlashIntensityMult;
        le.branchFlashIntensityMult = branchFlashIntensityMult;

        le.emissionColor = emissionColor;
        le.fadedEmissionColor = fadedEmissionColor;
        le.lineWidth = lineWidth * branchLineWidthMult;
        le.dartLeaderLineWidth = dartLeaderLineWidth;

        le.fadeSpeed = fadeSpeed;
        le.drawSpeed = drawSpeed;
        le.numReturnStrokes = numReturnStrokes;
        le.returnStrokeSpeed = returnStrokeSpeed;
        le.dartLeaderFadeSpeed = dartLeaderFadeSpeed;

        le.maxAngleDirectionChange = maxAngleDirectionChange;
        le.randomnessWeight = randomnessWeight * randomnessWeightBranchMult;
        le.randomnessWeightBranchMult = randomnessWeightBranchMult;

        le.isPerpetual = isPerpetual;

        le.attractorOuterBound = attractorOuterBound;
        le.attractorInnerBound = attractorInnerBound;
        le.attractorObj = attractorObj;
        le.attractorInfluenceWeight = attractorInfluenceWeight;
    }

}
