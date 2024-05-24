using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using ros2_msgs.msg;
using ROS2;
using Autonomous;
using Car;
using moa_msgs.msg;
using std_msgs.msg;
using geometry_msgs;
using geometry_msgs.msg;
using Pose = geometry_msgs.msg.Pose;
using Transform = UnityEngine.Transform;

public class TrajectorySubscriber : MonoBehaviour
{
    public Transform carTransform;
    private CarController carController;
    private TrackController trackController;
    private Vector3 initial_position;
    private Quaternion initial_orientation;
    private ROS2Node ros2Node;
    private ROS2UnityCore ros2UnityCore = new();
    private ISubscription<PoseArray> trajectorySub;
    private bool hasNewTrajectory = false;
    private Queue<Pose> trajectoryQueue;
    private void Start()
    {
        carTransform = transform;
        initial_position = carTransform.position;
        initial_orientation = carTransform.rotation;
        
        // to find the TrackController
        trackController = FindObjectOfType<TrackController>();

        Debug.Log("Start");
        
        if (trackController == null)
        {
            Debug.LogError("TrackController not found in the scene!");
        }
    }
    public bool Config() {
        carController = GetComponent<CarController>();
        return true;
    }
    public bool SpinUp() {
        if (!ros2UnityCore.Ok()) {
            Debug.LogError("Failed to find ROS2 Core");
            return false;
        }
        ros2Node = ros2UnityCore.CreateNode($"{carController.carName}TrajectoryNode");
        trajectorySub = ros2Node.CreateSubscription<PoseArray>("moa/selected_trajectory", SelectedTrajectoryCallback);
        Debug.Log("Subscriber for moa/selected_trajectory established");
        return true;
    }
    private void SelectedTrajectoryCallback(PoseArray message) {
        if (message != null)
        {
            // Handle PoseArray data received from the subscriber
            Debug.Log($"Received PoseArray data for moa/selected_trajectory: {message}");
        }
        else
        {
            return;
        }
        lock (trajectoryQueue)
        {
            trajectoryQueue.Clear(); // Clear previous trajectory
            foreach (var pose in message.Poses)
            {
                trajectoryQueue.Enqueue(pose); // Enqueue new poses
            }
            hasNewTrajectory = true;
        }
    }
    public void FixedUpdate()
    {
        if (hasNewTrajectory && trajectoryQueue.Count > 0)
        {
            lock (trajectoryQueue)
            {
                Pose nextPose = trajectoryQueue.Dequeue();
                UpdateCarTransform(nextPose);
                hasNewTrajectory = trajectoryQueue.Count > 0;
            }
        }
    }
    private void UpdateCarTransform(Pose pose)
    {
        // Convert pose to Unity transform
        Vector3 newPosition = new Vector3((float)pose.Position.X, (float)pose.Position.Y, (float)pose.Position.Z);
        Quaternion newRotation = new Quaternion((float)pose.Orientation.X, (float)pose.Orientation.Y, (float)pose.Orientation.Z, (float)pose.Orientation.W);

        // Update the car's transform
        carTransform.position = newPosition;
        carTransform.rotation = newRotation;
    }
}