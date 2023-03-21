using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningCreator : MonoBehaviour
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

    [Header("Lightning Effect Type")]
    [SerializeField] GameObject lightningObj;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            CreateLightningBolt(startNode, targetNode);
        }
    }

    void CreateLightningBolt(GameObject start, GameObject target)
    {
        GameObject lightningBolt = Instantiate(lightningObj) as GameObject;
        LightningEffect le = lightningBolt.GetComponent<LightningEffect>();
        le.startNode = start;
        le.targetNode = target;
        le.nodeCount = nodeCount;

        le.maxBranchCount = maxBranchCount;
        le.chanceOfBranchAtNode = chanceOfBranchAtNode;
        le.maxBranchesAtNode = maxBranchesAtNode;
        le.branchScale = branchScale;
        le.branchLineWidthMult = branchLineWidthMult;

        le.maxBranchDepth = maxBranchDepth;

        le.preFlashIntensity = preFlashIntensity;
        le.flashIntensity = flashIntensity;
        le.branchPreFlashIntensityMult = branchPreFlashIntensityMult;
        le.branchFlashIntensityMult = branchFlashIntensityMult;

        le.emissionColor = emissionColor;
        le.fadedEmissionColor = fadedEmissionColor;

        le.fadeSpeed = fadeSpeed;
        le.drawSpeed = drawSpeed;

        le.randomScaleOnMainPath = randomScaleOnMainPath;
        le.randomScaleBranchTarget = randomScaleBranchTarget;
        le.randomScaleBranchPath = randomScaleBranchPath;

        le.isPerpetual = isPerpetual;

    }
}
