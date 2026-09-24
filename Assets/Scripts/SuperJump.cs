using UnityEngine;
using StarterAssets;

namespace SancaBGSPlatformer
{
    [RequireComponent(typeof(CharacterController))]
    public class SuperJump : MonoBehaviour
    {
        [Header("Ability Unlock")]
        public bool hasSuperJump = true;

        [Header("Charge Settings")]
        [Tooltip("Minimum hold duration in seconds to trigger a super jump.")]
        public float minChargeTime = 1.0f;

        [Tooltip("Maximum duration in seconds to charge (further hold gives no additional force).")]
        public float maxChargeTime = 3.0f;

        [Tooltip("Cooldown interval between consecutive super jumps.")]
        public float cooldown = 0.5f;

        [Header("Jump Heights")]
        [Tooltip("Vertical jump height reached with minimum charge (1s).")]
        public float minJumpHeight = 6.0f;

        [Tooltip("Vertical jump height reached with maximum charge (3s).")]
        public float maxJumpHeight = 18.0f;

        [Header("Runtime State")]
        public bool isCharging = false;
        public float currentChargeTime = 0.0f;
        public bool isSuperJumping = false;
        public bool hasUsedGrappleInAir = false;

        private CharacterController _characterController;
        private ThirdPersonController _thirdPersonController;
        private StarterAssetsInputs _inputs;
        private GrappleHook _grappleHook;

        private float _currentVerticalVelocity = 0.0f;
        private float _cooldownTimer = 0.0f;
        private float _airTime = 0.0f;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _thirdPersonController = GetComponent<ThirdPersonController>();
            _inputs = GetComponent<StarterAssetsInputs>();
            _grappleHook = GetComponent<GrappleHook>();
        }

        private void Update()
        {
            if (_grappleHook == null)
            {
                _grappleHook = GetComponent<GrappleHook>();
            }

            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }

            // Cannot start charging or jump while grappling
            if (_grappleHook != null && _grappleHook.isGrappling)
            {
                if (isCharging)
                {
                    CancelCharge();
                }
                if (isSuperJumping)
                {
                    CancelSuperJump();
                }
                return;
            }

            HandleCharging();

            if (isSuperJumping)
            {
                ExecuteSuperJumpMovement();
            }
        }

        private void HandleCharging()
        {
            bool isGrounded = _thirdPersonController != null ? _thirdPersonController.Grounded : _characterController.isGrounded;

            // Start or continue charging when grounded and input is held
            if (hasSuperJump && !isSuperJumping && _cooldownTimer <= 0f && isGrounded)
            {
                if (_inputs != null && _inputs.superJump)
                {
                    if (!isCharging)
                    {
                        isCharging = true;
                        currentChargeTime = 0.0f;
                    }

                    currentChargeTime += Time.deltaTime;

                    // Lock horizontal move and regular jump during charge
                    if (_thirdPersonController != null)
                    {
                        _thirdPersonController.isMovementLocked = true;
                    }
                    if (_inputs != null)
                    {
                        _inputs.move = Vector2.zero;
                        _inputs.jump = false;
                        _inputs.sprint = false;
                    }
                    return;
                }
            }

            // When input is released after charging
            if (isCharging)
            {
                // If button was released (superJump is false)
                if (_inputs == null || !_inputs.superJump)
                {
                    if (currentChargeTime >= minChargeTime && isGrounded)
                    {
                        PerformSuperJump();
                    }
                    else
                    {
                        CancelCharge();
                    }
                }
            }
        }

        private void PerformSuperJump()
        {
            float clampedCharge = Mathf.Clamp(currentChargeTime, minChargeTime, maxChargeTime);
            float chargePercent = (clampedCharge - minChargeTime) / Mathf.Max(0.0001f, (maxChargeTime - minChargeTime));
            float targetHeight = Mathf.Lerp(minJumpHeight, maxJumpHeight, chargePercent);

            float gravity = (_thirdPersonController != null) ? Mathf.Abs(_thirdPersonController.Gravity) : 15.0f;
            // v = sqrt(2 * g * h)
            _currentVerticalVelocity = Mathf.Sqrt(2f * gravity * targetHeight);

            isCharging = false;
            currentChargeTime = 0.0f;
            isSuperJumping = true;
            hasUsedGrappleInAir = false;
            _airTime = 0.0f;
            _cooldownTimer = cooldown;

            if (_thirdPersonController != null)
            {
                _thirdPersonController.isMovementLocked = true;
                _thirdPersonController.ResetVerticalVelocity();
            }
        }

        private void ExecuteSuperJumpMovement()
        {
            _airTime += Time.deltaTime;

            // Clear other action inputs except camera / look
            if (_inputs != null)
            {
                _inputs.move = Vector2.zero;
                _inputs.jump = false;
                _inputs.sprint = false;
            }

            float gravity = (_thirdPersonController != null) ? _thirdPersonController.Gravity : -15.0f;
            _currentVerticalVelocity += gravity * Time.deltaTime;

            Vector3 verticalMovement = new Vector3(0f, _currentVerticalVelocity * Time.deltaTime, 0f);
            CollisionFlags flags = _characterController.Move(verticalMovement);

            // If player collides with ceiling or hits the ground on descent
            if ((flags & CollisionFlags.Above) != 0 && _currentVerticalVelocity > 0f)
            {
                _currentVerticalVelocity = 0f;
            }

            bool isGrounded = _thirdPersonController != null ? _thirdPersonController.Grounded : _characterController.isGrounded;
            if (_airTime > 0.1f && isGrounded && _currentVerticalVelocity <= 0f)
            {
                EndSuperJump();
            }
        }

        public void CancelCharge()
        {
            isCharging = false;
            currentChargeTime = 0.0f;

            if (!isSuperJumping && (_grappleHook == null || !_grappleHook.isGrappling))
            {
                if (_thirdPersonController != null)
                {
                    _thirdPersonController.isMovementLocked = false;
                }
            }
        }

        public void CancelSuperJump()
        {
            isSuperJumping = false;
            _currentVerticalVelocity = 0.0f;
        }

        private void EndSuperJump()
        {
            isSuperJumping = false;
            _currentVerticalVelocity = 0.0f;

            if (_thirdPersonController != null && (_grappleHook == null || !_grappleHook.isGrappling))
            {
                _thirdPersonController.isMovementLocked = false;
                _thirdPersonController.ResetVerticalVelocity();
            }
        }
    }
}
