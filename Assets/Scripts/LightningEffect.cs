using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningEffect : LightningCreator
{
    public Vector3 startNode;
    public Vector3 targetNode;
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

    int nodePointer;
    Vector3 currentNode;
    float timeSinceNodeCreation;
    
    int branchesRemaining;
    int pointOfLastBranch;

    Light lightSource;
    float timeSinceFlash;



    // Start is called before the first frame update
    void Start()
    {

        branchesRemaining = maxBranchCount;
        nodePointer = 0;
        currentNode = startNode;
        timeSinceNodeCreation = 0.0f;

        lr = GetComponent<LineRenderer>();
        lr.positionCount = 1;
        lineMaterial = lr.material;
        lr.SetPosition(0, startNode);

        currentStage = Stage.Grow;

        lightSource = GetComponent<Light>();
        lightSource.intensity = preFlashIntensity;

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
            if (lr.GetPosition(nodePointer) == currentNode) //Only calculate new node when previous segment has finished animating
            {
                float distanceToTarget = Vector3.Distance(lr.GetPosition(nodePointer), targetNode);
                if (distanceToTarget < targetInnerThreshold)
                {
                    currentStage = Stage.Flash;
                }
                else //First decide if branch will be generated, then create the next point on the path
                {
                    if (maxBranchDepth > 1)
                    {
                        CalculateIfBranch(nodePointer, currentNode);
                    }

                    nodePointer++;
                    currentNode = GenerateNode(nodePointer);

                    lr.positionCount++;
                    lr.SetPosition(nodePointer, lr.GetPosition(nodePointer - 1)); //Set position to beginning of new segment

                    timeSinceNodeCreation = 0.0f;

                }
            }
            else
            {
                timeSinceNodeCreation += Time.deltaTime;
                Vector3 newLinePos = Vector3.Lerp(lr.GetPosition(nodePointer - 1), currentNode, timeSinceNodeCreation * drawSpeed);
                lr.SetPosition(nodePointer, newLinePos);
                transform.position = newLinePos;
            }

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


    Vector3 GenerateNode(int nodeNumber)
    {
        Vector3 prevNode = lr.GetPosition(nodeNumber - 1);
        float distanceToTarget = Vector3.Distance(prevNode, targetNode);
        if (distanceToTarget < nodeScale) { return targetNode; } //If distance to target is less than the length of a node, then return the target node to prevent overshooting and circling back

        Vector3 directionPreviousSegment;
        Vector3 newDirection;

        //Direction to target
        Vector3 directionToTarget = (targetNode - prevNode).normalized;
        //Direction of previous segment (if first segment after start, then direction = direct vector)
        if (nodeNumber > 1) { directionPreviousSegment = (prevNode - lr.GetPosition(nodeNumber - 2)).normalized; }
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
        return prevNode + (newDirection * nodeScale);
    }


    void CreateBranch(Vector3 branchPoint)
    {
        GameObject branch = Instantiate(lightningObj) as GameObject;
        LightningEffect le = branch.GetComponent<LightningEffect>();
        le.parentBranch = this;

        le.startNode = branchPoint;
        le.targetNode = targetNode;
        le.nodeScale = nodeScale;
        le.targetOuterThreshold = targetOuterThreshold;
        le.targetInnerThreshold = targetInnerThreshold;

        le.maxBranchCount = maxBranchCount;
        le.chanceOfBranchAtNode = chanceOfBranchAtNode * chanceOfBranchScaleMult;
        le.chanceOfBranchScaleMult = chanceOfBranchScaleMult;
        le.minNodesBetweenBranching = minNodesBetweenBranching;
        le.maxBranchesAtNode = maxBranchesAtNode;
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


    void DrawNode(int i, Vector3 node)
    {
        lr.positionCount = i + 1;
        lr.SetPosition(i, node);
    }


    void CalculateIfBranch(int i, Vector3 node)
    {
        if ((branchesRemaining > 0) && (i - pointOfLastBranch > minNodesBetweenBranching) && (Random.Range(0.0f, 1.0f) <= chanceOfBranchAtNode))
        {
            int numBranchesatPoint = Mathf.Min(branchesRemaining, Random.Range(1, maxBranchesAtNode));
            for (int j = 0; j < numBranchesatPoint; j++)
            {
                CreateBranch(node);
                branchesRemaining--;
            }
            pointOfLastBranch = i;
        }
    }
}
