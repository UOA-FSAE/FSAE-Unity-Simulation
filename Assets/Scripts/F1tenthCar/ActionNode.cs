using ROS2;
using std_msgs.msg;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class ActionNode : MonoBehaviour {
    // Internals
    private CarController carController;
    private ROS2Node ros2Node;

    // ROS2
    private readonly ROS2UnityCore ros2UnityCore = new();
    private ISubscription<Float32> subscriptionCmdSteering;
    private ISubscription<Float32> subscriptionCmdThrottle;
    private ISubscription<Bool> subscriptionReadyUp;

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
        subscriptionCmdThrottle = ros2Node.CreateSubscription<Float32>(
            $"{carController.carName}/cmd_throttle",
            throttle_callback
        );
        subscriptionCmdSteering = ros2Node.CreateSubscription<Float32>(
            $"{carController.carName}/cmd_steering",
            steering_callback
        );
        subscriptionCmdSteering = ros2Node.CreateSubscription<Float32>(
            $"{carController.carName}/cmd_steering",
            steering_callback
        );

        return true;
    }

    private void throttle_callback(Float32 msg) {
        carController.SetCurrentSetThrottle(msg.Data);
    }

    private void steering_callback(Float32 msg) {
        carController.SetCurrentSetSteeringAngle(msg.Data);
    }

    private void readyup_callback(Bool msg) {
        carController.isReady = msg.Data;
    }
}