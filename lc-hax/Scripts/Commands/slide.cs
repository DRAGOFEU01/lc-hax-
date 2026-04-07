using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using GameNetcodeStuff;

// Namespace pour éviter tout conflit avec tes anciens scripts
namespace MyCustomCommands 
{
    [Command("cluster")]
    sealed class ClusterTrapCommand : ICommand {

        // --- LA STRUCTURE DE PLACEMENT (Basée sur ton exemple RandomCommand) ---
        static ObjectPlacement<Transform, PlaceableShipObject> CreateCagePlacement(PlaceableShipObject obj, PlayerControllerB target) {
            
            // STRATEGIE DE BLOCAGE "VIERGE DE FER" :
            // Au lieu de mettre tout le monde à 0,0,0, on génère un petit décalage aléatoire.
            // Certains objets seront devant, d'autres derrière, d'autres dans les jambes.
            // Cela crée un "nuage" de collisions solide impossible à traverser.
            
            // UnityEngine.Random.insideUnitSphere génère un point dans une sphère de rayon 1.
            // On multiplie par 0.8f pour que ça reste très serré contre le joueur.
            Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * 0.8f;
            
            // On s'assure que la hauteur (Y) est au moins au niveau des jambes (0.2) et pas sous le sol.
            randomOffset.y = Mathf.Abs(randomOffset.y) + 0.2f;

            // STRATEGIE DE ROTATION :
            // X = -90 : Pour redresser les objets (logique de ton exemple).
            // Y = Random : Pour qu'ils ne soient pas tous alignés pareil, créant un désordre bloquant.
            float randomRotY = UnityEngine.Random.Range(0f, 360f);

            return new() {
                TargetObject = target.transform,
                GameObject = obj,
                
                // Position calculée pour entourer le joueur
                PositionOffset = randomOffset,

                // Rotation : -90 pour redresser, et rotation Y aléatoire pour le chaos
                RotationOffset = new Vector3(-90.0f, randomRotY, 0.0f)
            };
        }

        // --- BOUCLE DE SPAM (Temps Réel) ---
        // Cette fonction force la position à chaque frame.
        static Action<float> TrapLoop(ObjectPlacement<Transform, PlaceableShipObject> placement) => (_) => {
            if (placement.GameObject == null || placement.TargetObject == null) return;
            
            // C'est ta méthode "propre" qui utilise le Transform parent.
            Helper.PlaceObjectAtTransform(placement);
        };

        // --- INITIALISATION ---
        static Action<PlaceableShipObject> InitTrap(PlayerControllerB target, float duration) => (shipObject) => {
            // Pour chaque objet, on calcule une position unique dans la cage
            var placement = ClusterTrapCommand.CreateCagePlacement(shipObject, target);

            // On lance le maintien en position (TransientBehaviour ne bloque pas le chat)
            Helper.CreateComponent<TransientBehaviour>()
                  .Init(ClusterTrapCommand.TrapLoop(placement), duration);
        };

        public async Task Execute(Arguments args, CancellationToken cancellationToken) {
            // Validation
            if (args.Length is 0) {
                Chat.Print("Usage: cluster <duration>");
                return;
            }

            if (!float.TryParse(args[0], out float duration)) {
                Chat.Print("Erreur: La durée doit être un nombre.");
                return;
            }

            // Sélection de la victime
            var victims = StartOfRound.Instance.allPlayerScripts
                            .Where(p => p.isPlayerControlled && !p.isPlayerDead)
                            .ToArray();

            if (victims.Length == 0) {
                Chat.Print("Personne à piéger !");
                return;
            }

            var target = victims[UnityEngine.Random.Range(0, victims.Length)];
            Chat.Print($"CAGE ACTIVÉE sur {target.playerUsername} pour {duration}s");

            // Récupération de TOUS les objets
            // On utilise FindObjects qui est la méthode standard de l'injecteur
            Helper.FindObjects<PlaceableShipObject>()
                  .ForEach(ClusterTrapCommand.InitTrap(target, duration));
            
            // Fin de la commande (Libère le chat immédiatement)
            await Task.CompletedTask;
        }
    }
}