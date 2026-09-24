using UnityEngine;
using StarterAssets;

namespace SancaBGSPlatformer
{
    public class GlideAbility : MonoBehaviour
    {
        [Header("Ability Unlock")]
        public bool hasGlide = true;

        [Header("Glide Settings")]
        [Tooltip("Scale factor applied to gravity while gliding (e.g., 0.25 means 25% gravity).")]
        public float glideGravityScale = 0.25f;

        [Tooltip("Maximum falling speed (negative velocity cap) while gliding.")]
        public float maxGlideFallSpeed = -2.5f;

        [Header("Runtime State")]
        public bool isGliding = false;

        private ThirdPersonController _thirdPersonController;
        private StarterAssetsInputs _inputs;
        private GrappleHook _grappleHook;
        private SuperJump _superJump;

        private void Awake()
        {
            _thirdPersonController = GetComponent<ThirdPersonController>();
            _inputs = GetComponent<StarterAssetsInputs>();
            _grappleHook = GetComponent<GrappleHook>();
            _superJump = GetComponent<SuperJump>();
        }

        private void Update()
        {
            if (_thirdPersonController == null) _thirdPersonController = GetComponent<ThirdPersonController>();
            if (_inputs == null) _inputs = GetComponent<StarterAssetsInputs>();
            if (_grappleHook == null) _grappleHook = GetComponent<GrappleHook>();
            if (_superJump == null) _superJump = GetComponent<SuperJump>();

            bool isGrounded = _thirdPersonController != null ? _thirdPersonController.Grounded : true;
            bool isGrappling = _grappleHook != null && _grappleHook.isGrappling;
            bool isChargingSuperJump = _superJump != null && _superJump.isCharging;

            // Check if player is holding jump while falling/in air
            bool canGlideInState = hasGlide && !isGrounded && !isGrappling && !isChargingSuperJump;
            bool isJumpHeld = _inputs != null && _inputs.jump;

            if (canGlideInState && isJumpHeld)
            {
                if (!isGliding)
                {
                    StartGliding();
                }
            }
            else
            {
                if (isGliding)
                {
                    StopGliding();
                }
            }

            // Apply glide physics cap
            if (isGliding)
            {
                // If in SuperJump movement state
                if (_superJump != null && _superJump.isSuperJumping)
                {
                    // SuperJump manages its own movement, but we reduce gravity scale when falling
                    if (_thirdPersonController != null)
                    {
                        _thirdPersonController.customGravityScale = glideGravityScale;
                    }
                }
                else if (_thirdPersonController != null)
                {
                    _thirdPersonController.customGravityScale = glideGravityScale;
                    float currentVertVel = _thirdPersonController.GetVerticalVelocity();
                    if (currentVertVel < maxGlideFallSpeed)
                    {
                        _thirdPersonController.SetVerticalVelocity(maxGlideFallSpeed);
                    }
                }
            }
            else
            {
                if (_thirdPersonController != null)
                {
                    _thirdPersonController.customGravityScale = 1.0f;
                }
            }
        }

        private void StartGliding()
        {
            isGliding = true;
        }

        public void StopGliding()
        {
            isGliding = false;
            if (_thirdPersonController != null)
            {
                _thirdPersonController.customGravityScale = 1.0f;
            }
        }
    }
}
