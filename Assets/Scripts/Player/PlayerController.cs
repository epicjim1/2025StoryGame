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
        public float movingThreshold = 0.01f;

        [Header("Camera Settings")]
        public float lookSenseH = 0.1f;
        public float lookSenseV = 0.1f;
        public float lookLimitV = 89f;

        private PlayerLocomotionInput playerLocomotionInput;
        private PlayerState playerState;

        private Vector2 cameraRotation = Vector2.zero;
        private Vector2 playerTargetRotation = Vector2.zero;
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
            HandleLateralMovement();
        }

        private void UpdateMovementState()
        {
            bool isMovementInput = playerLocomotionInput.MovementInput != Vector2.zero;    //order
            bool isMovingLaterally = IsMovingLaterally();                                   //matter
            bool isSprinting = playerLocomotionInput.SprintToggledOn && isMovingLaterally; //order matters
            //Debug.Log(isMovingLaterally);

            PlayerMovementState lateralState = isSprinting ? PlayerMovementState.Sprinting :
                                               isMovingLaterally || isMovementInput ? PlayerMovementState.Running : PlayerMovementState.Idling;

            playerState.SetPlayerMovementState(lateralState);
        }
        private Vector3 newVelocity = Vector3.zero;
        private void HandleLateralMovement()
        {
            // Create quick references for current state
            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;

            float lateralAcceleration = isSprinting ? sprintAccelerataion : runAcceleration;
            float clampLateralMagnitude = isSprinting ? sprintSpeed : runSpeed;

            // State dependent acceleration and speed
            Vector3 cameraForwardXZ = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(playerCamera.transform.right.x, 0f, playerCamera.transform.right.z).normalized;
            Vector3 movementDirection = cameraRightXZ * playerLocomotionInput.MovementInput.x + cameraForwardXZ * playerLocomotionInput.MovementInput.y;

            Vector3 movementDelta = movementDirection * lateralAcceleration;
             newVelocity += movementDelta;//= characterController.velocity + movementDelta;

            // Add drag to player
            Vector3 currentDrag = newVelocity.normalized * drag;
            newVelocity = (newVelocity.magnitude > drag) ? newVelocity - currentDrag : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(newVelocity, clampLateralMagnitude);

            // Move character only use once
            characterController.Move(newVelocity * Time.deltaTime);
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
            Vector3 lateralVelocity = new Vector3(newVelocity.x, 0f, newVelocity.z);
            //Debug.Log(lateralVelocity);
            return lateralVelocity.magnitude > movingThreshold;
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
