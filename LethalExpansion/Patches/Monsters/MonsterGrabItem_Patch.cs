using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace LethalExpansion.Patches.Monsters
{
    public class MonsterGrabItem_Patch
    {
        public static void MonsterGrabItem(EnemyAI __instance, NetworkObject item)
        {
            GrabbableObject grabbableObject = item.GetComponent<GrabbableObject>();
            if (grabbableObject.GetIsOnLandmine())
            {
                grabbableObject.GetLandmine().ItemHeld(grabbableObject);
            }
            LethalExpansion.Log.LogInfo("Item Grabbed by monster.");
        }
        public static void MonsterDropItem_Patch(EnemyAI __instance, NetworkObject item)
        {
            LethalExpansion.Log.LogInfo("Item Dropped by monster.");
        }
        public static void KillEnemy_Patch(EnemyAI __instance)
        {
            LethalExpansion.Log.LogInfo("Monster Killed.");
        }
    }
}
