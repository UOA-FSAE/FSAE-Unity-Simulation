using System;
using System.Collections.Generic;
using ROS2;
using UnityEngine;

namespace Enviroment_Controllers {
    public class RaceControllerNode : MonoBehaviour {
        private RaceController raceController;
        
        // ROS2
        private ROS2UnityCore ros2UnityCore = new ROS2UnityCore();
        private ROS2Node ros2Node;
        private ISubscription<std_msgs.msg.String> subscriptionCarCreate;
        private ISubscription<std_msgs.msg.String> subscriptionCarReset;
        
        public bool Config() {
            raceController = GetComponent<RaceController>();
            
            return true;
        }
    
        public bool SpinUp() {
            if (!ros2UnityCore.Ok()) {
                Debug.Log($"RaceControllerNode has failed to find to Ros2 Core");
                return false;
            }
        
            ros2Node = ros2UnityCore.CreateNode($"RaceControllerNode");
            
            
            subscriptionCarCreate = ros2Node.CreateSubscription<std_msgs.msg.String>(
                $"race_control/request_car",
                RequestCarCallback
            );
            subscriptionCarReset = ros2Node.CreateSubscription<std_msgs.msg.String>(
                $"race_control/reset",
                ResetCarCallback
            );


            return true;
        }

        private void RequestCarCallback(std_msgs.msg.String msg) {
            raceController.CreateNewCar(msg.Data);
        }
        private void ResetCarCallback(std_msgs.msg.String msg) {
            raceController.ResetCar(msg.Data);
        }
        
        public void PublishRaceStart() {
            // TODO!
            throw new NotImplementedException();
        }
    }
}