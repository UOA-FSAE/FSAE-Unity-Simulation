using System.Collections.Generic;
using UnityEngine;

namespace Car {
    public class CarStats {
        // Car data
        public string carName;
        private Vector3 carStartingLocation;
        public int currentLapCount;
        public Vector3 currentPosition;
        public bool isCrashed;

        // Track stats
        public int position;
        public float trackProgress; // % the way around the track

        public void UpdateTrackProgress(List<Vector3> trackCenterLine, Vector3 carCurrentLocation) {
            currentPosition = carCurrentLocation;
            if (trackCenterLine == null || trackCenterLine.Count < 2) return;

            var closestDistance = float.MaxValue;
            var closestIndex = -1;
            for (var i = 0; i < trackCenterLine.Count; i++) {
                var distance = Vector3.Distance(carCurrentLocation, trackCenterLine[i]);
                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            float totalDistance = 0;
            float distanceToClosestPoint = 0;
            float distanceFromStartToClosestPoint = 0;
            var hasPassedStartingPoint = false;

            for (var i = 1; i < trackCenterLine.Count; i++) {
                var segmentDistance = Vector3.Distance(trackCenterLine[i], trackCenterLine[i - 1]);
                totalDistance += segmentDistance;

                if (hasPassedStartingPoint) distanceFromStartToClosestPoint += segmentDistance;

                if (i == closestIndex) {
                    distanceToClosestPoint = distanceFromStartToClosestPoint;
                    hasPassedStartingPoint = false;
                }

                if (trackCenterLine[i] == carStartingLocation) hasPassedStartingPoint = true;
            }

            if (hasPassedStartingPoint)
                distanceFromStartToClosestPoint +=
                    Vector3.Distance(trackCenterLine[0], trackCenterLine[^1]);

            var progress = (distanceToClosestPoint + distanceFromStartToClosestPoint) % totalDistance;
            trackProgress = progress / totalDistance * 100f;
            
            // TODO: make it so that it increases the lap count and keeps track of the time of lap

            trackProgress = Mathf.Clamp(trackProgress, 0, 99);
        }
    }
}