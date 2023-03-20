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

    [Header("Other")]
    [SerializeField] float randomScaleOnMainPath;
    [SerializeField] float randomScaleOnBranches;
    [SerializeField] float drawSpeed;


    [Header("DO NOT CHANGE")]
    [SerializeField] GameObject lightningBranch;

    LightningGenerator parentBranch;
    LineRenderer lr;
    int nodePointer;
    float error;
    enum Stage
    {
        Grow,
        Flash,
        Fade,
        Ended
    }
    Stage currentStage;
    int branchesRemaining;


    // Start is called before the first frame update
    void Start()
    {
        branchesRemaining = maxBranchCount;
        nodePointer = 1;
        error = 0.0f;

        lr = GetComponent<LineRenderer>();
        lr.positionCount = 1;
        lr.SetPosition(0, startNode.transform.position);

        currentStage = Stage.Grow;

    }

    // Update is called once per frame
    void Update()
    {
        if (parentBranch) //If this instance is a branch of a bigger lightning section, then stop growing when parent reaches destination
        {
            currentStage = parentBranch.currentStage;
        }

        if (currentStage == Stage.Grow)
        {
            Debug.Log(error);
            error += Time.deltaTime * drawSpeed;
            while (error >= 0.5 && nodePointer < nodeCount)
            {
                Vector3 newNode = GenerateNode(nodePointer);
                if(maxBranchDepth > 1)
                {
                    CalculateIfBranch(nodePointer, newNode);
                }
                DrawNode(nodePointer, newNode);
                nodePointer++;
                error--;
            }
            if (nodePointer >= nodeCount)
            {
                currentStage = Stage.Flash;
            }
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

        GameObject branch = Instantiate(lightningBranch) as GameObject;
        LightningGenerator branchGenerator = branch.GetComponent<LightningGenerator>();
        branchGenerator.startNode = branchStart;
        branchGenerator.targetNode = branchTarget;
        branchGenerator.nodeCount = nodeCountOnBranch;
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
