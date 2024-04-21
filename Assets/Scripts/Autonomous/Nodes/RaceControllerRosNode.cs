using ROS2;
using ros2_msgs.msg;
using std_msgs.msg;
using UnityEngine;

namespace Autonomous.Nodes {
    public class RaceControllerRosNode {
        private readonly ROS2UnityCore ros2UnityCore = new();
        private ROS2Node ros2Node;
        private RaceController raceController;

        private IPublisher<RaceStats> raceStatsPublisher;
        private ISubscription<String> resetSubscription;
        private ISubscription<String> createSubscription;
        private ISubscription<String> deleteSubscription;
        
        public bool Config(RaceController parentRaceController) {
            raceController = parentRaceController;
            
            return true;
        }

        public bool SpinUp() {
            if (!ros2UnityCore.Ok()) {
                Debug.Log($"Race Controller node has failed to find to Ros2 Core");
                return false;
            }

            ros2Node = ros2UnityCore.CreateNode($"RaceControllerNode");
            
            raceStatsPublisher = ros2Node.CreatePublisher<RaceStats>("race_controller/race_stats");
            
            resetSubscription = ros2Node.CreateSubscription<String>("race_controller/reset", ResetCarCallback);
            createSubscription = ros2Node.CreateSubscription<String>("race_controller/create", CreateCarCallback);
            deleteSubscription = ros2Node.CreateSubscription<String>("race_controller/delete", DeleteCarCallback);
            
            return true;
        }

        public void PublishRaceStats(RaceStats raceStats) => raceStatsPublisher.Publish(raceStats);
        private void CreateCarCallback(String msg) => raceController.carCreateQueue.Enqueue(msg.Data);
        private void ResetCarCallback(String msg) => raceController.carResetQueue.Enqueue(msg.Data);
        private void DeleteCarCallback(String msg) => raceController.carDeleteQueue.Enqueue(msg.Data);

    }
}