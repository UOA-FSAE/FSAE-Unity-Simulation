using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Autonomous.Nodes;
using rcl_interfaces.msg;

namespace Autonomous {
    [RequireComponent(typeof(TrackController))]
    public class RaceController : MonoBehaviour {
        private TrackController trackController;
        private RaceControllerRosNode raceControllerRosNode;

        public CarController carPrefab;
        public int maxNumberOfCarsInSim = 10;
        public CarQueue<string> carResetQueue = new();
        public CarQueue<string> carCreateQueue = new();

        private List<CarController> listOfCars = new();
        
        private void Start() {
            trackController = GetComponent<TrackController>();
            
            raceControllerRosNode = new RaceControllerRosNode();
            raceControllerRosNode.Config(this);
            raceControllerRosNode.SpinUp();

            trackController.CreateTrack();
        }

        private void Update() {
            if (carCreateQueue.Count > 0 && listOfCars.Count != maxNumberOfCarsInSim) 
                SpawnCar(carCreateQueue.Dequeue());
            if (carResetQueue.Count > 0) 
                ResetCar(carResetQueue.Dequeue());
        }

        private void ResetCar(string carConfig) {
            var car = listOfCars.Where(car => car.carName == carConfig).ToList()[0];
            var percent_around_track =
                TrackMeasurer.GetPercentCoverage(car.transform.position, trackController.trackPoints);
            Debug.Log($"POSTION:{percent_around_track}");
            var position = TrackMeasurer.GetPositionOnSpline(trackController.trackPoints, percent_around_track, out var rotation);
            car.transform.position = position;
            car.transform.rotation = rotation;
        }

        private void SpawnCar(string carConfig) {
            var position = TrackMeasurer.GetPositionOnSpline(trackController.trackPoints, 0f, out var rotation);
            var car = Instantiate(carPrefab, position, rotation);
            car.transform.localScale = new Vector3(1, 1, 1);
            car.Config(carConfig);
            listOfCars.Add(car);
        }
    }
    
    public class CarQueue<T> {
        private readonly Queue<T> carCreationQueue = new();

        public int Count => carCreationQueue.Count;
        public void Enqueue(T carConfig) => carCreationQueue.Enqueue(carConfig);
        internal T Dequeue() => carCreationQueue.Dequeue();
    }
}