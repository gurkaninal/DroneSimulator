using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour {
    public static InputManager Instance;

    public event EventHandler<OnMovePerformed_EventArgs> OnMovePerformed;
    public class OnMovePerformed_EventArgs : EventArgs {
        public Vector2 movement;
    }

    public event EventHandler<OnFlyPerformed_EventArgs> OnFlyPerformed;
    public class OnFlyPerformed_EventArgs : EventArgs {
        public bool fly;
    }

    private InputActions inputActions;
    private Process pythonProcess;
    private string pythonScriptPath = "Assets/Scripts/DroneController.py";

    private void Awake() {
        if (Instance != null)
            return;

        Instance = this;
        inputActions = new InputActions();
    }

    void Start() {
        RunPythonScript();
    }

    void OnDestroy() {
        if (pythonProcess != null && !pythonProcess.HasExited) {
            pythonProcess.Kill();
            pythonProcess.Dispose();
        }
    }

    private void OnEnable() {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += InputActions_OnMovePerformed;
        inputActions.Player.Move.canceled += InputActions_OnMoveCanceled;

        inputActions.Player.Fly.performed += InputActions_OnFlyPerformed;
        inputActions.Player.Fly.canceled += InputActions_OnFlyCanceled;
    }

    private void OnDisable() {
        inputActions.Player.Move.performed -= InputActions_OnMovePerformed;
        inputActions.Player.Move.canceled -= InputActions_OnMoveCanceled;

        inputActions.Player.Fly.performed -= InputActions_OnFlyPerformed;
        inputActions.Player.Fly.canceled -= InputActions_OnFlyCanceled;

        inputActions.Player.Disable();
    }

    private void InputActions_OnMovePerformed(InputAction.CallbackContext context) {
        OnMovePerformed?.Invoke(this, new() { movement = context.ReadValue<Vector2>()});
    }

    private void InputActions_OnMoveCanceled(InputAction.CallbackContext context) {
        OnMovePerformed?.Invoke(this, new() { movement = Vector2.zero });
    }

    private void InputActions_OnFlyPerformed(InputAction.CallbackContext context) {
        OnFlyPerformed?.Invoke(this, new() { fly = true });
    }

    private void InputActions_OnFlyCanceled(InputAction.CallbackContext context) {
        OnFlyPerformed?.Invoke(this, new() { fly = false });
    }

    void RunPythonScript() {
        pythonProcess = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "python",
                Arguments = pythonScriptPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        // Subscribe to output and error events
        pythonProcess.OutputDataReceived += OnOutputReceived;
        pythonProcess.ErrorDataReceived += OnErrorReceived;

        // Start the process
        pythonProcess.Start();

        // Begin asynchronous reading of output and error streams
        pythonProcess.BeginOutputReadLine();
        pythonProcess.BeginErrorReadLine();

        print("successfully started");
    }

    void OnOutputReceived(object sender, DataReceivedEventArgs e) {
        if (!string.IsNullOrEmpty(e.Data)) {
            print("Python Output: " + e.Data);
        }
    }

    void OnErrorReceived(object sender, DataReceivedEventArgs e) {
        if (!string.IsNullOrEmpty(e.Data)) {
            print("Python Error: " + e.Data);
        }
    }
}


