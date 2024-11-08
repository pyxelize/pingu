using System;
using Godot;

namespace _CollisionsPhysiques.Scripts.GameObjects.Collectables
{
    public partial class Collectable : Area2D
    {
        [Export] private float growFactor = 0.05f;

        [Export] public CollectableTypes type { get; protected set;}

        public override void _Ready()
        {
            base._Ready();
            BodyEntered += OnCollectableOverlap;
        }

        /// <summary>
        /// Executed when an object overlap a collectable
        /// </summary>
        /// <param name="pBody">the object that collide with the collectable</param>
        /// <param name="pItem">the collectable item (fish or burger)</param>
        /// <param name="pIsFish">bool true if the Collectable is fish</param>
        private void OnCollectableOverlap(Node pBody)
        {
            if (pBody is not Player) return;
            Player.GetInstance().Grow(growFactor * (int)Enum.GetValues(type.GetType()).GetValue((int)type));
            QueueFree();
        }

    }
}
