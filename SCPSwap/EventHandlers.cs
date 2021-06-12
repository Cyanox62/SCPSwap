using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Loader;
using MEC;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SCPSwap
{
	public sealed class EventHandlers
	{
		private Dictionary<Player, Player> ongoingReqs = new Dictionary<Player, Player>();

		private List<CoroutineHandle> coroutines = new List<CoroutineHandle>();
		private Dictionary<Player, CoroutineHandle> reqCoroutines = new Dictionary<Player, CoroutineHandle>();

		private bool allowSwaps = false;
		private bool isRoundStarted = false;

		private Dictionary<string, RoleType> valid = new Dictionary<string, RoleType>()
		{
			{"173", RoleType.Scp173},
			{"peanut", RoleType.Scp173},
			{"939", RoleType.Scp93953},
			{"dog", RoleType.Scp93953},
			{"079", RoleType.Scp079},
			{"computer", RoleType.Scp079},
			{"106", RoleType.Scp106},
			{"larry", RoleType.Scp106},
			{"096", RoleType.Scp096},
			{"shyguy", RoleType.Scp096},
			{"049", RoleType.Scp049},
			{"doctor", RoleType.Scp049},
			{"0492", RoleType.Scp0492},
			{"zombie", RoleType.Scp0492},
			{"966", RoleType.None }
		};

		public ScpSwap plugin;

		public EventHandlers(ScpSwap plugin) => this.plugin = plugin;

		private IEnumerator<float> SendRequest(Player source, Player dest)
		{
			ongoingReqs.Add(source, dest);
			dest.Broadcast(5, "<i>You have an SCP Swap request!\nCheck your console by pressing [`] or [~]</i>");
			dest.ReferenceHub.characterClassManager.TargetConsolePrint(dest.ReferenceHub.scp079PlayerScript.connectionToClient, $"You have received a swap request from {source.ReferenceHub.nicknameSync.Network_myNickSync} who is SCP-{valid.FirstOrDefault(x => x.Value == source.Role).Key}. Would you like to swap with them? Type \".scpswap yes\" to accept or \".scpswap no\" to decline.", "yellow");
			yield return Timing.WaitForSeconds(plugin.Config.SwapRequestTimeout);
			TimeoutRequest(source);
		}

		private void TimeoutRequest(Player source)
		{
			if (ongoingReqs.ContainsKey(source))
			{
				Player dest = ongoingReqs[source];
				source.ReferenceHub.characterClassManager.TargetConsolePrint(source.ReferenceHub.scp079PlayerScript.connectionToClient, "The player did not respond to your request.", "red");
				dest.ReferenceHub.characterClassManager.TargetConsolePrint(dest.ReferenceHub.scp079PlayerScript.connectionToClient, "Your swap request has timed out.", "red");
				ongoingReqs.Remove(source);
			}
		}

		private void PerformSwap(Player source, Player dest)
		{
			bool source966 = source.SessionVariables.ContainsKey("is966") && (bool)source.SessionVariables["is966"];
			bool dest966 = dest.SessionVariables.ContainsKey("is966") && (bool)dest.SessionVariables["is966"];
			source.ReferenceHub.characterClassManager.TargetConsolePrint(source.ReferenceHub.scp079PlayerScript.connectionToClient, "Swap successful!", "green");

			RoleType sRole = source.Role;
			RoleType dRole = dest.Role;

			Vector3 sPos = source.Position;
			Vector3 dPos = dest.Position;

			float sHealth = source.Health;
			float dHealth = dest.Health;

			if (!dest966) source.Role = dRole;
			else Swap966(source);
			source.Position = dPos;
			source.Health = dHealth;

			if (!source966) dest.Role = sRole;
			else Swap966(dest);
			dest.Position = sPos;
			dest.Health = sHealth;

			ongoingReqs.Remove(source);
		}

		public void OnRoundStart()
		{
			allowSwaps = true;
			isRoundStarted = true;
			Timing.CallDelayed(plugin.Config.SwapTimeout, () => allowSwaps = false);
		}

		public void OnRoundRestart()
		{
			// fail safe
			isRoundStarted = false;
			Timing.KillCoroutines(coroutines.ToArray());
			Timing.KillCoroutines(reqCoroutines.Values.ToArray());
			coroutines.Clear();
			reqCoroutines.Clear();
		}

		public void OnRoundEnd(RoundEndedEventArgs ev)
		{
			isRoundStarted = false;
			Timing.KillCoroutines(coroutines.ToArray());
			Timing.KillCoroutines(reqCoroutines.Values.ToArray());
			coroutines.Clear();
			reqCoroutines.Clear();
		}

		public void OnWaitingForPlayers()
		{
			allowSwaps = false;
		}

		public void OnConsoleCommand(SendingConsoleCommandEventArgs ev)
		{
			if (ev.Name.ToLower().Contains("scpswap"))
			{
				ev.IsAllowed = false;
				if (!isRoundStarted)
				{
					ev.ReturnMessage = "The round hasn't started yet!";
					ev.Color = "red";
					return;
				}

				if (!(ev.Player.Team == Team.SCP))
				{
					ev.ReturnMessage = "You're not an SCP, why did you think that would work.";
					ev.Color = "red";
					return;
				}

				if (!allowSwaps)
				{
					ev.ReturnMessage = "SCP swap period has expired.";
					ev.Color = "red";
					return;
				}

				switch (ev.Arguments.Count)
				{
					case 1:
						switch (ev.Arguments[0].ToLower())
						{
							case "yes":
								Player swap = ongoingReqs.FirstOrDefault(x => x.Value == ev.Player).Key;
								if (swap != null)
								{
									PerformSwap(swap, ev.Player);
									ev.ReturnMessage = "Swap successful!";
									Timing.KillCoroutines(reqCoroutines[swap]);
									reqCoroutines.Remove(swap);
									ev.Color = "green";
									return;
								}
								ev.ReturnMessage = "You do not have a swap request.";
								break;
							case "no":
								swap = ongoingReqs.FirstOrDefault(x => x.Value == ev.Player).Key;
								if (swap != null)
								{
									ev.ReturnMessage = "Swap request denied.";
									swap.ReferenceHub.characterClassManager.TargetConsolePrint(swap.ReferenceHub.scp079PlayerScript.connectionToClient, "Your swap request has been denied.", "red");
									Timing.KillCoroutines(reqCoroutines[swap]);
									reqCoroutines.Remove(swap);
									ongoingReqs.Remove(swap);
									return;
								}
								ev.ReturnMessage = "You do not have a swap reqest.";
								break;
							case "cancel":
								if (ongoingReqs.ContainsKey(ev.Player))
								{
									Player dest = ongoingReqs[ev.Player];
									dest.ReferenceHub.characterClassManager.TargetConsolePrint(dest.ReferenceHub.scp079PlayerScript.connectionToClient, "Your swap request has been cancelled.", "red");
									Timing.KillCoroutines(reqCoroutines[ev.Player]);
									reqCoroutines.Remove(ev.Player);
									ongoingReqs.Remove(ev.Player);
									ev.ReturnMessage = "You have cancelled your swap request.";
									return;
								}
								ev.ReturnMessage = "You do not have an outgoing swap request.";
								break;
							default:
								if (!valid.ContainsKey(ev.Arguments[0]))
								{
									ev.ReturnMessage = "Invalid SCP.";
									ev.Color = "red";
									return;
								}

								if (ongoingReqs.ContainsKey(ev.Player))
								{
									ev.ReturnMessage = "You already have a request pending!";
									ev.Color = "red";
									return;
								}

								RoleType role = valid[ev.Arguments[0]];
								bool is966 = false;
								if (ev.Player.SessionVariables.ContainsKey("is966"))
								{
									is966 = (bool)ev.Player.SessionVariables["is966"];
								}
								bool req966 = ev.Arguments[0] == "966";
								if (plugin.Config.SwapBlacklist.Contains((int)role))
								{
									ev.ReturnMessage = "That SCP is blacklisted.";
									ev.Color = "red";
									return;
								}

								if (ev.Player.Role == role || (is966 && req966))
								{
									ev.ReturnMessage = "You cannot swap with your own role.";
									ev.Color = "red";
									return;
								}

								if (!req966) swap = Player.List.FirstOrDefault(x => role == RoleType.Scp93953 ? x.Role == role || x.Role == RoleType.Scp93989 : x.Role == role);
								else swap = Player.List.FirstOrDefault(x => x.SessionVariables.ContainsKey("is966") && (bool) x.SessionVariables["is966"]);
								if (swap != null)
								{
									reqCoroutines.Add(ev.Player, Timing.RunCoroutine(SendRequest(ev.Player, swap)));
									ev.ReturnMessage = "Swap request sent!";
									ev.Color = "green";
									return;
								}
								if (plugin.Config.SwapAllowNewScps)
								{
									if (!req966) ev.Player.ReferenceHub.characterClassManager.SetPlayersClass(role, ev.Player.ReferenceHub.gameObject);
									else Swap966(ev.Player);
									ev.ReturnMessage = "Could not find a player to swap with, you have been made the specified SCP.";
									ev.Color = "green";
									return;
								}
								ev.ReturnMessage = "No players found to swap with.";
								ev.Color = "red";
								break;
						}
						break;
					default:
						ev.ReturnMessage = "USAGE: SCPSWAP [SCP NUMBER]";
						ev.Color = "red";
						break;
				}
			}
		}

		public void Swap966(Player newPlayer)
		{
			Assembly assembly = Loader.Plugins.First(pl => pl.Name == "scp966")?.Assembly;
			if (assembly == null) return;
			assembly.GetType("scp966.API")?.GetMethod("Swap", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new[] { newPlayer });
		}
	}
}
