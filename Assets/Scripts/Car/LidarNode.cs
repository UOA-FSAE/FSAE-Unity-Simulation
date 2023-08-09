using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ROS2;
using sensor_msgs.msg;
using Unity.VisualScripting;
using UnitySensors;

[RequireComponent(typeof(CarController))]
public class LidarNode : MonoBehaviour {
    // Data
    public sensor_msgs.msg.LaserScan laserScanMsg = new LaserScan();

    // Configs
    public int lidarRadius; //TODO!: Setup it so that no matter the density of lidar points it still does the correct Radius in deg
    
    // Internals
    private CarController carController;
    private VelodyneSensor velodyneSensor;
        
    // ROS2
    private ROS2UnityCore ros2UnityCore = new ROS2UnityCore();
    private ROS2Node ros2Node;
    private IPublisher<sensor_msgs.msg.LaserScan> publisherLidarLaserScan;

    public bool Config() {
        carController = GetComponent<CarController>();
        velodyneSensor = GetComponentInChildren<VelodyneSensor>();

        
        
        return true;
    }
    
    public bool spin_up() {
        if (!ros2UnityCore.Ok()) {
            Debug.Log($"{carController.carName}ActionNode has failed to find to Ros2 Core");
            return false;
        }
        
        ros2Node = ros2UnityCore.CreateNode($"{carController.carName}LidarNode");
        publisherLidarLaserScan =
            ros2Node.CreatePublisher<sensor_msgs.msg.LaserScan>($"{carController.carName}/LaserScan");

        return true;
    }

    private void Update() {
        // lidar calculations
        if (!velodyneSensor) return;    // TODO!: Should crash / send debug message when can't find
        
        velodyneSensor.CompleteJob();

        float[] ranges = velodyneSensor.distances.AsReadOnly().ToArray();
        ranges = ranges[..(lidarRadius/2)].Concat(ranges[(360 - lidarRadius/2)..]).ToArray();
        
        for (int idx = 0; idx < ranges.Length; idx++)
            if (ranges[idx] < 1f)
                ranges[idx] = float.PositiveInfinity;

        laserScanMsg.Ranges = ranges;
        publisherLidarLaserScan.Publish(laserScanMsg);
        
        if (carController.enableDebug) DrawDebugThings();
    }

    private void DrawDebugThings() {
        // Debug draw lidar lines
        Vector3[] points = velodyneSensor.points.AsReadOnly().ToArray();
        points = points[..(lidarRadius/2)].Concat(points[(360 - lidarRadius/2)..]).ToArray();

        var sensorPosition = velodyneSensor.transform.position;
        foreach (var point in points) {
            Debug.DrawLine(
                sensorPosition,
                velodyneSensor.transform.rotation * point + sensorPosition,
                Color.green,
                0.001f
            );
        }
    }
}
