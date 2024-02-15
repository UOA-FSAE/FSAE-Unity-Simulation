using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ros2_msgs.msg;
using ROS2;
using Autonomous;
using Car;
using moa_msgs.msg;
using std_msgs.msg;
using geometry_msgs;

public class ConeDetectionPublisher : MonoBehaviour
{
    private CarController carController;
    public List<Vector3> LeftConePositions;
    public List<Vector3> RightConePositions;
    public Transform carTransform;
    private TrackController trackController;
    private ROS2Node ros2Node;
    private readonly ROS2UnityCore ros2UnityCore = new();
    private IPublisher<ConeMap> cone_detection_pub;
    private Vector3 initial_position;
    private Quaternion initial_orientation;

    private void Start()
    {
        carTransform = transform;
        initial_position = carTransform.position;
        initial_orientation = carTransform.rotation;
        
        // to find the TrackController
        trackController = FindObjectOfType<TrackController>();

        Debug.Log("Hello!");
        
        if (trackController == null)
        {
            Debug.LogError("TrackController not found in the scene!");
        }
        else
        {
            LeftConePositions = trackController.GetLeftConePositions(); // get list of cone positions from TrackController
            RightConePositions = trackController.GetRightConePositions();
        }
    }
    
    public bool Config() {
        carController = GetComponent<CarController>();
        return true;
    }
    
    public bool SpinUp() {
        if (!ros2UnityCore.Ok()) {
            Debug.Log($"{carController.carName}Camera node has failed to find to Ros2 Core");
            return false;
        }

        ros2Node = ros2UnityCore.CreateNode($"{carController.carName}CameraNode");
        cone_detection_pub = ros2Node.CreateSensorPublisher<ConeMap>($"cone_detection");
        Debug.Log("Cameara Publisher established");
        return true;
    }
    public void FixedUpdate()
    {
        PublishConePositions(LeftConePositions, RightConePositions);
    }
    
    private void PublishConePositions(List<Vector3> LeftConePositions, List<Vector3> RightConePositions) {
        
        // Filter cones based on relative position to the car
        List <Vector3> LeftFilteredCones = filter_invisible_cones(LeftConePositions, carTransform);
        List <Vector3> RightFilteredCones = filter_invisible_cones(RightConePositions, carTransform);
        // ideally publish filteredCones positions here
        // for each Vector3, publish msg with formatted x,y,z coordinates 
        ConeMap output_cones = get_packed_ConeMap_message(LeftFilteredCones, RightFilteredCones, carTransform);
        
        cone_detection_pub.Publish(output_cones);
    }

    private List<Vector3> filter_invisible_cones(List<Vector3> Unfiltered_Cones, Transform carTransform)
    {
        // Filter cones based on relative position to the car
        List<Vector3> filteredCones = new List<Vector3>();
        foreach (Vector3 conePos in Unfiltered_Cones)
        {
            Vector3 relativePos = carTransform.InverseTransformPoint(conePos); // method transforms world space position into local space relative to the car
            
            // Check if the cone is in front of the car 
            if (relativePos.z > 0 && relativePos.magnitude < 50)  // forward direction is along positive z-axis with limited distance
            {
                filteredCones.Add(relativePos);
            }

        }

        return filteredCones;
    }
    private ConeMap get_packed_ConeMap_message(List<Vector3> left_cones, List<Vector3> right_cones, Transform carTransform)
    {
        // cone_type: 0 for blue, 2 for yellow
        ConeMap output_cone_map = new ConeMap();
        Cone[] list_of_cones = new Cone[1 + left_cones.Count + right_cones.Count];
        output_cone_map.Cones = list_of_cones;

        Vector3 Car_Position = normalize_car_position(carTransform.position);
        Quaternion Car_Orientation = normalized_car_orientation(carTransform.rotation);
        Cone Car_Cone = pack_to_cone_msg(Car_Position, Car_Orientation, 0);
        output_cone_map.Cones[0] = Car_Cone;
        
        //Debug.Log(Car_Position);
        //Debug.Log(Car_Orientation);

        int index = 1;
        foreach (Vector3 cone in left_cones)
        {
            output_cone_map.Cones[index] = pack_to_cone_msg(normalize_cone_position(cone), new Quaternion(), 0);
            index++;
        }
        
        foreach (Vector3 cone in right_cones)
        {
            output_cone_map.Cones[index] = pack_to_cone_msg(normalize_cone_position(cone), new Quaternion(), 2);
            index++;
        }
        
        return output_cone_map;
    }

    private Cone pack_to_cone_msg(Vector3 position, Quaternion rotation, int cone_type)
    {
        Cone output = new Cone();
        if (cone_type == 0)
        {
            output.Colour = 0;
        }
        else if (cone_type == 2)
        {
            output.Colour = 2;
        }

        //Quaternion rotate90 = Quaternion.Euler(0, 0, 0);
        // The following need to change later since only w is used in cone map
        output.Pose.Pose.Orientation.X = 0;
        output.Pose.Pose.Orientation.Y = 0;
        output.Pose.Pose.Orientation.Z = 0;
        // Make angle range from -pi to pi
        output.Pose.Pose.Orientation.W = rotation.eulerAngles[2] * 2 * Mathf.PI / 360;
        output.Pose.Pose.Position.X = position.x;
        output.Pose.Pose.Position.Y = position.y;
        output.Pose.Pose.Position.Z = position.z;
        return output;
    }

    private Vector3 normalize_car_position(Vector3 position)
    {   
        // Normalize to have initial position 0
        Vector3 normalized_left_handed = Quaternion.Inverse(initial_orientation) * (position - initial_position);
        return new Vector3(normalized_left_handed.x, normalized_left_handed.z, normalized_left_handed.y);
    }
    
    private Quaternion normalized_car_orientation(Quaternion orientation)
    {
        Quaternion normalized_left_handed = orientation * Quaternion.Inverse(initial_orientation);
        return new Quaternion(-normalized_left_handed.x, -normalized_left_handed.z, -normalized_left_handed.y,
            normalized_left_handed.w);
    }
    
    private Vector3 normalize_cone_position(Vector3 position)
    {   
        Vector3 normalized_left_handed = position;
        return new Vector3(normalized_left_handed.x, normalized_left_handed.z, normalized_left_handed.y);
    }
}

// Need help with:
// pack filtered Cones into cone message and publishing them with topic cone_detection 
