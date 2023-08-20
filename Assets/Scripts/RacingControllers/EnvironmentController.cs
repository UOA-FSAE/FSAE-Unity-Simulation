using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RacingControllers {
    
    /*  TODO!: Create race Controller functions to reset cars and create them.
     *  Create cars
     *  Create cars configs
     *  Create car stats
     *  Create node for dealing with ros2
     *  Fix issue with spline killing its self when its not selected in the editor
     *  Add sim time multiplier
     */
    
    [RequireComponent(typeof(SplineCreator))]
    public class EnvironmentController : MonoBehaviour {
        public bool drawTrackDebugLines = false;

        public int trackGenerationSeed = 0;
        public float trackThickness = 5f;
        public float trackWallHeight = 10f;
        public Material trackWallMaterial;
        public CarController carPrefab;
        
        private List<Vector3> trackPoints;
        private List<Vector3> leftEdge;
        private List<Vector3> rightEdge;

        private Mesh leftEdgeWallMesh;
        private Mesh rightEdgeWallMesh;
        private GameObject leftEdgeChild;
        private GameObject rightEdgeChild;

        private SplineCreator splineCreator;

        private void Start() {
            splineCreator = GetComponent<SplineCreator>();
            
            CreateTrack();
        }

        private void Update() {
            // Debug draw
            if (!drawTrackDebugLines) return;
            DrawSpline(trackPoints, Color.red);
            DrawSpline(leftEdge, Color.blue);
            DrawSpline(rightEdge, Color.green);
        }

        [ContextMenu("Create track")]
        public void CreateTrack() {
            splineCreator.Seed = trackGenerationSeed;
            
            GenerateTrackLines();
            CreateAndRenderWallMeshes();
        }

        [ContextMenu("Create car at start of track")]
        public void CreateCarAtStartOfTrack() {
            CreateCarAtPercentAroundTrack(0f);
        }
        
        public void CreateCarAtPercentAroundTrack(float percentage) {
            Vector3 position = GetPositionOnSpline(trackPoints, percentage, out var rotation);
            Instantiate(carPrefab, position, rotation);
        }
        
        private void GenerateTrackLines() {
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
            leftEdgeWallMesh = CreateWallMesh(leftEdge);
            leftEdgeWallMesh = MakeDoubleSided(leftEdgeWallMesh);
            rightEdgeWallMesh = CreateWallMesh(rightEdge);
            rightEdgeWallMesh = MakeDoubleSided(rightEdgeWallMesh);

            leftEdgeChild = CreateOrUpdateChildWithMesh("LeftEdgeChild", leftEdgeWallMesh, leftEdgeChild);
            rightEdgeChild = CreateOrUpdateChildWithMesh("RightEdgeChild", rightEdgeWallMesh, rightEdgeChild);
        }
        private GameObject CreateOrUpdateChildWithMesh(string childName, Mesh mesh, GameObject existingChild) {
            GameObject child;
            if (existingChild == null)
            {
                // If there's no existing child, create a new one
                child = new GameObject(childName);
                child.transform.SetParent(this.transform);
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
            MeshFilter meshFilter = child.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            // Set the material for the MeshRenderer
            MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
            meshRenderer.material = trackWallMaterial;

            // Set the mesh for the MeshCollider
            MeshCollider meshCollider = child.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            return child;
        }
        private void DrawSpline(List<Vector3> spline, Color colour) {
            var prevPoint = spline[0];
            foreach (var point in spline) {
                Debug.DrawLine(prevPoint, point, colour);
                prevPoint = point;
            }
            Debug.DrawLine(spline.Last(), spline[0], colour);
        }
        private Mesh CreateWallMesh(List<Vector3> points) {
            Mesh mesh = new Mesh();

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            for (int i = 0; i < points.Count; i++)
            {
                // Add bottom vertex
                vertices.Add(points[i]);

                // Add top vertex
                vertices.Add(new Vector3(points[i].x, points[i].y + trackWallHeight, points[i].z));
            }

            // Generate triangles
            for (int i = 0; i < points.Count; i++)
            {
                int bottomLeft = i * 2;
                int topLeft = i * 2 + 1;
                int bottomRight = (i + 1) % points.Count * 2; // Use modulo for looping
                int topRight = (i + 1) % points.Count * 2 + 1; // Use modulo for looping

                // First triangle
                triangles.Add(bottomLeft);
                triangles.Add(topRight);
                triangles.Add(topLeft);

                // Second triangle
                triangles.Add(bottomLeft);
                triangles.Add(bottomRight);
                triangles.Add(topRight);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            return mesh;
        }
        private Vector3 GetPositionOnSpline(List<Vector3> spline, float percentage, out Quaternion rotation) {
            if (spline == null || spline.Count < 2)
            {
                rotation = Quaternion.identity;
                return Vector3.zero;
            }

            float totalLength = 0f;
            for (int i = 0; i < spline.Count - 1; i++)
            {
                totalLength += Vector3.Distance(spline[i], spline[i + 1]);
            }

            float targetLength = totalLength * percentage;
            float accumulatedLength = 0f;

            for (int i = 0; i < spline.Count - 1; i++)
            {
                float segmentLength = Vector3.Distance(spline[i], spline[i + 1]);
                if (accumulatedLength + segmentLength >= targetLength)
                {
                    float segmentPercentage = (targetLength - accumulatedLength) / segmentLength;
                    Vector3 direction = (spline[i + 1] - spline[i]).normalized;
                    rotation = Quaternion.LookRotation(direction);
                    return Vector3.Lerp(spline[i], spline[i + 1], segmentPercentage);
                }
                accumulatedLength += segmentLength;
            }

            Vector3 endDirection = (spline[spline.Count - 1] - spline[spline.Count - 2]).normalized;
            rotation = Quaternion.LookRotation(endDirection);
            return spline[spline.Count - 1];
        }
        private static Vector3 GetCenterPoint(List<Vector3> points)
        {
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException("List of points is null or empty.");
            }

            float sumX = 0;
            float sumY = 0;
            float sumZ = 0;

            foreach (Vector3 point in points)
            {
                sumX += point.x;
                sumY += point.y;
                sumZ += point.z;
            }

            return new Vector3(sumX / points.Count, sumY / points.Count, sumZ / points.Count);
        }
        private static void TranslatePoints(List<Vector3> points, Vector3 translationAmount)
        {
            if (points == null)
            {
                throw new ArgumentException("List of points is null.");
            }

            for (int i = 0; i < points.Count; i++)
            {
                points[i] -= translationAmount;
            }
        }
        private static void GetEdgeSplines(List<Vector3> spline, float thickness, 
                                          out List<Vector3> leftEdge, out List<Vector3> rightEdge) {
            leftEdge = new List<Vector3>();
            rightEdge = new List<Vector3>();

            List<Vector3> perps = new List<Vector3>();

            // Calculate perpendicular vectors for each point
            for (int i = 0; i < spline.Count; i++)
            {
                Vector3 tangent;
        
                // Calculate the tangent vector
                if (i == 0) // First point
                {
                    tangent = spline[i + 1] - spline[i];
                }
                else if (i == spline.Count - 1) // Last point
                {
                    tangent = spline[i] - spline[i - 1];
                }
                else // Middle points
                {
                    tangent = spline[i + 1] - spline[i - 1];
                }

                // Normalize the tangent
                tangent.Normalize();

                // Compute the perpendicular vector in X-Z (swap x and z and negate the new z)
                Vector3 perp = new Vector3(tangent.z, 0, -tangent.x);
                perps.Add(perp);
            }

            // Generate the left and right splines
            for (int i = 0; i < spline.Count; i++)
            {
                Vector3 perp = perps[i];

                // For inner edge smoothing: average adjacent perpendicular vectors
                if (i > 0 && i < spline.Count - 1)
                {
                    perp = (perps[i - 1] + perp + perps[i + 1]) / 3.0f;
                }

                leftEdge.Add(spline[i] + perp * (thickness / 2));
                rightEdge.Add(spline[i] - perp * (thickness / 2));
            }
        }
        private static bool DoSegmentsIntersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4) {
            // Using determinant method to check for intersection
            float det = (p2.x - p1.x) * (p4.z - p3.z) - (p2.z - p1.z) * (p4.x - p3.x);
            if (det == 0) return false;  // Parallel segments

            float lambda = ((p4.z - p3.z) * (p4.x - p1.x) + (p3.x - p4.x) * (p4.z - p1.z)) / det;
            float gamma = ((p1.z - p2.z) * (p4.x - p1.x) + (p2.x - p1.x) * (p4.z - p1.z)) / det;

            return (0 < lambda && lambda < 1) && (0 < gamma && gamma < 1);
        }
        private static List<Vector3> RemoveSelfIntersections(List<Vector3> spline) {
            for (int i = 0; i < spline.Count - 1; i++)
            {
                for (int j = i + 2; j < spline.Count - 1; j++)
                {
                    if (DoSegmentsIntersect(spline[i], spline[i + 1], spline[j], spline[j + 1]))
                    {
                        // Remove points between i+1 and j
                        spline.RemoveRange(i + 1, j - i);
                        return RemoveSelfIntersections(spline);  // Recursively clean up further intersections
                    }
                }
            }
            return spline;
        }
        private Mesh MakeDoubleSided(Mesh mesh) {
            Mesh doubleSidedMesh = new Mesh();

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
            for (int i = 0; i < vertices.Length; i++)
            {
                newVertices[vertices.Length + i] = vertices[i];
            }

            // Duplicate and reverse triangles
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int offset = vertices.Length;
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