using System.Collections.Generic;
using Exiled.API.Interfaces;

namespace SCPSwap
{
	public sealed class Config : IConfig
	{
		public bool IsEnabled { get; set; } = true;
		public bool SwapAllowNewScps { get; set; } = false;
		public float SwapTimeout { get; set; } = 60f;
		public float SwapRequestTimeout { get; set; } = 20f;
		public List<int> SwapBlacklist { get; private set; } = new List<int>() { 10 };
	}
}
