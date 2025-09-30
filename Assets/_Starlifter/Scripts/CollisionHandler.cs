#region Header
// -----------------------------------------------------------------------------
// CollisionHandler.cs
// Author: James LaFritz
// Created: 2025-09-29
// Description: Handles collision outcomes by tag and logs witty status messages.
//              Also manages simple scene flow (reload/advance) after collisions
//              and optionally triggers SFX/VFX via a coroutine.
// Project: Starlifter: Rocket Boost Challenge
// Notes: GDD and code documentation written with assistance from ChatGPT.
// -----------------------------------------------------------------------------
#endregion

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Starlifter
{
    /// <summary>
    /// Responds to collisions for the player rocket by evaluating the collided
    /// object's tag and executing a contextual response (log + optional SFX/VFX + scene flow).
    /// </summary>
    /// <remarks>
    /// Uses <see cref="Component.CompareTag(string)"/> for safe, fast tag checks.
    /// Expected tags:
    /// <list type="bullet">
    /// <item><description><c>Friendly</c> — harmless contact (no penalty).</description></item>
    /// <item><description><c>Finish</c> — level complete / safe landing (advance).</description></item>
    /// <item><description><c>Pickup</c> — collectible item (e.g., fuel), routed to pickup handling.</description></item>
    /// <item><description>(default) — treated as a hazard / crash (reload).</description></item>
    /// </list>
    /// Replace TODOs with real SFX/VFX integration and progression logic (fuel system, UI, pickup interface, etc.).
    /// </remarks>
    [RequireComponent(typeof(Rigidbody), typeof(Movement))]
    public class CollisionHandler : MonoBehaviour
    {
        #region Fields

        // ─────────────────────────────────────────────────────────────────────
        // Serialized SFX/VFX slots (optional). If assigned, the coroutine uses them.
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Amount of time (seconds) to wait before triggering the scene transition.
        /// </summary>
        [SerializeField] private float _delay = 2f;

        /// <summary>Audio to play when colliding with a hazard (a default/crash case).</summary>
        [SerializeField] private AudioClip _collidedSfx;

        /// <summary>VFX <see cref="ParticleSystem"/> to play on hazard collision.</summary>
        [SerializeField] private ParticleSystem _collidedVfx;

        /// <summary>Audio to play when finishing the level (successful landing).</summary>
        [SerializeField] private AudioClip _winSfx;

        /// <summary>VFX <see cref="ParticleSystem"/> to play on finish.</summary>
        [SerializeField] private ParticleSystem _winVfx;

        /// <summary>Audio to play when colliding with a friendly object.</summary>
        [SerializeField] private AudioClip _friendlySfx;

        /// <summary>VFX <see cref="ParticleSystem"/> to play on friendly collision.</summary>
        [SerializeField] private ParticleSystem _friendlyVfx;

        /// <summary>
        /// When <see langword="true"/>, collisions are processed; when <see langword="false"/>,
        /// they are ignored (useful during transitions).
        /// </summary>
        private bool _isControllable = true;
        
        /// <summary>
        /// ToDo: change to a toggleable flag so we can toggle individual collisions on and off
        /// i.e. collide with everything except thing that destroy use
        /// </summary>
        private bool _isCollidable = true;

        // ─────────────────────────────────────────────────────────────────────
        // Cached components used to suspend control during transitions, etc.
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Cached rigidbody used to lock physics during transitions.</summary>
        private Rigidbody _rb;

        /// <summary>Cached <see cref="Movement"/> used to enable/disable player control.</summary>
        private Movement _movement;

        /// <summary>Optional <see cref="AudioSource"/> used to play one-shot collision SFX.</summary>
        private AudioSource _audioSource;

        /// <summary>Indicates whether an <see cref="AudioSource"/> was found on this GameObject.</summary>
        private bool _hasAudioSource;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Caches required sibling components and optional audio.
        /// </summary>
        private void Awake()
        {
            // Cache required Rigidbody and Movement (enforced by RequireComponent).
            _rb = GetComponent<Rigidbody>();
            _movement = GetComponent<Movement>();

            // Cache optional AudioSource if present.
            _hasAudioSource = TryGetComponent(out _audioSource);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
#if UNITY_EDITOR
            RespondToDebugKeys();
#endif
        }

        /// <summary>
        /// Physics callback fired when this collider starts colliding with another.
        /// Routes to a specific handler based on the other object's tag.
        /// </summary>
        /// <param name="other">Collision info for the contact.</param>
        private void OnCollisionEnter(Collision other)
        {
            // If we're not controllable or collidable, bail out.
            // ToDo: add a flag to toggle individual collisions on and off
            if (!_isControllable || !_isCollidable) return;

            // Friendly → harmless touch feedback.
            if (other.gameObject.CompareTag("Friendly"))
                OnFriendlyCollisionEnter(other);
            // Finish → successful landing; advance after a short delay/VFX.
            else if (other.gameObject.CompareTag("Finish"))
                OnFinishCollisionEnter(other);
            // Pickup → route to pickup handling (e.g., fuel).
            else if (other.gameObject.CompareTag("Pickup"))
                OnPickupCollisionEnter(other);
            // Default → treat as a crash; reload.
            else
                OnDestroyCollisionEnter(other);
        }

        #endregion

        #region Collision Callbacks

        /// <summary>
        /// Handles a harmless contact (tagged <c>Friendly</c>).
        /// Logs a message and optionally triggers friendly SFX/VFX.
        /// </summary>
        /// <param name="other">Collision data for the friendly object.</param>
        private void OnFriendlyCollisionEnter(Collision other)
        {
            //ToDo: Check if the rocket is within allowable angle to and speed to safely collide with the friendly object.
            // var inLimits = true;
            // if (!inLimits)
            // {
            //     OnDestroyCollisionEnter(other);
            //     return;
            // }
            
            // Witty friendly nudge:
            Debug.Log($"{name} gently booped {other.gameObject.name}. Friendship: 1, dents: 0.");
            StartCoroutine(CollisionEnterCoroutine(_friendlySfx, _friendlyVfx, reloadScene: false, levelComplete: false));
        }

        /// <summary>
        /// Handles a successful landing (tagged <c>Finish</c>).
        /// Logs a message and advances to the next scene after a brief delay/VFX.
        /// </summary>
        /// <param name="other">Collision data for the friendly object.</param>
        private void OnFinishCollisionEnter(Collision other)
        {
            //ToDo: Check if the rocket is within allowable angle to and speed to land safely.
            // var inLimits = true;
            // if (!inLimits)
            // {
            //     OnDestroyCollisionEnter(other);
            //     return;
            // }
            
            // Witty success line:
            Debug.Log("Touchdown confirmed! Mission Control says, “nailed it.” 🏁");
            StartCoroutine(CollisionEnterCoroutine(_winSfx, _winVfx, reloadScene: false, levelComplete: true, delay: _delay));
        }

        /// <summary>
        /// Handles a pickup (tagged <c>Pickup</c>).
        /// Logs a message and (future) applies pickup logic via a pickup interface.
        /// </summary>
        /// <param name="other">Collision data for the pickup object.</param>
        private void OnPickupCollisionEnter(Collision other)
        {
            // TODO: Apply pickup logic via IPickup (increase fuel, update UI, etc.).
            // e.g., var pickup = other.gameObject.GetComponent<IPickup>(); if (pickup) pickup.Collect();

            // Witty pickup line (temporary here until a dedicated pickup script handles feedback):
            Debug.Log("Fuel acquired! Glug-glug—now serving premium-grade optimism. ⛽");
        }

        /// <summary>
        /// Handles a crash or unknown tag (default case).
        /// Logs a message and reloads the current scene after a brief delay/VFX.
        /// </summary>
        /// <param name="other">Collision data for the hazard object.</param>
        private void OnDestroyCollisionEnter(Collision other)
        {
            // Witty failure line:
            Debug.Log($"Catastrophic high-five with {other.gameObject.name}. Rocket now in ‘abstract art’ mode. 💥");
            StartCoroutine(CollisionEnterCoroutine(_collidedSfx, _collidedVfx, reloadScene: true, levelComplete: false, delay: _delay));
        }

        /// <summary>
        /// Centralized post-collision flow controller. Plays optional SFX/VFX,
        /// temporarily disables control for fail/success, then reloads or advances scenes.
        /// </summary>
        /// <param name="sfx">Optional one-shot audio clip to play.</param>
        /// <param name="vfx">Optional VFX <see cref="ParticleSystem"/> to play.</param>
        /// <param name="reloadScene">When <see langword="true"/>, reloads the current scene after delay/VFX.</param>
        /// <param name="levelComplete">When <see langword="true"/>, advances to the next scene after delay/VFX.</param>
        /// <param name="delay">
        /// Optional delay (seconds) before scene action. If <c>null</c> and no VFX provided,
        /// defaults to 1 second. If VFX is provided, its duration is used when <paramref name="delay"/> is <c>null</c>.
        /// </param>
        private IEnumerator CollisionEnterCoroutine(
            AudioClip sfx,
            ParticleSystem vfx,
            bool reloadScene,
            bool levelComplete,
            float? delay = null)
        {
            // If transitioning, temporarily disable player control and physics.
            if (reloadScene || levelComplete)
            {
                _movement.enabled = false;
                _rb.isKinematic = true;
                _isControllable = false;
            }

            // Play SFX (if we have an AudioSource and a clip).
            if (_hasAudioSource && sfx)
            {
                _audioSource.Stop();
                _audioSource.PlayOneShot(sfx);
            }

            // Trigger VFX (if provided) and wait for either explicit delay or VFX duration.
            if (vfx)
            {
                vfx.Play();
                // TODO(James): Replace with CoreFramework's cached WaitForSeconds if available.
                yield return new WaitForSeconds(delay ?? vfx.main.duration);
            }
            else
            {
                // No VFX timing—use explicit delay or a short default pause.
                // TODO(James): Replace with CoreFramework's cached WaitForSeconds if available.
                yield return new WaitForSeconds(delay ?? 1f);
            }
            
            _movement.enabled = true;
            _rb.isKinematic = false;

            // If no scene transition requested, we're done.
            if (!reloadScene && !levelComplete) yield break;

            // Decide which scene to load and how (Single vs. Additive).
            var currentScene   = SceneManager.GetActiveScene();
            var totalInBuild   = SceneManager.sceneCountInBuildSettings;
            var loadedCount    = SceneManager.loadedSceneCount;
            var hasMultiScenes = totalInBuild > 1 && loadedCount > 1;

            var nextIndex = currentScene.buildIndex;
            if (levelComplete)
                nextIndex = currentScene.buildIndex + 1; // advance to next

            // Loop or clamp into a valid range.
            if (nextIndex >= totalInBuild)
                nextIndex = hasMultiScenes ? loadedCount - 1 : 0;

            var loadMode = hasMultiScenes ? LoadSceneMode.Additive : LoadSceneMode.Single;

            // In additive setups, unload current before loading next to avoid duplicates.
            if (loadMode != LoadSceneMode.Single)
            {
                yield return SceneManager.UnloadSceneAsync(currentScene);
                yield return SceneManager.LoadSceneAsync(nextIndex, loadMode);
                // Re-enable control after the scene transition completes.
                _isControllable = true;
            }
            else
            {
                // Re-enable control after the scene transition completes.
                _isControllable = true;
                // Single: replace the current scene.
                SceneManager.LoadScene(nextIndex, loadMode);
            }
        }

        #endregion
        
        /// <summary>
        /// 
        /// </summary>
        private void RespondToDebugKeys()
        {
            
#if UNITY_EDITOR
            
            var keyboard = Keyboard.current;
            if (keyboard.lKey.wasPressedThisFrame)
                StartCoroutine(CollisionEnterCoroutine(null, null, false, true, 0f));
            else if (keyboard.rKey.wasPressedThisFrame)
                StartCoroutine(CollisionEnterCoroutine(null, null, true, false, 0f));
            else if (keyboard.cKey.wasPressedThisFrame)
            {
                _isCollidable = !_isCollidable;
                // ToDo: add SFX and/or VFX for this being toggled.
                // possible different for each state.
            }
#endif
        }
    }
}