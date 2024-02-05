using System;
using System.Collections.Generic;
using UnityEngine;
using ros2_msgs.msg;
using ROS2;
using Autonomous;
using Car;

public class ConeDetectionPublisher : MonoBehaviour
{
    public List<Vector3> conePositions;
    public Transform carTransform;
    private TrackController trackController;

    private void Start()
    {
        carTransform = transform;
        
        // to find the TrackController
        trackController = FindObjectOfType<TrackController>();
        
        if (trackController == null)
        {
            Debug.LogError("TrackController not found in the scene!");
        }
        else
        {
            conePositions = trackController.GetConePositions(); // get list of cone positions from TrackController
        }
    }

    public void FixedUpdate()
    {
        PublishConePositions(conePositions);
    }
    void PublishConePositions(List<Vector3> conePositions) {
        
        // Filter cones based on relative position to the car
        List<Vector3> filteredCones = new List<Vector3>();
        foreach (Vector3 conePos in conePositions)
        {
            Vector3 relativePos = carTransform.InverseTransformPoint(conePos); // method transforms world space position into local space relative to the car

            // Check if the cone is in front of the car 
            if (relativePos.z > 0)  // forward direction is along positive z-axis
            {
                filteredCones.Add(relativePos);
            }
        }
        // ideally publish filteredCones positions here
        // for each Vector3, publish msg with formatted x,y,z coordinates 
    }
}

// Need help with:
// pack filtered Cones into cone message and publishing them with topic cone_detection 
