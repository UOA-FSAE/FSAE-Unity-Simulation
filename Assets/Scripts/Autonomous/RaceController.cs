using UnityEngine;
using System;
using System.Collections.Generic;
using Autonomous.Nodes;

namespace Autonomous {
    [RequireComponent(typeof(TrackController))]
    public class RaceController : MonoBehaviour {
        private TrackController trackController;
        private RaceControllerRosNode raceControllerRosNode;

        public CarController carPrefab;
        
        public CarQueue<string> carResetQueue = new();
        public CarQueue<string> carCreateQueue = new();
        
        private void Start() {
            trackController = GetComponent<TrackController>();
            
            raceControllerRosNode = new RaceControllerRosNode();
            raceControllerRosNode.Config(this);
            raceControllerRosNode.SpinUp();

            trackController.CreateTrack();
        }

        private void Update() {
            if (carCreateQueue.Count > 0) SpawnCar(carCreateQueue.Dequeue());
            if (carResetQueue.Count > 0) ResetCar(carCreateQueue.Dequeue());
        }

        private void ResetCar(string carConfig) {
            throw new NotImplementedException();
        }

        private void SpawnCar(string carConfig) {
            throw new NotImplementedException();
        }
    }
    
    public class CarQueue<T> {
        private readonly Queue<T> carCreationQueue = new();

        public int Count => carCreationQueue.Count;
        public void Enqueue(T carConfig) => carCreationQueue.Enqueue(carConfig);
        internal T Dequeue() => carCreationQueue.Dequeue();
    }
}