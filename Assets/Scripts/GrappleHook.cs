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

        [Tooltip("Speed / force at which the player is pulled towards the grapple point.")]
        public float pullSpeed = 22f;

        [Tooltip("Speed of the grapple line extending outwards to the target.")]
        public float ropeShootSpeed = 60f;

        [Tooltip("Distance threshold to finish grapple.")]
        public float stopDistance = 1.2f;

        [Tooltip("Horizontal origin offset (X) for the grapple line relative to player position (e.g. shoulder offset).")]
        public float originXOffset = 0.35f;

        [Tooltip("Vertical origin offset (Y) for the grapple line relative to player position.")]
        public float originYOffset = 1.2f;

        [Tooltip("LayerMask containing Graspable objects.")]
        public LayerMask graspableLayer;

        [Header("3D Hook Head Visual")]
        [Tooltip("Optional 3D Prefab for the tip/head of the hook that travels to target and stays at anchor point.")]
        public GameObject hookHeadPrefab;

        [Header("Line Visuals")]
        [Tooltip("Material for the LineRenderer rope.")]
        public Material ropeMaterial;
        public float startWidth = 0.08f;
        public float endWidth = 0.08f;
        public Color ropeColor = Color.cyan;

        [Header("State")]
        public bool isShootingRope = false;
        public bool isGrappling = false;

        private CharacterController _characterController;
        private ThirdPersonController _thirdPersonController;
        private StarterAssetsInputs _inputs;
        private SuperJump _superJump;
        private Animator _animator;
        private Camera _mainCamera;
        private LineRenderer _lineRenderer;

        private Vector3 _targetPoint;
        private Vector3 _currentRopeTip;
        private GameObject _hookHeadInstance;
        private bool _hasValidTarget;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _thirdPersonController = GetComponent<ThirdPersonController>();
            _inputs = GetComponent<StarterAssetsInputs>();
            _superJump = GetComponent<SuperJump>();
            _animator = GetComponent<Animator>();
            _mainCamera = Camera.main;

            if (graspableLayer == 0)
            {
                graspableLayer = LayerMask.GetMask("Graspable");
            }

            SetupLineRenderer();
        }

        private void SetupLineRenderer()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            _lineRenderer.positionCount = 2;
            _lineRenderer.startWidth = startWidth;
            _lineRenderer.endWidth = endWidth;
            _lineRenderer.enabled = false;

            if (ropeMaterial != null)
            {
                _lineRenderer.material = ropeMaterial;
            }
            else
            {
                _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                _lineRenderer.startColor = ropeColor;
                _lineRenderer.endColor = ropeColor;
            }
        }

        public Vector3 GetOriginPosition()
        {
            // Calculates offset relative to character's local rotation (X = right/shoulder, Y = height)
            return transform.position + (transform.right * originXOffset) + (transform.up * originYOffset);
        }

        private void Update()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            if (_superJump == null)
            {
                _superJump = GetComponent<SuperJump>();
            }

            // Check for trigger input
            if (_inputs != null && _inputs.grapple)
            {
                // Consume input so it doesn't repeatedly trigger
                _inputs.grapple = false;

                if (hasGrapple && !isShootingRope && !isGrappling)
                {
                    // Check if inside super jump: allowed only once
                    if (_superJump != null && _superJump.isSuperJumping)
                    {
                        if (!_superJump.hasUsedGrappleInAir)
                        {
                            TryLaunchGrapple();
                        }
                    }
                    else
                    {
                        TryLaunchGrapple();
                    }
                }
            }

            if (isShootingRope)
            {
                UpdateShootingRope();
            }
            else if (isGrappling)
            {
                PullPlayer();
            }
        }

        private void TryLaunchGrapple()
        {
            if (_mainCamera == null) return;

            // Center of screen / viewport ray
            Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            // Raycast against any layer (~0), ignoring objects with "Player" tag or player's own colliders
            RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            bool foundValidHit = false;
            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Player") || hit.transform.root == transform.root)
                {
                    continue;
                }

                _hasValidTarget = true;
                StartShootingRope(hit.point);
                foundValidHit = true;
                break;
            }

            if (!foundValidHit)
            {
                _hasValidTarget = false;
                StartShootingRope(ray.GetPoint(maxDistance));
            }
        }

        private void StartShootingRope(Vector3 destination)
        {
            isShootingRope = true;
            isGrappling = false;
            _targetPoint = destination;
            _currentRopeTip = GetOriginPosition();

            // Destroy previous instance if any exists
            if (_hookHeadInstance != null)
            {
                Destroy(_hookHeadInstance);
            }

            // Instantiate hook head prefab if assigned
            if (hookHeadPrefab != null)
            {
                Vector3 launchDirection = (_targetPoint - _currentRopeTip);
                Quaternion rot = launchDirection != Vector3.zero ? Quaternion.LookRotation(launchDirection) : transform.rotation;
                _hookHeadInstance = Instantiate(hookHeadPrefab, _currentRopeTip, rot);
            }

            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = true;
                _lineRenderer.SetPosition(0, GetOriginPosition());
                _lineRenderer.SetPosition(1, _currentRopeTip);
            }
        }

        private void UpdateShootingRope()
        {
            // Player CAN move normally while the rope is extending towards target
            Vector3 origin = GetOriginPosition();

            // Move current rope tip toward target point
            _currentRopeTip = Vector3.MoveTowards(_currentRopeTip, _targetPoint, ropeShootSpeed * Time.deltaTime);

            // Update hook head object position & orientation
            if (_hookHeadInstance != null)
            {
                _hookHeadInstance.transform.position = _currentRopeTip;
                Vector3 launchDirection = (_targetPoint - origin);
                if (launchDirection != Vector3.zero)
                {
                    _hookHeadInstance.transform.rotation = Quaternion.LookRotation(launchDirection);
                }
            }

            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = true;
                _lineRenderer.SetPosition(0, origin);
                _lineRenderer.SetPosition(1, _currentRopeTip);
            }

            // Reached target point -> Transition to Grappling/Pulling if valid surface hit, otherwise cancel and destroy hook
            if (Vector3.Distance(_currentRopeTip, _targetPoint) <= 0.05f)
            {
                isShootingRope = false;
                if (_hasValidTarget)
                {
                    StartPullingPlayer();
                }
                else
                {
                    StopGrapple();
                }
            }
        }

        private void StartPullingPlayer()
        {
            isGrappling = true;

            // If super jumping, cancel super jump immediately and record usage
            if (_superJump != null && _superJump.isSuperJumping)
            {
                _superJump.hasUsedGrappleInAir = true;
                _superJump.CancelSuperJump();
            }

            if (_thirdPersonController != null)
            {
                _thirdPersonController.ResetVerticalVelocity();
                _thirdPersonController.isMovementLocked = true;
            }

            // Play InAir animation for grapple travel
            if (_animator != null)
            {
                _animator.speed = 1.0f;
                _animator.Play("InAir", 0, 0f);
            }

            // Rotate character body immediately towards target point
            Vector3 lookDirection = _targetPoint - transform.position;
            lookDirection.y = 0; // maintain horizontal focus
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }

            // Clear movement/jump inputs
            if (_inputs != null)
            {
                _inputs.move = Vector2.zero;
                _inputs.jump = false;
                _inputs.sprint = false;
                _inputs.superJump = false;
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
                _inputs.superJump = false;
            }

            Vector3 origin = GetOriginPosition();
            Vector3 direction = (_targetPoint - origin);
            float distance = direction.magnitude;

            // Ensure hook head stays at target surface location while pulling
            if (_hookHeadInstance != null)
            {
                _hookHeadInstance.transform.position = _targetPoint;
            }

            // Update LineRenderer as rope retracts while player moves towards target
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = true;
                _lineRenderer.SetPosition(0, origin);
                _lineRenderer.SetPosition(1, _targetPoint);
            }

            // Keep character facing target point while pulling
            Vector3 flatDirection = direction;
            flatDirection.y = 0;
            if (flatDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(flatDirection);
            }

            // Keep InAir animation playing
            if (_animator != null)
            {
                _animator.Play("InAir", 0);
            }

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
            isShootingRope = false;
            isGrappling = false;

            if (_hookHeadInstance != null)
            {
                Destroy(_hookHeadInstance);
                _hookHeadInstance = null;
            }

            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = false;
            }

            if (_thirdPersonController != null)
            {
                _thirdPersonController.ResetVerticalVelocity();
                _thirdPersonController.isMovementLocked = false;
            }
        }
    }
}
