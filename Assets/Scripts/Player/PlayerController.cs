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

        [Header("Base Movement")]
        public float runAcceleration = 0.25f;
        public float runSpeed = 4f;
        public float sprintAccelerataion = 50f;
        public float sprintSpeed = 7f;
        public float drag = 0.1f;
        public float gravity = 25f;
        public float jumpSpeed = 1.0f;
        public float movingThreshold = 0.01f;
        private Vector3 currentVelocity = Vector3.zero;

        [Header("Camera Settings")]
        public float lookSenseH = 0.1f;
        public float lookSenseV = 0.1f;
        public float lookLimitV = 89f;

        private PlayerLocomotionInput playerLocomotionInput;
        private PlayerState playerState;

        private Vector2 cameraRotation = Vector2.zero;
        private Vector2 playerTargetRotation = Vector2.zero;

        private float verticalVelocity = 0f;
        #endregion

        #region Startup
        private void Awake()
        {
            playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            playerState = GetComponent<PlayerState>();
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
            bool isMovementInput = playerLocomotionInput.MovementInput != Vector2.zero;    //order
            bool isMovingLaterally = IsMovingLaterally();                                   //matter
            bool isSprinting = playerLocomotionInput.SprintToggledOn && isMovingLaterally; //order matters
            bool isGrounded = IsGrounded();

            PlayerMovementState lateralState = isSprinting ? PlayerMovementState.Sprinting :
                                               isMovingLaterally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;

            playerState.SetPlayerMovementState(lateralState);

            // Control Airborn State
            if (!isGrounded && characterController.velocity.y > 0f)
            {
                playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
            }
            else if (!isGrounded && characterController.velocity.y <= 0f)
            {
                playerState.SetPlayerMovementState(PlayerMovementState.Falling);
            }
        }

        private void HandleVerticalMovement()
        {
            bool isGrounded = playerState.InGroundedState();

            if (isGrounded && verticalVelocity < 0)
                verticalVelocity = 0f;

            verticalVelocity -= gravity * Time.deltaTime;

            if (playerLocomotionInput.JumpPressed && isGrounded)
            {
                verticalVelocity += Mathf.Sqrt(jumpSpeed * 3 * gravity);
            }
        }

        private void HandleLateralMovement()
        {
            // Create quick references for current state
            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isGrounded = playerState.InGroundedState();

            float lateralAcceleration = isSprinting ? sprintAccelerataion : runAcceleration;
            float clampLateralMagnitude = isSprinting ? sprintSpeed : runSpeed;

            // State dependent acceleration and speed
            Vector3 cameraForwardXZ = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(playerCamera.transform.right.x, 0f, playerCamera.transform.right.z).normalized;
            Vector3 movementDirection = cameraRightXZ * playerLocomotionInput.MovementInput.x + cameraForwardXZ * playerLocomotionInput.MovementInput.y;

            Vector3 movementDelta = movementDirection * lateralAcceleration;
            currentVelocity += movementDelta;//= characterController.velocity + movementDelta;

            // Add drag to player
            Vector3 currentDrag = currentVelocity.normalized * drag;
            currentVelocity = (currentVelocity.magnitude > drag) ? currentVelocity - currentDrag : Vector3.zero;
            currentVelocity = Vector3.ClampMagnitude(currentVelocity, clampLateralMagnitude);
            currentVelocity.y += verticalVelocity;

            // Move character only use once
            characterController.Move(currentVelocity * Time.deltaTime);
        }
        #endregion

        #region Late Update Logic
        private void LateUpdate()
        {
            if (DialogueManager.GetInstance().dialogueIsPlaying)
            {
                return;
            }

            cameraRotation.x += lookSenseH * playerLocomotionInput.LookInput.x;
            cameraRotation.y = Mathf.Clamp(cameraRotation.y - lookSenseV * playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);

            playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * playerLocomotionInput.LookInput.x;
            transform.rotation = Quaternion.Euler(0f, playerTargetRotation.x, 0f);

            playerCamera.transform.rotation = Quaternion.Euler(cameraRotation.y, cameraRotation.x, 0f);
        }
        #endregion

        #region State Checks
        private bool IsMovingLaterally()
        {
            Vector3 lateralVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            
            return lateralVelocity.magnitude > movingThreshold;
        }
        #endregion

        private bool IsGrounded()
        {
            return characterController.isGrounded;
        }

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
