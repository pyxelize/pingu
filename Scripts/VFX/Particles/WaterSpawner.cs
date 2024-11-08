using Godot;
using System;

namespace _CollisionsPhysiques.Scripts.VFX.Particles
{
    public partial class WaterSpawner : Marker2D
    {
        [Export] private PackedScene waterDrop;
        [Export] private float timeBetweenDropSpawn = 0.05f;

        private Timer rain = new Timer();

        public override void _Ready()
        {
            rain.WaitTime = timeBetweenDropSpawn;
            rain.OneShot = false;
            rain.Autostart = true;
            rain.Timeout += SpawnWaterDrop;
            AddChild(rain);

            GD.Randomize();
        }

        private void SpawnWaterDrop()
        {
            Camera2D lCamera = Main.GetInstance().camera;
            RigidBody2D lRigidBody2D = waterDrop.Instantiate() as RigidBody2D;
            Vector2 lViewportSize = GetViewportRect().Size;

            // Calculer une position X aléatoire dans la zone visible de la caméra
            float minX = lCamera.GlobalPosition.X - lViewportSize.X * lCamera.Zoom.X;
            float maxX = lCamera.GlobalPosition.X + lViewportSize.X * lCamera.Zoom.X;
            float randomX = GD.Randf() * (maxX - minX) + minX;

            // Positionner la goutte d'eau avec la position Y du spawner et la position X aléatoire
            lRigidBody2D.Position = new Vector2(randomX, Position.Y);

            Timer waterTimer = new Timer();
            waterTimer.OneShot = true;
            waterTimer.WaitTime = 5f;
            waterTimer.Timeout += () => lRigidBody2D.QueueFree();

            lRigidBody2D.AddChild(waterTimer);
            AddChild(lRigidBody2D);
            waterTimer.Start();
        }
    }
}
