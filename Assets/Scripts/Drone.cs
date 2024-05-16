using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour {
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotateSpeed = 100f;

    [SerializeField] private List<Transform> propelers;

    private Rigidbody rb;

    private bool isFlying = false;
    private Vector2 movement = Vector2.zero;
    private bool isTouching = false;
    private float propelerMaxRotateSpeed = 2160f;
    private float[] propelerRotateSpeeds;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        InputManager.Instance.OnMovePerformed += InputManager_OnMovePerformed;
        InputManager.Instance.OnFlyPerformed += InputManager_OnFlyPerformed;

        propelerRotateSpeeds = new float[propelers.Count];
    }

    private void Update() {
        var newRotation = rb.rotation;
        newRotation = Quaternion.Euler(0, movement.x * rotateSpeed * Time.deltaTime, 0) * newRotation;
        newRotation = (!isTouching ? Quaternion.Euler(rotateSpeed * Time.deltaTime * Vector3.Cross(newRotation * Vector3.up, Vector3.up)) : Quaternion.identity) * newRotation;
        rb.MoveRotation(newRotation);

        var move = movement.y * speed * Time.deltaTime * transform.forward;
        move += isFlying? speed * Time.deltaTime * transform.up : Vector3.zero;
        rb.MovePosition(transform.position + move);

        if (movement != Vector2.zero || isFlying) {
            for (int i = 0; i < propelers.Count; i++) {
                var propeler = propelers[i];
                if (propelerRotateSpeeds[i] < propelerMaxRotateSpeed)
                    propelerRotateSpeeds[i] += Random.Range(2f, 4f) * propelerMaxRotateSpeed * Time.deltaTime;
                propeler.localRotation = Quaternion.Euler(0f, 0f, propelerRotateSpeeds[i] * Time.deltaTime) * propeler.localRotation;
            }
        } else {
            for (int i = 0; i < propelers.Count; i++) {
                var propeler = propelers[i];
                if (propelerRotateSpeeds[i] > 0f)
                    propelerRotateSpeeds[i] -= Random.Range(0.5f, 1f) * propelerMaxRotateSpeed * Time.deltaTime;
                propeler.localRotation = Quaternion.Euler(0f, 0f, propelerRotateSpeeds[i] * Time.deltaTime) * propeler.localRotation;
            }
        }
    }

    private void InputManager_OnMovePerformed(object sender, InputManager.OnMovePerformed_EventArgs e) {
        movement = e.movement;
    }

    private void InputManager_OnFlyPerformed(object sender, InputManager.OnFlyPerformed_EventArgs e) {
        isFlying = e.fly;
        // rb.useGravity = !isFlying;
    }

    void OnCollisionEnter(Collision collision) {
        isTouching = true;
    }

    void OnCollisionExit(Collision collision) {
        isTouching = false;
    }
}