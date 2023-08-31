using System;
using UnityEngine;
using ROS2;

namespace RacingControllers {
    [RequireComponent(typeof(EnvironmentController))]
    public class EnvironmentControllerNode : MonoBehaviour {
        // ROS2
        private ROS2UnityCore ros2UnityCore = new ROS2UnityCore();
        private ROS2Node ros2Node;

        private EnvironmentController environmentController;

        private void Start() {
            environmentController = GetComponent<EnvironmentController>();
            
            
        }
    }
}