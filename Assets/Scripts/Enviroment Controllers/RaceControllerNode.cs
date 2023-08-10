using System;
using System.Collections.Generic;
using ROS2;
using UnityEngine;

namespace Enviroment_Controllers {
    public class RaceControllerNode : MonoBehaviour {
        // ROS2
        private ROS2UnityCore ros2UnityCore = new ROS2UnityCore();
        private ROS2Node ros2Node;
        private List<IPublisher<Message>> publisherTopics = new List<IPublisher<Message>>();
        
        
        public bool Config() {
            return true;
        }
    
        public bool spin_up() {
            if (!ros2UnityCore.Ok()) {
                Debug.Log($"RaceControllerNode has failed to find to Ros2 Core");
                return false;
            }
        
            ros2Node = ros2UnityCore.CreateNode($"RaceControllerNode");
            publisherLidarLaserScan =
                ros2Node.CreatePublisher<sensor_msgs.msg.LaserScan>($"{carController.carName}/LaserScan");

            return true;
        }
        
        public void PublishRaceStart() {
            // TODO!
            throw new NotImplementedException();
        }

        public void CreateTopic(string topic, Type msgType) {
            publisherTopics.Add(ros2Node.CreatePublisher<msgType>());
        }
        
        public void PublishMessage(string topic, Message msg) {
            // TODO!
            throw new NotImplementedException();
        }
    }
}