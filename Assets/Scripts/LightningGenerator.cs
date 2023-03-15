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
    [SerializeField] int branchCount;
    [SerializeField] float chanceOfBranchAtNode;
    [SerializeField] int maxBranchesAtNode;
    [SerializeField] int nodeCountPerBranch;
    [SerializeField] float branchScale;
    [SerializeField] float branchLineWidthMult;

    [Header("Subbranches")]
    [SerializeField] int subBranchCount;
    [SerializeField] int maxBranchesAtSubNode;
    [SerializeField] int nodeCountPerSubBranch;
    [SerializeField] float subBranchScale;

    [Header("Other")]
    [SerializeField] float randomScaleOnMainPath;
    [SerializeField] float randomScaleOnBranches;
    [SerializeField] float drawSpeed;


    [Header("DO NOT CHANGE")]
    [SerializeField] GameObject lightningBranch;

    LightningGenerator parentBranch;
    LineRenderer lr;
    Vector3[] nodeArray;
    int drawPointer;
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
        nodeArray = new Vector3[nodeCount];

        nodeArray = GeneratePath(startNode.transform.position, targetNode.transform.position, randomScaleOnMainPath, nodeCount);

        branchesRemaining = branchCount;

        lr = GetComponent<LineRenderer>();
        lr.positionCount = 1;
        drawPointer = 0;
        error = 0.0f;

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
            while (error >= 0.5 && drawPointer < nodeArray.Length)
            {
                DrawPathSection(drawPointer);
                drawPointer++;
                error--;
            }
            if (drawPointer >= nodeArray.Length)
            {
                currentStage = Stage.Flash;
            }
        }

    }

    Vector3[] GeneratePath(Vector3 start, Vector3 target, float randomMult, int sectionCount)
    {
        float offset = (start - target).magnitude * randomMult;

        Vector3[] outputArray = new Vector3[sectionCount];

        outputArray[0] = start;

        for (int i = 1; i < sectionCount; i++)
        {
            //Direct vector
            Vector3 directVector = Vector3.Lerp(start, target, (1.0f / sectionCount) * (i + 1.0f));
            //Vector with random offset
            Vector3 randomVector = directVector + new Vector3(Random.Range(-(offset), offset),
                                                              Random.Range(-(offset), offset),
                                                              Random.Range(-(offset), offset));
            //Create node as linear interpolation between offset position, and position along the normal, to ensure the lightning strikes its target
            //Starts as 5050 between direct and random, eventually moving directly to the target for the final node
            Vector3 newNode = Vector3.Lerp(randomVector, directVector, Mathf.Lerp(0.5f, 1.0f, (1.0f / sectionCount) * (i + 1.0f)));
            outputArray[i] = newNode;
        }

        return outputArray;

    }

    void CreateBranch(Vector3 branchPoint)
    {
        float offset = (startNode.transform.position - targetNode.transform.position).magnitude * randomScaleOnBranches;

        GameObject branchStart = new GameObject();
        GameObject branchTarget = new GameObject();

        branchStart.transform.position = branchPoint;
        Vector3 targetOffset = new Vector3(Random.Range(-(offset), offset),
                                           Random.Range(-(offset), offset),
                                           Random.Range(-(offset), offset));
        branchTarget.transform.position = Vector3.Lerp(branchPoint, targetOffset, branchScale * nodeCountPerBranch);

        GameObject branch = Instantiate(lightningBranch) as GameObject;
        LightningGenerator branchGenerator = branch.GetComponent<LightningGenerator>();
        branchGenerator.startNode = branchStart;
        branchGenerator.targetNode = branchTarget;
        branchGenerator.nodeCount = nodeCountPerBranch;
        branchGenerator.branchCount = subBranchCount;
        branchGenerator.subBranchCount = 0;
        branchGenerator.randomScaleOnMainPath = randomScaleOnMainPath;
        branchGenerator.branchScale = subBranchScale;
        branchGenerator.parentBranch = this;

        LineRenderer branchLine = branch.GetComponent<LineRenderer>();
        branchLine.widthMultiplier *= branchLineWidthMult;
    }

    void DrawPathSection(int i)
    {
        lr.positionCount = i + 1;
        lr.SetPosition(i, nodeArray[i]);

        if (branchesRemaining > 0 && Random.Range(0.0f, 1.0f) <= chanceOfBranchAtNode)
        {
            int numBranchesatPoint = Mathf.Min(branchesRemaining, Random.Range(1, maxBranchesAtNode));
            for (int j = 0; j < numBranchesatPoint; j++)
            {
                CreateBranch(nodeArray[i]);
                branchesRemaining--;
            }
        }

    }

}
