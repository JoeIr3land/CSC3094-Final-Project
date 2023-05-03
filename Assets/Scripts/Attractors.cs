using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractors : MonoBehaviour
{
    public Vector3 sourcePos;
    public Vector3 targetPos;
    public int num_attractors;
    public float sphereCentre;
    public float sphereRadius;

    public List<Vector3> attractorsList;


    // Start is called before the first frame update
    void Start()
    {
        attractorsList = new List<Vector3>();
        GenerateAttractors();
    }

    private void GenerateAttractors()
    {
        float sphereRadius_Adjusted = sphereRadius * 0.5f * ((targetPos - sourcePos).magnitude); //sphereRadius refers to radius size relative to halfway distance between source and target
        for (int i = 0; i < num_attractors; i++)
        {
            //Initially place attractor at a point between source and target
            Vector3 newAttractor = Vector3.Lerp(sourcePos, targetPos, sphereCentre); //sphereCenter refers to where along the line between source and target
            Vector3 randomVector = new Vector3(Random.Range(-sphereRadius_Adjusted, sphereRadius_Adjusted),
                                               Random.Range(-sphereRadius_Adjusted, sphereRadius_Adjusted),
                                               Random.Range(-sphereRadius_Adjusted, sphereRadius_Adjusted));
            newAttractor += randomVector;
            attractorsList.Add(newAttractor);
        }
    }

    public List<Vector3> GetAttractorsInRange(Vector3 pos, float bound)
    {
        List<Vector3> attractorsInRange = new List<Vector3>();
        for (int i = 0; i < attractorsList.Count; i++)
        {
            float distance = (attractorsList[i] - pos).magnitude;
            if (distance <= bound) { attractorsInRange.Add(attractorsList[i]); }
        }
        return attractorsInRange;
    }

    public bool KillAttractorsInRange(Vector3 pos, float bound)
    {
        int i = 0;
        bool doneIterating = false;
        bool deleteoccurred = false;
        while (!doneIterating)
        {
            float distance = (attractorsList[i] - pos).magnitude;
            if (distance <= bound)
            {
                attractorsList.RemoveAt(i);
                deleteoccurred = true;
            }
            else { i++; }
            if(i >= attractorsList.Count) { doneIterating = true; }
        }
        return deleteoccurred;
    }
}
