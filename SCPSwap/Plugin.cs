using EXILED;

namespace SCPSwap
{
    public class Plugin : EXILED.Plugin
    {
		private EventHandlers ev;

		public override void OnEnable()
		{
			ev = new EventHandlers();
			Events.WaitingForPlayersEvent += ev.OnWaitingForPlayers;
			Events.RoundStartEvent += ev.OnRoundStart;
			Events.RoundEndEvent += ev.OnRoundEnd;
			Events.RoundRestartEvent += ev.OnRoundRestart;
			Events.ConsoleCommandEvent += ev.OnConsoleCommand;
		}

		public override void OnDisable()
		{
			Events.WaitingForPlayersEvent -= ev.OnWaitingForPlayers;
			Events.RoundStartEvent -= ev.OnRoundStart;
			Events.RoundEndEvent -= ev.OnRoundEnd;
			Events.RoundRestartEvent -= ev.OnRoundRestart;
			Events.ConsoleCommandEvent -= ev.OnConsoleCommand;
			ev = null;
		}

		public override void OnReload() { }

		public override string getName { get; } = "SCPSwap";
	}
}
