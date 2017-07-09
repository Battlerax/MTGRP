
var FreeCam = null;
var normalspeed = 0.5;
var highspeed = 1.5;


API.onServerEventTrigger.connect(function(eventName, args) {
	if(eventName == "FreeCamEvent")
	{
		FreeCam = API.createCamera(args[0], new Vector3(0, 0, 0));
		API.setActiveCamera(FreeCam);
		API.setPlayerInvincible(true);
		API.setHudVisible(false);
		

	}
	if(eventName == "FreeCamEventStop")
	{
		var cameraPos = API.getCameraPosition(FreeCam);
		API.setEntityPosition(API.getLocalPlayer(), new Vector3(cameraPos.X, cameraPos.Y, cameraPos.Z+0.5));
		API.setActiveCamera(null);
		API.setPlayerInvincible(false);
		API.setHudVisible(true);
		FreeCam = null;


	}
});

API.onUpdate.connect(function() {
	if(FreeCam != null)
	{
		API.setCameraRotation(FreeCam, API.getGameplayCamRot());	
		var cameraRot = API.getCameraRotation(FreeCam);
		var cameraPos = API.getCameraPosition(FreeCam);
		API.setEntityPosition(API.getLocalPlayer(), new Vector3(cameraPos.X, cameraPos.Y, cameraPos.Z-2.5));
		API.setEntityRotation(API.getLocalPlayer(), new Vector3(0, 0, cameraRot.Z));
		var pi = 3.141592654
		var xradian = ((cameraRot.X*pi) / 180);
		var yradian = ((cameraRot.Z*pi) / 180);
		var zradian = ((cameraRot.Z*pi) / 180);
		if(API.isControlPressed(87)) // W button normal speed move straight
		{
			API.enableControlThisFrame(87);			
			var OldPos = API.getCameraPosition(FreeCam);
			var newx = -(Math.sin(yradian)*normalspeed);
			var newy = Math.cos(yradian)*normalspeed; 
			var newz = Math.sin(xradian)*normalspeed; // up or down					
			API.setCameraPosition(FreeCam, new Vector3(OldPos.X+newx, OldPos.Y+newy, OldPos.Z+newz));	

		}
		if(!API.isChatOpen())
		{
			if(API.isControlPressed(21)) // Left Shift button high speed move 
			{	
				if(API.isControlPressed(87)) // straight
				{
					API.enableControlThisFrame(21);	
					var OldPos = API.getCameraPosition(FreeCam);
					var newx = -(Math.sin(yradian)*highspeed);
					var newy = Math.cos(yradian)*highspeed; 
					var newz = Math.sin(xradian)*highspeed;						
					API.setCameraPosition(FreeCam, new Vector3(OldPos.X+newx, OldPos.Y+newy, OldPos.Z+newz));				
				}
				if(API.isControlPressed(268)) // back
				{
					API.enableControlThisFrame(87);			
					var OldPos = API.getCameraPosition(FreeCam);
					var newx = Math.sin(yradian)*highspeed;
					var newy = -(Math.cos(yradian)*highspeed); 
					var newz = -(Math.sin(xradian)*highspeed); // up or down					
					API.setCameraPosition(FreeCam, new Vector3(OldPos.X+newx, OldPos.Y+newy, OldPos.Z+newz));	

				}
				if(API.isControlPressed(35)) // Right
				{
					API.enableControlThisFrame(35);			
					var OldPos = API.getCameraPosition(FreeCam);
					var newx = Math.cos(yradian)*highspeed;
					var newy = Math.sin(yradian)*highspeed; 
					var newz = -(Math.sin(xradian)*highspeed); // up or down					
					API.setCameraPosition(FreeCam, new Vector3(OldPos.X+newx, OldPos.Y+newy, OldPos.Z+newz));	

				}
				if(API.isControlPressed(34)) // Left
				{
					API.enableControlThisFrame(34);			
					var OldPos = API.getCameraPosition(FreeCam);
					var newx = -(Math.cos(yradian)*highspeed);
					var newy = -(Math.sin(yradian)*highspeed); 
					var newz = Math.sin(xradian)*highspeed; // up or down					
					API.setCameraPosition(FreeCam, new Vector3(OldPos.X+newx, OldPos.Y+newy, OldPos.Z+newz));	

				}
			}
		}
		if(API.isControlPressed(268)) // S button normal speed move back
		{
			API.enableControlThisFrame(87);			
			var OldPos = API.getCameraPosition(FreeCam);
			var newx = Math.sin(yradian)*normalspeed;
			var newy = -(Math.cos(yradian)*normalspeed); 
			var newz = -(Math.sin(xradian)*normalspeed); // up or down					
			API.setCameraPosition(FreeCam, new Vector3(OldPos.X+newx, OldPos.Y+newy, OldPos.Z+newz));	

		}
		if(API.isControlPressed(35)) // D button normal speed move right
		{
			API.enableControlThisFrame(35);			
			var OldPos = API.getCameraPosition(FreeCam);
			var newx = Math.cos(yradian)*normalspeed;
			var newy = Math.sin(yradian)*normalspeed; 
			var newz = -(Math.sin(xradian)*normalspeed); // up or down					
			API.setCameraPosition(FreeCam, new Vector3(OldPos.X+newx, OldPos.Y+newy, OldPos.Z+newz));	

		}
		if(API.isControlPressed(34)) // A button normal speed move left
		{
			API.enableControlThisFrame(34);			
			var OldPos = API.getCameraPosition(FreeCam);
			var newx = -(Math.cos(yradian)*normalspeed);
			var newy = -(Math.sin(yradian)*normalspeed); 
			var newz = Math.sin(xradian)*normalspeed; // up or down					
			API.setCameraPosition(FreeCam, new Vector3(OldPos.X+newx, OldPos.Y+newy, OldPos.Z+newz));	

		}
	}



	
	// 
	
});