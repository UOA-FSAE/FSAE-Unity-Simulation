using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceControl : MonoBehaviour {
    [SerializeField] private int numberOfCars;
    [SerializeField] private float width;
    [SerializeField] private float length;
    [SerializeField] private float widthOfTrack;
    [SerializeField] private int numberOfCorners;

    [SerializeField] private RaceTrack raceTrack;

    private void Start() {
        raceTrack.set_config(width, length, widthOfTrack, numberOfCorners);
        raceTrack.create_random_track();
    }

    public void load_config_from_file() {
        // TODO!
    }
    
    public void spawn_new_car() {
        // TODO!
    }

    public void start_race() {
        // TODO!
    }

    public void end_race() {
        // TODO!
    }

    public void remake_track() {
        // TODO!
    }

    public void get_race_state() {
        // TODO!
    }
}
