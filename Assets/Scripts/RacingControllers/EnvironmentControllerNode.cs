using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ROS2;
using ros2_msgs.msg;

namespace RacingControllers {
    [RequireComponent(typeof(EnvironmentController))]
    public class EnvironmentControllerNode : MonoBehaviour {
        /*  TODO!:
         *  Add Subscriber that listens to create new cars
         *  Add Publisher to publish when race is ready to start
         */
        
        // ROS2
        private ROS2UnityCore ros2UnityCore = new ROS2UnityCore();
        private ROS2Node ros2Node;
        private IPublisher<ros2_msgs.msg.RaceStats> raceStatePublisher;
        private ISubscription<std_msgs.msg.String> resetSubscription;

        private EnvironmentController environmentController;

        private void Start() {
            environmentController = GetComponent<EnvironmentController>();
        }

        private void Update() {
            // TODO!: add publish frequency
            PublishRaceState();
        }

        public void SpinUp() {
            if (!ros2UnityCore.Ok()) return;

            ros2Node = ros2UnityCore.CreateNode("RaceController");
            raceStatePublisher = ros2Node.CreatePublisher<ros2_msgs.msg.RaceStats>("race_controller/race_stats");
            resetSubscription = ros2Node.CreateSubscription<std_msgs.msg.String>("race_controller/reset", ResetCarCallback);
        }

        private void ResetCarCallback(std_msgs.msg.String msg) {
            
        }

        private void PublishRaceState() {
            // TODO!: make less shit
            var car_stats = environmentController.GetAllCarStats();

            var race_stats = new ros2_msgs.msg.RaceStats();

            race_stats.Car_num = environmentController.numberOfCarsInSimulation;
            CarStats[] listOfCarStats = new CarStats[environmentController.numberOfCarsInSimulation];

            var i = 0;
            foreach (var carStat in car_stats) {
                var carStatRosMsg = new ros2_msgs.msg.CarStats();
                
                carStatRosMsg.Track_progress = carStat.trackProgress;
                carStatRosMsg.Car_name = carStat.carName;
                carStatRosMsg.Is_crashed = carStat.isCrashed;
                
                listOfCarStats[i++] = carStatRosMsg;
            }

            race_stats.Car_stats = listOfCarStats;
            raceStatePublisher.Publish(race_stats);
        }
    }
}