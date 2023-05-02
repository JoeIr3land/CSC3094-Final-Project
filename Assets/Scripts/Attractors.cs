using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractors : MonoBehaviour
{
    public Vector3 sourcePos;
    public Vector3 targetPos;
    public int num_attractors;

    public List<Vector3> attractorsList;


    // Start is called before the first frame update
    void Start()
    {
        attractorsList = new List<Vector3>();
        GenerateAttractors();
    }

    private void GenerateAttractors()
    {
        for (int i = 0; i < num_attractors; i++)
        {
            Vector3 newAttractor = new Vector3(Random.Range(sourcePos.x, targetPos.x),
                                               Random.Range(sourcePos.y, targetPos.y),
                                               Random.Range(sourcePos.z, targetPos.z));
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
