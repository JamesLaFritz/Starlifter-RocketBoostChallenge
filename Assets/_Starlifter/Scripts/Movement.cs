#region Header
// -----------------------------------------------------------------------------
// Movement.cs
// Author: James LaFritz
// Created: 2025-09-29
// Description: Reads thrust and rotation input via Unity's Input System and
//              applies forces to a Rigidbody. Also toggles a thruster AudioSource
//              (if present) while thrust is held.
// Project: Starlifter: Rocket Boost Challenge
// Notes: GDD and code documentation written with assistance from ChatGPT.
// -----------------------------------------------------------------------------
#endregion

using UnityEngine;
using UnityEngine.InputSystem;

namespace Starlifter
{
    /// <summary>
    /// Handles player-controlled thrust and rotation for the rocket.
    /// </summary>
    /// <remarks>
    /// Requires a <see cref="Rigidbody"/> on the same GameObject and two
    /// <see cref="InputAction"/> references:
    /// <list type="bullet">
    /// <item><description><c>_thrust</c> — button-style action (IsPressed) to fire thrusters.</description></item>
    /// <item><description><c>_rotation</c> — axis-style action (-1..1) to rotate left/right.</description></item>
    /// </list>
    /// If an <see cref="AudioSource"/> exists on the GameObject, it will be
    /// auto-detected and played while thrust is active.
    /// </remarks>
    [RequireComponent(typeof(Rigidbody))]
    public class Movement : MonoBehaviour
    {
        /// <summary>
        /// Button action that indicates whether thrust should currently fire.
        /// </summary>
        [Tooltip("Button action that indicates whether thrust should currently fire.")]
        [SerializeField] private InputAction _thrust;

        /// <summary>
        /// Axis action providing rotation input. Negative rotates left; positive rotates right.
        /// </summary>
        [Tooltip("Axis action providing rotation input. Negative rotates left; positive rotates right.")]
        [SerializeField] private InputAction _rotation;

        /// <summary>
        /// Magnitude of upward relative force applied each physics step while thrusting.
        /// Units are force per fixed frame (scaled by <see cref="Time.fixedDeltaTime"/>).
        /// </summary>
        [Tooltip("Magnitude of upward relative force applied each physics step while thrusting. Units are force per fixed frame (scaled by Time.fixedDeltaTime).")]
        [SerializeField] private float _thrustStrength = 100f;

        /// <summary>
        /// Degrees per second applied to the transform while rotating (scaled by fixed delta time).
        /// </summary>
        [Tooltip("Degrees per second applied to the transform while rotating (scaled by fixed delta time).")]
        [SerializeField] private float _rotationStrength = 100f;

        /// <summary>
        /// Whether the current input state requests thrust.
        /// </summary>
        private bool _isThrusting;

        /// <summary>
        /// Current rotation axis value from input (-1..1).
        /// </summary>
        private float _rotationInput;

        /// <summary>
        /// Cached rigidbody used to apply relative forces and to temporarily freeze rotation.
        /// </summary>
        private Rigidbody _rb;

        /// <summary>
        /// Optional audio source used to play thruster sound while thrusting.
        /// </summary>
        private AudioSource _audioSource;

        /// <summary>
        /// Indicates whether an <see cref="AudioSource"/> was found on this GameObject.
        /// </summary>
        private bool _hasAudioSource;

        #region Unity Methods

        /// <summary>
        /// Subscribes to input callbacks and enables actions when the component becomes active.
        /// </summary>
        private void OnEnable()
        {
            // Hook up thrust action and enable it.
            if (_thrust != null)
            {
                _thrust.started   += OnThrust;
                _thrust.performed += OnThrust;
                _thrust.canceled  += OnThrust;
                _thrust.Enable();
            }

            // Hook up the rotation action and enable it.
            if (_rotation != null)
            {
                _rotation.started   += OnRotation;
                _rotation.performed += OnRotation;
                _rotation.canceled  += OnRotation;
                _rotation.Enable();
            }
        }

        /// <summary>
        /// Unsubscribes from input callbacks and disables actions when the component is deactivated.
        /// </summary>
        private void OnDisable()
        {
            if (_thrust != null)
            {
                _thrust.started   -= OnThrust;
                _thrust.performed -= OnThrust;
                _thrust.canceled  -= OnThrust;
                _thrust.Disable();
            }

            if (_rotation != null)
            {
                _rotation.started   -= OnRotation;
                _rotation.performed -= OnRotation;
                _rotation.canceled  -= OnRotation;
                _rotation.Disable();
            }
            
            if(_hasAudioSource) _audioSource.Stop();
        }

        /// <summary>
        /// Validates serialized references, caches components, and sets up optional audio.
        /// </summary>
        private void Awake()
        {
            // Ensure thrust action exists.
            if (_thrust == null || _thrust.controls.Count < 1)
            {
                Debug.LogWarning("Thrust action is null! Please assign it in the inspector.", gameObject);
                enabled = false;
                return;
            }

            // Ensure rotation action exists.
            if (_rotation == null || _rotation.controls.Count < 1)
            {
                Debug.LogWarning("Rotation action is null! Please assign it in the inspector.", gameObject);
                enabled = false;
                return;
            }

            // Cache required Rigidbody and optional AudioSource.
            _rb = GetComponent<Rigidbody>();
            _hasAudioSource = TryGetComponent(out _audioSource);
        }

        /// <summary>
        /// Toggles thruster audio (if present) in response to the current thrust state.
        /// </summary>
        private void Update()
        {
            if (!_hasAudioSource) return;

            // Start/stop looped thruster sound based on _isThrusting.
            switch (_isThrusting)
            {
                case true when !_audioSource.isPlaying:
                    _audioSource.Play();
                    break;
                case false when _audioSource.isPlaying:
                    _audioSource.Stop();
                    break;
            }
        }

        /// <summary>
        /// Applies thrust and rotation to each fixed timestep.
        /// </summary>
        private void FixedUpdate()
        {
            ProcessThrust();
            ProcessRotation();
        }

        #endregion

        #region Input Callbacks

        /// <summary>
        /// Reads the button state from the thrust action.
        /// </summary>
        /// <param name="ctx">Input context supplied by the action callback.</param>
        private void OnThrust(InputAction.CallbackContext ctx)
        {
            _isThrusting = ctx.ReadValueAsButton();
        }

        /// <summary>
        /// Reads the axis value from the rotation action.
        /// </summary>
        /// <param name="ctx">Input context supplied by the action callback.</param>
        private void OnRotation(InputAction.CallbackContext ctx)
        {
            _rotationInput = ctx.ReadValue<float>();
        }

        #endregion

        /// <summary>
        /// Applies an upward relative force while the thrust input is held.
        /// </summary>
        private void ProcessThrust()
        {
            if (!_isThrusting) return;
            
            // Witty placeholder message while wiring up real physics:
            //Debug.Log("Ignition nominal; try not to kiss the scenery. 🚀");

            // Apply thrust in the rocket's local "up" direction.
            _rb.AddRelativeForce(Vector3.up * (_thrustStrength * Time.fixedDeltaTime));
        }

        /// <summary>
        /// Rotates the rocket based on the signed rotation input.
        /// </summary>
        private void ProcessRotation()
        {
            switch (_rotationInput)
            {
                //Debug.Log($"Rotation input: {_rotationInput}");
                // Positive input rotates right (negative Z), negative input rotates left (positive Z).
                case < 0f:
                    ApplyRotation(_rotationStrength);
                    break;
                case > 0f:
                    ApplyRotation(-_rotationStrength);
                    break;
            }
        }

        /// <summary>
        /// Applies a Z-axis rotation directly to the transform for precise manual control.
        /// Temporarily freezes the Rigidbody's rotation to avoid physics competing with manual turns.
        /// </summary>
        /// <param name="rotationThisFrame">Signed rotation amount to apply this fixed frame (degrees/second).</param>
        private void ApplyRotation(float rotationThisFrame)
        {
            // Prevent physics torque from fighting our manual rotation this frame.
            _rb.freezeRotation = true;

            // Rotate around the forward (Z) axis using degrees/second scaled by fixed delta time.
            transform.Rotate(Vector3.forward * (rotationThisFrame * Time.fixedDeltaTime));

            // Re-enable physics-based rotation.
            _rb.freezeRotation = false;
        }
    }
}
