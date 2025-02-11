﻿using Sandbox;

partial class SandboxPlayer : Player
{
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceJumpReleased;

	private DamageInfo lastDamage;

	[Net] public PawnController VehicleController { get; set; }
	[Net] public PawnAnimator VehicleAnimator { get; set; }
	[Net, Predicted] public ICamera VehicleCamera { get; set; }
	[Net, Predicted] public Entity Vehicle { get; set; }
	[Net, Predicted] public ICamera MainCamera { get; set; }

	public bool SpawnMenuOpened { get; set; } = false;

	public ICamera LastCamera { get; set; }


	/// <summary>
	/// The clothing container is what dresses the citizen
	/// </summary>
	public Clothing.Container Clothing = new();

	/// <summary>
	/// Default init
	/// </summary>
	public SandboxPlayer()
	{
		Inventory = new Inventory( this );
	}

	/// <summary>
	/// Initialize using this client
	/// </summary>
	public SandboxPlayer( Client cl ) : this()
	{
		// Load clothing from client data
		Clothing.LoadFromClient( cl );
	}

	public override void Spawn()
	{
		MainCamera = new FirstPersonAimController();
		LastCamera = MainCamera;

		base.Spawn();
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new MoveController();
		Animator = new StandardPlayerAnimator();

		MainCamera = LastCamera;
		Camera = MainCamera;

		if ( DevController is NoclipController )
		{
			DevController = null;
		}

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Clothing.DressEntity( this );

		Inventory.Add( new PhysGun(), true );
		Inventory.Add( new GravGun() );
		Inventory.Add( new Tool() );
		Inventory.Add( new Pistol() );
		Inventory.Add( new Flashlight() );

		base.Respawn();
	}

	public override void OnKilled()
	{
		base.OnKilled();

		if ( lastDamage.Flags.HasFlag( DamageFlags.Vehicle ) )
		{
			Particles.Create( "particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position );
			Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
			PlaySound( "kersplat" );
		}

		VehicleController = null;
		VehicleAnimator = null;
		VehicleCamera = null;
		Vehicle = null;

		BecomeRagdollOnClient( Velocity, lastDamage.Flags, lastDamage.Position, lastDamage.Force, GetHitboxBone( lastDamage.HitboxIndex ) );
		LastCamera = MainCamera;
		MainCamera = new SpectateRagdollCamera();
		Camera = MainCamera;
		Controller = null;

		EnableAllCollisions = false;
		EnableDrawing = false;

		Inventory.DropActive();
		Inventory.DeleteContents();
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( GetHitboxGroup( info.HitboxIndex ) == 1 )
		{
			info.Damage *= 10.0f;
		}

		lastDamage = info;

		TookDamage( lastDamage.Flags, lastDamage.Position, lastDamage.Force );

		base.TakeDamage( info );
	}

	[ClientRpc]
	public void TookDamage( DamageFlags damageFlags, Vector3 forcePos, Vector3 force )
	{
	}

	public override PawnController GetActiveController()
	{
		if ( VehicleController != null ) return VehicleController;
		if ( DevController != null ) return DevController;

		return base.GetActiveController();
	}

	public override PawnAnimator GetActiveAnimator()
	{
		if ( VehicleAnimator != null ) return VehicleAnimator;

		return base.GetActiveAnimator();
	}

	public ICamera GetActiveCamera()
	{
		if ( VehicleCamera != null ) return VehicleCamera;

		return MainCamera;
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
			return;

		if ( VehicleController != null && DevController is NoclipController )
		{
			DevController = null;
		}

		var controller = GetActiveController();
		if ( controller != null )
			EnableSolidCollisions = !controller.HasTag( "noclip" );

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );

		if ( Input.Pressed( InputButton.View ) )
		{
			if ( MainCamera is not FirstPersonAimController )
			{
				MainCamera = new FirstPersonAimController();
			}
			else
			{
				MainCamera = new ThirdPersonAimController();
			}
		}

		Camera = GetActiveCamera();

		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();
			if ( dropped != null )
			{
				dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRot.Forward * 500.0f + Vector3.Up * 100.0f, true );
				dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				timeSinceDropped = 0;
			}
		}

		if ( Input.Released( InputButton.Jump ) )
		{
			if ( timeSinceJumpReleased < 0.3f )
			{
				Game.Current?.DoPlayerNoclip( cl );
			}

			timeSinceJumpReleased = 0;
		}

		if ( Input.Left != 0 || Input.Forward != 0 )
		{
			timeSinceJumpReleased = 1;
		}

		if ( Input.Pressed( InputButton.Menu ) )
		{
			SpawnMenuOpened = !SpawnMenuOpened;
			SpawnMenu.Instance?.Parent.SetClass( "spawnmenuopen", SpawnMenuOpened );
		}
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );
	}

	[ServerCmd( "inventory_current" )]
	public static void SetInventoryCurrent( string entName )
	{
		var target = ConsoleSystem.Caller.Pawn;
		if ( target == null ) return;

		var inventory = target.Inventory;
		if ( inventory == null )
			return;

		for ( int i = 0; i < inventory.Count(); ++i )
		{
			var slot = inventory.GetSlot( i );
			if ( !slot.IsValid() )
				continue;

			if ( !slot.ClassInfo.IsNamed( entName ) )
				continue;

			inventory.SetActiveSlot( i, false );

			break;
		}
	}

	[ServerCmd( "inventorynext_sv" )]
	public static void InventoryNextCMD()
	{
		var target = ConsoleSystem.Caller.Pawn as SandboxPlayer;
		if ( target == null )
			return;

		target.Inventory?.SwitchActiveSlot( 1, true );
	}

	[ClientCmd( "inventorynext" )]
	public static void SpawnMenuNextPredicted()
	{
		var target = Local.Pawn as SandboxPlayer;
		if ( target == null )
			return;

		if ( !target.SpawnMenuOpened )
			ConsoleSystem.Run( "inventorynext_sv" );
		else
		{
			if ( SpawnMenu.ActiveSection == SpawnMenu.MenuSection.Props )
			{
				SpawnMenu.Instance.Spawns.SetClass( "active", false );
				SpawnMenu.Instance.Tabs[0].SetClass( "active", false );
				SpawnMenu.Instance.Entities.SetClass( "active", true );
				SpawnMenu.Instance.Tabs[1].SetClass( "active", true );
				SpawnMenu.ActiveSection = SpawnMenu.MenuSection.Entities;
				SpawnMenu.Instance.Tabs[2].SetClass( "active", false );
			} else if ( SpawnMenu.ActiveSection == SpawnMenu.MenuSection.Entities )
			{
				SpawnMenu.Instance.Tabs[0].SetClass( "active", false );
				SpawnMenu.Instance.Tabs[1].SetClass( "active", false );
				SpawnMenu.ActiveSection = SpawnMenu.MenuSection.Tools;
				SpawnMenu.Instance.Tabs[2].SetClass( "active", true );
			}
		}
			
	}

	[ServerCmd( "inventoryprev_sv" )]
	public static void InventoryPrevCMD()
	{
		var target = ConsoleSystem.Caller.Pawn as SandboxPlayer;
		if ( target == null )
			return;

		target.Inventory?.SwitchActiveSlot( -1, true );
	}

	[ClientCmd( "inventoryprev" )]
	public static void SpawnMenuPrevPredicted()
	{
		var target = Local.Pawn as SandboxPlayer;
		if ( target == null )
			return;

		if ( !target.SpawnMenuOpened )
			ConsoleSystem.Run( "inventoryprev_sv" );
		else
		{
			if ( SpawnMenu.ActiveSection == SpawnMenu.MenuSection.Entities )
			{
				SpawnMenu.Instance.Spawns.SetClass( "active", true );
				SpawnMenu.Instance.Tabs[0].SetClass( "active", true );
				SpawnMenu.Instance.Entities.SetClass( "active", false );
				SpawnMenu.Instance.Tabs[1].SetClass( "active", false );
				SpawnMenu.ActiveSection = SpawnMenu.MenuSection.Props;
			} else if ( SpawnMenu.ActiveSection == SpawnMenu.MenuSection.Tools )
			{
				SpawnMenu.Instance.Tabs[0].SetClass( "active", false );
				SpawnMenu.Instance.Tabs[1].SetClass( "active", true );
				SpawnMenu.ActiveSection = SpawnMenu.MenuSection.Entities;
				SpawnMenu.Instance.Tabs[2].SetClass( "active", false );
			}
		}

	}

	// TODO

	//public override bool HasPermission( string mode )
	//{
	//	if ( mode == "noclip" ) return true;
	//	if ( mode == "devcam" ) return true;
	//	if ( mode == "suicide" ) return true;
	//
	//	return base.HasPermission( mode );
	//	}
}
