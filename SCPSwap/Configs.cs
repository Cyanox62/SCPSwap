using System.Collections.Generic;

namespace SCPSwap
{
	class Configs
	{
		internal static List<int> blacklist;

		internal static bool allowNewScps;

		internal static float reqTimeout;
		internal static float swapTimeout;

		public static void ReloadConfigs()
		{
			blacklist = Plugin.Config.GetIntList("swap_blacklist");
			if (blacklist == null || blacklist.Count == 0)
			{
				blacklist = new List<int>() { 10 };
			}

			allowNewScps = Plugin.Config.GetBool("swap_allow_new_scps", true);

			swapTimeout = Plugin.Config.GetFloat("swap_timeout", 60f);
			reqTimeout = Plugin.Config.GetFloat("swap_request_timeout", 20f);
		}
	}
}
