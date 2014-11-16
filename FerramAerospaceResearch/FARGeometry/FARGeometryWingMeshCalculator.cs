﻿using System;
using System.Collections.Generic;
using System.Linq;
using KSP;
using UnityEngine;
using ferram4;

namespace FerramAerospaceResearch.FARGeometry
{
    public class FARGeometryWingMeshCalculator
    {
        private Part wingPart;

        public FARGeometryWingMeshCalculator(Part p)
        {
            wingPart = p;
        }

        public List<FARGeometryPoint> CalculateWingPlanformPoints()
        {
            List<Vector3d> vertices = GetPlanformVertices();

            return GrahamsScan(vertices);
        }

        #region Graham's Scan for Convex Hull

        //Something to make the turning of the algorithm clearer
        private enum TURN
        {
            LEFT,
            RIGHT,
            NONE
        }

        //Calculates whether the turn is right, left, or not at all for a path going from vert1 to vert2 to vert3
        private TURN TurnDirection(Vector3d vert1, Vector3d vert2, Vector3d vert3)
        {
            double tmp = (vert2[0] - vert1[0]) * (vert3[1] - vert1[1]) - (vert3[0] - vert1[0]) * (vert2[1] - vert1[1]);
            if (tmp > 0)
                return TURN.LEFT;

            if (tmp < 0)
                return TURN.RIGHT;

            return TURN.NONE;
        }

        private List<Vector3d> CorrectHullMistakes(List<Vector3d> hull, Vector3d r)
        {
            while (hull.Count > 1 && TurnDirection(hull[hull.Count - 2], hull[hull.Count - 1], r) != TURN.LEFT)
                hull.RemoveAt(hull.Count - 1);

            if(hull.Count == 0 || hull[hull.Count - 1] != r)
                hull.Add(r);
            return hull;
        }

        private List<Vector3d> GrahamsScanVerts(List<Vector3d> verts)
        {
            Debug.Log(verts.Count);
            //verts = VectorMergeSort(verts, 0);
            verts.Sort(new Vector3dXComparer());
            Debug.Log(verts.Count);
            List<Vector3d> l = new List<Vector3d>();
            List<Vector3d> u = new List<Vector3d>();
            for (int i = 0; i < verts.Count; i++)
            {
                l = CorrectHullMistakes(l, verts[i]);
                u = CorrectHullMistakes(u, verts[verts.Count - (1 + i)]);
            }

            verts = l;
            verts.AddRange(u.GetRange(1, u.Count - 2));

            return verts;
        }

        private List<FARGeometryPoint> GrahamsScan(List<Vector3d> verts)
        {
            verts = GrahamsScanVerts(verts);

            List<FARGeometryPoint> points = new List<FARGeometryPoint>();
            for (int i = 0; i < verts.Count; i++)
            {
                FARGeometryPoint newPoint = new FARGeometryPoint(verts[i]);
                points.Add(newPoint);
            }
            for (int i = 0; i < verts.Count; i++)
            {
                int ip1 = i + 1;
                int im1 = i - 1;

                if(ip1 == verts.Count)
                    ip1 = 0;
                if(im1 == -1)
                    im1 = verts.Count - 1;

                points[i].connectedPoints.Add(points[ip1]);
                points[i].connectedPoints.Add(points[im1]);
            }
            return points;
        }

        public class Vector3dXComparer : Comparer<Vector3d>
        {
            public override int Compare(Vector3d x, Vector3d y)
            {
                if (x.x > x.y)
                    return 1;
                else
                    return -1;
            }
        }
        /*
        private List<Vector3d> VectorMergeSort(List<Vector3d> list, int sortIndex)
        {
            // Base case. A list of zero or one elements is sorted, by definition.
            if (list.Count <= 1)
                return list;

            int middle = (int)(list.Count * 0.5);

            // Recursive case. First, *divide* the list into equal-sized sublists.
            List<Vector3d> left = list.GetRange(0, middle);
            List<Vector3d> right = list.GetRange(middle, list.Count - middle);

            left = VectorMergeSort(left, sortIndex);
            right = VectorMergeSort(right, sortIndex);

            return VectorMerge(left, right, sortIndex);
        }

        private List<Vector3d> VectorMerge(List<Vector3d> left, List<Vector3d> right, int sortIndex)
        {
            List<Vector3d> result = new List<Vector3d>();
            while (left.Count > 0 || right.Count > 0)
            {
                if (left.Count > 0 && right.Count > 0)
                    if (left[0][sortIndex] <= right[0][sortIndex])
                    {
                        result.Add(left[0]);
                        left.RemoveAt(0);
                    }
                    else
                    {
                        result.Add(right[0]);
                        right.RemoveAt(0);
                    }
                else if (left.Count > 0)
                {
                    result.Add(left[0]);
                    left.RemoveAt(0);
                }
                else if (right.Count > 0)
                {
                    result.Add(right[0]);
                    right.RemoveAt(0);
                }
            }
            return result;
        }*/


        #endregion

        private List<Vector3d> GetPlanformVertices()
        {
            //Get all the transforms that this part has that are models
            Transform[] modelTransforms = FARGeoUtil.PartModelTransformArray(wingPart);

            //Get a transformation from world space to part-local space, for converting vertex locations to part-space
            Matrix4x4 partMatrix = wingPart.transform.worldToLocalMatrix;

            List<Vector3d> vertices = new List<Vector3d>();

            foreach (Transform t in modelTransforms)
            {
                MeshFilter mf = t.GetComponent<MeshFilter>();
                if (mf == null)
                    continue;
                Mesh m = mf.mesh;

                if (m == null)
                    continue;

                //Get a matrix for converting mesh-local points to part-local space
                Matrix4x4 orientMatrix = partMatrix * t.localToWorldMatrix;

                foreach (Vector3 vertex in m.vertices)
                {
                    Vector3 v = orientMatrix.MultiplyPoint(vertex); //Apply to this vertex
                    v.z = 0;        //Wings must be flat along part.forward, which is measured in Vector3.z, so we set this to 0 to flatten the points out
                    vertices.Add(v);
                }
            }
            return vertices;
        }
    }
}