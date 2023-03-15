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

    [Header("Subbranches")]
    [SerializeField] int subBranchCount;
    [SerializeField] int maxBranchesAtSubNode;
    [SerializeField] int nodeCountPerSubBranch;
    [SerializeField] float subBranchScale;

    [Header("Other")]
    [SerializeField] float randomScaleOnMainPath;
    [SerializeField] float randomScaleOnBranches;


    [Header("DO NOT CHANGE")]
    [SerializeField] GameObject lightningBranch;

    LineRenderer lr;
    Vector3[] nodeArray;
    List<Vector3> branchList;


    // Start is called before the first frame update
    void Start()
    {
        nodeArray = new Vector3[nodeCount];
        branchList = new List<Vector3>();

        nodeArray = GeneratePath(startNode.transform.position, targetNode.transform.position, randomScaleOnMainPath, nodeCount);
        GenerateBranches();

        lr = GetComponent<LineRenderer>();
        lr.positionCount = nodeArray.Length;
        lr.SetPositions(nodeArray);
    }

    // Update is called once per frame
    void Update()
    {
        
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

    void GenerateBranches()
    {
        int branchesRemaining = branchCount;

        for (int i = 0; i < nodeCount-1; i++)
        {
            if(branchesRemaining > 0 && Random.Range(0.0f, 1.0f) <= chanceOfBranchAtNode)
            {
                int numBranchesatPoint = Mathf.Min(branchesRemaining, Random.Range(1, maxBranchesAtNode));
                for (int j = 0; j < numBranchesatPoint; j++)
                {
                    Debug.Log("Adding branch at: ");
                    Debug.Log(nodeArray[i]);
                    branchList.Add(nodeArray[i]);
                    branchesRemaining--;
                }
            }

        }

        for (int i = 1; i < branchList.Count; i++)
        {
            float offset = (startNode.transform.position - targetNode.transform.position).magnitude * randomScaleOnBranches;

            GameObject branchStart = new GameObject();
            GameObject branchTarget = new GameObject();
            branchStart.transform.position = branchList[i];
            Vector3 targetOffset = new Vector3(Random.Range(-(offset), offset),
                                               Random.Range(-(offset), offset),
                                               Random.Range(-(offset), offset));
            branchTarget.transform.position = Vector3.Lerp(branchList[i], targetOffset, branchScale*nodeCountPerBranch);

            GameObject branch = Instantiate(lightningBranch) as GameObject;
            LightningGenerator branchGenerator = branch.GetComponent<LightningGenerator>();
            branchGenerator.startNode = branchStart;
            branchGenerator.targetNode = branchTarget;
            branchGenerator.nodeCount = nodeCountPerBranch;
            branchGenerator.branchCount = subBranchCount;
            branchGenerator.subBranchCount = 0;
            branchGenerator.randomScaleOnMainPath = randomScaleOnMainPath;
            branchGenerator.branchScale = subBranchScale;

        }
    }
    
}
