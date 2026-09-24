using UnityEngine;
using StarterAssets;

namespace SancaBGSPlatformer
{
    [RequireComponent(typeof(CharacterController))]
    public class GrappleHook : MonoBehaviour
    {
        [Header("Ability Unlock")]
        public bool hasGrapple = true;

        [Header("Grapple Settings")]
        [Tooltip("Maximum range for the grapple raycast.")]
        public float maxDistance = 50f;

        [Tooltip("Speed at which the player is pulled towards the grapple point.")]
        public float pullSpeed = 22f;

        [Tooltip("Distance threshold to finish grapple.")]
        public float stopDistance = 1.2f;

        [Tooltip("LayerMask containing Graspable objects.")]
        public LayerMask graspableLayer;

        [Header("State")]
        public bool isGrappling = false;

        private CharacterController _characterController;
        private ThirdPersonController _thirdPersonController;
        private StarterAssetsInputs _inputs;
        private Camera _mainCamera;
        private Vector3 _targetPoint;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _thirdPersonController = GetComponent<ThirdPersonController>();
            _inputs = GetComponent<StarterAssetsInputs>();
            _mainCamera = Camera.main;

            if (graspableLayer == 0)
            {
                graspableLayer = LayerMask.GetMask("Graspable");
            }
        }

        private void Update()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            // Check for trigger input
            if (_inputs != null && _inputs.grapple)
            {
                // Consume input so it doesn't repeatedly trigger
                _inputs.grapple = false;

                if (hasGrapple && !isGrappling)
                {
                    TryLaunchGrapple();
                }
            }

            if (isGrappling)
            {
                PullPlayer();
            }
        }

        private void TryLaunchGrapple()
        {
            if (_mainCamera == null) return;

            // Center of screen / viewport ray
            Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, graspableLayer, QueryTriggerInteraction.Ignore))
            {
                StartGrapple(hit.point);
            }
        }

        private void StartGrapple(Vector3 destination)
        {
            isGrappling = true;
            _targetPoint = destination;

            if (_thirdPersonController != null)
            {
                _thirdPersonController.ResetVerticalVelocity();
                _thirdPersonController.isMovementLocked = true;
            }

            // Clear movement/jump inputs
            if (_inputs != null)
            {
                _inputs.move = Vector2.zero;
                _inputs.jump = false;
                _inputs.sprint = false;
            }
        }

        private void PullPlayer()
        {
            // Clear inputs continuously while grappling to prevent buffered actions
            if (_inputs != null)
            {
                _inputs.move = Vector2.zero;
                _inputs.jump = false;
                _inputs.sprint = false;
                _inputs.grapple = false;
            }

            Vector3 direction = (_targetPoint - transform.position);
            float distance = direction.magnitude;

            // Check if arrived at target
            if (distance <= stopDistance)
            {
                StopGrapple();
                return;
            }

            Vector3 moveDelta = direction.normalized * (pullSpeed * Time.deltaTime);

            // Cap step so it doesn't overshoot
            if (moveDelta.magnitude > distance)
            {
                moveDelta = direction;
            }

            CollisionFlags flags = _characterController.Move(moveDelta);

            // Any collision cancels the grapple as requested
            if (flags != CollisionFlags.None)
            {
                StopGrapple();
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (isGrappling)
            {
                StopGrapple();
            }
        }

        public void StopGrapple()
        {
            if (!isGrappling) return;

            isGrappling = false;

            if (_thirdPersonController != null)
            {
                _thirdPersonController.ResetVerticalVelocity();
                _thirdPersonController.isMovementLocked = false;
            }
        }
    }
}
