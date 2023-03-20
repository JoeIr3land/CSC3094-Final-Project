using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningGenerator : MonoBehaviour
{

    [Header("Main Path")]
    [SerializeField] GameObject startNode;
    [SerializeField] GameObject targetNode;
    [SerializeField] int nodeCount;

    [Header("Branches")]
    [SerializeField] int maxBranchCount;
    [SerializeField] float chanceOfBranchAtNode;
    [SerializeField] int maxBranchesAtNode;
    [SerializeField] float branchScale;
    [SerializeField] float branchLineWidthMult;

    [Header("Subbranches")]
    [SerializeField] int maxBranchDepth;

    [Header("Light")]
    [SerializeField] float preFlashIntensity;
    [SerializeField] float flashIntensity;
    [SerializeField] float fadeSpeed;

    [Header("Line")]
    [SerializeField] Color emissionColor;
    [SerializeField] Color fadedemissionColor;

    [Header("Other")]
    [SerializeField] float randomScaleOnMainPath;
    [SerializeField] float randomScaleOnBranches;
    [SerializeField] float drawSpeed;
    [SerializeField] bool isPerpetual;


    [Header("DO NOT CHANGE")]
    [SerializeField] GameObject lightningObj;


    LightningGenerator parentBranch;
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
            /*error += Time.deltaTime * drawSpeed;
            while (error >= 0.5 && nodePointer < nodeCount)
            {
                Vector3 newNode = GenerateNode(nodePointer);
                if(maxBranchDepth > 1)
                {
                    CalculateIfBranch(nodePointer, newNode);
                }
                DrawNode(nodePointer, newNode);
                transform.position = newNode;
                nodePointer++;
                error--;
            }*/
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
                lineMaterial.SetColor("_EmissionColor", Color.Lerp(emissionColor, fadedemissionColor, timeSinceFlash * fadeSpeed));
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
        float offset = (startNode.transform.position - targetNode.transform.position).magnitude * randomScaleOnBranches;
        int nodeCountOnBranch = nodeCount - nodeIndex; //Max number of nodes equal nodes remaining of parent branch, so it stops generating when the main path reaches its target
        GameObject branchStart = new GameObject();
        GameObject branchTarget = new GameObject();

        branchStart.transform.position = node;
        Vector3 targetOffset = new Vector3(Random.Range(-(offset), offset),
                                           Random.Range(-(offset), offset),
                                           Random.Range(-(offset), offset));
        branchTarget.transform.position = Vector3.Lerp(node, targetOffset, branchScale * nodeCountOnBranch);

        GameObject branch = Instantiate(lightningObj) as GameObject;
        LightningGenerator branchGenerator = branch.GetComponent<LightningGenerator>();
        branchGenerator.startNode = branchStart;
        branchGenerator.targetNode = branchTarget;
        branchGenerator.nodeCount = this.nodeCount;
        branchGenerator.maxBranchCount = this.maxBranchCount;
        branchGenerator.randomScaleOnMainPath = this.randomScaleOnMainPath;
        branchGenerator.branchScale = this.branchScale;
        branchGenerator.maxBranchDepth = this.maxBranchDepth - 1;

        branchGenerator.parentBranch = this;

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
