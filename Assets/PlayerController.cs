using UnityEngine;

namespace FinalCharacterController
{
    [DefaultExecutionOrder(-1)]
    public class PlayerController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera playerCamera;

        [Header("Base Movement")]
        public float runAcceleration = 0.25f;
        public float runSpeed = 4f;
        public float drag = 0.1f;

        [Header("Camera Settings")]
        public float lookSenseH = 0.1f;
        public float lookSenseV = 0.1f;
        public float lookLimitV = 89f;

        private PlayerLocomotionInput playerLocomotionInput;
        private Vector2 cameraRotation = Vector2.zero;
        private Vector2 playerTargetRotation = Vector2.zero;

        private void Awake()
        {
            playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        }

        private void Update()
        {
            if (DialogueManager.GetInstance().dialogueIsPlaying)
            {
                return;
            }

            Vector3 cameraForwardXZ = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z).normalized;
            Vector3 cameraRightXZ = new Vector3(playerCamera.transform.right.x, 0f, playerCamera.transform.right.z).normalized;
            Vector3 movementDirection = cameraRightXZ * playerLocomotionInput.MovementInput.x + cameraForwardXZ * playerLocomotionInput.MovementInput.y;

            Vector3 movementDelta = movementDirection * runAcceleration * Time.deltaTime;
            Vector3 newVelocity = characterController.velocity + movementDelta;

            Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
            newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
            newVelocity = Vector3.ClampMagnitude(newVelocity, runSpeed);

            // Move character only use once
            characterController.Move(newVelocity * Time.deltaTime);
        }

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
