using System.Threading;
using System.Threading.Tasks;

namespace Hax;

[Command("sell")]
sealed class SellCommand : ICommand {
    public async Task Execute(Arguments args, CancellationToken cancellationToken) {
        // Cette commande ne fait plus le calcul complexe, elle active juste le mode Auto
        // Vous pouvez aussi taper /sell dans le chat
        Chat.Print("Use F11 to toggle Auto-Sell mode at the company!");
        await Task.CompletedTask;
    }
}