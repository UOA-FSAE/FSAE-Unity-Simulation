using System;
using System.Collections.Generic;
using System.Linq;
using Enviroment_Controllers;
using UnityEngine;

namespace Enviroment_Controllers {
    [RequireComponent(typeof(RaceControllerNode))]
    public abstract class RaceController : MonoBehaviour{
        public int numberOfCars;
        public bool hasRaceStarted = false;
        public CarConfig carConfig;
        public CarController carPrefab;
        public List<CarController> carsInSimulationInstances;
        protected List<string> nameOfCarsToBeMade = new List<string>();
        
        // ROS2
        protected RaceControllerNode raceControllerNode;
        
        public abstract void ResetRace();
        public abstract void CreateNewCar(string nameOfCar);
        public abstract void ResetCar(string carName);

        private void Update() {
            if (!hasRaceStarted) StartRaceIfReady();

            if (nameOfCarsToBeMade.Count > 0) {
                CarController carInstance = Instantiate(carPrefab) as CarController;
                carInstance.carName = nameOfCarsToBeMade[0];
                nameOfCarsToBeMade.RemoveAt(0);
                carsInSimulationInstances.Add(carInstance);
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
