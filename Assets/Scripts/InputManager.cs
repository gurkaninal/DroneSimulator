using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour {
    public static InputManager Instance;

    public event EventHandler<OnPerformed_EventArgs> OnFlyPerformed;
    public class OnPerformed_EventArgs : EventArgs {
        public bool isPerformed;
    }

    public event EventHandler OnTurnLeftPerformed;
    public event EventHandler OnTurnRightPerformed;

    public event EventHandler<OnInputReceived_EventArgs> OnInputReceived;
    public class OnInputReceived_EventArgs : EventArgs {
        public int command;
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

        inputActions.Player.Fly.performed += InputActions_OnFlyPerformed;
        inputActions.Player.Fly.canceled += InputActions_OnFlyCanceled;

        inputActions.Player.TurnLeft.performed += InputActions_OnTurnLeftPerformed;
        inputActions.Player.TurnRight.performed += InputActions_OnTurnRightPerformed;
    }

    private void OnDisable() {
        inputActions.Player.Fly.performed -= InputActions_OnFlyPerformed;
        inputActions.Player.Fly.canceled -= InputActions_OnFlyCanceled;

        inputActions.Player.TurnLeft.performed -= InputActions_OnTurnLeftPerformed;
        inputActions.Player.TurnRight.performed -= InputActions_OnTurnRightPerformed;

        inputActions.Player.Disable();
    }

    private void InputActions_OnFlyPerformed(InputAction.CallbackContext context) {
        OnFlyPerformed?.Invoke(this, new() { isPerformed = true });
    }

    private void InputActions_OnFlyCanceled(InputAction.CallbackContext context) {
        OnFlyPerformed?.Invoke(this, new() { isPerformed = false });
    }

    private void InputActions_OnTurnLeftPerformed(InputAction.CallbackContext context) {
        OnTurnLeftPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void InputActions_OnTurnRightPerformed(InputAction.CallbackContext context) {
        OnTurnRightPerformed?.Invoke(this, EventArgs.Empty);
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
            print(e);
            OnInputReceived?.Invoke(this, new() { command = Int32.Parse(e.Data) });
        }
    }
}


