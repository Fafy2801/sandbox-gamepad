using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public class MoveController : WalkController
{
	public MoveController()
	{
		Duck = new Duck( this );
		Unstuck = new Unstuck( this );
	}

	public override void BuildInput( InputBuilder input )
	{
		var player = Local.Pawn as SandboxPlayer;
		if ( player == null || !player.SpawnMenuOpened )
		{
			base.BuildInput( input );
			return;
		}

		// Why does Math.Round return double ???
		int deltaV = MathX.FloorToInt( input.AnalogMove.x + 0.5f );
		int deltaH = MathX.FloorToInt( input.AnalogMove.y + 0.5f );

		var instance = SpawnMenu.Instance;
		switch( SpawnMenu.ActiveSection ){
			case SpawnMenu.MenuSection.Props:
				MoveGrid( instance.Spawns, deltaH, deltaV );
				break;
			case SpawnMenu.MenuSection.Entities:
				MoveGrid( instance.Entities, deltaH, deltaV );
				break;
			case SpawnMenu.MenuSection.Tools:
				if ( deltaV > 0 && !MovedForward )
					instance.ToolSelect( Math.Clamp( instance.ToolSelected - 1, 0, instance.ToolButtons.Count - 1 ) );
				else if ( deltaV < 0 && !MovedBack )
					instance.ToolSelect( Math.Clamp( instance.ToolSelected + 1, 0, instance.ToolButtons.Count - 1 ) );
				break;
		}

		if ( input.Pressed( InputButton.Jump ))
			switch ( SpawnMenu.ActiveSection )
			{
				case SpawnMenu.MenuSection.Props:
					var mdl = instance.Spawns.PanelData[instance.Spawns.Selected];
					if ( mdl != null )
						ConsoleSystem.Run( "spawn", "models/" + mdl );
					break;
				case SpawnMenu.MenuSection.Entities:
					var ent = instance.Entities.PanelData[instance.Entities.Selected];
					if ( ent != null )
						ConsoleSystem.Run( "spawn_entity", ent );
					break;
				case SpawnMenu.MenuSection.Tools:
					ConsoleSystem.Run( "inventory_current", "weapon_tool" );
					break;

			}

		// This prevents infinite movement
		MovedLeft = deltaH > 0;
		MovedRight = deltaH < 0;
		MovedForward = deltaV > 0;
		MovedBack = deltaV < 0;

		// And this is so we don't move in the world
		bool menu = input.Down( InputButton.Menu );
		input.Clear();
		input.SetButton( InputButton.Menu, menu );
	}

	// Spawn menu navigation
	public bool MovedLeft { get; set; } = false;
	public bool MovedRight { get; set; } = false;
	public bool MovedForward { get; set; } = false;
	public bool MovedBack { get; set; } = false;

	public void MoveGrid(NavigatableGrid grid, int deltaH, int deltaV)
	{
		// input.Pressed would work for keyboard, but not for joystick
		if ( !MovedLeft && deltaH > 0 )
			grid.SwitchHorizontal( false );
		else if ( !MovedRight && deltaH < 0 )
			grid.SwitchHorizontal( true );
		else if ( !MovedForward && deltaV > 0 )
			grid.SwitchVertical( false );
		else if ( !MovedBack && deltaV < 0 )
			grid.SwitchVertical( true );
	}
}
