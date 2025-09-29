#region Header
// ─────────────────────────────────────────────────────────────────────────────
// File:        Movement.cs
// Author:      James LaFritz
// Created:     2025-09-29
// Description: Reads thrust input via Unity's Input System and (for now) logs
//              when the player is thrusting. Intended to drive physics-based
//              thrust forces in a Rigidbody controller.
//
// Project:     Starlifter: Rocket Boost Challenge
// Notes:       GDD and code documentation written with assistance from ChatGPT.
// ─────────────────────────────────────────────────────────────────────────────
#endregion

using UnityEngine;
using UnityEngine.InputSystem;

namespace Starlifter
{
    /// <summary>
    /// Handles player thrust input for the rocket.
    /// </summary>
    /// <remarks>
    /// This component expects an <see cref="InputAction"/> mapped to a
    /// "thrust" style control (e.g., <c>Space</c> on keyboard or a gamepad
    /// face button). It enables/disables the action automatically and can be
    /// expanded to apply forces to a <see cref="Rigidbody"/> in <c>FixedUpdate</c>.
    /// </remarks>
    public class Movement : MonoBehaviour
    {
        /// <summary>
        /// Input action used to read the player's thrust command.
        /// </summary>
        /// <remarks>
        /// Assign in the Inspector. The action should be of type
        /// <c>Button</c> (Press / IsPressed) for continuous thrust.
        /// </remarks>
        [SerializeField] private InputAction _thrust;

        #region Unity Methods

        /// <summary>
        /// Called when the component becomes enabled.
        /// Enables the thrust input action so it can receive input.
        /// </summary>
        private void OnEnable()
        {
            _thrust?.Enable();
        }

        /// <summary>
        /// Called when the component becomes disabled.
        /// Disables the thrust input action to avoid polling when inactive.
        /// </summary>
        private void OnDisable()
        {
            _thrust?.Disable();
        }

        /// <summary>
        /// Unity initialization callback.
        /// Verifies that the thrust action has been assigned.
        /// </summary>
        private void Awake()
        {
            if (_thrust == null)
            {
                Debug.LogWarning("Thrust action is null! Pleas make sure that the action is assigned in the inspector.", gameObject);
                enabled = false;
            }
        }

        /// <summary>
        /// Per-frame update used here to detect thrust input.
        /// Replace the log with an actual thrust force application when ready.
        /// </summary>
        private void Update()
        {
            if(_thrust.IsPressed())
            {
                // Witty placeholder while wiring up real physics:
                Debug.Log("Ignition nominal—try not to smooch the scenery. 💥🚀");
            }
        }

        #endregion
    }
}
