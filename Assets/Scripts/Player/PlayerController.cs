using UnityEngine;

namespace FinalCharacterController
{
    [DefaultExecutionOrder(-1)]
    public class PlayerController : MonoBehaviour
    {
        #region Class Variables
        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera playerCamera;
        public float RotationMismatch { get; private set; } = 0f;
        public bool IsRotatingToTarget { get; private set; } = false;

        [Header("Base Movement")]
        public float walkAcceleration = 25f;
        public float walkSpeed = 2f;
        public float runAcceleration = 0.25f;
        public float runSpeed = 6f;
        public float sprintAccelerataion = 50f;
        public float sprintSpeed = 9f;
        public float inAirAcceleration = 25f;
        public float drag = 0.1f;
        public float gravity = 25f;
        public float jumpSpeed = 1.0f;
        public float movingThreshold = 0.01f;
        private Vector3 currentVelocity = Vector3.zero;

        [Header("Animation")]
        public float playerModelRotationSpeed = 10f;
        public float rotateToTargetTime = 0.67f;

        [Header("Camera Settings")]
        public float lookSenseH = 0.1f;
        public float lookSenseV = 0.1f;
        public float lookLimitV = 89f;

        [Header("Environment Details")]
        [SerializeField] private LayerMask groundLayers;

        private PlayerLocomotionInput playerLocomotionInput;
        private PlayerState playerState;

        private Vector2 cameraRotation = Vector2.zero;
        private Vector2 playerTargetRotation = Vector2.zero;

        private bool jumpedLastFrame = false;
        private bool isRotatingClockwise = false;
        private float rotatingToTargetTimer = 0f;
        private float verticalVelocity = 0f;
        private float antiBump;
        private float stepOffset;

        private PlayerMovementState lastMovementState = PlayerMovementState.Falling;
        #endregion

        #region Startup
        private void Awake()
        {
            playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            playerState = GetComponent<PlayerState>();

            antiBump = sprintSpeed;
            stepOffset = characterController.stepOffset;
        }
        #endregion

        #region Update Logic
        private void Update()
        {
            if (DialogueManager.GetInstance().dialogueIsPlaying)
            {
                return;
            }

            UpdateMovementState();
            HandleVerticalMovement();
            HandleLateralMovement();
        }

        private void UpdateMovementState()
        {
            lastMovementState = playerState.CurrentPlayerMovementState;

            bool canRun = CanRun();
            bool isMovementInput = playerLocomotionInput.MovementInput != Vector2.zero;    //order
            bool isMovingLaterally = IsMovingLaterally();                                   //matter
            bool isSprinting = playerLocomotionInput.SprintToggledOn && isMovingLaterally; //order matters
            bool isWalking = isMovingLaterally && (!canRun || playerLocomotionInput.WalkToggledOn); //matters
            bool isGrounded = IsGrounded();

            PlayerMovementState lateralState = isWalking ? PlayerMovementState.Walking :
                                               isSprinting ? PlayerMovementState.Sprinting :
                                               isMovingLaterally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;

            playerState.SetPlayerMovementState(lateralState);

            // Control Airborn State
            if ((!isGrounded || jumpedLastFrame) && currentVelocity.y > 0f)
            {
                playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
                jumpedLastFrame = false;
                characterController.stepOffset = 0f;
            }
            else if ((!isGrounded || jumpedLastFrame) && currentVelocity.y <= 0f)
            {
                playerState.SetPlayerMovementState(PlayerMovementState.Falling);
                jumpedLastFrame = false;
                characterController.stepOffset = 0f;
            }
            else
            {
                characterController.stepOffset = stepOffset;
            }
        }

        private void HandleVerticalMovement()
        {
            bool isGrounded = playerState.InGroundedState();

            verticalVelocity -= gravity * Time.deltaTime;

            if (isGrounded && verticalVelocity < 0)
                verticalVelocity = -antiBump;

            if (playerLocomotionInput.JumpPressed && isGrounded)
            {
                verticalVelocity += Mathf.Sqrt(jumpSpeed * 3 * gravity);
                jumpedLastFrame = true;
            }

            if (playerState.IsStateGroundedState(lastMovementState) && !isGrounded)
            {
                verticalVelocity += antiBump;
            }
        }

        private void HandleLateralMovement()
        {
            // Create quick references for current state
            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isGrounded = playerState.InGroundedState();
            bool isWalking = playerState.CurrentPlayerMovementState == PlayerMovementState.Walking;

            float lateralAcceleration = !isGrounded ? inAirAcceleration :
                                        isWalking ? walkAcceleration :
                                        isSprinting ? sprintAccelerataion : runAcceleration;
            float clampLateralMagnitude = !isGrounded ? sprintSpeed :
                                          isWalking ? walkSpeed :
                                          isSprinting ? sprintSpeed : runSpeed;

            // State dependent acceleration and speed
            Vector3 cameraForwardXZ = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(playerCamera.transform.right.x, 0f, playerCamera.transform.right.z).normalized;
            Vector3 movementDirection = cameraRightXZ * playerLocomotionInput.MovementInput.x + cameraForwardXZ * playerLocomotionInput.MovementInput.y;

            Vector3 movementDelta = movementDirection * lateralAcceleration;
            currentVelocity += movementDelta;//= characterController.velocity + movementDelta;

            // Add drag to player
            Vector3 currentDrag = currentVelocity.normalized * drag;
            currentVelocity = (currentVelocity.magnitude > drag) ? currentVelocity - currentDrag : Vector3.zero;
            currentVelocity = Vector3.ClampMagnitude(new Vector3(currentVelocity.x, 0f, currentVelocity.z), clampLateralMagnitude);
            currentVelocity.y += verticalVelocity;
            currentVelocity = !isGrounded ? HandleSteepWalls(currentVelocity) : currentVelocity;

            // Move character only use once
            characterController.Move(currentVelocity * Time.deltaTime);
        }

        private Vector3 HandleSteepWalls(Vector3 velocity)
        {
            Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(characterController, groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            bool validAngle = angle <= characterController.slopeLimit;

            if (!validAngle && verticalVelocity < 0f)
                velocity = Vector3.ProjectOnPlane(velocity, normal);

            return velocity;
        }
        #endregion

        #region Late Update Logic
        private void LateUpdate()
        {
            if (DialogueManager.GetInstance().dialogueIsPlaying)
            {
                return;
            }

            UpdateCameraRotation();
        }

        private void UpdateCameraRotation()
        {
            cameraRotation.x += lookSenseH * playerLocomotionInput.LookInput.x;
            cameraRotation.y = Mathf.Clamp(cameraRotation.y - lookSenseV * playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);

            playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * playerLocomotionInput.LookInput.x;

            float rotationTolerance = 90f;
            bool isIdling = playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            IsRotatingToTarget = rotatingToTargetTimer > 0;

            // ROTATE if we're not idling
            if (!isIdling)
            {
                RotatePlayerToTarget();
            }
            // If rotation mismath not within tolerance, or rotate to target is active, ROTATE
            else if (Mathf.Abs(RotationMismatch) > rotationTolerance || IsRotatingToTarget)
            {
                UpdateIdleRotation(rotationTolerance);
            }

            playerCamera.transform.rotation = Quaternion.Euler(cameraRotation.y, cameraRotation.x, 0f);

            // Get angle between camera and player
            Vector3 camForwardProjectedXZ = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;
            Vector3 crossProduct = Vector3.Cross(transform.forward, camForwardProjectedXZ);
            float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
            RotationMismatch = sign * Vector3.Angle(transform.forward, camForwardProjectedXZ);
        }

        private void UpdateIdleRotation(float rotationTolerance)
        {
            // Initiate new rotation direction
            if (Mathf.Abs(RotationMismatch) > rotationTolerance)
            {
                rotatingToTargetTimer = rotateToTargetTime;
                isRotatingClockwise = RotationMismatch > rotationTolerance;
            }
            rotatingToTargetTimer -= Time.deltaTime;

            // Rotate player
            if (isRotatingClockwise && RotationMismatch > 0f ||
                !isRotatingClockwise && RotationMismatch < 0f)
            {
                RotatePlayerToTarget();
            }
        }

        private void RotatePlayerToTarget()
        {
            Quaternion targetRotationX = Quaternion.Euler(0f, playerTargetRotation.x, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, playerModelRotationSpeed * Time.deltaTime);
        }
        #endregion

        #region State Checks
        private bool IsMovingLaterally()
        {
            Vector3 lateralVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            
            return lateralVelocity.magnitude > movingThreshold;
        }

        private bool IsGrounded()
        {
            /*if (characterController.isGrounded) return true;

            RaycastHit hit;
            float extraHeight = 0.2f; // Extend slightly beyond the capsule bottom
            Vector3 origin = transform.position + Vector3.up * 0.1f; // Offset to avoid self-intersection

            //Debug.DrawRay(origin, Vector3.down * (characterController.height / 2 + extraHeight), Color.green, 1f);
            if (Physics.Raycast(origin, Vector3.down, out hit, characterController.height / 2 + extraHeight))
            {
                return true;
            }

            return false;*/
            bool grounded = playerState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();

            return grounded;
        }

        private bool IsGroundedWhileGrounded()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - characterController.radius, transform.position.z);

            bool grounded = Physics.CheckSphere(spherePosition, characterController.radius, groundLayers, QueryTriggerInteraction.Ignore);

            return grounded;
        }

        private bool IsGroundedWhileAirborne()
        {
            Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(characterController, groundLayers);
            float angle = Vector3.Angle(normal, Vector3.up);
            //print(angle);
            bool validAngle = angle <= characterController.slopeLimit;

            return characterController.isGrounded && validAngle;
        }

        private bool CanRun()
        {
            // This means player is moving diagonally at 45 degrees or forward, if so, we can run
            return playerLocomotionInput.MovementInput.y >= Mathf.Abs(playerLocomotionInput.MovementInput.x);
        }
        #endregion

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("NPC") && DialogueManager.GetInstance().dialogueIsPlaying)
            {
                Vector3 direction = other.transform.position - transform.position;
                direction.y = 0; // Keep rotation only on the Y-axis
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Smooth rotation
            }
        }
    }
}
