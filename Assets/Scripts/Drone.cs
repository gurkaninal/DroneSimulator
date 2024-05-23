using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour {
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private List<Transform> propelers;

    private Rigidbody rb;

    private bool fly = false;
    private float turnDegree = 0f;
    private bool isTouching = false;
    private float propelerMaxRotateSpeed = 1800f;
    private float[] propelerRotateSpeeds;
    private float flyDuration = 0;
    private float max_height = 25f;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        InputManager.Instance.OnFlyPerformed += InputManager_OnFlyPerformed;
        InputManager.Instance.OnTurnLeftPerformed += InputManager_OnTurnLeftPerformed;
        InputManager.Instance.OnTurnRightPerformed += InputManager_OnTurnRightPerformed;
        InputManager.Instance.OnInputReceived += InputManager_OnInputReceived;

        propelerRotateSpeeds = new float[propelers.Count];
    }

    private void Update() {
        var newRotation = rb.rotation;

        // Get the current rotation angles around x and z axes
        float currentXAngle = newRotation.eulerAngles.x;
        float currentZAngle = newRotation.eulerAngles.z;

        // Calculate the target rotation based on turnDegree while preserving x and z rotations
        Quaternion targetRotation = Quaternion.Euler(currentXAngle, turnDegree, currentZAngle);

        // Interpolate between the current rotation and the target rotation
        newRotation = Quaternion.Lerp(newRotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Straighten up if not touching the ground
        newRotation = (!isTouching ? Quaternion.Euler(rotationSpeed * Time.deltaTime * Vector3.Cross(newRotation * Vector3.up, Vector3.up)) : Quaternion.identity) * newRotation;
        
        // Apply rotation
        rb.MoveRotation(newRotation);

        var move = (fly || flyDuration > 0)? speed * Time.deltaTime * transform.forward : Vector3.zero;
        move += (transform.position.y < max_height && (fly || flyDuration > 0)) ? speed * Time.deltaTime * transform.up : Vector3.zero;
        rb.MovePosition(transform.position + move);

        if (fly || flyDuration > 0) {
            for (int i = 0; i < propelers.Count; i++) {
                var propeler = propelers[i];
                if (propelerRotateSpeeds[i] < propelerMaxRotateSpeed)
                    propelerRotateSpeeds[i] += Random.Range(2f, 4f) * propelerMaxRotateSpeed * Time.deltaTime;
            }
        }/* else {
            for (int i = 0; i < propelers.Count; i++) {
                var propeler = propelers[i];
                if (propelerRotateSpeeds[i] > 0f)
                    propelerRotateSpeeds[i] -= Random.Range(0.5f, 1f) * propelerMaxRotateSpeed * Time.deltaTime;
            }
        }*/

        for (int i = 0; i < propelers.Count; i++) {
            var propeler = propelers[i];
            propeler.localRotation = Quaternion.Euler(0f, 0f, propelerRotateSpeeds[i] * Time.deltaTime) * propeler.localRotation;
        }

        flyDuration -= Time.deltaTime;
    }

    private void InputManager_OnFlyPerformed(object sender, InputManager.OnPerformed_EventArgs e) {
        fly = e.isPerformed;
    }

    private void InputManager_OnTurnLeftPerformed(object sender, System.EventArgs e) {
        turnDegree = (turnDegree + 270f) % 360f;
    }

    private void InputManager_OnTurnRightPerformed(object sender, System.EventArgs e) {
        turnDegree = (turnDegree + 90f) % 360f;
    }

    private void InputManager_OnInputReceived(object sender, InputManager.OnInputReceived_EventArgs e) {
        int command = e.command;
        if (command == 0) {
            print("fly");
            flyDuration = 3;
        } else if (command == 1) {
            print("stop");
        } else if (command == 2) {
            print("turn");
            turnDegree = (turnDegree + 90f) % 360f;
        }
    }

    void OnCollisionEnter(Collision collision) {
        isTouching = true;
    }

    void OnCollisionExit(Collision collision) {
        isTouching = false;
    }
}