using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningEffect : LightningCreator
{
    public Vector3 sourcePos;
    public Vector3 targetPos;
    LightningEffect parentBranch;

    enum Stage
    {
        Grow,
        Flash,
        Fade,
        Ended
    }
    Stage currentStage;

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



    // Start is called before the first frame update
    void Start()
    {
        transform.position = sourcePos;
        branchesRemaining = maxBranchCount;
        currentStage = Stage.Grow;
        currentSegmentPos = sourcePos;
        segmentPointer = 0;
        error = 0.0f;

        lightSource = GetComponent<Light>();
        lightSource.intensity = preFlashIntensity;

        lr = GetComponent<LineRenderer>();
        lr.positionCount = 1;
        lineMaterial = lr.material;
        lr.SetPosition(0, sourcePos);
    }

    // Update is called once per frame
    void Update()
    {
        if (parentBranch & currentStage == Stage.Grow) //If this instance is a branch of a bigger lightning section, then stop growing when parent reaches destination
        {
            if (parentBranch.currentStage != Stage.Grow)
            {
                currentStage = Stage.Flash;
            }
        }

        if (currentStage == Stage.Grow)
        {
            Grow();
        }

        else if (currentStage == Stage.Flash)
        {
            if (parentBranch) { parentBranch.currentStage = Stage.Flash; } //Makes whole structure flash if sub-branch reaches target first
            lightSource.intensity = flashIntensity; //Set point light intensity
            timeSinceFlash = 0.0f;
            lineMaterial.SetColor("_EmissionColor", emissionColor); //Set line renderer's material emission colour
            if (!isPerpetual)
            {
                currentStage = Stage.Fade;
            }
        }

        else if (currentStage == Stage.Fade)
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
                if (checkDistanceToTarget())
                {
                    currentStage = Stage.Flash;
                }
                else //First decide if branch will be generated, then create the next point on the path
                {
                    if (maxBranchDepth > 1)
                    {
                        CalculateIfBranch(segmentPointer, currentSegmentPos);
                    }

                    segmentPointer++;
                    currentSegmentPos = GenerateSegment(segmentPointer);
                    Debug.Log("new segment");

                    lr.positionCount++;
                    lr.SetPosition(segmentPointer, lr.GetPosition(segmentPointer - 1)); //Set position to beginning of new segment
                    timeSinceSegmentCreation = 0.0f;
                }
            }
            else
            {
                moveLinePos();
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

                if (checkDistanceToTarget())
                {
                    currentStage = Stage.Flash;
                    error = 0.0f;
                }

            }
        }
    }

    void moveLinePos()
    {
        timeSinceSegmentCreation += Time.deltaTime;
        Vector3 newLinePos = Vector3.Lerp(lr.GetPosition(segmentPointer - 1), currentSegmentPos, (timeSinceSegmentCreation * drawSpeed) / segmentSize);
        lr.SetPosition(segmentPointer, newLinePos);
        transform.position = newLinePos;
    }

    bool checkDistanceToTarget()
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


    void CreateBranch(Vector3 branchPoint)
    {
        GameObject branch = Instantiate(lightningObj) as GameObject;
        LightningEffect le = branch.GetComponent<LightningEffect>();
        le.parentBranch = this;

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
        le.flashIntensity = preFlashIntensity * branchPreFlashIntensityMult;
        le.branchPreFlashIntensityMult = branchPreFlashIntensityMult;
        le.branchFlashIntensityMult = branchFlashIntensityMult;

        le.fadeSpeed = fadeSpeed;
        le.drawSpeed = drawSpeed;

        le.maxAngleDirectionChange = maxAngleDirectionChange;
        le.randomnessWeight = randomnessWeight * randomnessWeightBranchMult;
        le.randomnessWeightBranchMult = randomnessWeightBranchMult;
        le.isPerpetual = isPerpetual;

        LineRenderer branchLine = branch.GetComponent<LineRenderer>();
        branchLine.widthMultiplier *= branchLineWidthMult;
    }

    void CalculateIfBranch(int i, Vector3 position)
    {
        if ((branchesRemaining > 0) && (i - pointOfLastBranch > minSegmentsBetweenBranching) && (Random.Range(0.0f, 1.0f) <= chanceOfBranchAtPosition))
        {
            int numBranchesatPoint = Mathf.Min(branchesRemaining, Random.Range(1, maxBranchesAtPosition));
            for (int j = 0; j < numBranchesatPoint; j++)
            {
                CreateBranch(position);
                branchesRemaining--;
            }
            pointOfLastBranch = i;
        }
    }
}
