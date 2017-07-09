//Spawn Sequence Camera Positions & Rotations
var spawn_sequence_campos = [new Vector3(-2015.038, -1242.178, 35.5102), new Vector3(-1956.667, -1235.156, 32.49281), 
new Vector3(-379.8545, -2652.042, 118.9725), new Vector3(-382.2806, -2585.594, 112.1685),
new Vector3(425.1143, -665.7761, 33.75719), new Vector3(425.1143, -665.7761, 33.75719),
new Vector3(402.1645, -679.8258, 32.45131), new Vector3(402.1645, -679.8258, 32.45131),
new Vector3(424.0868, -672.4219, 28.90000), new Vector3(424.0868, -672.4219, 28.90000),
new Vector3(424.0948, -669.6714, 29.85921), new Vector3(423.974, -662.6024, 29.5749)]

var spawn_sequence_camrot = [new Vector3(0, 0, -83.11333), new Vector3(0, 0, -83.11333), 
new Vector3(0, 0, 0), new Vector3(0, 0, 0), 
new Vector3(0, 0, -40.47105), new Vector3(0, 0, -40.47105),
new Vector3(0, 0, -68.40357), new Vector3(0, 0, -68.40357),
new Vector3(0, 0, -1.961894), new Vector3(0, 0, -1.961894),
new Vector3(0, 0, 179.6339), new Vector3(0, 0, 179.6339)]

//Introduction Camera Shots
var camera_pos = [
/*Sky view*/new Vector3(-1223.413, -1273.963, 69.314), new Vector3(-1188.292, -1205.442, 62.52615),
/*BUSSTATION*/ new Vector3(406.1239, -657.3362, 32.72522), new Vector3(412.9145, -652.224, 31.76311),
/*LSPD*/ new Vector3(406.311, -960.8304, 35.46336), new Vector3(409.0441, -963.017, 35.03337),
/*BANK*/ new Vector3(206.9372, 219.2671, 109.7756), new Vector3(216.2076, 196.4482, 111.3628),
/*24/7*/ new Vector3(-3222.568, 985.7988, 20.21917), new Vector3(-3229.906, 991.8315, 17.72657),
/*VEHDEALER*/  new Vector3(-93.74094, -1120.287, 38.32643), new Vector3(-87.93485, -1118.774, 36.5688),
/*Pier*/ new Vector3(-1901.026, -1225.53, 20.35778), new Vector3(-1845.703, -1277.283, 20.56891),
/*WOODS*/ new Vector3(-550.3875, 5571.181, 66.9862), new Vector3(-574.6476, 5540.013, 61.51825),
/*SHIPWRECK*/ new Vector3(-125.6273, -2316.191, -9.032106), new Vector3(-148.8436, -2325.469, -10.60085),
/*SkyView2*/ new Vector3(-2221.575, 415.6908, 232.9723), new Vector3(-2101.799, 350.2302, 214.9161),
/*SkyView3*/ new Vector3(-35.09233, -1821.837, 42.83126), new Vector3(30.9814, -1879.182, 37.73874),
/*SkyView4*/ new Vector3(2165.094, 2090.795, 156.4328), new Vector3(2151.098, 2122.872, 151.6403)
]

var camera_rot = [
/*SkyView1*/new Vector3(0, 0, -27.13915), new Vector3(0, 0, -27.13915),
/*BUSSTATION*/ new Vector3(0, 0, -47.28222), new Vector3(0, 0, -47.28222),
/*LSPD*/new Vector3(0, 0, -128.6599), new Vector3(0, 0, -128.6599),
/*BANK*/ new Vector3(0, 0, -85.50804), new Vector3(0, 0, -50.79863),
/*24/7*/ new Vector3(0, 0, 49.99916), new Vector3(0, 0, 49.99916),
/*VEHDEALER*/ new Vector3(0, 0, -75.39452), new Vector3(0, 0, -75.39452),
/*Pier*/new Vector3(0, 0, -73.99986), new Vector3(0, 0, -17.86307),
/*WOODS*/ new Vector3(0, 0, 142.1026), new Vector3(0, 0, 142.1026),
/*SHIPWRECK*/ new Vector3(0, 0, 111.7729), new Vector3(0, 0, 111.7729),
/*SkyView2*/ new Vector3(0, 0, -118.7154), new Vector3(0, 0, -118.7154),
/*SkyView3*/ new Vector3(0, 0, -130.9785), new Vector3(0, 0, -130.9785),
/*SkyView4*/ new Vector3(0, 0, 23.58142), new Vector3(0, 0, 23.58142)
]

//Current Index
var current_cam_index = 0;
var spawn_cam_index = 0;
var spawn_cam_entity = 0;
var current_text_index = 0;
var showtext = false;
var isonintro = false;

//Drawtext for each shot
var shot_text = [
"Welcome to Moving Target Gaming Roleplay.\nOn our server you can turn your dreams into reality\nas you take control of a character you create.\nLet us show you the basics.", 
"This is the Dashhound bus station and where\nyou will begin your journey in the city of Los Santos.\nThe bus station is located near some important locations.",
"The Los Santos Police headquarters is just down the road.\nThis is where police officers take charge of the\nstreets of Los Santos in an attempt to stamp out crime.\nYou can join them by applying on the forums or give\nthem a run for their money as a criminal.",
"This is one of the many banks found in Los Santos.\nHere you can withdraw and deposit money into your personal\naccount as well as wire transfer money to other players.\nYou can also manage your account from ATMs\nlocated throughout the city.",
"There are a number of businesses scattered around the city.\nBuy the essentials at a 24/7 or grab a bite to eat at a local restaurant.",
"This is the Motorsport Vehicle Dealership. Here you can buy your very\nfirst car right from the start or save up your money\nand buy a more luxerious car later.\nThere are lots of unoccupied cars throughout Los Santos waiting to be hotwired.",
"These are some of the many jobs located throughout the state of San Andreas.\nYou can hit the waters for fish, grab a hatchet\nand chop some trees, collect trash, and many other exciting jobs.", 
"You can even grab a rifle and hunting license and see how you fare\nagainst nature itself in the hunting grounds of Paleto Bay.",
"If none of that gets you excited, grab a scuba diving kit from a\nhardware store and hit the waters of Los Santos.\nRumor has it there are ocean wrecks filled with treasure.",
"That's all we have to show you for now but the development team is\nalways expanding the server to have exciting new features.", 
"Make sure to check out our forums and become apart of the community at www.mt-gaming.com.",
"If you need further help use /n(ewbie) chat to get help from players\nor /ask to get help from a moderator directly.\nNow, get going! Los Santos isn't going to run itself."
]

API.onServerEventTrigger.connect(function (eventName, args) {
    if (eventName == "start_introduction") {
		isonintro = true;
		API.setHudVisible(false);
		API.setChatVisible(false);
		API.startAudio("audio_resources/introduction.mp3", true);
		spawn_sequence_start();
	}

	if (eventName == "stop_introduction"){
		stop_introduction();
	}
});

function entity_spawn(type) {
    var player = API.getLocalPlayer();
	switch(type){
		case 0:
			API.triggerServerEvent("bus_driving_bridge");
			break;
		case 2:
			API.triggerServerEvent("bus_driving_station");
			break;
		case 4:
			API.triggerServerEvent("player_exiting_bus");
			break;	
	}
}

function spawn_sequence_start() {
	while (spawn_cam_index < spawn_sequence_campos.length){
		entity_spawn(spawn_cam_entity);
		var current_cam = API.createCamera(spawn_sequence_campos[spawn_cam_index], spawn_sequence_camrot[spawn_cam_index]);
		API.setActiveCamera(current_cam);
		set_focus(spawn_sequence_campos[spawn_cam_index]);
		spawn_cam_index++;
		var next_cam = API.createCamera(spawn_sequence_campos[spawn_cam_index], spawn_sequence_camrot[spawn_cam_index]);
		spawn_cam_index++;
		spawn_cam_entity++;
		API.interpolateCameras(current_cam, next_cam, 3000, false, false);
		API.sleep(3000);
	}

	next_camera();
	API.setEntityDimension(API.getLocalPlayer(), 0);
}

function next_camera() {
	showtext = true;
	while (current_cam_index < camera_pos.length){
		var current_cam = API.createCamera(camera_pos[current_cam_index], camera_rot[current_cam_index]);
		API.setActiveCamera(current_cam);
		current_cam_index++;
		var next_cam = API.createCamera(camera_pos[current_cam_index], camera_rot[current_cam_index]);
		set_focus(camera_pos[current_cam_index]);
		current_cam_index++;
		current_text_index++;
		API.interpolateCameras(current_cam, next_cam, 10000, false, false);
		API.sleep(10000);
	}

	stop_introduction();
}

function set_focus(vect){
	API.callNative("0xBB7454BAFF08FE25", vect.X, vect.Y, vect.Z, 0.0, 0.0, 0.0); //SET FOCUS
}

function stop_introduction(){
	isonintro = false;
	showtext = false;
	API.setHudVisible(true);
	API.setChatVisible(true);
	API.stopAudio();
	API.setActiveCamera(null);
	API.callNative("0x31B73D1EA9F01DA2"); //RESET FOCUS
	current_cam_index = 0;
	spawn_cam_index = 0;
	API.triggerServerEvent("finish_intro");
}


API.onUpdate.connect(function () {

	if (isonintro == true){
	API.disableAllControlsThisFrame();
	}

	if (showtext == true){
		API.drawText(shot_text[spawn_cam_index], API.getScreenResolutionMantainRatio().Width - 15, 160, 0.5, 96, 183, 255, 255, 0, 2, true, true, 0);
	}

});