﻿using DeltaEngine.Datatypes;

namespace DeltaEngine.Input
{
	/// <summary>
	/// Mouse input message for remote input via networking.
	/// </summary>
	public class MouseMessage
	{
		protected MouseMessage() {}

		public MouseMessage(Point position, int scrollWheel, MouseButton[] pressedButtons = null)
		{
			Position = position;
			ScrollWheel = scrollWheel;
			PressedButtons = pressedButtons ?? new MouseButton[0];
		}

		public Point Position { get; private set; }
		public MouseButton[] PressedButtons { get; private set; }
		public int ScrollWheel { get; private set; }
	}
}