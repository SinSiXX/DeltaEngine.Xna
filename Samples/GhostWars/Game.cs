﻿using DeltaEngine.Core;
using DeltaEngine.Input;
using DeltaEngine.Platforms;

namespace GhostWars
{
	public class Game : App
	{
		public Game()
		{
			new MainMenu(Resolve<Window>(), Resolve<Mouse>());
		}

		static void Main()
		{
			new Game().Run();
		}
	}
}