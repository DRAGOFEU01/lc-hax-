using System.Collections;
using UnityEngine;
using GameNetcodeStuff;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hax;

[Command("trap")]
sealed class TrapCommand : ICommand 
{
    public async Task Execute(Arguments args, CancellationToken cancellationToken) 
    {
        if (args.Length == 0)
        {
            TrapLogic.TargetPlayer = null;
            HUDManager.Instance.DisplayTip("TRAP MOD", "DÉSACTIVÉ. Libération.", false, false, "LC_Tip1");
            await Task.CompletedTask;
            return;
        }

        string targetName = args[0].ToLower();
        PlayerControllerB? foundPlayer = null;

        foreach (var player in StartOfRound.Instance.allPlayerScripts)
        {
            if (player.playerUsername.ToLower().Contains(targetName))
            {
                foundPlayer = player;
                break;
            }
        }

        if (foundPlayer == null)
        {
            HUDManager.Instance.DisplayTip("ERREUR", $"Joueur '{targetName}' introuvable !", true, false, "LC_Tip1");
            return;
        }

        TrapLogic.TargetPlayer = foundPlayer;
        HUDManager.Instance.DisplayTip("TRAP MOD", $"CIBLE : {foundPlayer.playerUsername}", false, false, "LC_Tip1");
        
        await Task.CompletedTask;
    }
}

public class TrapLogic : MonoBehaviour 
{
    public static PlayerControllerB? TargetPlayer;

    void LateUpdate() 
    {
        if (TargetPlayer == null || TargetPlayer.isPlayerDead) return;

        Vector3 targetPos = TargetPlayer.transform.position;

        AutoParentToShip[] shipObjects = Object.FindObjectsOfType<AutoParentToShip>();
        
        foreach (var obj in shipObjects)
        {
            if (obj == null || !obj.gameObject.activeInHierarchy) continue;
            if (obj.transform.parent == null) continue;

            float offset = (obj.GetInstanceID() % 10) / 5f; 
            
            obj.transform.position = targetPos + new Vector3(0, 0.5f + offset, 0);
            obj.transform.LookAt(targetPos);
        }

        GrabbableObject[] items = Object.FindObjectsOfType<GrabbableObject>();

        foreach (var item in items)
        {
            if (item == null || item.playerHeldBy == TargetPlayer) continue;

            if (StartOfRound.Instance.elevatorTransform != null)
            {
                float dist = Vector3.Distance(item.transform.position, StartOfRound.Instance.elevatorTransform.position);
                if (dist > 25f) continue;
            }

            item.transform.position = targetPos + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(0.2f, 1.5f), UnityEngine.Random.Range(-0.5f, 0.5f));
            
            if (item.TryGetComponent(out Rigidbody rb))
            {
                rb.velocity = Vector3.zero;
            }
        }
    }
}