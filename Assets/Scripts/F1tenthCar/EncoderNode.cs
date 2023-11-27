using System;
using ROS2;
using sensor_msgs.msg;
using std_msgs.msg;
using UnityEngine;
using UnitySensors;

namespace Car {
    [RequireComponent(typeof(CarController))]
    [RequireComponent(typeof(IMUSensor))]
    [RequireComponent(typeof(Transform))]
    public class EncoderNode : MonoBehaviour {
        private CarController carController;
        private IMUSensor imuSensor;
        private Float64 velocityMessage = new Float64();
        private IPublisher<Float64> velocityPublisher;
        private ROS2Node ros2Node;

        private readonly ROS2UnityCore ros2UnityCore = new();

        private void Update() {
            if (!imuSensor) return;
            velocityMessage.Data = Convert.ToDouble(imuSensor.localVelocity.z);
            Debug.Log(velocityMessage.Data);
            velocityPublisher.Publish(velocityMessage);
        }

        public bool Config() {
            carController = GetComponent<CarController>();
            imuSensor = GetComponent<IMUSensor>();

            return true;
        }

        public bool SpinUp() {
            if (!ros2UnityCore.Ok()) {
                Debug.Log($"{carController.carName}EncoderNode has failed to find to Ros2 Core");
                return false;
            }
            Debug.Log("Encoder node online");
            ros2Node = ros2UnityCore.CreateNode($"{carController.carName}EncoderNode");
            velocityPublisher = ros2Node.CreatePublisher<Float64>($"{carController.carName}/speed");

            return true;
        }
    }
}