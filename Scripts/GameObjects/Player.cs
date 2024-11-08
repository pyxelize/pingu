using System;
using _CollisionsPhysiques.Scripts.Tools;
using Godot;

namespace _CollisionsPhysiques.Scripts.GameObjects
{
    public partial class Player : CharacterBody2D
    {
        private const string ANIM_PATH = "anim";
        private const string ANIM_WALK = "walk";
        private const string ANIM_IDLE = "idle";

        private AnimatedSprite2D anim;

        private static Player instance;

        #region PLAYER MOVEMENTS
        // Toujours exporter des variables de tweak dans l'inspecteur. Les Game designers vous en remercieront
        [Export] private float movespeed = 500f;
        [Export] private float gravity = 90f;

        [Export] private float minFallForce = 5f;
        [Export] private float maxFallForce = 1000f;

        [Export] private float jumpForce = 1300f;
        [Export] private float jumpForceFactor = 200f;

        private Vector2 direction;

        #endregion

        #region PLAYER GROW AND DIET

        [Export] private float playerMaxScale = 3f;
        [Export] private float playerMinScale;

        #endregion

        #region HEALTH (jump and speed control)

        private float health = 0.75f;
        private const float MIN_HEALTH = 0.5f;
        private const float MAX_HEALTH = 1f;

        #endregion

        public override void _Ready()
        {
            base._Ready();
            if (instance == null) instance = this;
            else QueueFree();

            anim = (AnimatedSprite2D)GetNode(ANIM_PATH);
        }

        public override void _Process(double pDelta)
        {
            float lDelta = (float)pDelta;
            PlayerMovements();
            PlayerAnimation();
        }

        /// <summary>
        /// Set gravity, x and y velocity for the player
        /// </summary>
        private void PlayerMovements()
        {
            //gravity
            direction.Y += gravity;
            if (direction.Y > maxFallForce) direction.Y = maxFallForce;
            if (IsOnFloor()) direction = new Vector2(direction.X, 0f);

            // moving player on x axis
            direction.X = Input.GetActionStrength(Inputs.ACTION_MOVE_RIGHT) - Input.GetActionStrength(Inputs.ACTION_MOVE_LEFT);

            if (direction.X == -1) anim.FlipH = true;
            else if (direction.X == 1) anim.FlipH = false;

            // impact player speed (x axis) with health factor
            direction.X *= movespeed * health;

            PlayerJump();

            Velocity = direction; // Attribuer la direction actuelle à la propriété Velocity
            MoveAndSlide();
        }



        /// <summary>
        /// Capacity to jump
        /// </summary>
        private void PlayerJump()
        {
            if (IsOnFloor() && Input.IsActionJustPressed(Inputs.ACTION_JUMP)) direction.Y = -jumpForce * health;
        }

        /// <summary>
        /// Set player animation state
        /// </summary>
        private void PlayerAnimation()
        {
            if (Input.IsActionPressed(Inputs.ACTION_MOVE_LEFT) || Input.IsActionPressed(Inputs.ACTION_MOVE_RIGHT))
            {
                if (anim.Animation != ANIM_WALK) anim.Animation = ANIM_WALK;
            }
            else
            {
                if (anim.Animation != ANIM_IDLE) anim.Animation = ANIM_IDLE;
            }
        }
        public void Grow(float pGrowthFactor)
        {
            GD.Print(pGrowthFactor);
            if ((anim.Scale.X > playerMinScale && pGrowthFactor < 0) || (anim.Scale.X < playerMaxScale && pGrowthFactor > 0)) anim.Scale -= new Vector2(pGrowthFactor, pGrowthFactor);
            if ((health < MAX_HEALTH && pGrowthFactor < 0) || (health > MIN_HEALTH && pGrowthFactor > 0) ) health += pGrowthFactor * health;
            jumpForce += jumpForceFactor * Math.Sign(pGrowthFactor);
        }

        public static Player GetInstance()
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
