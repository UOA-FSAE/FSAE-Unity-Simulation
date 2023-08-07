using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceTrack : MonoBehaviour {
    public bool debug;

    [SerializeField] private float width;
    [SerializeField] private float length;
    [SerializeField] private float widthOfTrack;
    [SerializeField] private int numberOfCorners;

    public void set_config(float width, float length, float widthOfTrack, int numberOfCorners) {
        this.width = width;
        this.length = length;
        this.widthOfTrack = widthOfTrack;
        this.numberOfCorners = numberOfCorners;
    }
    
    public void create_random_track() {
        // TODO!
    }

    public void create_track_from_file() {
        // TODO!
    }

    public void save_track_to_file() {
        // TODO!
    }

    private void create_voronoi() {
        // TODO!
    }
}
