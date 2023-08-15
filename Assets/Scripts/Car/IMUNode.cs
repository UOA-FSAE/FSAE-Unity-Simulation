using builtin_interfaces.msg;
using ROS2;
using sensor_msgs.msg;
using UnityEngine;
using UnitySensors;
using System.Collections;
using Unity.VisualScripting;
using Time = UnityEngine.Time;

namespace Car {
    [RequireComponent(typeof(CarController))]
    [RequireComponent(typeof(IMUSensor))]
    public class IMUNode : MonoBehaviour {
        public sensor_msgs.msg.Imu imuMsg = new Imu();

        private CarController carController;
        private IMUSensor imuSensor;

        private ROS2UnityCore ros2UnityCore = new ROS2UnityCore();
        private ROS2Node ros2Node;
        private IPublisher<sensor_msgs.msg.Imu> publisherImu;

        public bool Config() {
            carController = GetComponent<CarController>();
            imuSensor = GetComponent<IMUSensor>();
        
            return true;
        }

        public bool SpinUp() {
            if (!ros2UnityCore.Ok()) {
                Debug.Log($"{carController.carName}ImuNode has failed to find to Ros2 Core");
                return false;
            }
        
            ros2Node = ros2UnityCore.CreateNode($"{carController.carName}ImuNode");
            publisherImu =
                ros2Node.CreateSensorPublisher<sensor_msgs.msg.Imu>($"{carController.carName}/imu");

            return true;
        }

        private void Update() {
            if (!imuSensor) return;
            
            imuMsg.SetHeaderFrame($"{carController.carName}_imu");
            
            imuMsg.Angular_velocity.X = imuSensor.angularVelocity.x;
            imuMsg.Angular_velocity.Y = imuSensor.angularVelocity.y;
            imuMsg.Angular_velocity.Z = imuSensor.angularVelocity.z;
            
            imuMsg.Orientation.X = imuSensor.rotation.x;
            imuMsg.Orientation.Y = imuSensor.rotation.y;
            imuMsg.Orientation.Z = imuSensor.rotation.z;
            imuMsg.Orientation.W = imuSensor.rotation.w;
            
            imuMsg.Linear_acceleration.X = imuSensor.acceleration.x;
            imuMsg.Linear_acceleration.Y = imuSensor.acceleration.y;
            imuMsg.Linear_acceleration.Z = imuSensor.acceleration.z;
            
            publisherImu.Publish(imuMsg);
        }
    }
}
