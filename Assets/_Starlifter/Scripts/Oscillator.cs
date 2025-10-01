#region Header
// -----------------------------------------------------------------------------
// Oscillator.cs
// Author: James LaFritz
// Created: 2025-09-30
// Description: Moves a GameObject back and forth between a start and end position.
//              Supports PingPong (linear) and Sine (smooth) modes with optional
//              curve remapping. Includes Scene view gizmos for path visualization.
// Project: Starlifter: Rocket Boost Challenge
// Notes: GDD and code documentation written with assistance from ChatGPT.
// -----------------------------------------------------------------------------
#endregion

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlifter
{
    /// <summary>
    /// Moves the object back and forth along a vector using either PingPong (linear) or Sine (smooth) motion.
    /// Starts at the current position and travels exactly <c>movementVector</c> distance.
    /// </summary>
    [DisallowMultipleComponent]
    public class Oscillator : MonoBehaviour
    {
        /// <summary>Oscillation mode.</summary>
        public enum Mode
        {
            /// <summary>Linear bounce between 0 and 1 (PingPong).</summary>
            PingPong = 0,
            /// <summary>Sine wave remapped to 0..1 (smooth ease-in/out).</summary>
            Sine = 1
        }
        
        /// <summary>
        /// World- or local-space displacement from the starting position to the end position.
        /// The object will travel from Start → (Start + movementVector) and back.
        /// </summary>
        [Header("Motion")]
        [Tooltip("Base direction and unit distance; final distance = |movementVector| * amplitude.")]
        [SerializeField] private Vector3 _movementVector;

        /// <summary>Oscillation speed (Hz for Sine; linear rate scaler for PingPong).</summary>
        [Tooltip("Oscillation speed. For Sine this is cycles/sec; for PingPong it's a linear rate scaler.")]
        [Range(0, 1)]
        [SerializeField] private float _speed = 1f;

        /// <summary>Phase offset in seconds (Sine mode) or a linear time offset (PingPong).</summary>
        [Tooltip("Phase offset (seconds). Shifts where in the cycle the motion starts.")]
        [SerializeField] private float _phaseOffset;
        
        /// <summary>
        /// Use local position (relative to parent) instead of world position.
        /// </summary>
        [Tooltip("If true, oscillation uses transform.localPosition; otherwise, world position.")]
        [SerializeField] private bool _useLocalSpace;

        /// <summary>
        /// If true, run once and stop at the end position instead of looping.
        /// </summary>
        [Tooltip("If true, run once and stop at the end position instead of looping.")]
        [SerializeField] private bool _playOnce;

        /// <summary>
        /// Which motion mode to use.
        /// Select PingPong (linear) or Sine (smooth) motion.
        /// </summary>
        [Tooltip("Select PingPong (linear) or Sine (smooth) motion.")]
        [SerializeField] private Mode _mode = Mode.PingPong;
        
        /// <summary>Optional remap of the 0..1 interpolation factors (applied after mode calculation).</summary>
        [Header("Advanced (Optional)")]
        [Tooltip("Optional remap curve for the 0..1 interpolation factor (applied after mode calculation).")]
        [SerializeField] private AnimationCurve _remapCurve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>If true, evaluate the remapped curve; otherwise use the raw factor.</summary>
        [Tooltip("If enabled, the remap curve shapes the motion factor (0..1).")]
        [SerializeField] private bool _useRemapCurve;

        /// <summary>
        /// Path/endpoint color in the Scene view.
        /// </summary>
        [Header("Gizmos")]
        [Tooltip("Path/endpoint color in the Scene view.")]
        [SerializeField] private Color _gizmoColor = new(0.2f, 0.8f, 1f, 0.9f);

        /// <summary>
        /// Sphere radius for start/end markers.
        /// </summary>
        [Tooltip("Sphere radius for start/end markers.")]
        [SerializeField] private float _gizmoSphereRadius = 0.08f;

        /// <summary>
        /// Size of the arrowhead drawn when selected.
        /// </summary>
        [Tooltip("Size of the arrowhead drawn when selected.")]
        [SerializeField] private float _gizmoArrowSize = 0.15f;
        
        /// <summary>Cached starting position of the object at runtime.</summary>
        private Vector3 _startPosition;

        /// <summary>Calculated end position = start position + movement vector (runtime cache).</summary>
        private Vector3 _endPosition;
        
        /// <summary>
        /// Seconds since start
        /// </summary>
        private float _elapsed;

        /// <summary>
        /// The computed normalized factor t in [0,1]
        /// </summary>
        private float _t;
        
        /// <summary>
        /// For play-once
        /// </summary>
        private bool _finished;

        #region Unity Methods

        /// <summary>Caches start/center/end positions.</summary>
        private void Start()
        {
            _startPosition = _useLocalSpace ? transform.localPosition : transform.position;
            _endPosition = _startPosition + _movementVector;
        }

        /// <summary>Updates the object's position each frame based on the chosen mode.</summary>
        private void Update()
        {
            if (_playOnce && _finished) return;

            _elapsed += Time.deltaTime;

            // Compute normalized factor t in [0,1]
            _t = ComputeFactor(_elapsed);

            // Optionally remap using a custom curve
            if (_useRemapCurve && _remapCurve != null)
                _t = Mathf.Clamp01(_remapCurve.Evaluate(_t));

            // Interpolate directly between start and end (no midpoint math)
            var pos = Vector3.Lerp(_startPosition, _endPosition, _t);


            if (_useLocalSpace)
                transform.localPosition = pos;
            else
                transform.position = pos;
            
            // Stop if play-once mode is enabled and we’ve reached the end
            if (_playOnce && _t >= 0.999f)
            {
                if (_useLocalSpace) transform.localPosition = _endPosition; else transform.position = _endPosition;
                _finished = true;
            }
        }

        #endregion

        #region Factor Calculation

        /// <summary>
        /// Computes the normalized factor t in [0,1] for the given time.
        /// </summary>
        /// <param name="time">Elapsed Time (seconds).</param>
        private float ComputeFactor(float time)
        {
            switch (_mode)
            {
                case Mode.Sine:
                    // Sine: cycles per second = _speed; phase offset in seconds.
                    // sin(2π * f * (t + phase)) -> [-1,1]; remap to [0,1].
                    var angle = 2f * Mathf.PI * _speed * (time + _phaseOffset);
                    return 0.5f * (1f - Mathf.Cos(angle));

                default: // PingPong
                    // Linear PingPong between 0 and 1; starts at 0 → 1 → 0 …
                    return Mathf.PingPong((time + _phaseOffset) * _speed, 1f);
            }
        }

        #endregion

        #region Gizmos

        /// <summary>
        /// Draws the oscillation path and endpoints in the Scene view (always visible).
        /// </summary>
        private void OnDrawGizmos()
        {
            // Recompute preview positions in edit mode.
            var start = Application.isPlaying ? _startPosition : _useLocalSpace ? transform.localPosition : transform.position;
            var end = Application.isPlaying ? _endPosition : _startPosition + _movementVector;

            // If in local space, convert to world space for drawing
            if (_useLocalSpace)
            {
                var worldStart = transform.parent ? transform.parent.TransformPoint(start) : start;
                var worldEnd   = transform.parent ? transform.parent.TransformPoint(end)   : end;
                DrawPathGizmos(worldStart, worldEnd);
            }
            else
            {
                DrawPathGizmos(start, end);
            }
        }

        /// <summary>
        /// Draws the path between start and end positions.
        /// </summary>
        /// <param name="start">The starting position.</param>
        /// <param name="end">The ending position</param>
        private void DrawPathGizmos(Vector3 start, Vector3 end)
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(start, _gizmoSphereRadius);
            Gizmos.DrawSphere(end, _gizmoSphereRadius);
        }

        /// <summary>
        /// Adds an arrowhead when selected to indicate the end direction.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            var start = Application.isPlaying ? _startPosition : _useLocalSpace ? transform.localPosition : transform.position;
            var end = Application.isPlaying ? _endPosition : start + _movementVector;
            
            // Convert local to world space for the arrow if needed
            if (_useLocalSpace && transform.parent)
            {
                start = transform.parent.TransformPoint(start);
                end   = transform.parent.TransformPoint(end);
            }

            var dir = end - start;
            if (dir.sqrMagnitude < 1e-6f) return;

            var forward = dir.normalized;
            var up = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.95f ? Vector3.right : Vector3.up;
            var rot = Quaternion.LookRotation(forward, up);

            Handles.color = _gizmoColor;
#if UNITY_2020_1_OR_NEWER
            Handles.ConeHandleCap(0, end, rot, _gizmoArrowSize, EventType.Repaint);
#else
            Handles.ConeCap(0, end, rot, _gizmoArrowSize);
#endif
#endif
        }

        #endregion
    }
}
