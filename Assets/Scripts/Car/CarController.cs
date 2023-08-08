using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AxleInfo {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; // is this wheel attached to motor?
    public bool steering; // does this wheel apply steer angle?
}

[RequireComponent(typeof(ActionNode))]
[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour {
    /* TODO!: List of things need to be done for the Car to be finished
     * - Create Sensor node for the Lidar
     * - Create Sensor node for Odom
     * - Create Sensor node for IMU
     * - Create Config setup for cars details
     * */
    

    public string carName;
    [SerializeField] private float currentSetThrottle = 0f;
    [SerializeField] private float currentSetSteeringAngle = 0f;

    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public float maxSteeringAngle; // maximum steer angle the wheel can have

    private ActionNode actionNode;
    
    public void FixedUpdate() {

        float motor = maxMotorTorque * currentSetThrottle * 0.01f;
            
        foreach (AxleInfo axleInfo in axleInfos) {
            if (axleInfo.steering) {
                axleInfo.leftWheel.steerAngle = currentSetSteeringAngle;
                axleInfo.rightWheel.steerAngle = currentSetSteeringAngle;
            }
            if (axleInfo.motor) {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
        }
    }

    private void Awake() {
        actionNode = GetComponent<ActionNode>();
        actionNode.Config();
        actionNode.spin_up();
    }

    public bool SetCurrentSetThrottle(float newSetThrottle) {
        currentSetThrottle = Math.Clamp(newSetThrottle, -100f, 100f);
        return true;
    }

    public bool SetCurrentSetSteeringAngle(float newSetSteeringAngle) {
        currentSetSteeringAngle = Math.Clamp(newSetSteeringAngle, -maxSteeringAngle, maxSteeringAngle);
        return true;
    }
}
