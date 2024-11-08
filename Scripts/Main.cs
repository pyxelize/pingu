using System;
using Godot;

// Author : Julien Fournier

namespace _CollisionsPhysiques.Scripts {
	
	public partial class Main : Node2D
	{
        [Export] public Camera2D camera { get; private set; }

        private static Main instance;

        public override void _Ready()
		{
            if (instance == null) instance = this;
            else QueueFree();
        }

        public static Main GetInstance()
        {
            return instance;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            instance = null;
        }
    }
}
