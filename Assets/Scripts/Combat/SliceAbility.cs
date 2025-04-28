using UnityEngine;
using System.Collections;

namespace PDXUnderground.Combat
{
    /// <summary>
    /// Slice ability: dual-wield cards for close quarters melee attack
    /// </summary>
    public class SliceAbility : CombatAbility
    {
        [Header("Slice Ability Properties")]
        [SerializeField] private float sliceAttackArc = 120f; // Attack arc in degrees
        [SerializeField] private int sliceHitCount = 2; // Number of hits in the combo
        [SerializeField] private float sliceComboDelay = 0.2f; // Delay between hits
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private Transform attackOrigin;

        [Header("Slice VFX")]
        [SerializeField] private GameObject sliceVFXPrefab;
        [SerializeField] private Color sliceVFXColor = Color.red;

        protected override void Awake()
        {
            base.Awake();
            
            // Set default animation trigger for slice
            if (string.IsNullOrEmpty(animationTriggerName))
            {
                animationTriggerName = "Slice";
            }
            
            // If no attack origin specified, use transform
            if (attackOrigin == null)
            {
                attackOrigin = transform;
            }
        }

        protected override void ExecuteAbilityEffect()
        {
            // Start the slice combo coroutine
            StartCoroutine(PerformSliceCombo());
        }

        private IEnumerator PerformSliceCombo()
        {
            for (int i = 0; i < sliceHitCount; i++)
            {
                // Perform the actual slice attack
                PerformSliceAttack();
                
                // Wait before next hit in combo
                if (i < sliceHitCount - 1)
                {
                    yield return new WaitForSeconds(sliceComboDelay);
                }
            }
        }

        private void PerformSliceAttack()
        {
            // Find targets in range and arc
            Collider[] hitColliders = Physics.OverlapSphere(attackOrigin.position, range, targetLayers);
            
            int targetsHit = 0;
            
            foreach (var hitCollider in hitColliders)
            {
                // Check if target is within arc
                Vector3 directionToTarget = (hitCollider.transform.position - attackOrigin.position).normalized;
                float angleToTarget = Vector3.Angle(attackOrigin.forward, directionToTarget);
                
                if (angleToTarget <= sliceAttackArc * 0.5f)
                {
                    // Check for critical hit
                    bool isCritical = RollForCriticalHit();
                    
                    // Apply damage
                    ApplyDamage(hitCollider.gameObject, baseDamage, isCritical);
                    
                    // Spawn slice VFX
                    SpawnSliceVFX(hitCollider.transform.position);
                    
                    targetsHit++;
                }
            }
            
            // If no targets were hit, spawn VFX in front anyway for visual feedback
            if (targetsHit == 0)
            {
                Vector3 forwardPos = attackOrigin.position + attackOrigin.forward * range * 0.7f;
                SpawnSliceVFX(forwardPos);
            }
            
            Debug.Log($"Slice attack hit {targetsHit} targets");
        }

        private void SpawnSliceVFX(Vector3 position)
        {
            if (sliceVFXPrefab != null)
            {
                GameObject vfx = Instantiate(sliceVFXPrefab, position, Quaternion.identity);
                
                // Try to set color if possible
                ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var mainModule = ps.main;
                    mainModule.startColor = sliceVFXColor;
                }
                
                Destroy(vfx, 1f);
            }
        }

        // Visualize the attack arc in the editor
        private void OnDrawGizmosSelected()
        {
            if (attackOrigin == null)
                return;
                
            Gizmos.color = Color.red;
            
            // Draw attack range
            Gizmos.DrawWireSphere(attackOrigin.position, range);
            
            // Draw attack arc
            float halfArc = sliceAttackArc * 0.5f * Mathf.Deg2Rad;
            Vector3 leftArcDir = Quaternion.Euler(0, -sliceAttackArc * 0.5f, 0) * attackOrigin.forward;
            Vector3 rightArcDir = Quaternion.Euler(0, sliceAttackArc * 0.5f, 0) * attackOrigin.forward;
            
            Gizmos.DrawRay(attackOrigin.position, leftArcDir * range);
            Gizmos.DrawRay(attackOrigin.position, rightArcDir * range);
            
            // Draw arc segments
            int segments = 10;
            Vector3 prevPos = attackOrigin.position + leftArcDir * range;
            
            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                float angle = -halfArc + t * sliceAttackArc * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
                Vector3 arcPos = attackOrigin.position + attackOrigin.TransformDirection(direction) * range;
                
                Gizmos.DrawLine(prevPos, arcPos);
                prevPos = arcPos;
            }
        }
    }
}

