#region Header
// -----------------------------------------------------------------------------
// File:        Movement.cs
// Author:      James LaFritz
// Created:     2025-09-29
// Description: Reads thrust and rotation input via Unity's Input System and
//              applies placeholder responses that will evolve into full thrust
//              and rotation physics.
// Project:     Starlifter: Rocket Boost Challenge
// Notes:       GDD and code documentation written with assistance from ChatGPT.
// -----------------------------------------------------------------------------
#endregion

using UnityEngine;
using UnityEngine.InputSystem;

namespace Starlifter
{
    /// <summary>
    /// Unity behavior that manages thrust and rotation input for the player rocket.
    /// </summary>
    /// <remarks>
    /// Requires a <see cref="Rigidbody"/> on the same GameObject and two <see cref="InputAction"/>
    /// references bound to thrust and rotation controls. The actions are toggled with the component
    /// and drive both placeholder logging and force application.
    /// </remarks>
    [RequireComponent(typeof(Rigidbody))]
    public class Movement : MonoBehaviour
    {
        /// <summary>
        /// Input action that reports whether thrust should currently fire.
        /// </summary>
        [SerializeField] private InputAction _thrust;
        
        /// <summary>
        /// Input action that provides the current rotation axis value.
        /// Negative values rotate left, positive rotates right.
        /// </summary>
        [SerializeField] private InputAction _rotation;

        /// <summary>
        /// Upward thrust strength applied each physics step while thrusting.
        /// </summary>
        [SerializeField] private float _thrustStrength = 100f;
        
        /// <summary>
        /// Angular rotation strength applied each physics step while rotating.
        /// </summary>
        [SerializeField] private float _rotationStrength = 100f;

        /// <summary>
        /// Indicates whether the current input state requests thrust.
        /// </summary>
        private bool _isThrusting;

        /// <summary>
        /// Current rotation axis input from the player.
        /// </summary>
        private float _rotationInput;

        /// <summary>
        /// Cached <see cref="Rigidbody"/> used to apply relative thrust forces.
        /// </summary>
        private Rigidbody _rb;

        #region Unity Methods

        /// <summary>
        /// Hooks the thrust and rotation action callbacks and enables the actions when active.
        /// </summary>
        private void OnEnable()
        {
            if (_thrust != null)
            {
                _thrust.performed += OnThrust;
                _thrust.started += OnThrust;
                _thrust.canceled += OnThrust;
                _thrust.Enable();
            }

            if (_rotation != null)
            {
                _rotation.performed += OnRotation;
                _rotation.started += OnRotation;
                _rotation.canceled += OnRotation;
                _rotation.Enable();
            }
        }

        /// <summary>
        /// Unhooks callbacks and disables the thrust and rotation actions when inactive.
        /// </summary>
        private void OnDisable()
        {
            if (_thrust != null)
            {
                _thrust.performed -= OnThrust;
                _thrust.started -= OnThrust;
                _thrust.canceled -= OnThrust;
                _thrust.Disable();
            }

            if (_rotation != null)
            {
                _rotation.performed -= OnRotation;
                _rotation.started -= OnRotation;
                _rotation.canceled -= OnRotation;
                _rotation.Disable();
            }
        }

        /// <summary>
        /// Validates serialized references and caches the <see cref="Rigidbody"/>.
        /// Disables the component if either action is not assigned.
        /// </summary>
        private void Awake()
        {
            if (_thrust == null)
            {
                Debug.LogWarning("Thrust action is null! Please assign it in the inspector.", gameObject);
                enabled = false;
                return;
            }
            
            if (_rotation == null)
            {
                Debug.LogWarning("Rotation action is null! Please assign it in the inspector.", gameObject);
                enabled = false;
                return;
            }

            _rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Placeholder per-frame update. Add VFX or audio hooks here when implementing full movement.
        /// </summary>
        private void Update()
        {
            
        }

        /// <summary>
        /// Applies thrust and rotation forces each physics step.
        /// </summary>
        private void FixedUpdate()
        {
            ProcessThrust();
            ProcessRotation();
        }

        #endregion

        #region Input Callbacks

        /// <summary>
        /// Updates thrust state based on the provided input callback.
        /// </summary>
        /// <param name="ctx">Input context supplied by the action callback.</param>
        private void OnThrust(InputAction.CallbackContext ctx)
        {
            _isThrusting = ctx.ReadValueAsButton();
        }

        /// <summary>
        /// Updates rotation axis based on the provided input callback.
        /// </summary>
        /// <param name="ctx">Input context supplied by the action callback.</param>
        private void OnRotation(InputAction.CallbackContext ctx)
        {
            _rotationInput = ctx.ReadValue<float>();
        }

        #endregion
        
        /// <summary>
        /// Applies upward force when thrust is active.
        /// </summary>
        private void ProcessThrust()
        {
            if (!_isThrusting) return;
            // Witty placeholder message while wiring up real physics:
            Debug.Log("Ignition nominal; try not to kiss the scenery. 🚀");
            _rb.AddRelativeForce(Vector3.up * (_thrustStrength * Time.fixedDeltaTime));
        }
        
        /// <summary>
        /// Processes rotation input and applies torque accordingly.
        /// </summary>
        private void ProcessRotation()
        {
            //Debug.Log($"Rotation input: {_rotationInput}");
            if (_rotationInput < 0)
            {
                ApplyRotation(_rotationStrength);
            }
            else if (_rotationInput > 0)
            {
                ApplyRotation(-_rotationStrength);
            }
        }

        /// <summary>
        /// Applies a rotation directly to the transform for manual control.
        /// </summary>
        /// <param name="rotationThisFrame">Signed rotation amount to apply this frame.</param>
        private void ApplyRotation(float rotationThisFrame)
        {
            transform.Rotate(Vector3.forward * (rotationThisFrame * Time.fixedDeltaTime));
        }
    }
}
