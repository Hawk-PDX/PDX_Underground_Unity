using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

namespace PDXUnderground.AI.Enemies
{
    /// <summary>
    /// Controls the behavior of rats in the Shanghai Tunnels.
    /// Handles movement, detection, and response to environmental factors.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class RatBehavior : MonoBehaviour
    {
        #region Inspector Properties
        
        [Header("Movement Settings")]
        [Tooltip("Speed while patrolling")]
        [SerializeField] private float _patrolSpeed = 1.5f;
        [Tooltip("Speed while chasing player")]
        [SerializeField] private float _chaseSpeed = 3.5f;
        [Tooltip("Speed while retreating")]
        [SerializeField] private float _retreatSpeed = 4.0f;
        [Tooltip("Distance at which rat can detect player")]
        [SerializeField] private float _detectionRange = 8.0f;
        
        [Header("Patrol Settings")]
        [SerializeField] private Transform[] patrolWaypoints;
        [SerializeField] private float waypointWaitTime = 2.0f;
        [SerializeField] private float waypointReachedDistance = 0.5f;
        
        [Header("Light Response")]
        [SerializeField] private bool afraidOfLight = true;
        [SerializeField] private float lightIntensityThreshold = 0.7f;
        
        
        [Header("Sound")]
        [SerializeField] private float idleSoundInterval = 10f;
        [SerializeField] private float squeakChance = 0.3f;
        [SerializeField] private AudioClip squeakSound;
        [SerializeField] private AudioClip[] idleSounds;  // Array of idle sounds the rat can make
        [SerializeField] private float minTimeBetweenSounds = 5f;
        [SerializeField] private float maxTimeBetweenSounds = 15f;
        [SerializeField] private AudioClip[] alertSounds;  // Sounds played when rat becomes alert
        
        #endregion
        
        #region Public Properties
        
        // These public properties ensure compatibility with the ShanghaiTunnels script
        public float patrolSpeed
        {
            get => _patrolSpeed;
            set => _patrolSpeed = value;
        }
        
        public float chaseSpeed
        {
            get => _chaseSpeed;
            set => _chaseSpeed = value;
        }
        
        public float retreatSpeed
        {
            get => _retreatSpeed;
            set => _retreatSpeed = value;
        }
        
        public float detectionRange
        {
            get => _detectionRange;
            set => _detectionRange = value;
        }
        
        #endregion
        
        #region Private Variables
        
        private NavMeshAgent navAgent;
        private Animator animator;
        private AudioSource audioSource;
        private Transform playerTransform;
        private Interaction.ShanghaiTunnels tunnelsReference;
        
        private enum RatState { Idle, Patrol, Alert, Chase, Retreat }
        private RatState currentState = RatState.Idle;
        
        private int currentWaypointIndex = 0;
        private bool atWaypoint = false;
        private float nextSoundTime;
        private Vector3 lastKnownPlayerPosition;
        private Coroutine currentBehaviorRoutine;
        
        private const float MIN_LIGHT_CHECK_INTERVAL = 0.5f;
        
        #endregion
        
        #region Unity Lifecycle Methods
        
        private void Awake()
        {
            // Initialize idleSounds and alertSounds if not set
            if (idleSounds == null)
                idleSounds = new AudioClip[0];
                
            if (alertSounds == null)
                alertSounds = new AudioClip[0];
                
            navAgent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            
            // Add audio source if not present
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f; // Full 3D sound
                audioSource.minDistance = 1.0f;
                audioSource.maxDistance = 15.0f;
                audioSource.playOnAwake = false;
            }
            
            // Set up navigation agent
            navAgent.speed = patrolSpeed;
            navAgent.stoppingDistance = 0.5f;
            
            // Schedule first sound
            ScheduleNextSound();
        }
        
        private void Start()
        {
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            
            // Find ShanghaiTunnels reference
            tunnelsReference = FindObjectOfType<Interaction.ShanghaiTunnels>();
            
            // Start initial behavior
            ChangeState(RatState.Idle);
            
            // Start environmental awareness routine
            StartCoroutine(CheckForLightSources());
        }
        
        private void Update()
        {
            // Check for player detection
            if (currentState != RatState.Chase && playerTransform != null)
            {
                CheckForPlayerDetection();
            }
            
            // Check for sound playing
            if (Time.time > nextSoundTime && (currentState == RatState.Idle || currentState == RatState.Patrol))
            {
                PlayRandomSound(idleSounds, 0.4f);
                ScheduleNextSound();
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw waypoints if available
            if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < patrolWaypoints.Length; i++)
                {
                    if (patrolWaypoints[i] != null)
                    {
                        Gizmos.DrawSphere(patrolWaypoints[i].position, 0.2f);
                        
                        // Draw lines between waypoints
                        if (i < patrolWaypoints.Length - 1 && patrolWaypoints[i + 1] != null)
                        {
                            Gizmos.DrawLine(patrolWaypoints[i].position, patrolWaypoints[i + 1].position);
                        }
                    }
                }
            }
        }
        
        #endregion
        
        #region State Machine
        
        private void ChangeState(RatState newState)
        {
            if (currentState == newState) return;
            
            // Exit previous state logic
            if (currentBehaviorRoutine != null)
            {
                StopCoroutine(currentBehaviorRoutine);
                currentBehaviorRoutine = null;
            }
            
            // Update state
            currentState = newState;
            
            // Enter new state logic
            switch (newState)
            {
                case RatState.Idle:
                    navAgent.isStopped = true;
                    UpdateAnimation("idle");
                    currentBehaviorRoutine = StartCoroutine(IdleBehavior());
                    break;
                    
                case RatState.Patrol:
                    navAgent.isStopped = false;
                    navAgent.speed = patrolSpeed;
                    UpdateAnimation("walk");
                    currentBehaviorRoutine = StartCoroutine(PatrolBehavior());
                    break;
                    
                case RatState.Alert:
                    navAgent.isStopped = true;
                    UpdateAnimation("alert");
                    PlayRandomSound(alertSounds, 0.7f);
                    currentBehaviorRoutine = StartCoroutine(AlertBehavior());
                    break;
                    
                case RatState.Chase:
                    navAgent.isStopped = false;
                    navAgent.speed = chaseSpeed;
                    UpdateAnimation("run");
                    currentBehaviorRoutine = StartCoroutine(ChaseBehavior());
                    break;
                    
                case RatState.Retreat:
                    navAgent.isStopped = false;
                    navAgent.speed = retreatSpeed;
                    UpdateAnimation("run");
                    currentBehaviorRoutine = StartCoroutine(RetreatBehavior());
                    break;
            }
            
            Debug.Log($"Rat state changed to {newState}");
        }
        
        private void UpdateAnimation(string animationState)
        {
            if (animator != null)
            {
                animator.SetTrigger(animationState);
                
                // Set speed parameters if needed
                animator.SetFloat("Speed", navAgent.speed / chaseSpeed);
                animator.SetBool("IsMoving", currentState != RatState.Idle && currentState != RatState.Alert);
            }
        }
        
        #endregion
        
        #region Behavior Routines
        
        private IEnumerator IdleBehavior()
        {
            // Wait for a random time before patrolling
            float idleDuration = Random.Range(2f, 5f);
            yield return new WaitForSeconds(idleDuration);
            
            // Move to patrol state if we have waypoints
            if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                ChangeState(RatState.Patrol);
            }
        }
        
        private IEnumerator PatrolBehavior()
        {
            if (patrolWaypoints == null || patrolWaypoints.Length == 0)
            {
                ChangeState(RatState.Idle);
                yield break;
            }
            
            while (true)
            {
                // If we're waiting at a waypoint
                if (atWaypoint)
                {
                    yield return new WaitForSeconds(waypointWaitTime);
                    atWaypoint = false;
                    
                    // Move to next waypoint
                    currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
                }
                
                // Check if the current waypoint is valid
                if (patrolWaypoints[currentWaypointIndex] == null)
                {
                    // Skip invalid waypoints
                    currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
                    continue;
                }
                
                // Set destination to current waypoint
                navAgent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
                
                // Wait until we reach the waypoint
                while (navAgent.pathPending || navAgent.remainingDistance > waypointReachedDistance)
                {
                    // Check if path is invalid
                    if (navAgent.pathStatus == NavMeshPathStatus.PathInvalid)
                    {
                        // Move to next waypoint
                        currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
                        break;
                    }
                    
                    yield return null;
                }
                
                // We've reached the waypoint
                atWaypoint = true;
            }
        }
        
        private IEnumerator AlertBehavior()
        {
            // Look toward player
            if (playerTransform != null)
            {
                Vector3 lookDirection = playerTransform.position - transform.position;
                lookDirection.y = 0; // Keep on ground plane
                
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }
            
            // Wait in alert state
            yield return new WaitForSeconds(1.0f);
            
            // Transition to chase
            if (playerTransform != null)
            {
                lastKnownPlayerPosition = playerTransform.position;
                ChangeState(RatState.Chase);
            }
            else
            {
                ChangeState(RatState.Patrol);
            }
        }
        
        private IEnumerator ChaseBehavior()
        {
            while (true)
            {
                if (playerTransform == null)
                {
                    ChangeState(RatState.Patrol);
                    yield break;
                }
                
                // Update last known position
                lastKnownPlayerPosition = playerTransform.position;
                
                // Set destination to player
                navAgent.SetDestination(lastKnownPlayerPosition);
                
                // Check if we've lost sight of player
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer > detectionRange * 1.5f)
                {
                    ChangeState(RatState.Patrol);
                    yield break;
                }
                
                yield return null;
            }
        }
        
        private IEnumerator RetreatBehavior()
        {
            // Determine retreat direction (away from player or light)
            Vector3 retreatDirection;
            
            if (playerTransform != null)
            {
                retreatDirection = (transform.position - playerTransform.position).normalized;
            }
            else
            {
                // Just retreat backward
                retreatDirection = -transform.forward;
            }
            
            // Find a point to retreat to
            Vector3 retreatPoint = transform.position + retreatDirection * 10f;
            
            // Sample a valid position on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(retreatPoint, out hit, 10f, NavMesh.AllAreas))
            {
                retreatPoint = hit.position;
            }
            
            // Set destination
            navAgent.SetDestination(retreatPoint);
            
            // Wait until we reach the destination or get close enough
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
            
            // Return to patrol
            ChangeState(RatState.Patrol);
        }
        
        #endregion
        
        #region Environment Awareness
        
        private void CheckForPlayerDetection()
        {
            if (playerTransform == null) return;
            
            // Calculate distance to player
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distanceToPlayer <= detectionRange)
            {
                // Check if we have line of sight
                RaycastHit hit;
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                
                if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
                {
                    // If we hit the player, become alert
                    if (hit.transform == playerTransform)
                    {
                        if (currentState == RatState.Idle || currentState == RatState.Patrol)
                        {
                            ChangeState(RatState.Alert);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Periodically check for nearby light sources
        /// </summary>
        private IEnumerator CheckForLightSources()
        {
            while (true)
            {
                // Skip check if rat is not afraid of light
                if (!afraidOfLight)
                {
                    yield return new WaitForSeconds(MIN_LIGHT_CHECK_INTERVAL);
                    continue;
                }
                
                // Find nearby lights
                Light[] nearbyLights = GameObject.FindObjectsOfType<Light>();
                bool inBrightLight = false;
                
                foreach (Light light in nearbyLights)
                {
                    if (light.type == LightType.Directional)
                        continue; // Skip directional lights
                    
                    float distanceToLight = Vector3.Distance(transform.position, light.transform.position);
                    
                    // Check if the rat is within range of the light and if it's bright enough
                    if (distanceToLight < light.range && light.intensity > lightIntensityThreshold)
                    {
                        inBrightLight = true;
                        break;
                    }
                }
                
                // React to bright light if in chase or alert state
                if (inBrightLight && (currentState == RatState.Alert || currentState == RatState.Chase))
                {
                    ChangeState(RatState.Retreat);
                }
                
                yield return new WaitForSeconds(MIN_LIGHT_CHECK_INTERVAL);
            }
        }
        
        /// <summary>
        /// Schedules the next sound to be played
        /// </summary>
        private void ScheduleNextSound()
        {
            if (idleSounds == null || idleSounds.Length == 0)
                return;
                
            nextSoundTime = Time.time + Random.Range(minTimeBetweenSounds, maxTimeBetweenSounds);
        }
        
        /// <summary>
        /// Plays a random sound from the provided array
        /// </summary>
        private void PlayRandomSound(AudioClip[] sounds, float volume = 1.0f)
        {
            if (sounds == null || sounds.Length == 0 || audioSource == null)
                return;
                
            AudioClip sound = sounds[Random.Range(0, sounds.Length)];
            if (sound != null)
            {
                // Add some variation in pitch
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(sound, volume);
            }
        }
        
        /// <summary>
        /// Notify the ShanghaiTunnels script of rat events
        /// </summary>
        private void NotifyTunnelSystem(string eventType)
        {
            if (tunnelsReference != null)
            {
                // Call various methods on the ShanghaiTunnels script
                // based on eventType if needed
                switch (eventType)
                {
                    case "Spotted":
                        // Maybe call a method when player is spotted
                        // tunnelsReference.OnRatSpottedPlayer();
                        break;
                    case "Retreated":
                        // Maybe call a method when rat retreats
                        // tunnelsReference.OnRatRetreated();
                        break;
                }
            }
        }
        
        #endregion
    }
}
