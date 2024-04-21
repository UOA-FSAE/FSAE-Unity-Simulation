using System.Linq;
using ROS2;
using sensor_msgs.msg;
using UnityEngine;
using UnitySensors;

[RequireComponent(typeof(CarController))]
public class LidarNode : MonoBehaviour {
    // Configs
    public int
        lidarRadius; //TODO!: Setup it so that no matter the density of lidar points it still does the correct Radius in deg

    // Internals
    private CarController carController;

    // Data
    public LaserScan laserScanMsg = new();
    private IPublisher<LaserScan> publisherLidarLaserScan;
    private ROS2Node ros2Node;

    // ROS2
    private readonly ROS2UnityCore ros2UnityCore = new();
    private VelodyneSensor velodyneSensor;

    private void Update() {
        // lidar calculations
        if (!velodyneSensor) return; // TODO!: Should crash / send debug message when can't find

        velodyneSensor.CompleteJob();

        var ranges = velodyneSensor.distances.AsReadOnly().ToArray();
        ranges = ranges[..(lidarRadius / 2)].Concat(ranges[(360 - lidarRadius / 2)..]).ToArray();

        for (var idx = 0; idx < ranges.Length; idx++)
            if (ranges[idx] < 1f)
                ranges[idx] = float.PositiveInfinity;

        laserScanMsg.Ranges = ranges;
        publisherLidarLaserScan.Publish(laserScanMsg);

        if (carController.enableDebug) DrawDebugThings();
    }

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
            ros2Node.CreateSensorPublisher<LaserScan>($"{carController.carName}/LaserScan");

        return true;
    }
    


    private void DrawDebugThings() {
        // Debug draw lidar lines
        var points = velodyneSensor.points.AsReadOnly().ToArray();
        points = points[..(lidarRadius / 2)].Concat(points[(360 - lidarRadius / 2)..]).ToArray();

        var sensorPosition = velodyneSensor.transform.position;
        foreach (var point in points)
            Debug.DrawLine(
                sensorPosition,
                velodyneSensor.transform.rotation * point + sensorPosition,
                Color.green,
                0.001f
            );
    }
}