using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameNetcodeStuff;
using UnityEngine;

[Command("carpet")]
sealed class CarpetCommand : ICommand
{
    private static readonly Dictionary<PlaceableShipObject, PlayerControllerB> ActiveFollowers = new();

    // Fonction qui colle un meuble aux pieds du joueur
    static Action<float> StickToPlayerFeet(PlaceableShipObject shipObject) => (timeElapsed) =>
    {
        if (shipObject == null || !ActiveFollowers.TryGetValue(shipObject, out var targetPlayer)) return;

        if (targetPlayer == null || targetPlayer.isPlayerDead)
        {
            ActiveFollowers.Remove(shipObject);
            return;
        }

        // POSITION : juste aux pieds du joueur
        Vector3 feetOffset = new Vector3(0f, 0.1f, 0f); // léger décalage pour pas passer dans le sol
        Vector3 targetPos = targetPlayer.transform.position + feetOffset;

        // ROTATION : retourné à l'envers (face vers le bas)
        Quaternion upsideDownRotation = Quaternion.Euler(180f, targetPlayer.transform.eulerAngles.y, 0f);

        // Application
        shipObject.transform.position = targetPos;
        shipObject.transform.rotation = upsideDownRotation;

        // Synchronisation réseau
        Helper.PlaceObjectAtPosition(shipObject, targetPos, upsideDownRotation.eulerAngles);
    };

    public async Task Execute(Arguments args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            Chat.Print("Usage: carpet <nom_du_joueur>");
            return;
        }

        PlayerControllerB targetPlayer = Helper.GetActivePlayer(args[0]);
        if (targetPlayer == null)
        {
            Chat.Print("Joueur introuvable.");
            return;
        }

        // Tous les meubles de la scène
        var allFurniture = UnityEngine.Object.FindObjectsOfType<PlaceableShipObject>();

        if (ActiveFollowers.Count > 0)
        {
            ActiveFollowers.Clear();
            Chat.Print("Carpet désactivé.");
        }
        else
        {
            Chat.Print($"Carpet activé sur {targetPlayer.playerUsername}. {allFurniture.Length} meubles volants !");
            foreach (var furniture in allFurniture)
            {
                ActiveFollowers[furniture] = targetPlayer;

                // Crée un composant qui applique StickToPlayerFeet à chaque frame
                Helper.CreateComponent<TransientBehaviour>()
                      .Init(CarpetCommand.StickToPlayerFeet(furniture), ulong.MaxValue);
            }
        }

        await Task.CompletedTask;
    }
}
