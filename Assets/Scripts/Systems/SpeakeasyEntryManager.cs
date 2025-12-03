
using UnityEngine;
using PDXUnderground.Player;
using PDXUnderground.Systems;

namespace PDXUnderground.Systems
{
    public class SpeakeasyEntryManager : MonoBehaviour
    {
        [Header("Entry Fees")]
        public float standardCoverCharge = 0.50f;
        public float vipLoungeAccess = 5.00f;
        public float backroomMinimumBet = 2.00f;
        
        [Header("Access Requirements")]
        public int standardReputation = 0;
        public int vipReputation = 3;
        
        public bool CanEnterStandard(float currentMoney)
        {
            return currentMoney >= standardCoverCharge;
        }
        
        public bool CanEnterVIP(float currentMoney, int reputation)
        {
            return currentMoney >= vipLoungeAccess && 
                   reputation >= vipReputation;
        }
        
        public bool CanJoinBackroom(float currentMoney, int reputation)
        {
            return currentMoney >= backroomMinimumBet &&
                   reputation >= 1;
        }
        
        public float PayCoverCharge(float currentMoney)
        {
            return currentMoney - standardCoverCharge;
        }
    }
}

