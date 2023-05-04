using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Octree
{
    public struct Node
    {
        public Node[] subNodes;
        public List<Vector3> attractorList;
        public Bounds boundary;
        public bool isLeaf;
    }

    public Node root;
    public int maxAttractorsPerOctree;

    public Vector3 rootCentre;
    public float boundsRadius;
    public List<Vector3> allAttractors;

    public void Start()
    {
        root = ConstructNode(allAttractors, rootCentre, boundsRadius);
        allAttractors.Clear(); //Clear space - allAttractors no longer needed
    }


    private Node ConstructNode(List<Vector3> attractorsInPrevNode, Vector3 nodeCentre, float nodeExtents)
    {
        Node newNode = new Node();
        newNode.boundary = new Bounds(nodeCentre, new Vector3(2 * nodeExtents, 2 * nodeExtents, 2 * nodeExtents));
        newNode.attractorList = new List<Vector3>();
        newNode.subNodes = new Node[8];

        List<Vector3> attractorsWithinNode = InsertAttractors(newNode, attractorsInPrevNode);
        if (attractorsWithinNode.Count > maxAttractorsPerOctree) //If node is full, divide into 8 subnodes
        {
            newNode.isLeaf = false;
            float subNodeExtents = nodeExtents / 2;
            Vector3[] subNodeCentres = new Vector3[8];
            subNodeCentres[0] = nodeCentre + new Vector3(subNodeExtents, subNodeExtents, subNodeExtents);
            subNodeCentres[1] = nodeCentre + new Vector3(-subNodeExtents, subNodeExtents, subNodeExtents);
            subNodeCentres[2] = nodeCentre + new Vector3(subNodeExtents, -subNodeExtents, subNodeExtents);
            subNodeCentres[3] = nodeCentre + new Vector3(subNodeExtents, subNodeExtents, -subNodeExtents);
            subNodeCentres[4] = nodeCentre + new Vector3(-subNodeExtents, -subNodeExtents, subNodeExtents);
            subNodeCentres[5] = nodeCentre + new Vector3(subNodeExtents, -subNodeExtents, -subNodeExtents);
            subNodeCentres[6] = nodeCentre + new Vector3(-subNodeExtents, subNodeExtents, -subNodeExtents);
            subNodeCentres[7] = nodeCentre + new Vector3(-subNodeExtents, -subNodeExtents, -subNodeExtents);
            for (int i=0; i<8; i++)
            {
                newNode.subNodes[i] = ConstructNode(attractorsWithinNode, subNodeCentres[i], subNodeExtents);
            }
            newNode.attractorList.Clear(); //Clear list of attractors since it will not be accessed on non-leaf nodes and is no longer needed
        }
        else
        {
            newNode.isLeaf = true;
        }
        return newNode;
    }


    private List<Vector3> InsertAttractors(Node node, List<Vector3> attractors)
    {
        Debug.Log(attractors.Count);
        foreach (Vector3 attractor in attractors)
        {
            if (node.boundary.Contains(attractor))
            {
                node.attractorList.Add(attractor);
            }
        }
        return node.attractorList;
    }

    public List<Vector3> Search(Vector3 pos, float bound)
    {
        Bounds posBounds = new Bounds(pos, new Vector3(2 * bound, 2 * bound, 2 * bound));
        return CheckTree(posBounds, root);
    }

    private List<Vector3> CheckTree(Bounds posBoundary, Node node)
    {
        List<Vector3> attractors = new List<Vector3>();
        if (posBoundary.Intersects(node.boundary))
        {
            if (node.isLeaf)
            {
                attractors = node.attractorList;
            }
            else
            {
                foreach (Node subNode in node.subNodes)
                {
                    attractors.AddRange(CheckTree(posBoundary, subNode));
                }
            }
        }
        return attractors;
    }

    public void RemoveAttractor(Vector3 attractor, Node node)
    {
        if (node.boundary.Contains(attractor))
        {
            if (node.isLeaf)
            {
                node.attractorList.Remove(attractor);
            }
            else
            {
                foreach(Node subNode in node.subNodes)
                {
                    RemoveAttractor(attractor, subNode);
                }
            }
        }
    }

}
