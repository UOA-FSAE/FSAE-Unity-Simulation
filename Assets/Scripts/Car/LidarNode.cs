using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROS2;
using UnityEditor.UI;

[RequireComponent(typeof(CarController))]
public class LidarNode : MonoBehaviour {
    // Internals
    private CarController carController; 
        
    // ROS2
    private ROS2UnityCore ros2UnityCore = new ROS2UnityCore();
    private ROS2Node ros2Node;
    
    public bool Config() {
        carController = GetComponent<CarController>();
        return true;
    }
    
    public bool spin_up() {
        if (!ros2UnityCore.Ok()) {
            Debug.Log($"{carController.carName}ActionNode has failed to find to Ros2 Core");
            return false;
        }
        
        ros2Node = ros2UnityCore.CreateNode($"{carController.carName}LidarNode");
        
        return true;
    }
}
