using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractors : MonoBehaviour
{
    public Vector3 sourcePos;
    public Vector3 targetPos;
    public int num_attractors;
    public float boundsCentre;
    public float boundsRadius;

    public Octree attractors;
    public int maxAttractorsPerOctree;

    // Start is called before the first frame update
    void Start()
    {
        attractors = GenerateAttractors();
    }

    private Octree GenerateAttractors()
    {
        List<Vector3> attractorsList = new List<Vector3>();
        float boundsRadius_InWorldCoords = boundsRadius * 0.5f * ((targetPos - sourcePos).magnitude); //boundsRadius refers to radius size relative to halfway distance between source and target
        Vector3 boundsCentre_InWorldCoords = Vector3.Lerp(sourcePos, targetPos, boundsCentre); //boundsCentre refers to where along the line between source and target
        for (int i = 0; i < num_attractors; i++)
        {
            //Initially place attractor at a point between source and target
            Vector3 newAttractor = boundsCentre_InWorldCoords; 
            Vector3 randomVector = new Vector3(Random.Range(-boundsRadius_InWorldCoords, boundsRadius_InWorldCoords),
                                               Random.Range(-boundsRadius_InWorldCoords, boundsRadius_InWorldCoords),
                                               Random.Range(-boundsRadius_InWorldCoords, boundsRadius_InWorldCoords));
            newAttractor += randomVector;
            attractorsList.Add(newAttractor);
        }

        Octree ot = new Octree();
        ot.rootCentre = boundsCentre_InWorldCoords;
        ot.boundsRadius = boundsRadius_InWorldCoords;
        ot.allAttractors = attractorsList;
        ot.maxAttractorsPerOctree = maxAttractorsPerOctree;
        ot.Start();
        return ot;
    }


    public List<Vector3> GetAttractorsInRange(Vector3 pos, float bound)
    {
        List<Vector3> possibleAttractors =  attractors.Search(pos, bound);
        List<Vector3> attractorsInRange = new List<Vector3>();
        for (int i = 0; i < possibleAttractors.Count; i++)
        {
            float distance = (possibleAttractors[i] - pos).magnitude;
            if (distance <= bound) { attractorsInRange.Add(possibleAttractors[i]); }
        }
        return attractorsInRange;
    }


    public bool KillAttractorsInRange(Vector3 pos, float bound)
    {
        List<Vector3> attractorsToCheck = attractors.Search(pos, bound);
        bool deleteOccurred = false;
        if(attractorsToCheck != null)
        {
            for(int i=0; i< attractorsToCheck.Count; i++)
            {
                float distance = (attractorsToCheck[i] - pos).magnitude;
                if (distance <= bound)
                {
                    attractors.RemoveAttractor(attractorsToCheck[i], attractors.root);
                    deleteOccurred = true;
                }
            }
        }
        return deleteOccurred;
    }


}
