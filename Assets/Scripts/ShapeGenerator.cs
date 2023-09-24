using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent (typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]

public class ShapeGenerator : MonoBehaviour
{
    Mesh mesh;
    MeshCollider meshCol;
    void Awake()
    {
        mesh = new Mesh();
        meshCol = GetComponent<MeshCollider>();
        GetComponent<MeshFilter>().mesh = mesh;
    }
    public void DrawCircle(int _segments, float _radius, float _angle) //soh cah toa
    {
        List<Vector3> GetCircumferencePoints(int sides, float radius)   
        {
            List<Vector3> points = new List<Vector3>();
            float circumferenceProgressPerStep = (float)1/sides;
            float TAU = 2*Mathf.PI;
            float radianProgressPerStep = circumferenceProgressPerStep*TAU;
            
            points.Add(Vector3.zero);
            for(int i = 0; i<sides + 1; i++)
            {
                float currentRadian = radianProgressPerStep*i;
                points.Add(new Vector3(Mathf.Cos(currentRadian)*radius, Mathf.Sin(currentRadian)*radius,0)); //this puts points on a circle
            }
            return points;
        }
        
        int[] DrawFilledTriangles(Vector3[] points)
        {   
            int triangleAmount = points.Length - 2;
            List<int> newTriangles = new List<int>();
            for(int i = 0; i<triangleAmount; i++)
            {
                newTriangles.Add(0);
                newTriangles.Add(i+2);
                newTriangles.Add(i+1);
            }
            return newTriangles.ToArray();
        }

    }
    public void DrawQuad(float _width, float _length)
    {

    }
}
