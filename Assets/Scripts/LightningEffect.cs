using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningEffect : LightningCreator
{
    public Vector3 sourcePos;
    public Vector3 targetPos;
    LightningEffect parentBranch;
    public int pointOfDivergenceFromParent;

    LineRenderer lr;
    Material lineMaterial;

    int segmentPointer;
    Vector3 currentSegmentPos;
    float error;
    float timeSinceSegmentCreation;
    
    int branchesRemaining;
    int pointOfLastBranch;

    Light lightSource;
    float timeSinceFlash;
    int numFlashesCompleted;


    enum Stage
    {
        SteppedLeaderGrow,
        SteppedLeaderFlash,
        SteppedLeaderFade,
        DartLeaderFlash,
        DartLeaderFade,
        Ended
    }
    Stage currentStage;


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
    }


    // Update is called once per frame
    void Update()
    {
        if (parentBranch & currentStage == Stage.SteppedLeaderGrow) //If this instance is a branch of a bigger lightning section, then stop growing when parent reaches destination
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
            if (parentBranch)//Makes whole structure flash if sub-branch reaches target first
            {
                if (parentBranch.currentStage == Stage.SteppedLeaderGrow) { parentBranch.currentStage = Stage.SteppedLeaderFlash; }
            }
            lightSource.intensity = flashIntensity; //Set point light intensity
            timeSinceFlash = 0.0f;
            lineMaterial.SetColor("_EmissionColor", emissionColor); //Set line renderer's material emission colour
            if (!isPerpetual)
            {
                currentStage = Stage.SteppedLeaderFade;
            }
        }

        else if (currentStage == Stage.SteppedLeaderFade)
        {
            timeSinceFlash += Time.deltaTime;
            if (lightSource.intensity > 0)
            {
                lightSource.intensity = Mathf.Lerp(flashIntensity, 0, timeSinceFlash * fadeSpeed);
                lineMaterial.SetColor("_EmissionColor", Color.Lerp(emissionColor, fadedEmissionColor, timeSinceFlash * fadeSpeed));
            }
            else
            {
                currentStage = Stage.Ended;
            }
        }

        else if (currentStage == Stage.DartLeaderFlash)
        {
            if(numFlashesCompleted == 0)
            {
                if (parentBranch) //Makes whole structure flash if sub-branch reaches target first, and make sure whole path flashes properly
                {
                    parentBranch.currentStage = Stage.SteppedLeaderFlash;
                    CalculateDartLeaderPath();
                }
                lr.widthMultiplier = dartLeaderLineWidth;
            }
            lightSource.intensity = flashIntensity;
            timeSinceFlash = 0.0f;
            lineMaterial.SetColor("_EmissionColor", emissionColor); //Set line renderer's material emission colour
            numFlashesCompleted++;
            if (!isPerpetual)
            {
                currentStage = Stage.DartLeaderFade;
            }
        }

        else if (currentStage == Stage.DartLeaderFade)
        {
            timeSinceFlash += Time.deltaTime;
            if(numFlashesCompleted < numReturnStrokes)
            {
                if(lightSource.intensity > 0)
                {
                    lightSource.intensity = Mathf.Lerp(flashIntensity, 0, timeSinceFlash * returnStrokeSpeed);
                    lineMaterial.SetColor("_EmissionColor", Color.Lerp(emissionColor, fadedEmissionColor, timeSinceFlash * returnStrokeSpeed));
                }
                else
                {
                    currentStage = Stage.DartLeaderFlash;
                }
            }
            else
            {
                if (lightSource.intensity > 0)
                {
                    lightSource.intensity = Mathf.Lerp(flashIntensity, 0, timeSinceFlash * dartLeaderFadeSpeed);
                    lineMaterial.SetColor("_EmissionColor", Color.Lerp(emissionColor, fadedEmissionColor, timeSinceFlash * dartLeaderFadeSpeed));
                }
                else
                {
                    currentStage = Stage.Ended;
                }
            }

        }

        else if (currentStage == Stage.Ended)
        {
            Destroy(lineMaterial);
            Destroy(gameObject);
        }

    }


    void Grow()
    {
        float distanceTravelled = Time.deltaTime * drawSpeed;
        if(distanceTravelled < segmentSize) //If distance travelled in a frame is less than the length of a segment, smoothly animate segment growth
        {
            if (lr.GetPosition(segmentPointer) == currentSegmentPos) //Calculate new segment when previous segment finishes animating
            {
                if (CheckDistanceToTarget())
                {
                    currentStage = Stage.SteppedLeaderFlash;
                }
                else //First decide if branch will be generated, then create the next point on the path
                {
                    if (maxBranchDepth > 1)
                    {
                        CalculateIfBranch(segmentPointer, currentSegmentPos);
                    }

                    segmentPointer++;
                    currentSegmentPos = GenerateSegment(segmentPointer);

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
                if (maxBranchDepth > 1)
                {
                    CalculateIfBranch(segmentPointer, currentSegmentPos);
                }

                segmentPointer++;
                currentSegmentPos = GenerateSegment(segmentPointer);

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


    void CalculateDartLeaderPath()
    {
        //Get segment positions from parent branch(es) and add to this instance's line renderer
        bool atMasterBranch = false;
        LightningEffect currentBranch = this;
        int currentBranchPoint;
        while (!atMasterBranch)
        {
            if (!currentBranch.parentBranch)
            {
                atMasterBranch = true;
            }
            else
            {
                currentBranchPoint = currentBranch.pointOfDivergenceFromParent;
                currentBranch = currentBranch.parentBranch;
                //Shift all linerenderer positions upwards
                int oldPositionCount = lr.positionCount;
                lr.positionCount += currentBranchPoint;
                for(int i=oldPositionCount-1; i>0; i--)
                {
                    lr.SetPosition(i + currentBranchPoint, lr.GetPosition(i));
                }
                //Then add points from parent branch, up until it reaches the point where branching occurs
                for(int i=0; i<=currentBranchPoint; i++)
                {
                    lr.SetPosition(i, currentBranch.lr.GetPosition(i));
                }

            }
        }
    }


    void MoveLinePos()
    {
        timeSinceSegmentCreation += Time.deltaTime;
        Vector3 newLinePos = Vector3.Lerp(lr.GetPosition(segmentPointer - 1), currentSegmentPos, (timeSinceSegmentCreation * drawSpeed) / segmentSize);
        lr.SetPosition(segmentPointer, newLinePos);
        transform.position = newLinePos;
    }


    bool CheckDistanceToTarget()
    {
        return Vector3.Distance(lr.GetPosition(segmentPointer), targetPos) < targetInnerThreshold;
    }


    Vector3 GenerateSegment(int segmentNumber)
    {
        Vector3 prevSegment = lr.GetPosition(segmentNumber - 1);
        float distanceToTarget = Vector3.Distance(prevSegment, targetPos);
        if (distanceToTarget < segmentSize) { return targetPos; } //If distance to target is less than the length of a node, then return the target node to prevent overshooting and circling back

        Vector3 directionPreviousSegment;
        Vector3 newDirection;

        //Direction to target
        Vector3 directionToTarget = (targetPos - prevSegment).normalized;
        //Direction of previous segment (if first segment after start, then direction = direct vector)
        if (segmentNumber > 1) { directionPreviousSegment = (prevSegment - lr.GetPosition(segmentNumber - 2)).normalized; }
        else { directionPreviousSegment = directionToTarget; }
        //Random direction
        Vector3 directionRandom = new Vector3(Random.Range(-1.0f, 1.0f),
                                              Random.Range(-1.0f, 1.0f),
                                              Random.Range(-1.0f, 1.0f));
        //Interpolate direction between moving directly to target, and moving in random position (when near target, move as directly to target as allowed within rotation)
        if (distanceToTarget > targetOuterThreshold) { newDirection = Vector3.Lerp(directionToTarget, directionRandom, randomnessWeight); }
        else { newDirection = directionToTarget; }
        //Clamp newDirection to be within allowed angle of direction change (if nearing inner threshold, do not clamp)
        float angle = Vector3.Angle(directionPreviousSegment, newDirection);
        float factor = maxAngleDirectionChange / angle;
        newDirection = Vector3.Slerp(directionPreviousSegment, newDirection, factor);
        //Calculate new node position
        return prevSegment + (newDirection * segmentSize);
    }


    void CreateBranch(int pointOfDivergence, Vector3 branchPoint)
    {
        GameObject branch = Instantiate(lightningObj) as GameObject;
        LightningEffect le = branch.GetComponent<LightningEffect>();
        le.parentBranch = this;
        le.pointOfDivergenceFromParent = pointOfDivergence;

        le.sourcePos = branchPoint;
        le.targetPos = targetPos;
        le.segmentSize = segmentSize;
        le.targetOuterThreshold = targetOuterThreshold;
        le.targetInnerThreshold = targetInnerThreshold;

        le.maxBranchCount = maxBranchCount;
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
    }


    void CalculateIfBranch(int i, Vector3 position)
    {
        if ((branchesRemaining > 0) && (i - pointOfLastBranch > minSegmentsBetweenBranching) && (Random.Range(0.0f, 1.0f) <= chanceOfBranchAtPosition))
        {
            int numBranchesatPoint = Mathf.Min(branchesRemaining, Random.Range(1, maxBranchesAtPosition));
            for (int j = 0; j < numBranchesatPoint; j++)
            {
                CreateBranch(i, position);
                branchesRemaining--;
            }
            pointOfLastBranch = i;
        }
    }
}
