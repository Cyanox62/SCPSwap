using EXILED;
using EXILED.Extensions;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSwap
{
	class EventHandlers
	{
		private Dictionary<ReferenceHub, ReferenceHub> ongoingReqs = new Dictionary<ReferenceHub, ReferenceHub>();

		private List<CoroutineHandle> coroutines = new List<CoroutineHandle>();
		private Dictionary<ReferenceHub, CoroutineHandle> reqCoroutines = new Dictionary<ReferenceHub, CoroutineHandle>();

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
			{"zombie", RoleType.Scp0492}
		};

		private IEnumerator<float> SendRequest(ReferenceHub source, ReferenceHub dest)
		{
			ongoingReqs.Add(source, dest);
			dest.Broadcast(5, "<i>You have an SCP Swap request!\nCheck your console by pressing [`] or [~]</i>", false);
			dest.characterClassManager.TargetConsolePrint(dest.scp079PlayerScript.connectionToClient, $"You have received a swap request from {source.nicknameSync.Network_myNickSync} who is SCP-{valid.FirstOrDefault(x => x.Value == source.GetRole()).Key}. Would you like to swap with them? Type \".scpswap yes\" to accept or \".scpswap no\" to decline.", "yellow");
			yield return Timing.WaitForSeconds(Configs.reqTimeout);
			TimeoutRequest(source);
		}

		private void TimeoutRequest(ReferenceHub source)
		{
			if (ongoingReqs.ContainsKey(source))
			{
				ReferenceHub dest = ongoingReqs[source];
				source.characterClassManager.TargetConsolePrint(source.scp079PlayerScript.connectionToClient, "The player did not respond to your request.", "red");
				dest.characterClassManager.TargetConsolePrint(dest.scp079PlayerScript.connectionToClient, "Your swap request has timed out.", "red");
				ongoingReqs.Remove(source);
			}
		}

		private void PerformSwap(ReferenceHub source, ReferenceHub dest)
		{
			source.characterClassManager.TargetConsolePrint(source.scp079PlayerScript.connectionToClient, "Swap successful!", "green");

			RoleType sRole = source.GetRole();
			RoleType dRole = dest.GetRole();

			Vector3 sPos = source.GetPosition();
			Vector3 dPos = dest.GetPosition();

			float sHealth = source.playerStats.health;
			float dHealth = dest.playerStats.health;

			source.SetRole(dRole);
			source.SetPosition(dPos);
			source.playerStats.health = dHealth;

			dest.SetRole(sRole);
			dest.SetPosition(sPos);
			dest.playerStats.health = sHealth;

			ongoingReqs.Remove(source);
		}

		public void OnRoundStart()
		{
			allowSwaps = true;
			isRoundStarted = true;
			Timing.CallDelayed(Configs.swapTimeout, () => allowSwaps = false);
		}

		public void OnRoundRestart()
		{
			// fail safe
			isRoundStarted = false;
			Timing.KillCoroutines(coroutines);
			Timing.KillCoroutines(reqCoroutines.Values);
			coroutines.Clear();
			reqCoroutines.Clear();
		}

		public void OnRoundEnd()
		{
			isRoundStarted = false;
			Timing.KillCoroutines(coroutines);
			Timing.KillCoroutines(reqCoroutines.Values);
			coroutines.Clear();
			reqCoroutines.Clear();
		}

		public void OnWaitingForPlayers()
		{
			isRoundStarted = false;
			allowSwaps = false;
			Configs.ReloadConfigs();
		}

		public void OnConsoleCommand(ConsoleCommandEvent ev)
		{
			string cmd = ev.Command.ToLower();
			if (cmd.StartsWith("scpswap"))
			{
				if (isRoundStarted)
				{
					if (ev.Player.GetTeam() == Team.SCP)
					{
						if (allowSwaps)
						{
							string[] args = cmd.Replace("scpswap", "").Trim().Split(' ');
							if (args.Length == 1)
							{
								if (args[0] == "yes")
								{
									ReferenceHub swap = ongoingReqs.FirstOrDefault(x => x.Value == ev.Player).Key;
									if (swap != null)
									{
										PerformSwap(swap, ev.Player);
										ev.ReturnMessage = "Swap successful!";
										Timing.KillCoroutines(reqCoroutines[swap]);
										reqCoroutines.Remove(swap);
										ev.Color = "green";
										return;
									}
									else
									{
										ev.ReturnMessage = "You do not have a swap request.";
										return;
									}
								}
								else if (args[0] == "no")
								{
									ReferenceHub swap = ongoingReqs.FirstOrDefault(x => x.Value == ev.Player).Key;
									if (swap != null)
									{
										ev.ReturnMessage = "Swap request denied.";
										swap.characterClassManager.TargetConsolePrint(swap.scp079PlayerScript.connectionToClient, "Your swap request has been denied.", "red");
										Timing.KillCoroutines(reqCoroutines[swap]);
										reqCoroutines.Remove(swap);
										ongoingReqs.Remove(swap);
										return;
									}
									else
									{
										ev.ReturnMessage = "You do not have a swap reqest.";
										return;
									}
								}
								else if (args[0] == "cancel")
								{
									if (ongoingReqs.ContainsKey(ev.Player))
									{
										ReferenceHub dest = ongoingReqs[ev.Player];
										dest.characterClassManager.TargetConsolePrint(dest.scp079PlayerScript.connectionToClient, "Your swap request has been cancelled.", "red");
										Timing.KillCoroutines(reqCoroutines[ev.Player]);
										reqCoroutines.Remove(ev.Player);
										ongoingReqs.Remove(ev.Player);
										ev.ReturnMessage = "You have cancelled your swap request.";
										return;
									}
									else
									{
										ev.ReturnMessage = "You do not have an outgoing swap request.";
										return;
									}
								}
								else if (valid.ContainsKey(args[0]))
								{
									if (!ongoingReqs.ContainsKey(ev.Player))
									{
										RoleType role = valid[args[0]];
										if (!Configs.blacklist.Contains((int)role))
										{
											if (ev.Player.GetRole() != role)
											{
												ReferenceHub swap = Player.GetHubs().FirstOrDefault(x => role == RoleType.Scp93953 ? x.GetRole() == role || x.GetRole() == RoleType.Scp93989 : x.GetRole() == role);
												if (swap != null)
												{
													reqCoroutines.Add(ev.Player, Timing.RunCoroutine(SendRequest(ev.Player, swap)));
													ev.ReturnMessage = "Swap request sent!";
													ev.Color = "green";
													return;
												}
												else
												{
													if (Configs.allowNewScps)
													{
														ev.Player.SetRole(role);
														ev.ReturnMessage = "Could not find a player to swap with, you have been made the specified SCP.";
														ev.Color = "green";
														return;
													}
													else
													{
														ev.ReturnMessage = "No players found to swap with.";
														return;
													}
												}
											}
											else
											{
												ev.ReturnMessage = "You cannot swap with your own role.";
												return;
											}
										}
										else
										{
											ev.ReturnMessage = "That SCP is blacklisted.";
											return;
										}
									}
									else
									{
										ev.ReturnMessage = "You already have a request pending!";
										return;
									}
								}
								else
								{
									ev.ReturnMessage = "Invalid SCP.";
									return;
								}
							}
							else
							{
								ev.ReturnMessage = "USAGE: SCPSWAP [SCP NUMBER]";
								return;
							}
						}
						else
						{
							ev.ReturnMessage = "SCP swap period has expired.";
							return;
						}
					}
					else
					{
						ev.ReturnMessage = "You're not an SCP, why did you think that would work.";
						return;
					}
				}
				else
				{
					ev.ReturnMessage = "The round hasn't started yet!";
					return;
				}
			}
		}
	}
}
