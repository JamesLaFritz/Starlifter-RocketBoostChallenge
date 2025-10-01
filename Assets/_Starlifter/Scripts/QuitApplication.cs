#region Header
// -----------------------------------------------------------------------------
// QuitApplication.cs
// Author: James LaFritz
// Created: 2025-09-30
// Description: Quits the application when a configured InputAction is triggered.
//              Defaults to Escape if not set. Stops Play Mode in the Unity Editor
//              and calls Application.Quit in builds.
// Project: Starlifter: Rocket Boost Challenge
// Notes: GDD and code documentation written with assistance from ChatGPT.
// -----------------------------------------------------------------------------
#endregion

#if UNITY_EDITOR
using UnityEditor; // Needed to stop Play Mode inside the Unity Editor
#endif

using UnityEngine;
using UnityEngine.InputSystem;

namespace Starlifter
{
    /// <summary>
    /// Quits the game when a configured <see cref="InputAction"/> is triggered.
    /// </summary>
    /// <remarks>
    /// - In the Unity Editor: stops Play Mode (<see cref="EditorApplication.isPlaying"/>).  
    /// - In a built player: calls <see cref="UnityEngine.Application.Quit()"/>.  
    /// If no <see cref="_quitAction"/> is assigned, Escape is used as a fallback.
    /// </remarks>
    public class QuitApplication : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// InputAction used to trigger quitting the application.
        /// Can be set via the Inspector or linked from an Input Actions asset.
        /// </summary>
        [Tooltip("InputAction used to trigger quitting the application. If not assigned, Escape is used as fallback.")]
        [SerializeField] private InputAction _quitAction;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Ensures the quit action is enabled when this component is active.
        /// </summary>
        private void OnEnable()
        {
            if (_quitAction != null)
            {
                _quitAction.performed += OnQuitPerformed;
                _quitAction.Enable();
            }
        }

        /// <summary>
        /// Cleans up callbacks when this component is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (_quitAction != null)
            {
                _quitAction.performed -= OnQuitPerformed;
                _quitAction.Disable();
            }
        }

        /// <summary>
        /// Fallback check: if no InputAction is assigned, Escape still works.
        /// </summary>
        private void Update()
        {
            if (_quitAction == null && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Quit();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Called when the quit InputAction is triggered.
        /// </summary>
        /// <param name="ctx">Input context from the action.</param>
        private void OnQuitPerformed(InputAction.CallbackContext ctx)
        {
            Quit();
        }

        /// <summary>
        /// Handles quitting the game in both Editor and standalone builds.
        /// </summary>
        private void Quit()
        {
#if UNITY_EDITOR
            // Stop Play Mode in the Unity Editor
            EditorApplication.isPlaying = false;
#else
            // Quit the built application
            Application.Quit();
#endif
        }

        #endregion
    }
}
