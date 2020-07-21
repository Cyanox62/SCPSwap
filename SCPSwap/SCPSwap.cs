using Exiled.API.Features;

namespace SCPSwap
{
    public class ScpSwap : Plugin<Config>
    {
		public EventHandlers Handler { get; private set; }
		public override string Name => nameof(ScpSwap);
		public override string Author => "Cyanox";

		public ScpSwap() { }

		public override void OnEnabled()
		{
			Handler = new EventHandlers(this);
			Exiled.Events.Handlers.Server.WaitingForPlayers += Handler.OnWaitingForPlayers;
			Exiled.Events.Handlers.Server.RoundStarted += Handler.OnRoundStart;
			Exiled.Events.Handlers.Server.RoundEnded += Handler.OnRoundEnd;
			Exiled.Events.Handlers.Server.RestartingRound += Handler.OnRoundRestart;
			Exiled.Events.Handlers.Server.SendingConsoleCommand += Handler.OnConsoleCommand;
		}

		public override void OnDisabled()
		{
			Exiled.Events.Handlers.Server.WaitingForPlayers -= Handler.OnWaitingForPlayers;
			Exiled.Events.Handlers.Server.RoundStarted -= Handler.OnRoundStart;
			Exiled.Events.Handlers.Server.RoundEnded -= Handler.OnRoundEnd;
			Exiled.Events.Handlers.Server.RestartingRound -= Handler.OnRoundRestart;
			Exiled.Events.Handlers.Server.SendingConsoleCommand -= Handler.OnConsoleCommand;
			Handler = null;
		}

		public override void OnReloaded() { }
	}
}
