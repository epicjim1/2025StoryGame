using UnityEngine;

namespace FinalCharacterController
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float locomotionBlendSpeed = 4f;

        private PlayerLocomotionInput playerLocomotionInput;
        private PlayerState playerState;
        private PlayerController playerController;

        private static int inputXHash = Animator.StringToHash("inputX");
        private static int inputYHash = Animator.StringToHash("inputY");
        private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
        private static int isIdlingHash = Animator.StringToHash("isIdling");
        private static int isGroundedHash = Animator.StringToHash("isGrounded");
        private static int isFallingHash = Animator.StringToHash("isFalling");
        private static int isJumpingHash = Animator.StringToHash("isJumping");
        private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");
        private static int rotationMismatchHash = Animator.StringToHash("rotationMismatch");
        private static int drawSwordTriggerHash = Animator.StringToHash("drawWeapon");
        private static int sheathSwordTriggerHash = Animator.StringToHash("sheathWeapon");

        private Vector3 currentBlendInput = Vector3.zero;

        private float sprintMaxBlendValue = 1.5f;
        private float runMaxBlendValue = 1.0f;
        private float walkMaxBlendValue = 0.5f;

        private void Awake()
        {
            playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            playerState = GetComponent<PlayerState>();
            playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            bool isIdling = playerState.CurrentPlayerMovementState == PlayerMovementState.Idling;
            bool isRunning = playerState.CurrentPlayerMovementState == PlayerMovementState.Running;
            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;
            bool isJumping = playerState.CurrentPlayerMovementState == PlayerMovementState.Jumping;
            bool isFalling = playerState.CurrentPlayerMovementState == PlayerMovementState.Falling;
            bool isGrounded = playerState.InGroundedState();

            bool isRunBlendValue = isRunning || isJumping || isFalling;

            Vector2 inputTarget = isSprinting ? playerLocomotionInput.MovementInput * sprintMaxBlendValue :
                                  isRunBlendValue ? playerLocomotionInput.MovementInput * runMaxBlendValue :
                                                    playerLocomotionInput.MovementInput * walkMaxBlendValue;

            currentBlendInput = Vector3.Lerp(currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

            animator.SetBool(isGroundedHash, isGrounded);
            animator.SetBool(isIdlingHash, isIdling);
            animator.SetBool(isFallingHash, isFalling);
            animator.SetBool(isJumpingHash, isJumping);
            animator.SetBool(isRotatingToTargetHash, playerController.IsRotatingToTarget);

            animator.SetFloat(inputXHash, currentBlendInput.x);
            animator.SetFloat(inputYHash, currentBlendInput.y);
            animator.SetFloat(inputMagnitudeHash, currentBlendInput.magnitude);
            animator.SetFloat(rotationMismatchHash, playerController.RotationMismatch);

            // --- New Draw/Sheath Weapon Logic ---
            if (playerLocomotionInput.DrawWeaponPressed)
            {
                // Toggle the weapon state
                playerState.ToggleWeaponDrawn();

                if (playerState.IsWeaponDrawn)
                {
                    // If weapon is now drawn, trigger the drawSword animation
                    animator.SetTrigger(drawSwordTriggerHash);
                }
                else
                {
                    // If weapon is now sheathed, trigger the sheathSword animation
                    animator.SetTrigger(sheathSwordTriggerHash);
                }
            }
        }
    }
}
