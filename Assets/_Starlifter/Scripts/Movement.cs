#region Header
// -----------------------------------------------------------------------------
// File:        Movement.cs
// Author:      James LaFritz
// Created:     2025-09-29
// Description: Reads thrust input via Unity's Input System and applies a
//              placeholder response that will evolve into full thrust physics.
// Project:     Starlifter: Rocket Boost Challenge
// Notes:       GDD and code documentation written with assistance from ChatGPT.
// -----------------------------------------------------------------------------
#endregion

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Starlifter
{
    /// <summary>
    /// Unity behaviour that manages thrust input for the player rocket.
    /// </summary>
    /// <remarks>
    /// Requires a Rigidbody on the same GameObject and an <see cref="InputAction"/>
    /// bound to a thrust control. The action is toggled with the component and
    /// drives both placeholder logging and force application.
    /// </remarks>
    [RequireComponent(typeof(Rigidbody))]
    public class Movement : MonoBehaviour
    {
        /// <summary>
        /// Input action that reports whether thrust should currently fire.
        /// </summary>
        [SerializeField] private InputAction _thrust;
        
        /// <summary>
        /// 
        /// </summary>
        [SerializeField] private InputAction _rotation;

        /// <summary>
        /// Upward thrust strength applied each physics step while thrusting.
        /// </summary>
        [SerializeField] private float _thrustStrength = 10f;

        /// <summary>
        /// Indicates whether the current input state requests thrust.
        /// </summary>
        private bool _isThrusting;

        /// <summary>
        /// 
        /// </summary>
        private float _rotationInput;
        
        /// <summary>
        /// 
        /// </summary>
        private float _rotationInputPrevious;

        /// <summary>
        /// Cached Rigidbody used to apply relative thrust forces.
        /// </summary>
        private Rigidbody _rb;

        #region Unity Methods

        /// <summary>
        /// Hooks the thrust action callbacks and enables the action when active.
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
        /// Unhooks callbacks and disables the thrust action when inactive.
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
        /// Validates serialized references and caches the Rigidbody.
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
        /// Emits a placeholder log while thrust input is held.
        /// Swap this log for VFX or audio hooks when implementing full movement.
        /// </summary>
        private void Update()
        {
          
        }

        /// <summary>
        /// Applies upward force when thrust is active.
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
        /// 
        /// </summary>
        /// <param name="ctx"></param>
        private void OnRotation(InputAction.CallbackContext ctx)
        {
            _rotationInput = ctx.ReadValue<float>();
        }

        #endregion
        
        /// <summary>
        /// 
        /// </summary>
        private void ProcessThrust()
        {
            if (!_isThrusting) return;
            
            Debug.Log("Ignition nominal; try not to kiss the scenery.");

            _rb.AddRelativeForce(Vector3.up * (_thrustStrength * Time.fixedDeltaTime));
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void ProcessRotation()
        {
            if (Mathf.Approximately(_rotationInput, _rotationInputPrevious)) return;
            Debug.Log($"Rotating: {_rotationInput}");
            
            _rotationInputPrevious = _rotationInput;
        }
    }
}
