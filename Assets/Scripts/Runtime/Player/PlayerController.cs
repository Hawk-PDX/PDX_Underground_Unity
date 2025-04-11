using UnityEngine;
using System.Collections;

namespace PDXUnderground.Player
{
    /// <summary>
    /// Handles player movement and controls for the PDX Underground game.
    /// Implements basic character movement, jumping, and integration with GamblerCharacter.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        #region Movement Settings
        [Header("Movement Settings")]
        [Tooltip("Base movement speed in units per second")]
        [SerializeField] private float baseMovementSpeed = 5.0f;
        
        [Tooltip("Current movement speed multiplier (modified by buzz level)")]
        [Range(0.1f, 2.0f)]
        public float movementSpeedMultiplier = 1.0f;
        
        [Tooltip("Rotation speed when turning")]
        [SerializeField] private float rotationSpeed = 10.0f;
        
        [Tooltip("Jump force")]
        [SerializeField] private float jumpForce = 7.0f;
        
        [Tooltip("Gravity multiplier")]
        [SerializeField] private float gravityMultiplier = 2.0f;
        
        [Tooltip("Maximum fall velocity")]
        [SerializeField] private float maxFallVelocity = 20.0f;
        #endregion
        
        #region Ground Check Settings
        [Header("Ground Check")]
        [Tooltip("Layer mask for ground detection")]
        [SerializeField] private LayerMask groundMask = ~0; // Default to all layers
        
        [Tooltip("Distance to check for ground")]
        [SerializeField] private float groundCheckDistance = 0.2f;
        
        [Tooltip("Ground check radius")]
        [SerializeField] private float groundCheckRadius = 0.3f;
        #endregion
        
        #region Character State
        // Movement state
        private Vector3 moveDirection = Vector3.zero;
        private Vector3 velocity = Vector3.zero;
        private bool isJumping = false;
        private bool isGrounded = true;
        
        // Input state
        private Vector2 movementInput;
        private bool jumpInput = false;
        
        // References
        private CharacterController characterController;
        private GamblerCharacter gamblerCharacter;
        private Camera playerCamera;
        
        // Public properties for state access
        public bool IsGrounded => isGrounded;
        public bool IsJumping => isJumping;
        public float CurrentSpeed => baseMovementSpeed * movementSpeedMultiplier;
        #endregion
        
        #region Unity Lifecycle
        private void Awake()
        {
            // Get required components
            characterController = GetComponent<CharacterController>();
            gamblerCharacter = GetComponent<GamblerCharacter>();
            
            // Find main camera
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogWarning("Main camera not found. Camera-relative movement will not work correctly.");
            }
        }
        
        private void Update()
        {
            // Process inputs
            ProcessInput();
            
            // Check ground status
            CheckGroundStatus();
            
            // Handle movement
            HandleMovement();
            
            // Handle jumping
            HandleJump();
            
            // Apply gravity
            ApplyGravity();
            
            // Apply final movement
            ApplyMovement();
        }
        
        private void OnDrawGizmosSelected()
        {
            // Visualize ground check
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, groundCheckRadius);
        }
        #endregion
        
        #region Input Processing
        /// <summary>
        /// Process player input for movement and actions
        /// </summary>
        private void ProcessInput()
        {
            // Get horizontal and vertical input for movement
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            // Store movement input
            movementInput = new Vector2(horizontal, vertical);
            
            // Check for jump input
            jumpInput = Input.GetButtonDown("Jump");
        }
        
        /// <summary>
        /// Calculate movement direction based on input and camera orientation
        /// </summary>
        private Vector3 CalculateMovementDirection()
        {
            // If no input, return zero vector
            if (movementInput.sqrMagnitude <= 0.01f)
                return Vector3.zero;
                
            // Calculate direction based on camera
            Vector3 direction = Vector3.zero;
            
            if (playerCamera != null)
            {
                // Create camera-relative movement direction
                Vector3 forward = playerCamera.transform.forward;
                Vector3 right = playerCamera.transform.right;
                
                // Remove vertical component
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();
                
                // Combine directions based on input
                direction = (forward * movementInput.y + right * movementInput.x).normalized;
            }
            else
            {
                // Fallback to world-relative movement if no camera
                direction = new Vector3(movementInput.x, 0, movementInput.y).normalized;
            }
            
            return direction;
        }
        #endregion
        
        #region Movement Handling
        /// <summary>
        /// Handle character movement based on input
        /// </summary>
        private void HandleMovement()
        {
            // Calculate movement direction
            Vector3 direction = CalculateMovementDirection();
            
            // If we have movement input, set move direction and rotate character
            if (direction.sqrMagnitude > 0.01f)
            {
                // Set move direction
                moveDirection = direction;
                
                // Rotate character to face movement direction
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                // No input, slow down movement
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, 10 * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Handle jumping logic
        /// </summary>
        private void HandleJump()
        {
            // Check if we can jump
            if (isGrounded && jumpInput)
            {
                // Apply jump velocity
                velocity.y = jumpForce;
                isJumping = true;
                
                // Debug output
                Debug.Log("Player jumped");
            }
        }
        
        /// <summary>
        /// Apply gravity to vertical movement
        /// </summary>
        private void ApplyGravity()
        {
            // Apply gravity when not grounded or when falling
            if (!isGrounded || velocity.y < 0)
            {
                // Apply gravity with multiplier
                velocity.y -= Physics.gravity.y * gravityMultiplier * Time.deltaTime;
                
                // Clamp fall speed
                velocity.y = Mathf.Max(velocity.y, -maxFallVelocity);
            }
            else if (isGrounded && velocity.y < 0)
            {
                // Small negative velocity when grounded to keep character grounded
                velocity.y = -2f;
                
                // Reset jump state if grounded
                if (isJumping)
                {
                    isJumping = false;
                }
            }
        }
        
        /// <summary>
        /// Check if the character is grounded
        /// </summary>
        private void CheckGroundStatus()
        {
            // Perform sphere cast to check ground
            isGrounded = Physics.SphereCast(
                transform.position + Vector3.up * 0.1f, // Start slightly above to avoid initial collisions
                groundCheckRadius,
                Vector3.down,
                out RaycastHit hit,
                groundCheckDistance + 0.1f,
                groundMask
            );
        }
        
        /// <summary>
        /// Apply final movement to character controller
        /// </summary>
        private void ApplyMovement()
        {
            // Combine horizontal movement and vertical velocity
            Vector3 movement = moveDirection * (baseMovementSpeed * movementSpeedMultiplier) * Time.deltaTime;
            movement.y = velocity.y * Time.deltaTime;
            
            // Move character
            characterController.Move(movement);
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Force the character to move in a specific direction
        /// </summary>
        /// <param name="direction">Direction to move</param>
        /// <param name="force">Force multiplier</param>
        public void ApplyForce(Vector3 direction, float force)
        {
            // Add force to velocity
            velocity += direction.normalized * force;
            
            // Debug output
            Debug.Log($"Applied force: {direction * force}");
        }
        
        /// <summary>
        /// Teleport the character to a position
        /// </summary>
        /// <param name="position">Target position</param>
        public void TeleportTo(Vector3 position)
        {
            // Disable character controller to avoid physics issues
            characterController.enabled = false;
            
            // Set position
            transform.position = position;
            
            // Re-enable character controller
            characterController.enabled = true;
            
            // Reset velocity
            velocity = Vector3.zero;
            
            // Debug output
            Debug.Log($"Teleported to: {position}");
        }
        
        /// <summary>
        /// Set movement speed multiplier directly
        /// </summary>
        /// <param name="multiplier">Speed multiplier</param>
        public void SetMovementSpeedMultiplier(float multiplier)
        {
            movementSpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 2f);
        }
        #endregion
    }
}

