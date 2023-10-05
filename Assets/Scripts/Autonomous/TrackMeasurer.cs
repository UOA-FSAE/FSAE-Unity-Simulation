
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Autonomous {
    public class TrackMeasurer : MonoBehaviour {
        public float GetPercentCoverage(Vector3 pointA, List<Vector3> trackPoints) {
            if (trackPoints.Count < 1) {
                return -1.0f;
            }
            Vector3 pointB = trackPoints[0];

            return GetPercentCoverage(pointA, pointB, trackPoints);
        }
        public float GetPercentCoverage(Vector3 pointA, Vector3 pointB, List<Vector3> trackPoints) {
            /*
            returns the percentage of the track (in distance) that lies between
            pointA and pointB following the direction of the track's spline.

            returns -1.0f on error
            */

            if (trackPoints.Count < 2) {
                return -1.0f;
            }

            float mapLength = 0.0f;

            float percentCovered;

            float distanceA = 0.0f;
            float closestTrackPointToADist = 1000000.0f;
            float distanceB = 0.0f;
            float closestTrackPointToBDist = 1000000.0f;
            Vector3 directionA1 = new Vector3(0,0,0);
            Vector3 directionA2 = new Vector3(0,0,0);
            Vector3 directionB1 = new Vector3(0,0,0);
            Vector3 directionB2 = new Vector3(0,0,0);
            float trackPointToADist = 0.0f;
            float trackPointToBDist = 0.0f;
            float deltaDistance;
            

            for (int i = 0; i < trackPoints.Count; i ++){
                
                // modulo is used to wrap around at the end of the loop
                int j = (i+1)%trackPoints.Count;

                // locate A along spline
                trackPointToADist = Vector3.Distance(pointA, trackPoints[i]);
                if (trackPointToADist < closestTrackPointToADist) {
                    distanceA = mapLength; 
                    closestTrackPointToADist = trackPointToADist;
                    directionA1 = trackPoints[j] - trackPoints[i]; // bearing of current part of track
                    directionA2 = pointA - trackPoints[i]; // bearing of pointA from track point
                }
                // locate B along spline
                trackPointToBDist = Vector3.Distance(pointB, trackPoints[i]);
                if (trackPointToBDist < closestTrackPointToBDist) {
                    distanceB = mapLength; 
                    closestTrackPointToBDist = trackPointToBDist;
                    directionB1 = trackPoints[i] - trackPoints[j]; // bearing of current part of track
                    directionB2 = pointB - trackPoints[i]; // bearing of pointB from track point
                }
                mapLength += Vector3.Distance(trackPoints[i], trackPoints[j]);
            }


            /* 
            additional precision:
            add or subtract the projected distance to track point depending
            on whether points A/B lie ahead or behind their respective
            closest track point.
            */
            distanceA += trackPointToADist * Vector3.Dot(Vector3.Normalize(directionA1), Vector3.Normalize(directionA2));
            distanceB += trackPointToBDist * Vector3.Dot(Vector3.Normalize(directionB1), Vector3.Normalize(directionB2));
            

            // wrap around edge case
            if (distanceA <= distanceB) {
                deltaDistance = distanceB - distanceA;
            }
            else {
                deltaDistance = (mapLength - distanceA) + distanceB; 
            }

            // percentage as a float in range [0.0, 1.0]
            percentCovered = deltaDistance/mapLength;
            return percentCovered;

        }

        public Vector3 GetPositionOnSpline(List<Vector3> trackPoints, float percentage, out Quaternion rotation) {
            
            // (handles percentage > 100%)
            percentage = percentage % 1;



            float mapLength = 0.0f;

            int i = 0;
            int j = 0;

            // modulo is used to wrap around at the end of the loops
            for (i = 0; i < trackPoints.Count; i ++){
                // modulo is used to wrap around at the end of the loop
                j = (i+1)%trackPoints.Count;
                mapLength += Vector3.Distance(trackPoints[i], trackPoints[j]);
            }

            i = 0;
            j = 0;
            float distance = 0.0f;

            while (distance/mapLength <= percentage) {
                // increment *around* track points
                i = j;

                // modulo is used to wrap around at the end of the loop
                j = (i+1)%trackPoints.Count;
                distance += Vector3.Distance(trackPoints[i], trackPoints[j]);

            }
            // position is between PointA and PointB
            Vector3 pointA = trackPoints[i];
            Vector3 pointB = trackPoints[j];


            //remove overshoot
            //lerpPercent = 1 - (overshot percent / track segment as percent of map length)
            float overshotPercent = (distance/mapLength - percentage);
            float segmentTrackPercent = (Vector3.Distance(pointA, pointB) / mapLength);
            float lerpPercent = 1 - overshotPercent/segmentTrackPercent;
            
            // interpolate back towards PointA
            Vector3 position = Vector3.Lerp(pointB, pointA, lerpPercent);
            
            // Calculate rotation
            Vector3 upwards = new Vector3(0.0f,0.0f,1.0f);
            Vector3 directionalVector = Vector3.Normalize(pointA - pointB);
            Vector3 rightwards = Vector3.Cross(directionalVector, upwards);

            // outputs
            rotation = Quaternion.AngleAxis(30, Vector3.up);
            return position;

        }

    }
}