using System;
using System.Collections.Generic;
using System.Linq;
using Enviroment_Controllers;
using UnityEngine;

namespace Enviroment_Controllers {
    [RequireComponent(typeof(RaceControllerNode))]
    [RequireComponent(typeof(TrackController))]
    public abstract class RaceController : MonoBehaviour{
        public int numberOfCars;
        public bool hasRaceStarted = false;
        public CarConfig carConfig;
        public CarController carPrefab;
        public List<CarController> carsInSimulationInstances;
        protected Queue<string> carCreateQueue = new Queue<string>();
        
        // ROS2
        protected RaceControllerNode raceControllerNode;

        public void ResetRace() {
            // TODO!
            throw new NotImplementedException();
        }

        public void ResetCar(string carName) {
            // TODO!
            throw new NotImplementedException();
        }

        private void Update() {
            if (!hasRaceStarted) StartRaceIfReady();

            if (carCreateQueue.Count > 0) {
                CreateNewCar(carCreateQueue.Dequeue());
            }
        }

        public void StartRaceIfReady() {
            if (carsInSimulationInstances.All(car => car.isReady) && carsInSimulationInstances.Count > 0) 
                ResetRace();
        }
        
        public CarStats GetCarStats(string nameOfCar) {
            // TODO!
            throw new NotImplementedException();
        }

        public CarStats[] GetAllCarStats() {
            return carsInSimulationInstances.Select(carInstance => GetCarStats(carInstance.carName)).ToArray();
        }
        
        public void CreateNewCar(string nameOfCar) {
            CarController carController = Instantiate(carPrefab);
        }

    }

    public abstract class CarStats {
        public string carName;
        public float percentComplete;
    }

    [Serializable]
    public class CarConfig {
        public float maxTorque;
        public float macSteeringAngle;
    }
}
