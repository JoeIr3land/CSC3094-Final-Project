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
    [SerializeField] protected GameObject attractorObj;

    [Header("Space Colonisation Parameters")]
    [SerializeField] public int num_attractors;
    [SerializeField] public float attractorBoundsCentre;
    [SerializeField] public float attractorBoundsRadius;
    [SerializeField] public float attractorInfluenceWeight;
    [SerializeField] public float attractorOuterBound;
    [SerializeField] public float attractorInnerBound;
    [SerializeField] public float randomInfluenceWeight;
    [SerializeField] public float branchAttractorInfluenceWeightMult;
    [SerializeField] public int maxAttractorsPerOctree;


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
        float scaleModifier = (start - target).magnitude / 8.660f; //Create more consistent results with different lightning bolt sizes
        GameObject lightningBolt = Instantiate(lightningObj) as GameObject;
        LightningEffect le = lightningBolt.GetComponent<LightningEffect>();
        le.sourcePos = start;
        le.targetPos = target;
        le.segmentSize = segmentSize * scaleModifier;
        le.targetOuterThreshold = targetOuterThreshold * scaleModifier;
        le.targetInnerThreshold = targetInnerThreshold * scaleModifier;

        le.maxBranchCount = (int)((float)maxBranchCount * scaleModifier);
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
        le.lineWidth = lineWidth * scaleModifier ;
        le.dartLeaderLineWidth = dartLeaderLineWidth * scaleModifier;

        le.fadeSpeed = fadeSpeed;
        le.drawSpeed = drawSpeed;
        le.numReturnStrokes = numReturnStrokes;
        le.returnStrokeSpeed = returnStrokeSpeed;
        le.dartLeaderFadeSpeed = dartLeaderFadeSpeed;

        le.maxAngleDirectionChange = maxAngleDirectionChange;
        le.randomnessWeight = randomnessWeight;
        le.randomnessWeightBranchMult = randomnessWeightBranchMult;

        le.isPerpetual = isPerpetual;

        if(le.GetType() == typeof(LE_SpaceColonisation))
        {
            GameObject attractors = Instantiate(attractorObj) as GameObject;
            Attractors att = attractors.GetComponent<Attractors>();
            att.sourcePos = start;
            att.targetPos = target;
            att.num_attractors = (int)((float)num_attractors * scaleModifier);
            att.boundsCentre = attractorBoundsCentre;
            att.boundsRadius = attractorBoundsRadius;
            att.maxAttractorsPerOctree = maxAttractorsPerOctree;
            le.attractorOuterBound = attractorOuterBound * scaleModifier;
            le.attractorInnerBound = attractorInnerBound * scaleModifier;
            le.attractorObj = attractors;
            le.attractorInfluenceWeight = attractorInfluenceWeight;
            le.randomInfluenceWeight = randomInfluenceWeight;
            le.branchAttractorInfluenceWeightMult = branchAttractorInfluenceWeightMult;
        }

    }
}
