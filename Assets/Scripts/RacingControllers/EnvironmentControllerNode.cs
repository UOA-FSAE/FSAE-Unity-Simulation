using ROS2;
using ros2_msgs.msg;
using std_msgs.msg;
using UnityEngine;

namespace RacingControllers {
    [RequireComponent(typeof(EnvironmentController))]
    public class EnvironmentControllerNode : MonoBehaviour {
        private EnvironmentController environmentController;
        private IPublisher<RaceStats> raceStatePublisher;
        private ISubscription<String> resetSubscription;

        private ROS2Node ros2Node;
        /*  TODO!:
         *  Add Subscriber that listens to create new cars
         *  Add Publisher to publish when race is ready to start
         */

        // ROS2
        private readonly ROS2UnityCore ros2UnityCore = new();

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
            raceStatePublisher = ros2Node.CreatePublisher<RaceStats>("race_controller/race_stats");
            resetSubscription = ros2Node.CreateSubscription<String>("race_controller/reset", ResetCarCallback);
        }

        private void ResetCarCallback(String msg) { }

        private void PublishRaceState() {
            // TODO!: make less shit
            var car_stats = environmentController.GetAllCarStats();

            var race_stats = new RaceStats();

            race_stats.Car_num = environmentController.numberOfCarsInSimulation;
            var listOfCarStats = new CarStats[environmentController.numberOfCarsInSimulation];

            var i = 0;
            foreach (var carStat in car_stats) {
                var carStatRosMsg = new CarStats();

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