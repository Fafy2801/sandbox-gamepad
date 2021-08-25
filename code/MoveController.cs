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

		// input.Pressed would work for keyboard, but not for joystick
		if ( !MovedLeft && deltaH > 0 )
			SpawnMenu.Instance.Spawns.SwitchHorizontal( false );
		else if ( !MovedRight && deltaH < 0 )
			SpawnMenu.Instance.Spawns.SwitchHorizontal( true );
		else if ( !MovedForward && deltaV > 0 )
			SpawnMenu.Instance.Spawns.SwitchVertical( false );
		else if ( !MovedBack && deltaV < 0 )
			SpawnMenu.Instance.Spawns.SwitchVertical( true );

		// This prevents infinite movement
		MovedLeft = deltaH > 0;
		MovedRight = deltaH < 0;
		MovedForward = deltaV > 0;
		MovedBack = deltaV < 0;

		bool menu = input.Down( InputButton.Menu );
		input.Clear();
		input.SetButton( InputButton.Menu, menu );
	}

	// Spawn menu navigation
	public bool MovedLeft { get; set; } = false;
	public bool MovedRight { get; set; } = false;
	public bool MovedForward { get; set; } = false;
	public bool MovedBack { get; set; } = false;
}
