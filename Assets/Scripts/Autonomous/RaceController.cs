using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
namespace Autonomous {
    [RequireComponent(typeof(TrackController))]
    public class RaceController : MonoBehaviour {
        private TrackController trackController;
        private void Start() {
            trackController = GetComponent<TrackController>();

            trackController.CreateTrack();
        }
        private void Update() {
            
        }

    }
}