using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Car;
using YamlDotNet.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;


public static class MeshCreatorHelper{
    public static bool DoSegmentsIntersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4) {
            // Using determinant method to check for intersection
            var det = (p2.x - p1.x) * (p4.z - p3.z) - (p2.z - p1.z) * (p4.x - p3.x);
            if (det == 0) return false; // Parallel segments

            var lambda = ((p4.z - p3.z) * (p4.x - p1.x) + (p3.x - p4.x) * (p4.z - p1.z)) / det;
            var gamma = ((p1.z - p2.z) * (p4.x - p1.x) + (p2.x - p1.x) * (p4.z - p1.z)) / det;

            return 0 < lambda && lambda < 1 && 0 < gamma && gamma < 1;
        }


    public static Mesh CreateWallMesh(List<Vector3> points, float trackWallHeight) {
        var mesh = new Mesh();

        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        for (var i = 0; i < points.Count; i++) {
            // Add bottom vertex
            vertices.Add(points[i]);

            // Add top vertex
            vertices.Add(new Vector3(points[i].x, points[i].y + trackWallHeight, points[i].z));
        }

        // Generate triangles
        for (var i = 0; i < points.Count; i++) {
            var bottomLeft = i * 2;
            var topLeft = i * 2 + 1;
            var bottomRight = (i + 1) % points.Count * 2; // Use modulo for looping
            var topRight = (i + 1) % points.Count * 2 + 1; // Use modulo for looping

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
}