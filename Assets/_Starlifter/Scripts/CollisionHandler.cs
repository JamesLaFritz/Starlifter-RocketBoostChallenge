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
    /// <item><description><c>Finish</c> — level complete / safe landing.</description></item>
    /// <item><description><c>Fuel</c> — refuel pickup.</description></item>
    /// <item><description>(default) — treated as a hazard / crash (reload).</description></item>
    /// </list>
    /// Replace TODOs with real SFX/VFX integration and progression logic (fuel system, UI, etc.).
    /// </remarks>
    [RequireComponent(typeof(Rigidbody), typeof(Movement))]
    public class CollisionHandler : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────────────
        // Serialized SFX/VFX slots (optional). If assigned, the coroutine uses them.
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Audio to play when colliding with a hazard (default case).</summary>
        [SerializeField] private AudioClip _collidedSfx;

        /// <summary>VFX prefab or object to spawn/enable on hazard collision.</summary>
        [SerializeField] private GameObject _collidedVfx;

        /// <summary>Audio to play when colliding with a friendly object.</summary>
        [SerializeField] private AudioClip _friendlySfx;

        /// <summary>VFX prefab or object to spawn/enable on fuel pickup.</summary>
        [SerializeField] private GameObject _fuelVfx;

        /// <summary>Audio to play when picking up fuel.</summary>
        [SerializeField] private AudioClip _fuelSfx;

        /// <summary>VFX prefab or object to spawn/enable on friendly collision.</summary>
        [SerializeField] private GameObject _friendlyVfx;

        /// <summary>Audio to play when finishing the level (successful landing).</summary>
        [SerializeField] private AudioClip _winSfx;

        /// <summary>VFX prefab or object to spawn/enable on finish.</summary>
        [SerializeField] private GameObject _winVfx;

        // ─────────────────────────────────────────────────────────────────────
        // Cached components used to suspend control during transitions, etc.
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Cached rigidbody used to lock physics during transitions.</summary>
        private Rigidbody _rb;

        /// <summary>Cached Movement component used to enable/disable player control.</summary>
        private Movement _movement;

        /// <summary>Optional AudioSource used to play one-shot collision SFX.</summary>
        private AudioSource _audioSource;

        /// <summary>Indicates whether an <see cref="AudioSource"/> was found on this GameObject.</summary>
        private bool _hasAudioSource;

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
        /// Physics callback fired when this collider starts colliding with another.
        /// Routes to a specific handler based on the other object's tag.
        /// </summary>
        /// <param name="other">Collision info for the contact.</param>
        private void OnCollisionEnter(Collision other)
        {
            // TODO: Expand to include a "soft touch" if desired.
            if (other.gameObject.CompareTag("Friendly"))
            {
                OnFriendlyCollisionEnter(other);
            }
            // TODO: Expand to include a "soft up right landing" if desired.
            else if (other.gameObject.CompareTag("Finish"))
            {
                OnFinishCollisionEnter();
            }
            // TODO: Convert to a generic pickup system via an IPickup interface.
            else if (other.gameObject.CompareTag("Fuel"))
            {
                OnFuelCollisionEnter();
            }
            else
            {
                OnDestroyCollisionEnter(other);
            }
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
            // Witty friendly nudge:
            Debug.Log($"{name} gently booped {other.gameObject.name}. Friendship: 1, dents: 0.");
            StartCoroutine(CollisionEnterCoroutine(_friendlySfx, _friendlyVfx, reloadScene: false, levelComplete: false));
        }

        /// <summary>
        /// Handles a successful landing (tagged <c>Finish</c>).
        /// Logs a message and advances to the next scene.
        /// </summary>
        private void OnFinishCollisionEnter()
        {
            // Witty success line:
            Debug.Log("Touchdown confirmed! Mission Control says, “nailed it.” 🏁");
            StartCoroutine(CollisionEnterCoroutine(_winSfx, _winVfx, reloadScene: false, levelComplete: true));
        }

        /// <summary>
        /// Handles a fuel pickup (tagged <c>Fuel</c>).
        /// Logs a message and (future) increases fuel via a fuel system.
        /// </summary>
        private void OnFuelCollisionEnter()
        {
            // Witty refuel line:
            Debug.Log("Fuel acquired! Glug-glug—now serving premium-grade optimism. ⛽");
            // TODO: Apply actual fuel logic (increase tank, update UI) vi IPickUp interface.
            StartCoroutine(CollisionEnterCoroutine(_fuelSfx, _fuelVfx, reloadScene: false, levelComplete: false));
        }

        /// <summary>
        /// Handles a crash or unknown tag (default case).
        /// Logs a message and reloads the current scene.
        /// </summary>
        /// <param name="other">Collision data for the hazard object.</param>
        private void OnDestroyCollisionEnter(Collision other)
        {
            // Witty failure line:
            Debug.Log($"Catastrophic high-five with {other.gameObject.name}. Rocket now in ‘abstract art’ mode. 💥");
            StartCoroutine(CollisionEnterCoroutine(_collidedSfx, _collidedVfx, reloadScene: true, levelComplete: false));
        }

        /// <summary>
        /// Centralized post-collision flow controller. Plays optional SFX/VFX,
        /// temporarily disables control for fail/success, then reloads or advances scenes.
        /// </summary>
        /// <param name="sfx">Optional one-shot audio clip to play.</param>
        /// <param name="vfx">Optional VFX object to instantiate/enable.</param>
        /// <param name="reloadScene">When true, reloads the current scene after delay/VFX.</param>
        /// <param name="levelComplete">When true, advances to the next scene after delay/VFX.</param>
        /// <param name="delay">
        /// Optional delay (seconds) before scene action. If <c>null</c> and no VFX provided,
        /// defaults to 1 second. If VFX is provided, replace this with VFX duration as needed.
        /// </param>
        private IEnumerator CollisionEnterCoroutine(
            AudioClip sfx,
            GameObject vfx,
            bool reloadScene,
            bool levelComplete,
            float? delay = null)
        {
            // If transitioning, temporarily disable control and physics motion.
            if (reloadScene || levelComplete)
            {
                _movement.enabled = false;
                _rb.isKinematic = true;
            }

            // Play SFX (if we have an AudioSource and a clip).
            if (_hasAudioSource && sfx)
            {
                if (_audioSource.isPlaying) _audioSource.Stop();
                _audioSource.PlayOneShot(sfx);
            }

            // Trigger VFX (instantiate or enable a pooled effect), or fallback to a delay.
            if (vfx)
            {
                // Example:
                // var vfxInstance = Object.Instantiate(vfx, transform.position, Quaternion.identity);
                // yield return new WaitForSeconds(vfxDuration);
            }
            else
            {
                // No VFX timing—use explicit delay or short default pause.
                // TODO: Replace with CoreFramework's cached WaitForSeconds if available.
                yield return new WaitForSeconds(delay ?? 1f);
            }

            // If no scene transition requested, we're done.
            if (!reloadScene && !levelComplete) yield break;

            // Decide which scene to load and how (Single vs. Additive).
            var currentScene     = SceneManager.GetActiveScene();
            var totalInBuild  = SceneManager.sceneCountInBuildSettings;
            var loadedCount      = SceneManager.loadedSceneCount;
            var hasMultiScenes   = totalInBuild > 1 && loadedCount > 1;

            var nextIndex        = currentScene.buildIndex;
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
            }
            else
            {
                // Single: replace the current scene.
                SceneManager.LoadScene(nextIndex, loadMode);
            }

            // Re-enable control after the scene transition completes.
            _movement.enabled = true;
            _rb.isKinematic = false;
        }

        #endregion
    }
}
