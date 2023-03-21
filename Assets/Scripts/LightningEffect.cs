using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningEffect : MonoBehaviour
{

    [Header("Main Path")]
    [SerializeField] public GameObject startNode;
    [SerializeField] public GameObject targetNode;
    [SerializeField] public int nodeCount;

    [Header("Branches")]
    [SerializeField] public int maxBranchCount;
    [SerializeField] public float chanceOfBranchAtNode;
    [SerializeField] public int maxBranchesAtNode;
    [SerializeField] public float branchScale;
    [SerializeField] public float branchLineWidthMult;

    [Header("Subbranches")]
    [SerializeField] public int maxBranchDepth;

    [Header("Light")]
    [SerializeField] public float preFlashIntensity;
    [SerializeField] public float flashIntensity;
    [SerializeField] public float branchPreFlashIntensityMult;
    [SerializeField] public float branchFlashIntensityMult;

    [Header("Line")]
    [SerializeField] public Color emissionColor;
    [SerializeField] public Color fadedEmissionColor;

    [Header("Animation")]
    [SerializeField] public float fadeSpeed;
    [SerializeField] public float drawSpeed;

    [Header("Path Randomness")]
    [SerializeField] public float randomScaleOnMainPath;
    [SerializeField] public float randomScaleBranchTarget;
    [SerializeField] public float randomScaleBranchPath;

    [Header("Other")]
    [SerializeField] public bool isPerpetual;

    [Header("DO NOT CHANGE")]
    [SerializeField] GameObject lightningObj;


    LightningEffect parentBranch;
    LineRenderer lr;
    int nodePointer;
    float error;
    Vector3 currentNode;
    float timeSinceNodeCreation;
    enum Stage
    {
        Grow,
        Flash,
        Fade,
        Ended
    }
    Stage currentStage;
    int branchesRemaining;
    Light lightSource;
    float timeSinceFlash;
    Material lineMaterial;


    // Start is called before the first frame update
    void Start()
    {

        branchesRemaining = maxBranchCount;
        nodePointer = 0;
        currentNode = startNode.transform.position;
        timeSinceNodeCreation = 0.0f;

        lr = GetComponent<LineRenderer>();
        lr.positionCount = 1;
        lineMaterial = lr.material;
        lr.SetPosition(0, startNode.transform.position);

        currentStage = Stage.Grow;

        lightSource = GetComponent<Light>();
        lightSource.intensity = preFlashIntensity;

    }

    // Update is called once per frame
    void Update()
    {
        if (parentBranch & currentStage == Stage.Grow) //If this instance is a branch of a bigger lightning section, then stop growing when parent reaches destination
        {
            if(parentBranch.currentStage != Stage.Grow)
            {
                currentStage = Stage.Flash;
            }
        }

        if (currentStage == Stage.Grow)
        {
            if(lr.GetPosition(nodePointer) == currentNode && nodePointer < nodeCount)
            {
                nodePointer++;
                if(nodePointer >= nodeCount)
                {
                    currentStage = Stage.Flash;
                }
                else
                {
                    if (maxBranchDepth > 1)
                    {
                        CalculateIfBranch(nodePointer, currentNode);
                    }

                    currentNode = GenerateNode(nodePointer);

                    lr.positionCount++;
                    lr.SetPosition(nodePointer, lr.GetPosition(nodePointer - 1)); //Set position to beginning of new line section

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

        else if(currentStage == Stage.Flash)
        {
            lightSource.intensity = flashIntensity;
            timeSinceFlash = 0.0f;
            lineMaterial.SetColor("_EmissionColor", emissionColor);
            if (!isPerpetual)
            {
                currentStage = Stage.Fade;
            }
        }

        else if(currentStage == Stage.Fade)
        {
            timeSinceFlash += Time.deltaTime;
            if(lightSource.intensity > 0)
            {
                lightSource.intensity = Mathf.Lerp(flashIntensity, 0, timeSinceFlash * fadeSpeed);
                lineMaterial.SetColor("_EmissionColor", Color.Lerp(emissionColor, fadedEmissionColor, timeSinceFlash * fadeSpeed));
            }
            else
            {
                currentStage = Stage.Ended;
            }
        }

        else if(currentStage == Stage.Ended)
        {
            if (parentBranch) //If this instance is a sub-branch, destroy its temporary start and target objects - does not destroy start and target of main lightning bolt
            {
                Destroy(startNode);
                Destroy(targetNode);
            }
            Destroy(lineMaterial);
            Destroy(gameObject);
        }

    }


    Vector3 GenerateNode(int nodeNumber)
    {

        Vector3 startPos = startNode.transform.position;
        Vector3 targetPos = targetNode.transform.position;
        float offset = (startPos - targetPos).magnitude * randomScaleOnMainPath;

        //Direct vector
        Vector3 directVector = Vector3.Lerp(startPos, targetPos, (1.0f / nodeCount) * (nodeNumber + 1.0f));
        //Vector with random offset
        Vector3 randomVector = directVector + new Vector3(Random.Range(-(offset), offset),
                                                          Random.Range(-(offset), offset),
                                                          Random.Range(-(offset), offset));
        //Create node as linear interpolation between offset position, and position along the normal, to ensure the lightning strikes its target
        //Starts as 5050 between direct and random, eventually moving directly to the target for the final node
        return Vector3.Lerp(randomVector, directVector, Mathf.Lerp(0.5f, 1.0f, (1.0f / nodeCount) * (nodeNumber + 1.0f)));
    }


    void CreateBranch(int nodeIndex, Vector3 node)
    {
        float offset = (startNode.transform.position - targetNode.transform.position).magnitude * randomScaleBranchTarget;
        int nodeCountOnBranch = nodeCount - nodeIndex; //Max number of nodes equal nodes remaining of parent branch, so it stops generating when the main path reaches its target
        GameObject branchStart = new GameObject();
        GameObject branchTarget = new GameObject();

        branchStart.transform.position = node;
        Vector3 targetOffset = new Vector3(Random.Range(-(offset), offset),
                                           Random.Range(-(offset), offset),
                                           Random.Range(-(offset), offset));
        branchTarget.transform.position = Vector3.Lerp(node, targetOffset, branchScale * nodeCountOnBranch);

        GameObject branch = Instantiate(lightningObj) as GameObject;
        LightningEffect bg = branch.GetComponent<LightningEffect>();
        bg.parentBranch = this;

        bg.startNode = branchStart;
        bg.targetNode = branchTarget;
        bg.nodeCount = this.nodeCount;

        bg.maxBranchCount = this.maxBranchCount;
        bg.chanceOfBranchAtNode = this.chanceOfBranchAtNode;
        bg.maxBranchesAtNode = this.maxBranchesAtNode;
        bg.branchScale = this.branchScale;
        bg.branchLineWidthMult = this.branchLineWidthMult;

        bg.maxBranchDepth = this.maxBranchDepth - 1;

        bg.preFlashIntensity = this.preFlashIntensity * branchPreFlashIntensityMult;
        bg.flashIntensity = this.preFlashIntensity * branchPreFlashIntensityMult;
        bg.branchPreFlashIntensityMult = this.branchPreFlashIntensityMult;
        bg.branchFlashIntensityMult = this.branchFlashIntensityMult;

        bg.fadeSpeed = fadeSpeed;
        bg.drawSpeed = drawSpeed;

        bg.randomScaleOnMainPath = randomScaleOnMainPath * randomScaleBranchPath;
        bg.randomScaleBranchPath = randomScaleBranchPath;
        bg.isPerpetual = isPerpetual;

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
            if (branchesRemaining > 0 && Random.Range(0.0f, 1.0f) <= chanceOfBranchAtNode)
            {
                int numBranchesatPoint = Mathf.Min(branchesRemaining, Random.Range(1, maxBranchesAtNode));
                for (int j = 0; j < numBranchesatPoint; j++)
                {
                    CreateBranch(i, node);
                    branchesRemaining--;
                }
            }
        }
}
