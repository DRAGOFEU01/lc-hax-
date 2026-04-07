using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;

[Command("jail")]
sealed class JailCommand : ICommand {
    private static readonly Dictionary<PlaceableShipObject, PlayerControllerB> ActiveFollowers = new();

    static Action<float> StickToPlayer(PlaceableShipObject shipObject) => (timeElapsed) => {
        if (shipObject == null || !ActiveFollowers.TryGetValue(shipObject, out var targetPlayer)) return;

        if (targetPlayer == null || targetPlayer.isPlayerDead) {
            ActiveFollowers.Remove(shipObject);
            return;
        }

        // 1. POSITION : On force le meuble à être EXACTEMENT sur le joueur
        // On utilise le transform du joueur comme parent virtuel
        Vector3 targetPos = targetPlayer.transform.position;

        // 2. ROTATION : Regarder vers le bas ET s'orienter selon la vue du joueur
        // targetPlayer.playerEyeUpperBodyObject est l'endroit où le joueur regarde
        Quaternion playerViewRotation = Quaternion.Euler(
            90f, // Couché à plat (face vers le bas)
            targetPlayer.transform.eulerAngles.y, 
            0f
        );

        // Application forcée
        shipObject.transform.position = targetPos;
        shipObject.transform.rotation = playerViewRotation;

        // 3. SYNCHRO RÉSEAU : On écrase la position pour tout le monde
        Helper.PlaceObjectAtPosition(shipObject, targetPos, playerViewRotation.eulerAngles);
    };

    public async Task Execute(Arguments args, CancellationToken cancellationToken) {
        if (args.Length == 0) {
            Chat.Print("Usage:  Jail <nom>");
            return;
        }

        PlayerControllerB targetPlayer = Helper.GetActivePlayer(args[0]);
        if (targetPlayer == null) {
            Chat.Print("Joueur introuvable.");
            return;
        }

        // On récupère TOUS les objets éparpillés
        var allFurniture = UnityEngine.Object.FindObjectsOfType<PlaceableShipObject>();

        if (ActiveFollowers.Count > 0) {
            ActiveFollowers.Clear();
            Chat.Print("Jail désactivée.");
        } 
        else {
            Chat.Print($"Jail activée sur {targetPlayer.playerUsername}. {allFurniture.Length} objets fusionnés.");
            foreach (var furniture in allFurniture) {
                ActiveFollowers[furniture] = targetPlayer;
                
                Helper.CreateComponent<TransientBehaviour>()
                      .Init(JailCommand.StickToPlayer(furniture), ulong.MaxValue);
            }
        }

        await Task.CompletedTask;
    }
}