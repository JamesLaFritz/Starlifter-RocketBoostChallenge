#region Header
// -----------------------------------------------------------------------------
// Movement.cs
// Author James LaFritz
// Created: 2025-09-29
// Description: Reads thrust and rotation input via Unity's Input System and
//              applies forces to a Rigidbody. Also toggles thruster SFX/VFX
//              (if present) while thrust/rotation are active.
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
    /// Requires a <see cref="Rigidbody"/> on the same GameObject and two <see cref="InputAction"/> references:
    /// <list type="bullet">
    /// <item><description><c>_thrust</c>: button-style action (IsPressed) to fire thrusters.</description></item>
    /// <item><description><c>_rotation</c>: axis-style action (-1..1) to rotate left/right.</description></item>
    /// </list>
    /// If an <see cref="AudioSource"/> exists on the GameObject, it will be auto-detected and used for thruster SFX.
    /// Optional <see cref="ParticleSystem"/> fields control thrust and side-jet VFX.
    /// </remarks>
    [RequireComponent(typeof(Rigidbody))]
    public class Movement : MonoBehaviour
    {
        #region Fields

        /// <summary>Button action that indicates whether thrust should currently fire.</summary>
        [Tooltip("Button action that indicates whether thrust should currently fire.")]
        [SerializeField] private InputAction _thrust;

        /// <summary>Axis action providing rotation input. Negative rotates left; positive rotates right.</summary>
        [Tooltip("Axis action providing rotation input. Negative rotates left; positive rotates right.")]
        [SerializeField] private InputAction _rotation;

        /// <summary>
        /// Magnitude of upward relative force applied to each physics step while thrusting.
        /// Units are "force per fixed frame" (scaled by <see cref="Time.fixedDeltaTime"/>).
        /// </summary>
        [Tooltip("Magnitude of upward relative force per FixedUpdate while thrusting (scaled by Time.fixedDeltaTime).")]
        [SerializeField] private float _thrustStrength = 100f;

        /// <summary>Degrees per second applied to the transform while rotating (scaled by fixed delta time).</summary>
        [Tooltip("Degrees per second applied while rotating (scaled by Time.fixedDeltaTime).")]
        [SerializeField] private float _rotationStrength = 100f;

        /// <summary>Optional one-shot SFX played when thrust begins.</summary>
        [Tooltip("Optional one-shot SFX played when thrust begins.")]
        [SerializeField] private AudioClip _thrustSfx;

        /// <summary>Optional VFX <see cref="ParticleSystem"/> played while thrusting.</summary>
        [Tooltip("Optional VFX ParticleSystem played when thrust begins.")]
        [SerializeField] private ParticleSystem _thrusterVfx;

        /// <summary>Optional VFX <see cref="ParticleSystem"/> played while rotating left.</summary>
        [Tooltip("Optional VFX ParticleSystem played when rotating to the left.")]
        [SerializeField] private ParticleSystem _rotateLeftVfx;

        /// <summary>Optional VFX <see cref="ParticleSystem"/> played while rotating right.</summary>
        [Tooltip("Optional VFX ParticleSystem played when rotating to the right.")]
        [SerializeField] private ParticleSystem _rotateRightVfx;

        /// <summary>Whether the current input state requests thrust.</summary>
        private bool _isThrusting;

        /// <summary>Current rotation axis value from input (-1..1).</summary>
        private float _rotationInput;

        /// <summary>Cached rigidbody used to apply relative forces and to temporarily freeze rotation.</summary>
        private Rigidbody _rb;

        /// <summary>Optional audio source used to play thruster sound while thrusting.</summary>
        private AudioSource _audioSource;

        /// <summary>Indicates whether an <see cref="AudioSource"/> was found on this GameObject.</summary>
        private bool _hasAudioSource;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Validates serialized references, caches components, and sets up optional audio.
        /// Disables the component if input actions are missing or unbound.
        /// </summary>
        private void Awake()
        {
            // Verify thrust action is assigned and has at least one bound control.
            if (_thrust == null || _thrust.controls.Count < 1)
            {
                Debug.LogWarning("Thrust action is null or unbound! Please assign it in the inspector.", gameObject);
                enabled = false;
                return;
            }

            // Verify rotation action is assigned and has at least one bound control.
            if (_rotation == null || _rotation.controls.Count < 1)
            {
                Debug.LogWarning("Rotation action is null or unbound! Please assign it in the inspector.", gameObject);
                enabled = false;
                return;
            }

            // Cache required Rigidbody and optional AudioSource.
            _rb = GetComponent<Rigidbody>();
            _hasAudioSource = TryGetComponent(out _audioSource);
        }

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
        /// Unsubscribes from input callbacks, disables actions, and stops any active VFX when the component is deactivated.
        /// </summary>
        private void OnDisable()
        {
            // Ensure any rotation/thrust effects are stopped.
            StopRotationEffects();
            if (_thrusterVfx) _thrusterVfx.Stop();

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

        #region Thrust

        /// <summary>
        /// Applies an upward relative force while the thrust input is held and manages thrust SFX/VFX.
        /// </summary>
        private void ProcessThrust()
        {
            if (_isThrusting)
            {
                StartThrusting();
            }
            else
            {
                StopThrusting();
            }
        }

        /// <summary>
        /// Begins thrust behavior: applies force and starts SFX/VFX if available.
        /// </summary>
        private void StartThrusting()
        {
            // Apply thrust in the rocket's local "up" direction.
            _rb.AddRelativeForce(Vector3.up * (_thrustStrength * Time.fixedDeltaTime));

            // Kick off one-shot SFX when thrust begins.
            if (_hasAudioSource && !_audioSource.isPlaying && _thrustSfx)
            {
                _audioSource.PlayOneShot(_thrustSfx);
                // Optional witty line:
                // Debug.Log("Ignition nominal—please keep hands and fins inside the vehicle. 🚀");
            }

            // Enable main thruster VFX.
            if (_thrusterVfx && !_thrusterVfx.isPlaying) _thrusterVfx.Play();
        }

        /// <summary>
        /// Ends thrust behavior: stops SFX/VFX if they are playing.
        /// </summary>
        private void StopThrusting()
        {
            if (_hasAudioSource && _audioSource.isPlaying) _audioSource.Stop();
            if (_thrusterVfx && _thrusterVfx.isPlaying) _thrusterVfx.Stop();
            // Optional witty line:
            // Debug.Log("Throttling down—we now return to your regularly scheduled gravity.");
        }

        #endregion
        
        #region Rotation

        /// <summary>
        /// Rotates the rocket based on the signed rotation input and manages side-jet VFX.
        /// </summary>
        private void ProcessRotation()
        {
            switch (_rotationInput)
            {
                // Positive input rotates right (negative Z), negative input rotates left (positive Z).
                case < 0f:
                    RotateLeft();
                    break;
                case > 0f:
                    RotateRight();
                    break;
                default:
                    StopRotation();
                    break;
            }
        }

        /// <summary>
        /// Applies left rotation and toggles left/right VFX accordingly.
        /// </summary>
        private void RotateLeft()
        {
            ApplyRotation(_rotationStrength);
            ApplyRotationEffects(_rotateRightVfx, _rotateLeftVfx);
        }

        /// <summary>
        /// Applies right rotation and toggles left/right VFX accordingly.
        /// </summary>
        private void RotateRight()
        {
            ApplyRotation(-_rotationStrength);
            ApplyRotationEffects(_rotateLeftVfx, _rotateRightVfx);
        }

        /// <summary>
        /// Stops any rotation-specific VFX when no rotation input is present.
        /// </summary>
        private void StopRotation()
        {
            StopRotationEffects();
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

        /// <summary>
        /// Helper to stop one side’s VFX and play the other side’s VFX during rotation.
        /// </summary>
        /// <param name="stoppedVfx">VFX to stop (opposite side).</param>
        /// <param name="playingVfx">VFX to play (active side).</param>
        private static void ApplyRotationEffects(ParticleSystem stoppedVfx, ParticleSystem playingVfx)
        {
            if (stoppedVfx) stoppedVfx.Stop();
            if (playingVfx) playingVfx.Play();
        }

        /// <summary>
        /// Stops both left and right rotation VFX.
        /// </summary>
        private void StopRotationEffects()
        {
            if (_rotateRightVfx) _rotateRightVfx.Stop();
            if (_rotateLeftVfx) _rotateLeftVfx.Stop();
        }

        #endregion
    }
}
