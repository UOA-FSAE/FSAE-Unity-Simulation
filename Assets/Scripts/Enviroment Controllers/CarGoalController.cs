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

    public new void ResetRace() {
        raceControllerNode.PublishRaceStart();
    }

    public new void CreateNewCar(string nameOfCar) {
        carCreateQueue.Enqueue(nameOfCar);
    }

    public new void ResetCar(string carName) {
        throw new NotImplementedException();
    }
}
