using UnityEngine;

namespace FinalCharacterController
{
    public class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float locomotionBlendSpeed = 4f;

        private PlayerLocomotionInput playerLocomotionInput;
        private PlayerState playerState;

        private static int inputXHash = Animator.StringToHash("inputX");
        private static int inputYHash = Animator.StringToHash("inputY");
        private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");

        private Vector3 currentBlendInput = Vector3.zero;

        private void Awake()
        {
            playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            playerState = GetComponent<PlayerState>();
        }

        private void Update()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            bool isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;

            Vector2 inputTarget = isSprinting ? playerLocomotionInput.MovementInput * 1.5f : playerLocomotionInput.MovementInput;
            currentBlendInput = Vector3.Lerp(currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

            animator.SetFloat(inputXHash, currentBlendInput.x);
            animator.SetFloat(inputYHash, currentBlendInput.y);
            animator.SetFloat(inputMagnitudeHash, currentBlendInput.magnitude);
        }
    }
}
