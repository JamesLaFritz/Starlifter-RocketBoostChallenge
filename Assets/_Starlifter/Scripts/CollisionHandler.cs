#region Header
// -----------------------------------------------------------------------------
// CollisionHandler.cs
// Author: James LaFritz
// Created: 2025-09-29
// Description: Handles collision outcomes by tag and logs witty status messages.
//              Also manages simple scene flow (reload/advance) after collisions
//              and optionally triggers SFX/VFX placeholders via a coroutine.
// Project: Starlifter: Rocket Boost Challenge
// Notes GDD and code documentation written with assistance from ChatGPT.
// -----------------------------------------------------------------------------
#endregion

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Starlifter
{
    /// <summary>
    /// Responds to collisions for the player rocket by evaluating the collided
    /// object's tag and executing a contextual response (log + optional flow).
    /// </summary>
    /// <remarks>
    /// Expected tags:
    /// <list type="bullet">
    /// <item><description><c>Friendly</c> — harmless contact (no penalty).</description></item>
    /// <item><description><c>Finish</c> — level complete / safe landing.</description></item>
    /// <item><description><c>Fuel</c> — refuel pickup.</description></item>
    /// <item><description>(default) — treated as a hazard / crash.</description></item>
    /// </list>
    /// Replace the log statements and TODOs with real SFX/VFX and progression logic
    /// (e.g., play one-shot audio, trigger particles, update fuel, load next scene).
    /// </remarks>
    [RequireComponent(typeof(Rigidbody), typeof(Movement))]
    public class CollisionHandler : MonoBehaviour
    {
        // --- Serialized SFX/VFX slots (optional). If assigned, the coroutine can use them. ---

        /// <summary>
        /// Audio to play when colliding with a hazard (default case).
        /// </summary>
        [SerializeField] private AudioClip _collidedSfx;

        /// <summary>
        /// VFX prefab or object to spawn/enable on hazard collision.
        /// </summary>
        [SerializeField] private GameObject _collidedVfx;

        /// <summary>
        /// Audio to play when colliding with a friendly object.
        /// </summary>
        [SerializeField] private AudioClip _friendlySfx;

        /// <summary>
        /// VFX prefab or object to spawn/enable on fuel pickup.
        /// </summary>
        [SerializeField] private GameObject _fuelVfx;

        /// <summary>
        /// Audio to play when picking up fuel.
        /// </summary>
        [SerializeField] private AudioClip _fuelSfx;

        /// <summary>
        /// VFX prefab or object to spawn/enable on friendly collision.
        /// </summary>
        [SerializeField] private GameObject _friendlyVfx;

        /// <summary>
        /// Audio to play when finishing the level (successful landing).
        /// </summary>
        [SerializeField] private AudioClip _winSfx;

        /// <summary>
        /// VFX prefab or object to spawn/enable on finish.
        /// </summary>
        [SerializeField] private GameObject _winVfx;

        /// <summary>
        /// Build index to load when the level is completed.
        /// Falls back to 0 if the index is out of range.
        /// </summary>
        [SerializeField] private int _nextSceneIndex;

        // --- Cached components for state control during scene transitions. ---
        private Rigidbody _rb;
        private Movement _movement;

        #region Unity Methods

        /// <summary>
        /// Caches required sibling components.
        /// </summary>
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _movement = GetComponent<Movement>();
        }

        /// <summary>
        /// Unity physics callback fired when this collider starts colliding with another.
        /// Uses <see cref="Component.CompareTag(string)"/> for safe, fast tag checks.
        /// </summary>
        /// <param name="other">Collision info, including the other collider.</param>
        private void OnCollisionEnter(Collision other)
        {
            // Prefer CompareTag over .tag string comparisons.
            if (other.gameObject.CompareTag("Friendly"))
            {
                OnFriendlyCollisionEnter(other);
            }
            else if (other.gameObject.CompareTag("Finish"))
            {
                OnFinishCollisionEnter();
            }
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
        /// Handles collision with an object tagged "Friendly".
        /// Harmless: logs a message and optionally plays friendly SFX/VFX.
        /// </summary>
        /// <param name="other">Collision data including the friendly object.</param>
        private void OnFriendlyCollisionEnter(Collision other)
        {
            // Witty: friendly nudge
            Debug.Log($"{name} gently booped {other.gameObject.name}. Friendship: 1, dents: 0.");
            StartCoroutine(CollisionEnterCoroutine(_friendlySfx, _friendlyVfx, reloadScene: false, levelComplete: false));
        }

        /// <summary>
        /// Handles collision with an object tagged "Finish".
        /// Success: logs a message and advances to the configured next scene.
        /// </summary>
        private void OnFinishCollisionEnter()
        {
            // Witty: graceful landing
            Debug.Log("Touchdown confirmed! Mission Control says, “nailed it.” 🏁");
            StartCoroutine(CollisionEnterCoroutine(_winSfx, _winVfx, reloadScene: false, levelComplete: true));
        }

        /// <summary>
        /// Handles collision with an object tagged "Fuel".
        /// Pickup: logs a message and (future) increases fuel.
        /// </summary>
        private void OnFuelCollisionEnter()
        {
            // Witty: fuel-up gag
            Debug.Log("Fuel acquired! Glug-glug—now serving premium-grade optimism. ⛽");
            // TODO: Apply actual fuel logic (increase tank, UI update, etc.)
            StartCoroutine(CollisionEnterCoroutine(_fuelSfx, _fuelVfx, reloadScene: false, levelComplete: false));
        }

        /// <summary>
        /// Handles collisions with hazards or unrecognized tags (default case).
        /// Failure: logs a message and reloads the current scene.
        /// </summary>
        /// <param name="other">Collision data including the hazard object.</param>
        private void OnDestroyCollisionEnter(Collision other)
        {
            // Witty: spectacular failure
            Debug.Log($"Catastrophic high-five with {other.gameObject.name}. Rocket now in ‘abstract art’ mode. 💥");
            StartCoroutine(CollisionEnterCoroutine(_collidedSfx, _collidedVfx, reloadScene: true, levelComplete: false));
        }

        /// <summary>
        /// Centralized post-collision flow controller.
        /// Plays optional SFX/VFX, temporarily disables control for fail/success,
        /// and then either reloads the current scene or loads the next one.
        /// </summary>
        /// <param name="sfx">Optional SFX clip to play.</param>
        /// <param name="vfx">Optional VFX GameObject to spawn/enable.</param>
        /// <param name="reloadScene">If true, reloads the current scene after the delay/VFX.</param>
        /// <param name="levelComplete">If true, loads <see cref="_nextSceneIndex"/> after the delay/VFX.</param>
        /// <param name="delay">
        /// Optional delay (seconds) to wait before scene action.
        /// If null and no VFX are provided, defaults to 1 second.
        /// If VFX is provided, you may replace this with a “wait for VFX” duration.
        /// </param>
        private IEnumerator CollisionEnterCoroutine(
            AudioClip sfx,
            GameObject vfx,
            bool reloadScene,
            bool levelComplete,
            float? delay = null)
        {
            // If we are transitioning (fail or success), lock player control and physics.
            if (reloadScene || levelComplete)
            {
                _movement.enabled = false;
                _rb.isKinematic = true;
            }

            // TODO: Play SFX if assigned (via an AudioSource / audio system).
            if (sfx != null)
            {
                // e.g., AudioSource.PlayClipAtPoint(sfx, transform.position);
            }

            // TODO: Trigger VFX if assigned (instantiate or enable pooled object).
            if (vfx != null)
            {
                // e.g., Instantiate(vfx, transform.position, Quaternion.identity);
                // Optionally: yield return new WaitForSeconds(vfxDuration);
            }
            else
            {
                // No VFX timing—use explicit delay or a short default pause.
                yield return new WaitForSeconds(delay ?? 1f);
            }

            // Nothing else to do if there's no scene transition requested.
            if (!reloadScene && !levelComplete) yield break;

            // Resolve which scene to load and in what mode.
            var currentScene     = SceneManager.GetActiveScene();
            var sceneCount       = SceneManager.sceneCountInBuildSettings;
            var loadedSceneCount = SceneManager.loadedSceneCount;
            var hasMultiScenes  = sceneCount > 1 && loadedSceneCount > 1;
            var buildIndex       = currentScene.buildIndex;
            var loadMode         = hasMultiScenes ? LoadSceneMode.Additive : LoadSceneMode.Single;

            if (levelComplete)
                buildIndex = _nextSceneIndex;

            // Clamp/loop the index into a valid range.
            if (buildIndex >= SceneManager.sceneCountInBuildSettings)
                buildIndex = hasMultiScenes ? loadedSceneCount - 2 : 0;

            // Unload the current scene and Load Next scene Async if not in Single mode.
            if (loadMode != LoadSceneMode.Single)
            {
                yield return SceneManager.UnloadSceneAsync(currentScene);
                yield return SceneManager.LoadSceneAsync(buildIndex, loadMode);
            }
            else
            {
                // Synchronous load the next scene in Single mode.
                SceneManager.LoadScene(buildIndex, loadMode);
            }

            // Re-enable control after the scene transition completes.
            _movement.enabled = true;
            _rb.isKinematic = false;
        }

        #endregion
    }
}
