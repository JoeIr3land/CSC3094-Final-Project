using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningCreator : MonoBehaviour
{
    [Header("Source/Target")]
    [SerializeField] public Vector3 source;
    [SerializeField] public Vector3 target;

    [Header("Main Path")]
    [SerializeField] public float segmentSize;
    [SerializeField] public float targetOuterThreshold;
    [SerializeField] public float targetInnerThreshold;

    [Header("Branches")]
    [SerializeField] public int maxBranchCount;
    [SerializeField] public float chanceOfBranchAtPosition;
    [SerializeField] public float chanceOfBranchScaleMult;
    [SerializeField] public float minSegmentsBetweenBranching;
    [SerializeField] public int maxBranchesAtPosition;
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
    [SerializeField] public float lineWidth;
    [SerializeField] public float dartLeaderLineWidth;

    [Header("Animation")]
    [SerializeField] public float fadeSpeed;
    [SerializeField] public float drawSpeed;
    [SerializeField] public int numReturnStrokes;
    [SerializeField] public float returnStrokeSpeed;
    [SerializeField] public float dartLeaderFadeSpeed;

    [Header("Path Randomness")]
    [SerializeField] public float maxAngleDirectionChange;
    [SerializeField] public float randomnessWeight;
    [SerializeField] public float randomnessWeightBranchMult;

    [Header("Other")]
    [SerializeField] public bool isPerpetual;

    [Header("Lightning Effect Type")]
    [SerializeField] protected GameObject lightningObj;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            CreateLightningBolt(source, target);
        }
    }

    void CreateLightningBolt(Vector3 start, Vector3 target)
    {
        GameObject lightningBolt = Instantiate(lightningObj) as GameObject;
        LightningEffect le = lightningBolt.GetComponent<LightningEffect>();
        le.sourcePos = start;
        le.targetPos = target;
        le.segmentSize = segmentSize;
        le.targetOuterThreshold = targetOuterThreshold;
        le.targetInnerThreshold = targetInnerThreshold;

        le.maxBranchCount = maxBranchCount;
        le.chanceOfBranchAtPosition = chanceOfBranchAtPosition;
        le.chanceOfBranchScaleMult = chanceOfBranchScaleMult;
        le.minSegmentsBetweenBranching = minSegmentsBetweenBranching;
        le.maxBranchesAtPosition = maxBranchesAtPosition;
        le.branchLineWidthMult = branchLineWidthMult;

        le.maxBranchDepth = maxBranchDepth;

        le.preFlashIntensity = preFlashIntensity;
        le.flashIntensity = flashIntensity;
        le.branchPreFlashIntensityMult = branchPreFlashIntensityMult;
        le.branchFlashIntensityMult = branchFlashIntensityMult;

        le.emissionColor = emissionColor;
        le.fadedEmissionColor = fadedEmissionColor;
        le.dartLeaderLineWidth = dartLeaderLineWidth;

        le.fadeSpeed = fadeSpeed;
        le.drawSpeed = drawSpeed;
        le.numReturnStrokes = numReturnStrokes;
        le.returnStrokeSpeed = returnStrokeSpeed;
        le.dartLeaderFadeSpeed = dartLeaderFadeSpeed;

        le.maxAngleDirectionChange = maxAngleDirectionChange;
        le.randomnessWeight = randomnessWeight;
        le.randomnessWeightBranchMult = randomnessWeightBranchMult;

        le.isPerpetual = isPerpetual;

        LineRenderer lr = lightningBolt.GetComponent<LineRenderer>();
        lr.widthMultiplier = lineWidth;

    }
}
