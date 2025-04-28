using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using PDXUnderground.Core.Interfaces;

namespace PDXUnderground.AI.Enemies
{
    /// <summary>
    /// Controls the behavior of rat enemies in the Shanghai Tunnels.
    /// Handles movement, detection, combat, and environmental interactions.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class RatBehavior : MonoBehaviour, IDamageable
    {
        #region Inspector Properties
        
        [Header("Basic Properties")]
        [Tooltip("Maximum health of the rat")]
        [SerializeField] private float maxHealth = 10f;
        
        [Tooltip("Speed while patrolling")]
        [SerializeField] private float patrolSpeed = 1.5f;
        
        [Tooltip("Speed while chasing player")]
        [SerializeField] private float chaseSpeed = 3.5f;
        
        [Tooltip("Speed while retreating")]
        [SerializeField] private float retreatSpeed = 4f;
        
        [Header("Patrol Settings")]
        [Tooltip("Whether the rat should patrol between waypoints")]
        [SerializeField] private bool usePatrol = true;
        
        [Tooltip("Waypoints for patrol path")]
        [SerializeField] private Transform[] patrolWaypoints;
        
        [Tooltip("Wait time at each waypoint")]
        [SerializeField] private float waypointWaitTime = 2f;
        
        [Tooltip("Maximum distance to randomly deviate from waypoint")]
        [SerializeField] private float waypointDeviation = 1.5f;
        
        [Header("Detection Settings")]
        [Tooltip("Distance at which rat can detect player")]
        [SerializeField] private float detectionRange = 8f;
        
        [Tooltip("Field of view angle for detection")]
        [SerializeField] private float fieldOfView = 120f;
        
        [Tooltip("Distance at which rat will start attacking")]
        [SerializeField] private float attackRange = 1.5f;
        
        [Tooltip("Whether lighting affects detection range")]
        [SerializeField] private bool lightingAffectsDetection = true;
        
        [Header("Attack Properties")]
        [Tooltip("Damage dealt per attack")]
        [SerializeField] private float attackDamage = 5f;
        
        [Tooltip("Cooldown between attacks")]
        [SerializeField] private float attackCooldown = 1.5f;
        
        [Tooltip("Whether rat should swarm with other rats")]
        [SerializeField] private bool useSwarmBehavior = true;
        
        [Header("Environmental Interaction")]
        [Tooltip("Whether rat should respond to environmental changes")]
        [SerializeField] private bool reactToEnvironment = true;
        
        [Tooltip("Whether rat is afraid of light")]
        [SerializeField] private bool afraidOfLight = true;
        
        [Tooltip("Light intensity that causes rat to retreat")]
        [SerializeField] private float retreatLightIntensity = 0.7f;
        
        [Header("Audio")]
        [Tooltip("Sounds when idle")]
        [SerializeField] private AudioClip[] idleSounds;
        
        [Tooltip("Sounds when spotting player")]
        [SerializeField] private AudioClip[] alertSounds;
        
        [Tooltip("Sounds when attacking")]
        [SerializeField] private AudioClip[] attackSounds;
        
        [Tooltip("Sounds when hurt")]
        [SerializeField] private AudioClip[] hurtSounds;
        
        [Tooltip("Sounds when dying")]
        [SerializeField] private AudioClip[] deathSounds;
        
        [Tooltip("Interval between idle sounds")]
        [SerializeField] private float idleSoundInterval = 7f;
        
        [Header("VFX")]
        [SerializeField] private ParticleSystem hurtParticles;
        [SerializeField] private ParticleSystem deathParticles;
        
        #endregion
        
        #region Enums and Nested Types
        
        /// <summary>
        /// Possible states for the rat AI
        /// </summary>
        public enum RatState
        {
            Idle,
            Patrol,
            Alert,
            Chase,
            Attack,
            Retreat,
            Stunned,
            Dead
        }
        
        #endregion
        
        #region Private Variables
        
        private RatState currentState = RatState.Idle;
        private NavMeshAgent navAgent;
        private Animator animator;
        private AudioSource audioSource;
        private Transform playerTransform;
        private float currentHealth;
        private int currentWaypointIndex = 0;
        private bool isWaiting = false;
        private Vector3 lastKnownPlayerPosition;
        private float lastAttackTime = 0f;
        private float nextIdleSoundTime = 0f;
        private Collider ratCollider;
        private Rigidbody ratRigidbody;
        private Light nearestLight;
        private bool isAlerted = false;
        private Coroutine currentBehaviorCoroutine;
        private float timeSinceLastStateChange = 0f;
        private bool isDead = false;
        
        // Constants
        private const float MIN_PATROL_DISTANCE = 1.0f;
        private const float MIN_LIGHT_CHECK_TIME = 0.5f;
        
        #endregion
        
        #region Unity Lifecycle Methods
        
        private void Awake()
        {
            // Get components
            navAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            ratCollider = GetComponent<Collider>();
            ratRigidbody = GetComponent<Rigidbody>();
            
            // If no audio source, add one
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f;
                audioSource.minDistance = 1.0f;
                audioSource.maxDistance = 15.0f;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
            }
            
            // Set initial values
            currentHealth = maxHealth;
            navAgent.speed = patrolSpeed;
            
            // Find player
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTransform = playerObject.transform;
            }
        }
        
        private void Start()
        {
            // Start the appropriate behavior
            StartCoroutine(IdleBehavior());
            
            // Start environmental awareness
            if (reactToEnvironment)
            {
                StartCoroutine(EnvironmentalAwarenessRoutine());
            }
            
            // Schedule first idle sound
            ScheduleNextIdleSound();
        }
        
        private void Update()
        {
            if (isDead) return;
            
            // Update time since last state change
            timeSinceLastStateChange += Time.deltaTime;
            
            // Play idle sounds at intervals when not alerted
            if (currentState == RatState.Idle || currentState == RatState.Patrol)
            {
                if (Time.time > nextIdleSoundTime)
                {
                    PlayRandomSound(idleSounds, 0.4f);
                    ScheduleNextIdleSound();
                }
            }
            
            // Check for player detection
            if (currentState != RatState.Stunned && currentState != RatState.Dead && currentState != RatState.Retreat)
            {
                CheckForPlayerDetection();
            }
            
            // Update animator
            UpdateAnimator();
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Draw field of view
            Gizmos.color = Color.blue;
            float halfFOV = fieldOfView / 2.0f;
            Vector3 rightDir = Quaternion.Euler(0, halfFOV, 0) * transform.forward;
            Vector3 leftDir = Quaternion.Euler(0, -halfFOV, 0) * transform.forward;
            Gizmos.DrawRay(transform.position, rightDir * detectionRange);
            Gizmos.DrawRay(transform.position, leftDir * detectionRange);
            
            // Draw the patrol path if available
            if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < patrolWaypoints.Length; i++)
                {
                    if (patrolWaypoints[i] != null)
                    {
                        Vector3 waypoint = patrolWaypoints[i].position;
                        Gizmos.DrawSphere(waypoint, 0.3f);
                        
                        // Draw lines between waypoints
                        if (i < patrolWaypoints.Length - 1 && patrolWaypoints[i+1] != null)
                        {
                            Gizmos.DrawLine(waypoint, patrolWaypoints[i+1].position);
                        }
                        else if (patrolWaypoints[0] != null)
                        {
                            // Connect last to first
                            Gizmos.DrawLine(waypoint, patrolWaypoints[0].position);
                        }
                    }
                }
            }
        }
        
        #endregion
        
        #region State Machine Methods
        
        /// <summary>
        /// Changes the current state and starts the appropriate behavior
        /// </summary>
        private void ChangeState(RatState newState)
        {
            if (currentState == newState) return;
            
            // End previous state behavior
            if (currentBehaviorCoroutine != null)
            {
                StopCoroutine(currentBehaviorCoroutine);
                currentBehaviorCoroutine = null;
            }
            
            // Store previous state for transitions
            RatState previousState = currentState;
            currentState = newState;
            timeSinceLastStateChange = 0f;
            
            // Start new state behavior
            switch (newState)
            {
                case RatState.Idle:
                    currentBehaviorCoroutine = StartCoroutine(IdleBehavior());
                    break;
                case RatState.Patrol:
                    currentBehaviorCoroutine = StartCoroutine(PatrolBehavior());
                    break;
                case RatState.Alert:
                    currentBehaviorCoroutine = StartCoroutine(AlertBehavior());
                    break;
                case RatState.Chase:
                    currentBehaviorCoroutine = StartCoroutine(ChaseBehavior());
                    break;
                case RatState.Attack:
                    currentBehaviorCoroutine = StartCoroutine(AttackBehavior());
                    break;
                case RatState.Retreat:
                    currentBehaviorCoroutine = StartCoroutine(RetreatBehavior());
                    break;
                case RatState.Stunned:
                    currentBehaviorCoroutine = StartCoroutine(StunnedBehavior());
                    break;
                case RatState.Dead:
                    currentBehaviorCoroutine = StartCoroutine(DeadBehavior());
                    break;
            }
            
            Debug.Log($"Rat state changed from {previousState} to {currentState}");
        }
        
        #endregion
        
        #region Behavior Coroutines
        
        /// <summary>
        /// Idle behavior: Rat stays in place
        /// </summary>
        private IEnumerator IdleBehavior()
        {
            navAgent.isStopped = true;
            
            float idleTime = Random.Range(2f, 5f);
            yield return new WaitForSeconds(idleTime);
            
            // After idling, patrol if able, otherwise remain idle
            if (usePatrol && patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                ChangeState(RatState.Patrol);
            }
        }
        
        /// <summary>
        /// Patrol behavior: Rat moves between waypoints
        /// </summary>
        private IEnumerator PatrolBehavior()
        {
            if (patrolWaypoints == null || patrolWaypoints.Length == 0)
            {
                ChangeState(RatState.Idle);
                yield break;
            }
            
            navAgent.isStopped = false;
            navAgent.speed = patrolSpeed;
            
            while (true)
            {
                if (isWaiting)
                {
                    yield return new WaitForSeconds(waypointWaitTime);
                    isWaiting = false;
                }
                
                // Check if we're at the current waypoint
                if (patrolWaypoints[currentWaypointIndex] != null)
                {
                    // Get base waypoint position
                    Vector3 waypoint = patrolWaypoints[currentWaypointIndex].position;
                    
                    // Add some random deviation if desired
                    if (waypointDeviation > 0)
                    {
                        Vector2 randomOffset = Random.insideUnitCircle * waypointDeviation;
                        waypoint += new Vector3(randomOffset.x, 0, randomOffset.y);
                    }
                    
                    // Set destination
                    navAgent.SetDestination(waypoint);
                    
                    // Wait until we reach the waypoint or get close enough
                    while (navAgent.pathPending || navAgent.remainingDistance > MIN_PATROL_DISTANCE)
                    {
                        // Check if path is invalid
                        if (navAgent.pathStatus == NavMeshPathStatus.PathInvalid)
                        {
                            // Try next waypoint
                            IncrementWaypointIndex();
                            break;
                        }
                        yield return null;
                    }
                    
                    // At waypoint, wait then move to next
                    isWaiting = true;
                    IncrementWaypointIndex();
                }
                else
                {
                    IncrementWaypointIndex();
                }
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Alert behavior: Rat notices something and becomes alert
        /// </summary>
        private IEnumerator AlertBehavior()
        {
            navAgent.isStopped = true;
            isAlerted = true;
            
            // Play alert sound
            PlayRandomSound(alertSounds, 0.7f);
            
            // Look towards the player
            if (playerTransform != null)
            {
                Vector3 lookDir = playerTransform.position - transform.position;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDir);
                }
            }
            
            // Wait for alert duration
            yield return new WaitForSeconds(1.0f);
            
            // After alert, transition to chase
            if (playerTransform != null)
            {
                lastKnownPlayerPosition = playerTransform.position;
                ChangeState(RatState.Chase);
            }
            else
            {
                // If somehow we lost the player, go back to patrol
                isAlerted = false;
                ChangeState(RatState.Patrol);
            }
        }
        
        /// <summary>
        /// Chase behavior: Rat pursues the player
        /// </summary>
        private IEnumerator ChaseBehavior()
        {
            navAgent.isStopped = false;
            navAgent.speed = chaseSpeed;
            
            while (true)
            {
                if (playerTransform == null)
                {
                    // Lost the player, go back to patrol
                    ChangeState(RatState.Patrol);
                    yield break;
                }
                
                // Update last known position
                lastKnownPlayerPosition = playerTransform.position;
                
                // Set destination to player
                navAgent.SetDestination(lastKnownPlayerPosition);
                
                // Check if we're in attack range
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer <= attackRange)
                {
                    ChangeState(RatState.Attack);
                    yield break;
                }
                
                // Check if we should give up chase (e.g., player too far or entered a safe zone)
                if (distanceToPlayer > detectionRange * 1.5f)
                {
                    // Lost the player, go back to patrol
                    isAlerted = false;
                    ChangeState(RatState.Patrol);
                    yield break;
                }
                
                // If in a bright area and afraid of light, retreat
                if (afraidOfLight && IsInBrightLight())
                {
                    ChangeState(RatState.Retreat);
                    yield break;
                }
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Attack behavior: Rat attacks the player
        /// </summary>
        private IEnumerator AttackBehavior()
        {
            navAgent.isStopped = true;
            
            while (true)
            {
                if (playerTransform == null)
                {
                    // Lost the player, go back to patrol
                    ChangeState(RatState.Patrol);
                    yield break;
                }
                
                // Look at player
                Vector3 lookDir = playerTransform.position - transform.position;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDir);
                }
                
                // Check if player is still in attack range
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer > attackRange)
                {
                    // Player moved away, chase again
                    ChangeState(RatState.Chase);
                    yield break;
                }
                
                // Attack if cooldown has passed
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    PerformAttack();
                    lastAttackTime = Time.time;
                    
                    // Wait for attack animation
                    yield return new WaitForSeconds(0.5f);
                }
                
                // If in a bright area and afraid of light, retreat
                if (afraidOfLight && IsInBrightLight())
                {
                    ChangeState(RatState.Retreat);
                    yield break;
                }
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Retreat behavior: Rat flees from player or light
        /// </summary>
        private IEnumerator RetreatBehavior()
        {
            navAgent.isStopped = false;
            navAgent.speed = retreatSpeed;
            
            // Find a retreat point away from player or light
            Vector3 retreatDirection = playerTransform != null 
                ? (transform.position - playerTransform.position).normalized 
                : (nearestLight != null ? (transform.position - nearestLight.transform.position).normalized : transform.forward);
            
            Vector3 retreatPoint = transform.position + retreatDirection * 10f;
            
            // Try to find a valid point on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(retreatPoint, out hit, 10f, NavMesh.AllAreas))
            {
                retreatPoint = hit.position;
            }
            
            navAgent.SetDestination(retreatPoint);
            
            // Wait until we reach the retreat point or get close enough
            float retreatTime = 0f;
            float maxRetreatTime = 5f;
            
            while (retreatTime < maxRetreatTime)
            {
                retreatTime += Time.deltaTime;
                
                // Check if we reached the destination
                if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
                {
                    break;
                }
                
                yield return null;
            }
            
            // After retreating, go back to patrol
            isAlerted = false;
            ChangeState(RatState.Patrol);
        }
        
        /// <summary>
        /// Stunned behavior: Rat is temporarily stunned
        /// </summary>
        private IEnumerator StunnedBehavior()
        {
            navAgent.isStopped = true;
            
            // Play hurt sound
            PlayRandomSound(hurtSounds, 0.6f);
            
            // Wait for stun duration
            yield return new WaitForSeconds(1.5f);
            
            // After stun, either retreat or resume chase based on health
            if (currentHealth < maxHealth * 0.3f)
            {
                ChangeState(RatState.Retreat);
            }
            else if (playerTransform != null)
            {
                ChangeState(RatState.Chase);
            }
            else
            {
                ChangeState(RatState.Patrol);
            }
        }
        
        /// <summary>
        /// Dead behavior: Rat is dead
        /// </summary>
        private IEnumerator DeadBehavior()
        {
            // Stop all movement
            navAgent.isStopped = true;
            
            // Play death sound
            PlayRandomSound(deathSounds, 1.0f);
            
            // Play death particles
            if (deathParticles != null)
            {
                deathParticles.Play();
            }
            
            // Disable collider for walkthrough
            if (ratCollider != null)
            {
                ratCollider.enabled = false;
            }
            
            // Make rigidbody kinematic
            if (ratRigidbody != null)
            {
                ratRigidbody.isKinematic = true;
            }
            
            // Wait a moment before cleaning up
            yield return new WaitForSeconds(5f);
            
            // Fade out and destroy
            float fadeTime = 2.0f;
            float elapsed = 0f;
            
            // Get all renderers
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            
            // Store original materials
            Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
            foreach (Renderer renderer in renderers)
            {
                originalMaterials[renderer] = renderer.materials;
            }
            
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / fadeTime);
                
                // Fade all materials
                foreach (Renderer renderer in renderers)
                {
                    Material[] materials = renderer.materials;
                    foreach (Material mat in materials)
                    {
                        Color color = mat.color;
                        color.a = alpha;
                        mat.color = color;
                    }
                    renderer.materials = materials;
                }
                
                yield return null;
            }
            
            // Destroy the rat
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Environmental awareness routine for responding to environment changes
        /// </summary>
        private IEnumerator EnvironmentalAwarenessRoutine()
        {
            while (true)
            {
                // Check for nearby lights
                FindNearestLight();
                
                // If afraid of light and in bright area, consider retreating
                if (afraidOfLight && IsInBrightLight() && 
                    (currentState == RatState.Idle || currentState == RatState.Patrol))
                {
                    ChangeState(RatState.Retreat);
                }
                
                // Wait before next check
                yield return new WaitForSeconds(MIN_LIGHT_CHECK_TIME);
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Increment the waypoint index for patrol
        /// </summary>
        private void IncrementWaypointIndex()
        {
            if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
            }
        }
        
        /// <summary>
        /// Check if player is detected
        /// </summary>
        private void CheckForPlayerDetection()
        {
            if (playerTransform == null)
                return;
                
            // Calculate distance to player
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            // Adjust detection range based on lighting
            float effectiveDetectionRange = detectionRange;
            if (lightingAffectsDetection)
            {
                // Get player in shadow or light status (could be more sophisticated)
                effectiveDetectionRange = IsPlayerInShadow() ? detectionRange * 0.6f : detectionRange;
            }
            
            // Check if player is within detection range
            if (distanceToPlayer <= effectiveDetectionRange)
            {
                // Check if player is within field of view
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToPlayer);
                
                if (angle <= fieldOfView * 0.5f)
                {
                    // Cast a ray to check for obstacles
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToPlayer, out hit, effectiveDetectionRange))
                    {
                        if (hit.transform == playerTransform)
                        {
                            // Player detected
                            if (!isAlerted)
                            {
                                ChangeState(RatState.Alert);
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Find the nearest light source
        /// </summary>
        private void FindNearestLight()
        {
            Light[] lights = FindObjectsOfType<Light>();
            float nearestDistance = float.MaxValue;
            nearestLight = null;
            
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                    continue; // Skip directional lights
                    
                float distance = Vector3.Distance(transform.position, light.transform.position);
                if (distance < nearestDistance && distance < light.range * 1.5f)
                {
                    nearestDistance = distance;
                    nearestLight = light;
                }
            }
        }
        
        /// <summary>
        /// Check if the rat is in a bright light area
        /// </summary>
        private bool IsInBrightLight()
        {
            if (nearestLight == null)
                return false;
                
            // Calculate distance to the light
            float distanceToLight = Vector3.Distance(transform.position, nearestLight.transform.position);
            
            // Check if distance is within range and light is bright enough
            return distanceToLight < nearestLight.range && nearestLight.intensity > retreatLightIntensity;
        }
        
        /// <summary>
        /// Check if player is in shadow (for detection modification)
        /// </summary>
        private bool IsPlayerInShadow()
        {
            if (playerTransform == null || nearestLight == null)
                return true; // Assume shadow if no light or player
                
            // Check if player is in shadow using raycasts
            Vector3 playerPos = playerTransform.position;
            Vector3 lightPos = nearestLight.transform.position;
            Vector3 dirToLight = (lightPos - playerPos).normalized;
            
            // Cast ray from player to light to check for obstacles
            RaycastHit hit;
            float distanceToLight = Vector3.Distance(playerPos, lightPos);
            if (Physics.Raycast(playerPos, dirToLight, out hit, distanceToLight))
            {
                // If we hit something between player and light, player is in shadow
                return true;
            }
            
            // Check light distance and intensity
            return distanceToLight > nearestLight.range || nearestLight.intensity < 0.3f;
        }
        
        /// <summary>
        /// Play a random sound from an array
        /// </summary>
        private void PlayRandomSound(AudioClip[] sounds, float volume = 1.0f)
        {
            if (sounds == null || sounds.Length == 0 || audioSource == null)
                return;
                
            // Select a random sound from the array
            AudioClip sound = sounds[Random.Range(0, sounds.Length)];
            if (sound != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f); // Add slight pitch variation
                audioSource.PlayOneShot(sound, volume);
            }
        }
        
        /// <summary>
        /// Perform an attack on the player
        /// </summary>
        private void PerformAttack()
        {
            if (playerTransform == null)
                return;
                
            // Play attack sound
            PlayRandomSound(attackSounds, 0.8f);
            
            // Check if player is in attack range and line of sight
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attackRange)
            {
                // Cast a ray to check for obstacles
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToPlayer, out hit, attackRange))
                {
                    if (hit.transform == playerTransform)
                    {
                        // Hit the player, deal damage
                        IDamageable playerDamageable = hit.transform.GetComponent<IDamageable>();
                        if (playerDamageable != null)
                        {
                            playerDamageable.TakeDamage(attackDamage);
                            Debug.Log($"Rat attacked player for {attackDamage} damage");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Update the animator parameters
        /// </summary>
        private void UpdateAnimator()
        {
            if (animator == null)
                return;
                
            // Update animation parameters based on state
            switch (currentState)
            {
                case RatState.Idle:
                    animator.SetBool("IsMoving", false);
                    animator.SetBool("IsAttacking", false);
                    animator.SetBool("IsAlert", false);
                    break;
                    
                case RatState.Patrol:
                    animator.SetBool("IsMoving", true);
                    animator.SetBool("IsAttacking", false);
                    animator.SetBool("IsAlert", false);
                    animator.SetFloat("MoveSpeed", patrolSpeed / chaseSpeed); // Normalized speed
                    break;
                    
                case RatState.Alert:
                    animator.SetBool("IsMoving", false);
                    animator.SetBool("IsAttacking", false);
                    animator.SetBool("IsAlert", true);
                    break;
                    
                case RatState.Chase:
                    animator.SetBool("IsMoving", true);
                    animator.SetBool("IsAttacking", false);
                    animator.SetBool("IsAlert", true);
                    animator.SetFloat("MoveSpeed", 1.0f); // Full chase speed
                    break;
                    
                case RatState.Attack:
                    animator.SetBool("IsMoving", false);
                    animator.SetBool("IsAttacking", true);
                    animator.SetBool("IsAlert", true);
                    break;
                    
                case RatState.Retreat:
                    animator.SetBool("IsMoving", true);
                    animator.SetBool("IsAttacking", false);
                    animator.SetBool("IsAlert", true);
                    animator.SetFloat("MoveSpeed", retreatSpeed / chaseSpeed); // Normalized speed
                    break;
                    
                case RatState.Stunned:
                    animator.SetBool("IsMoving", false);
                    animator.SetBool("IsAttacking", false);
                    animator.SetBool("IsStunned", true);
                    break;
                    
                case RatState.Dead:
                    animator.SetBool("IsMoving", false);
                    animator.SetBool("IsAttacking", false);
                    animator.SetBool("IsDead", true);
                    break;
            }
        }
        
        /// <summary>
        /// Schedule the next idle sound
        /// </summary>
        private void ScheduleNextIdleSound()
        {
            if (idleSounds == null || idleSounds.Length == 0)
                return;
                
            // Set next time with some randomness
            nextIdleSoundTime = Time.time + idleSoundInterval * Random.Range(0.8f, 1.2f);
        }
        
        #endregion
        
        #region IDamageable Implementation
        
        /// <summary>
        /// Take damage from an attack
        /// </summary>
        /// <param name="amount">Amount of damage to take</param>
        public void TakeDamage(float amount)
        {
            if (isDead)
                return;
                
            // Apply damage
            currentHealth -= amount;
            Debug.Log($"Rat took {amount} damage. Health: {currentHealth}/{maxHealth}");
            
            // Play hurt particles
            if (hurtParticles != null)
            {
                hurtParticles.Play();
            }
            
            // Play hurt sound
            PlayRandomSound(hurtSounds, 0.7f);
            
            // Check if dead
            if (currentHealth <= 0)
            {
                Die();
                return;
            }
            
            // If hit while idle or patrolling, go to alert state
            if (currentState == RatState.Idle || currentState == RatState.Patrol)
            {
                ChangeState(RatState.Alert);
            }
            // If heavily damaged (< 30% health), consider retreating
            else if (currentHealth < maxHealth * 0.3f && Random.value < 0.7f)
            {
                ChangeState(RatState.Retreat);
            }
            // Otherwise briefly stun
            else
            {
                ChangeState(RatState.Stunned);
            }
        }
        
        /// <summary>
        /// Kill the rat
        /// </summary>
        public void Die()
        {
            if (isDead)
                return;
                
            // Set state flags
            isDead = true;
            currentHealth = 0;
            
            // Change to dead state
            ChangeState(RatState.Dead);
            
            Debug.Log("Rat died");
        }
        
        /// <summary>
        /// Check if the rat is alive
        /// </summary>
        public bool IsAlive()
        {
            return !isDead && currentHealth > 0;
        }
        
        #endregion
    }
}
