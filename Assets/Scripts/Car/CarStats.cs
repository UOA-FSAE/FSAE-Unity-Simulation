namespace Car {
    public class CarStats {
        // Car data
        public string carName;
        public bool isCrashed;
        
        // Track stats
        public int position;
        public float trackProgress; // % the way around the track
    }

    public class CarConfig {
        public string carName;
        
        public bool hasLidar;
        public bool hasImu;

        public int lidarHz;
        public int imuHz;
    }
}