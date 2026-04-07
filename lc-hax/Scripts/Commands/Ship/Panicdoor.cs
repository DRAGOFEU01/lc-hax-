using System;
using System.Threading;
using System.Threading.Tasks;

[Command("panicdoor")]
sealed class PanicDoorCommand : ICommand, IShipDoor
{
    static CancellationTokenSource? _cts;

    public Task Execute(Arguments args, CancellationToken cancellationToken)
    {
        if (_cts != null)
        {
            try 
            { 
                _cts.Cancel(); 
                _cts.Dispose();
            } 
            catch { }
            
            _cts = null;
            return Task.CompletedTask;
        }

        _cts = new CancellationTokenSource();
        _ = this.RunPanicLoopAsync(_cts.Token);

        return Task.CompletedTask;
    }

    async Task RunPanicLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                this.SetShipDoorState(false);
                await Task.Delay(50, token);

                this.SetShipDoorState(true);
                await Task.Delay(50, token);
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            if (_cts != null && _cts.Token == token)
            {
                try { _cts.Dispose(); } catch { }
                _cts = null;
            }
        }
    }
}