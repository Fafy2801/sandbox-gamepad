using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public class FirstPersonAimController : FirstPersonCamera
{
	public override void BuildInput( InputBuilder input )
	{
		if ( !input.UsingMouse )
			input.AnalogLook *= Time.Delta * 130f;

		base.BuildInput( input );
	}
}

public class ThirdPersonAimController : ThirdPersonCamera
{
	public override void BuildInput( InputBuilder input )
	{
		if ( !input.UsingMouse )
			input.AnalogLook *= Time.Delta * 130f;

		base.BuildInput( input );
	}
}
