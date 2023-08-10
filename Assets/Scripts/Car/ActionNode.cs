using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROS2;
using UnityEditor.UI;

[RequireComponent(typeof(CarController))]
public class ActionNode : MonoBehaviour {
    // Internals
    private CarController carController; 
    
    // ROS2
    private ROS2UnityCore ros2UnityCore = new ROS2UnityCore();
    private ROS2Node ros2Node;
    private ISubscription<std_msgs.msg.Float32> subscriptionCmdThrottle;
    private ISubscription<std_msgs.msg.Float32> subscriptionCmdSteering;
    private ISubscription<std_msgs.msg.Bool> subscriptionReadyUp;

    public bool Config() {
        carController = GetComponent<CarController>();
        return true;
    }
    
    public bool spin_up() {
        if (!ros2UnityCore.Ok()) {
            Debug.Log($"{carController.carName}ActionNode has failed to find to Ros2 Core");
            return false;
        }
        
        ros2Node = ros2UnityCore.CreateNode($"{carController.carName}ActionNode");
        subscriptionCmdThrottle = ros2Node.CreateSubscription<std_msgs.msg.Float32>(
            $"{carController.carName}/cmd_throttle",
            throttle_callback
        );
        subscriptionCmdSteering = ros2Node.CreateSubscription<std_msgs.msg.Float32>(
            $"{carController.carName}/cmd_steering",
            steering_callback
        );
        subscriptionCmdSteering = ros2Node.CreateSubscription<std_msgs.msg.Float32>(
            $"{carController.carName}/cmd_steering",
            steering_callback
        );
        
        return true;
    }

    void throttle_callback(std_msgs.msg.Float32 msg) {
        carController.SetCurrentSetThrottle(msg.Data);
    }
    
    void steering_callback(std_msgs.msg.Float32 msg) {
        carController.SetCurrentSetSteeringAngle(msg.Data);
    }

    void readyup_callback(std_msgs.msg.Bool msg) {
        carController.isReady = msg.Data;
    }
}
