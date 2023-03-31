using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningCreator : MonoBehaviour
{
    [Header("Main Path")]
    [SerializeField] public GameObject startObj;
    [SerializeField] public GameObject targetObj;
    [SerializeField] public float nodeScale;
    [SerializeField] public float targetOuterThreshold;
    [SerializeField] public float targetInnerThreshold;

    [Header("Branches")]
    [SerializeField] public int maxBranchCount;
    [SerializeField] public float chanceOfBranchAtNode;
    [SerializeField] public float chanceOfBranchScaleMult;
    [SerializeField] public float minNodesBetweenBranching;
    [SerializeField] public int maxBranchesAtNode;
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
    [SerializeField] public float maxAngleDirectionChange;
    [SerializeField] public float randomnessWeight;
    [SerializeField] public float randomnessWeightBranchMult;

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
            CreateLightningBolt(startObj, targetObj);
        }
    }

    void CreateLightningBolt(GameObject start, GameObject target)
    {
        GameObject lightningBolt = Instantiate(lightningObj) as GameObject;
        LightningEffect le = lightningBolt.GetComponent<LightningEffect>();
        le.startNode = start.transform.position;
        le.targetNode = target.transform.position;
        le.nodeScale = nodeScale;
        le.targetOuterThreshold = targetOuterThreshold;
        le.targetInnerThreshold = targetInnerThreshold;

        le.maxBranchCount = maxBranchCount;
        le.chanceOfBranchAtNode = chanceOfBranchAtNode;
        le.chanceOfBranchScaleMult = chanceOfBranchScaleMult;
        le.minNodesBetweenBranching = minNodesBetweenBranching;
        le.maxBranchesAtNode = maxBranchesAtNode;
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

        le.maxAngleDirectionChange = maxAngleDirectionChange;
        le.randomnessWeight = randomnessWeight;
        le.randomnessWeightBranchMult = randomnessWeightBranchMult;

        le.isPerpetual = isPerpetual;

    }
}
