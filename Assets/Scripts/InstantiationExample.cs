using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace Autonomous
{
    [RequireComponent(typeof(SplineCreator))]
    public class InstantiationExample : MonoBehaviour
    {

        public bool drawTrackDebugLines;

        public int currentTrackGenerationSeed;
        
        public GameObject coneBlue;
        public GameObject coneRed;
        public GameObject parent;

        public int randomSeed;
        private SplineCreator splineCreator;
        public float trackThickness = 20f;
        public Material trackWallMaterial;

        private List<Vector3> leftEdge;
        private GameObject leftEdgeChild;
        private List<Vector3> rightEdge;
        private GameObject rightEdgeChild;

        public List<Vector3> trackPoints;

        private void Awake()
        {
            splineCreator = GetComponent<SplineCreator>();
        }
        private void Update()
        {
            // Debug draw
            if (!drawTrackDebugLines) return;
            DrawSpline(trackPoints, Color.red);
            DrawSpline(leftEdge, Color.blue);
            DrawSpline(rightEdge, Color.green);
        }

        public void CreateTrack()
        {
            splineCreator.Seed = currentTrackGenerationSeed;
            GenerateTrackLines();
            CreateAndRenderWallMeshes();
        }

        public void CreateTrack(int seed)
        {
            currentTrackGenerationSeed = seed;
            CreateTrack();
        }

        private void GenerateTrackLines()
        {
            splineCreator.GenerateVoronoi();
            splineCreator.GenerateSpline();
            trackPoints = splineCreator.Spline.AllSmoothPoints;
            var offsetPoint = GetCenterPoint(trackPoints);
            TranslatePoints(trackPoints, offsetPoint);
            GetEdgeSplines(trackPoints, trackThickness, out leftEdge, out rightEdge);
            RemoveSelfIntersections(leftEdge);
            RemoveSelfIntersections(rightEdge);
        }

        private void CreateAndRenderWallMeshes()
        {
            CreateWallMesh(leftEdge);
            CreateWallMesh(rightEdge);
        }


        private GameObject CreateOrUpdateChildWithMesh(string childName, Mesh mesh, GameObject existingChild)
        {
            GameObject child;
            if (existingChild == null)
            {
                // If there's no existing child, create a new one
                child = new GameObject(childName);
                child.transform.SetParent(transform);
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;

                // Add a MeshFilter, MeshRenderer, and MeshCollider
                child.AddComponent<MeshFilter>();
                child.AddComponent<MeshRenderer>();
                child.AddComponent<MeshCollider>();
            }
            else
            {
                child = existingChild;
            }

            // Update the MeshFilter with the new mesh
            var meshFilter = child.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            // Set the material for the MeshRenderer
            var meshRenderer = child.GetComponent<MeshRenderer>();
            meshRenderer.material = trackWallMaterial;

            // Set the mesh for the MeshCollider
            var meshCollider = child.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            return child;
        }


        private void DrawSpline(List<Vector3> spline, Color colour)
        {
            var prevPoint = spline[0];
            foreach (var point in spline)
            {
                Debug.DrawLine(prevPoint, point, colour);
                prevPoint = point;
            }

            Debug.DrawLine(spline.Last(), spline[0], colour);
        }

        private void CreateWallMesh(List<Vector3> points)
        {

            for (var i = 0; i < points.Count; i++)
            {
                Instantiate(coneRed, points[i], Quaternion.identity);
            }

        }

        public Vector3 GetPositionOnSpline(List<Vector3> spline, float percentage, out Quaternion rotation)
        {
            if (spline == null || spline.Count < 2)
            {
                rotation = Quaternion.identity;
                return Vector3.zero;
            }

            var totalLength = 0f;
            for (var i = 0; i < spline.Count - 1; i++) totalLength += Vector3.Distance(spline[i], spline[i + 1]);

            var targetLength = totalLength * percentage;
            var accumulatedLength = 0f;

            for (var i = 0; i < spline.Count - 1; i++)
            {
                var segmentLength = Vector3.Distance(spline[i], spline[i + 1]);
                if (accumulatedLength + segmentLength >= targetLength)
                {
                    var segmentPercentage = (targetLength - accumulatedLength) / segmentLength;
                    var direction = (spline[i + 1] - spline[i]).normalized;
                    rotation = Quaternion.LookRotation(direction);
                    return Vector3.Lerp(spline[i], spline[i + 1], segmentPercentage);
                }

                accumulatedLength += segmentLength;
            }

            var endDirection = (spline[spline.Count - 1] - spline[spline.Count - 2]).normalized;
            rotation = Quaternion.LookRotation(endDirection);
            return spline[spline.Count - 1];
        }


        private static Vector3 GetCenterPoint(List<Vector3> points)
        {
            if (points == null || points.Count == 0) throw new ArgumentException("List of points is null or empty.");

            float sumX = 0;
            float sumY = 0;
            float sumZ = 0;

            foreach (var point in points)
            {
                sumX += point.x;
                sumY += point.y;
                sumZ += point.z;
            }

            return new Vector3(sumX / points.Count, sumY / points.Count, sumZ / points.Count);
        }

        private static void TranslatePoints(List<Vector3> points, Vector3 translationAmount)
        {
            if (points == null) throw new ArgumentException("List of points is null.");

            for (var i = 0; i < points.Count; i++) points[i] -= translationAmount;
        }

        private static void GetEdgeSplines(List<Vector3> spline, float thickness,
                                           out List<Vector3> leftEdge, out List<Vector3> rightEdge)
        {
            leftEdge = new List<Vector3>();
            rightEdge = new List<Vector3>();

            var perps = new List<Vector3>();

            // Calculate perpendicular vectors for each point
            for (var i = 0; i < spline.Count; i++)
            {
                Vector3 tangent;

                // Calculate the tangent vector
                if (i == 0) // First point
                    tangent = spline[i + 1] - spline[i];
                else if (i == spline.Count - 1) // Last point
                    tangent = spline[i] - spline[i - 1];
                else // Middle points
                    tangent = spline[i + 1] - spline[i - 1];

                // Normalize the tangent
                tangent.Normalize();

                // Compute the perpendicular vector in X-Z (swap x and z and negate the new z)
                var perp = new Vector3(tangent.z, 0, -tangent.x);
                perps.Add(perp);
            }

            // Generate the left and right splines
            for (var i = 0; i < spline.Count; i++)
            {
                var perp = perps[i];

                // For inner edge smoothing: average adjacent perpendicular vectors
                if (i > 0 && i < spline.Count - 1) perp = (perps[i - 1] + perp + perps[i + 1]) / 3.0f;

                leftEdge.Add(spline[i] + perp * (thickness / 2));
                rightEdge.Add(spline[i] - perp * (thickness / 2));
            }
        }

        private static bool DoSegmentsIntersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            // Using determinant method to check for intersection
            var det = (p2.x - p1.x) * (p4.z - p3.z) - (p2.z - p1.z) * (p4.x - p3.x);
            if (det == 0) return false; // Parallel segments

            var lambda = ((p4.z - p3.z) * (p4.x - p1.x) + (p3.x - p4.x) * (p4.z - p1.z)) / det;
            var gamma = ((p1.z - p2.z) * (p4.x - p1.x) + (p2.x - p1.x) * (p4.z - p1.z)) / det;

            return 0 < lambda && lambda < 1 && 0 < gamma && gamma < 1;
        }

        private static List<Vector3> RemoveSelfIntersections(List<Vector3> spline)
        {
            for (var i = 0; i < spline.Count - 1; i++)
            {
                for (var j = i + 2; j < spline.Count - 1; j++)
                    if (DoSegmentsIntersect(spline[i], spline[i + 1], spline[j], spline[j + 1]))
                    {
                        // Remove points between i+1 and j
                        spline.RemoveRange(i + 1, j - i);
                        return RemoveSelfIntersections(spline); // Recursively clean up further intersections
                    }
            }

            return spline;
        }

        private Mesh MakeDoubleSided(Mesh mesh)
        {
            var doubleSidedMesh = new Mesh();

            // Get existing vertices and triangles
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;

            // Create new vertices and triangles arrays
            var newVertices = new Vector3[vertices.Length * 2];
            var newTriangles = new int[triangles.Length * 2];

            // Copy existing data
            vertices.CopyTo(newVertices, 0);
            triangles.CopyTo(newTriangles, 0);

            // Duplicate vertices
            for (var i = 0; i < vertices.Length; i++) newVertices[vertices.Length + i] = vertices[i];

            // Duplicate and reverse triangles
            for (var i = 0; i < triangles.Length; i += 3)
            {
                var offset = vertices.Length;
                newTriangles[triangles.Length + i] = triangles[i + 2] + offset;
                newTriangles[triangles.Length + i + 1] = triangles[i + 1] + offset;
                newTriangles[triangles.Length + i + 2] = triangles[i] + offset;
            }

            // Assign the new vertices and triangles to the mesh
            doubleSidedMesh.vertices = newVertices;
            doubleSidedMesh.triangles = newTriangles;

            // Recalculate normals for the mesh (important to ensure correct lighting/shading)
            doubleSidedMesh.RecalculateNormals();

            return doubleSidedMesh;
        }
    }
}