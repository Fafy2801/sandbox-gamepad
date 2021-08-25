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
		/*var player = Local.Client.Pawn as SandboxPlayer;
		if ( player.SpawnMenuOpened )
		{
			bool menu = input.Down( InputButton.Menu );
			input.Clear();
			input.SetButton( InputButton.Menu, menu );
		}
		else*/
			base.BuildInput( input );
	}
}
