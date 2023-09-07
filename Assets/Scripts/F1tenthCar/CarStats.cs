using System;
using System.Collections.Generic;
using UnityEngine;

namespace Car {
    public class CarStats : MonoBehaviour {
        // Car data
        public string carName;
        public int currentLapCount;
        public Vector3 currentPosition;
        public bool isCrashed;
        public ControllerType carControllerType;

        // Track stats
        public int position;
        public float trackProgress; // % the way around the track
        private Vector3 carStartingLocation;
        private List<Vector3> currentTrackPoints;

        public void Config(CarConfig carConfig) {
            carName = carConfig.carName;
            carStartingLocation = carConfig.startLocation;
            carControllerType = carConfig.carControllerType;
        }

        public void SetNewTrack(List<Vector3> trackPoints) {
            currentTrackPoints = trackPoints;
        }

        public void UpdateTrackProgress() {
            currentPosition = transform.position;
            if (currentTrackPoints == null || currentTrackPoints.Count < 2) return;

            var closestDistance = float.MaxValue;
            var closestIndex = -1;
            for (var i = 0; i < currentTrackPoints.Count; i++) {
                var distance = Vector3.Distance(transform.position, currentTrackPoints[i]);
                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            float totalDistance = 0;
            float distanceToClosestPoint = 0;
            float distanceFromStartToClosestPoint = 0;
            var hasPassedStartingPoint = false;

            for (var i = 1; i < currentTrackPoints.Count; i++) {
                var segmentDistance = Vector3.Distance(currentTrackPoints[i], currentTrackPoints[i - 1]);
                totalDistance += segmentDistance;

                if (hasPassedStartingPoint) distanceFromStartToClosestPoint += segmentDistance;

                if (i == closestIndex) {
                    distanceToClosestPoint = distanceFromStartToClosestPoint;
                    hasPassedStartingPoint = false;
                }

                if (currentTrackPoints[i] == carStartingLocation) hasPassedStartingPoint = true;
            }

            if (hasPassedStartingPoint)
                distanceFromStartToClosestPoint +=
                    Vector3.Distance(currentTrackPoints[0], currentTrackPoints[^1]);

            var progress = (distanceToClosestPoint + distanceFromStartToClosestPoint) % totalDistance;
            trackProgress = progress / totalDistance * 100f;

            // TODO: make it so that it increases the lap count and keeps track of the time of lap

            trackProgress = Mathf.Clamp(trackProgress, 0, 99);
        }
    }

    [Serializable]
    public class CarConfig {
        public string carName;
        public Vector3 startLocation;
        public float startPercentLocation;
        public ControllerType carControllerType;
    }

    public enum ControllerType {
        ThrottleSteering, // Throttle is % from 0 -> 100% and steering is angle
        TorqueSteering, // Torque is the troque requested and steering is angle
        Ackermann, // Ackermann steering message
        Twist // Twist steering message
    }
}