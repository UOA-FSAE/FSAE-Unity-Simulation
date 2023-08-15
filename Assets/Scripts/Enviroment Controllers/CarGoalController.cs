using System;
using System.Collections;
using System.Collections.Generic;
using Enviroment_Controllers;
using UnityEngine;

public class CarGoalController : RaceController {
    public Vector2 goalSpawnArea;
    public Vector2 carSpawnArea;
    public bool canSpawnRandomDirection;

    public GoalPoint goalPointPrefab;
    private GoalPoint goalPointPrefabInstance;

    private void Awake() {
        raceControllerNode = GetComponent<RaceControllerNode>();
        raceControllerNode.Config();
        raceControllerNode.SpinUp();
    }

    public override void ResetRace() {
        raceControllerNode.PublishRaceStart();
    }

    public override void CreateNewCar(string nameOfCar) {
        nameOfCarsToBeMade.Add(nameOfCar);
    }

    public override void ResetCar(string carName) {
        throw new NotImplementedException();
    }
}

public class CarGoalStats : CarStats {
    public Vector3 currentPosition;
    public float distanceToGoal;
}
