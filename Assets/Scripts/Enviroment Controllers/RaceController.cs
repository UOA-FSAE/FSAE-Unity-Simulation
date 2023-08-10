using System;
using System.Linq;
using UnityEngine;

namespace Enviroment_Controllers {
    public abstract class RaceController : MonoBehaviour{
        public int numberOfCars;
        public bool hasRaceStarted = false;
        public CarController carPrefab;
        public CarController[] carsInSimulationInstances;
        
        // ROS2
        [SerializeField] private RaceControllerNode raceControllerNode;
        
        public abstract void ResetRace();
        public abstract CarController CreateNewCar(string name);

        private void Update() {
            if (!hasRaceStarted) StartRaceIfReady();
        }

        public void StartRaceIfReady() {
            if (carsInSimulationInstances.All(car => car.isReady)) {
                raceControllerNode.PublishRaceStart();
            }
        }
        
        public CarStats GetCarStats(string nameOfCar) {
            // TODO!
            throw new NotImplementedException();
        }

        public CarStats[] GetAllCarStats() {
            return carsInSimulationInstances.Select(carInstance => GetCarStats(carInstance.carName)).ToArray();
        }
    }

    public class CarStats {
        public string carName;
    }
}
