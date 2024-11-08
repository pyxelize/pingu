using Godot;
using System;
using System.Collections.Generic;

// Author : Julien Fournier
namespace Com.IsartDigital.ProjectName {
	
	public partial class Main : Node2D
	{

        #region GameObjects and paths

        [Export] CharacterBody2D player;
        [Export] PackedScene burger;
        [Export] PackedScene fish;
        [Export] PackedScene waterDrop;


        [Export] Node2D burgerContainer;
        [Export] Node2D fishContainer;
        [Export] Marker2D waterSpawner;
        [Export] Camera2D camera;

        const string ANIM_PATH = "anim";
        const string ANIM_WALK = "walk";
        const string ANIM_IDLE = "idle";

        AnimatedSprite2D anim;

        Timer rain = new Timer();
        [Export] float timeBetweenDropSpawn = 0.05f; 

        #endregion

        #region PLAYER MOVEMENTS
        // Toujours exporter des variables de tweak dans l'inspecteur. Les Game designers vous en remercieront
        [Export] float movespeed = 500f;
        [Export] float gravity = 90f;
        [Export] float minFallForce = 5f;
        [Export] float maxFallForce = 1000f;
        [Export] float jumpForce = 1300f;
        [Export] float jumpForceFactor = 200f;

        Vector2 direction;

        #endregion

        #region PLAYER GROW AND DIET

        [Export] float playerMaxScale = 3f;
        [Export] float playerMinScale;

        [Export] float growFactor = 0.05f;
        [Export] float dietFactor = 0.05f;

        #endregion

        #region INPUT (action map) NAMES
        //Ajouter dans l'input map les controls avec ces noms là. Attention à ne pas vous tromper
        //controles Q et D ou <- -> pour déplacement horizontal et Z ou flèche haut pour vertical
        // setup dans projet > param projet > controls > ajouter une action
        const string ACTION_MOVE_RIGHT = "MoveRight";
        const string ACTION_MOVE_LEFT = "MoveLeft";
        const string ACTION_JUMP = "Jump";

        #endregion

        #region HEALTH (jump and speed control)

        float health = 0.75f;
        const float MIN_HEALTH = 0.5f;
        const float MAX_HEALTH = 1f;

        #endregion

        #region COLLECTABLES

        List<Area2D> burgerList = new List<Area2D>();
        List<Area2D> fishList = new List<Area2D>();

        #endregion


        public override void _Ready()
		{
            anim = (AnimatedSprite2D)player.GetNode(ANIM_PATH);
            FromContainerIntoList(burgerContainer, burgerList);
            FromContainerIntoList(fishContainer, fishList);

            AddListener(burgerList, false);
            AddListener(fishList, true);

            rain.WaitTime = timeBetweenDropSpawn;
            rain.OneShot = false;
            rain.Autostart = true;
            rain.Timeout += SpawnWaterDrop;
            AddChild(rain);

            GD.Randomize();
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
        void PlayerMovements()
        {
            //gravity
            direction.Y += gravity;
            if (direction.Y > maxFallForce) direction.Y = maxFallForce;
            if (player.IsOnFloor()) direction = new Vector2(direction.X, 0f);

            // moving player on x axis
            direction.X = Input.GetActionStrength(ACTION_MOVE_RIGHT) - Input.GetActionStrength(ACTION_MOVE_LEFT);

            if (direction.X == -1) anim.FlipH = true;
            else if(direction.X == 1) anim.FlipH = false;

            // impact player speed (x axis) with health factor
            direction.X *= movespeed * health;

            PlayerJump();

            player.Velocity = direction; // Attribuer la direction actuelle à la propriété Velocity
            player.MoveAndSlide();
        }



        /// <summary>
        /// Capacity to jump
        /// </summary>
        void PlayerJump()
        {
            if (player.IsOnFloor() && Input.IsActionJustPressed(ACTION_JUMP)) direction.Y = -jumpForce * health;
        }

        /// <summary>
        /// Set player animation state
        /// </summary>
        void PlayerAnimation()
        {
            if (Input.IsActionPressed(ACTION_MOVE_LEFT) || Input.IsActionPressed(ACTION_MOVE_RIGHT))
            {
                if (anim.Animation != ANIM_WALK) anim.Animation = ANIM_WALK;
            }
            else
            {
                if (anim.Animation != ANIM_IDLE) anim.Animation = ANIM_IDLE;
            }
        }

        /// <summary>
        /// Add all Area2D elements from pContainer into pList
        /// </summary>
        /// <param name="pContainer">Container of AREA2D elements </param>
        /// <param name="pList">list that stock area2D elements</param>
        void FromContainerIntoList(Node2D pContainer, List<Area2D> pList)
        {
            foreach (Area2D item in pContainer.GetChildren())
            {
                pList.Add(item);
            }
        }

        /// <summary>
        /// Listen Signals of Area2D elements of pList
        /// </summary>
        /// <param name="pList">list of Area2D elements</param>
        /// <param name="pMethodName">method to execute when signal is shooted</param>
        /// <param name="pEventName">event type to listen</param>
        /// <param name="pIsFish">is the current collectable is fish</param>
        void AddListener(List<Area2D> pList, bool pIsFish)
        {
            foreach (Area2D item in pList)
            {
                item.BodyEntered += (Node2D body) => OnCollectableOverlap(body, item, pIsFish);
            }
        }


        /// <summary>
        /// Executed when an object overlap a collectable
        /// </summary>
        /// <param name="pBody">the object that collide with the collectable</param>
        /// <param name="pItem">the collectable item (fish or burger)</param>
        /// <param name="pIsFish">bool true if the Collectable is fish</param>
        void OnCollectableOverlap(Node pBody, Area2D pItem, bool pIsFish)
        {
            if (pBody != player) return;

            pItem.QueueFree();

            if (pIsFish)
            {
                if (anim.Scale.X > playerMinScale) anim.Scale -= new Vector2(dietFactor, dietFactor);
                if (health < MAX_HEALTH) health += dietFactor * health;
                jumpForce += jumpForceFactor;
            }
            else
            {
                if (anim.Scale.X < playerMaxScale) anim.Scale += new Vector2(growFactor, growFactor);
                if (health > MIN_HEALTH) health -= growFactor * health;
                jumpForce -= jumpForceFactor;
            }
        }

        void SpawnWaterDrop()
        {
            RigidBody2D rigidBody2D = waterDrop.Instantiate() as RigidBody2D;

            Vector2 viewportSize = GetViewportRect().Size;

            // Calculer une position X aléatoire dans la zone visible de la caméra
            float minX = camera.GlobalPosition.X - viewportSize.X * camera.Zoom.X;
            float maxX = camera.GlobalPosition.X + viewportSize.X * camera.Zoom.X;
            float randomX = GD.Randf() * (maxX - minX) + minX;

            // Positionner la goutte d'eau avec la position Y du spawner et la position X aléatoire
            rigidBody2D.Position = new Vector2(randomX, waterSpawner.Position.Y);

            Timer waterTimer = new Timer();
            waterTimer.OneShot = true;
            waterTimer.WaitTime = 5f;
            waterTimer.Timeout += () => rigidBody2D.QueueFree();

            rigidBody2D.AddChild(waterTimer);
            AddChild(rigidBody2D);
            waterTimer.Start();
        }
	}
}
