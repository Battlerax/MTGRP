//Spawn Sequence Camera Positions & Rotations
var spawn_sequence_campos = [
    new Vector3(-2015.038, -1242.178, 35.5102), new Vector3(-1956.667, -1235.156, 32.49281),
    new Vector3(-379.8545, -2652.042, 118.9725), new Vector3(-382.2806, -2585.594, 112.1685),
    new Vector3(425.1143, -665.7761, 33.75719), new Vector3(425.1143, -665.7761, 33.75719),
    new Vector3(402.1645, -679.8258, 32.45131), new Vector3(402.1645, -679.8258, 32.45131),
    new Vector3(424.0868, -672.4219, 28.95000), new Vector3(424.0868, -672.4219, 28.95000),
    new Vector3(424.0948, -669.6714, 29.85921), new Vector3(423.974, -662.6024, 29.5749)
];

var spawn_sequence_camrot = [
    new Vector3(0, 0, -83.11333), new Vector3(0, 0, -83.11333),
    new Vector3(0, 0, 0), new Vector3(0, 0, 0),
    new Vector3(0, 0, -40.47105), new Vector3(0, 0, -40.47105),
    new Vector3(0, 0, -68.40357), new Vector3(0, 0, -68.40357),
    new Vector3(0, 0, -1.961894), new Vector3(0, 0, -1.961894),
    new Vector3(0, 0, 179.6339), new Vector3(0, 0, 179.6339)
];

//Introduction Camera Shots
var camera_pos = [
/*Sky view*/new Vector3(-1223.413, -1273.963, 69.314), new Vector3(-1188.292, -1205.442, 62.52615),
/*BUSSTATION*/ new Vector3(406.1239, -657.3362, 32.72522), new Vector3(412.9145, -652.224, 31.76311),
/*LSPD*/ new Vector3(406.311, -960.8304, 35.46336), new Vector3(409.0441, -963.017, 35.03337),
/*BANK*/ new Vector3(206.9372, 219.2671, 109.7756), new Vector3(216.2076, 196.4482, 111.3628),
/*24/7*/ new Vector3(-3222.568, 985.7988, 16.21917), new Vector3(-3229.906, 991.8315, 13.72657),
/*VEHDEALER*/ new Vector3(-93.74094, -1120.287, 29.32643), new Vector3(-87.93485, -1118.774, 26.5688),
/*Pier*/ new Vector3(-1901.026, -1225.53, 20.35778), new Vector3(-1845.703, -1277.283, 20.56891),
/*WOODS*/ new Vector3(-550.3875, 5571.181, 66.9862), new Vector3(-574.6476, 5540.013, 61.51825),
/*SHIPWRECK*/ new Vector3(-125.6273, -2316.191, -9.032106), new Vector3(-148.8436, -2325.469, -10.60085),
/*SkyView2*/ new Vector3(-2221.575, 415.6908, 232.9723), new Vector3(-2101.799, 350.2302, 214.9161),
/*SkyView3*/ new Vector3(-35.09233, -1821.837, 42.83126), new Vector3(30.9814, -1879.182, 37.73874),
/*SkyView4*/ new Vector3(412.3697, -638.3072, 35.48744), new Vector3(421.9597, -641.1315, 32.57067)
];

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
/*SkyView4*/ new Vector3(0, 0, -129.2393), new Vector3(0, 0, -112.0678)
];

var during_intro_pos = new Vector3(8.87535, 528.9718, 170.635);

//Current Index
var current_cam_index = 0;
var spawn_cam_index = 0;
var spawn_cam_entity = 0;
var current_text_index = -1;
var showtext = false;
var isonintro = false;

var vehicle = null;

//Drawtext for each shot
var shot_text = [
    "Welcome to ~r~Moving Target Gaming~w~ Roleplay.\nBefore you take control of the character you created,\nlet us show you the basics.",
    "This is the ~b~Dashhound bus station~w~.\nYou will begin your journey here in the city of Los Santos.\nIt is located near some important locations.",
    "The ~b~Los Santos Police Department~w~ is where\npolice officers take charge of the streets.\nYou can join them by applying on the forums.",
    "This is one of the many banks found in Los Santos.\nHere you can withdraw and deposit money into your bank account.\n~g~ATMs~w~ can be used for the same.",
    "There are a number of businesses scattered around the city.\nBuy the essentials at a ~g~24/7~w~ or\ngrab a bite to eat at a local ~g~restaurant~w~.",
    "This is the ~g~vehicle dealership.~w~\nBuy your very first car here or save up\nfor a more luxurious car later.",
    "There are many ways to make money.\nYou can choose from the many exciting jobs\nsuch as ~y~fisherman~w~ or ~y~trucker~w~, or find your own means of making money.",
    "You can even grab a rifle and hunting license and see how you fare\nagainst nature itself in the hunting grounds of ~b~Paleto Bay~w~.",
    "If that isn't of interest, grab a scuba diving \nkit from a ~g~hardware store~w~ and hit the waters.\nRumor has it there are ocean wrecks filled with ~b~treasure~w~.",
    "That's all we have to show you for now.\nThe development team is ~b~always~w~ expanding the\nserver~w~ with more exciting new features.",
    "Be sure to check out our forums and\nbecome a member of our community at\n~r~www.mt-gaming.com~w~.",
    "If you need further help, use ~g~/n~w~ chat to\nget help from players or ~g~/ask~w~ to get\nhelp from a moderator directly.\nNow, get going! ~b~Los Santos~w~ isn't going to run itself."
];

Event.OnServerEventTrigger.connect(function (eventName, args) {
    if (eventName === "start_introduction") {
		//API.setCefDrawState(false);
		isonintro = true;
		API.setHudVisible(false);
		API.setChatVisible(false);
        API.startMusic("audio_resources/introduction.mp3", true);
		spawn_sequence_start();
	}

	if (eventName === "stop_introduction"){
		stop_introduction();
	}
});

function entity_spawn(type) {
	switch(type){
		case 0:
			bus_driving_bridge();
			break;
		case 2:
			bus_driving_station();
			break;
		case 4:
			player_exiting_bus();
			break;	
	}
}

function bus_driving_bridge(){
	vehicle = API.createVehicle(-713569950, new Vector3(-276.1117, -2411.626, 59.68943), new Vector3(0, 0, 53.19402));
	API.setPlayerIntoVehicle(vehicle, -1);
	API.setVehicleEngineStatus(vehicle, true);
	API.callNative("TASK_VEHICLE_DRIVE_TO_COORD", API.getLocalPlayer(), vehicle, -582.3301, -2201.367, 56.25008, 120.0, 1.0, -713569950, 16777216, 1.0, true);
                    
}

function bus_driving_station(){
	vehicle = API.createVehicle(-713569950, new Vector3(513.3119, -676.2706, 25.19653), new Vector3(0, 0, 85.25442));
	API.setPlayerIntoVehicle(vehicle, -1);
	API.setVehicleEngineStatus(vehicle, true);
	API.callNative("TASK_VEHICLE_DRIVE_TO_COORD", API.getLocalPlayer(), vehicle, 464.645, -673.3629, 27.20791, 10.0, 1.0, -713569950, 16777216, 1.0, true);           
}

function player_exiting_bus(){
	vehicle = API.createVehicle(-713569950, new Vector3(429.8345, -672.5932, 29.05217), new Vector3(0.9295838, 3.945374, 90.3828));
	API.setPlayerIntoVehicle(vehicle, -1);
	API.setVehicleEngineStatus(vehicle, true);
	API.callNative("TASK_LEAVE_VEHICLE", API.getLocalPlayer(), vehicle, 0);
}

var timer = 0;
function spawn_sequence_start() {
	entity_spawn(spawn_cam_entity);
	var current_cam = API.createCamera(spawn_sequence_campos[spawn_cam_index], spawn_sequence_camrot[spawn_cam_index]);
	API.setActiveCamera(current_cam);
    set_focus(spawn_sequence_campos[spawn_cam_index]);
    /*if (spawn_cam_index < spawn_sequence_campos.length - 1) {
        set_focus(spawn_sequence_campos[spawn_cam_index + 1]);
    }*/
	spawn_cam_index++;
	var next_cam = API.createCamera(spawn_sequence_campos[spawn_cam_index], spawn_sequence_camrot[spawn_cam_index]);
	spawn_cam_index++;
	spawn_cam_entity++;
	API.interpolateCameras(current_cam, next_cam, 3000, false, false);
	timer = Date.now() + 3000;
}

function next_camera() {
	if (current_cam_index >= camera_pos.length){
		stop_introduction();
		return;
	}
	API.setEntityDimension(API.getLocalPlayer(), 0);
	API.setEntityPosition(API.getLocalPlayer(), new Vector3(433.2354, -645.8408, 28.72639));
	showtext = true;
	var current_cam = API.createCamera(camera_pos[current_cam_index], camera_rot[current_cam_index]);
	API.setActiveCamera(current_cam);
	current_cam_index++;
	var next_cam = API.createCamera(camera_pos[current_cam_index], camera_rot[current_cam_index]);
    set_focus(camera_pos[current_cam_index]);
    /*if (current_cam_index < camera_pos.length - 1) {
        set_focus(camera_pos[current_cam_index + 1]);
    }*/
	current_cam_index++;
	current_text_index++;
	API.interpolateCameras(current_cam, next_cam, 11000, false, false);
	timer = Date.now() + 11000;

}

function set_focus(vect){
    API.callNative("_SET_FOCUS_AREA", vect.X, vect.Y, vect.Z, 0.0, 0.0, 0.0); //SET FOCUS
}

function stop_introduction(){
	//API.setCefDrawState(true);
	isonintro = false;
	showtext = false;
	API.setHudVisible(true);
	API.setChatVisible(true);
    API.stopMusic();
	API.setActiveCamera(null);
    API.callNative("CLEAR_FOCUS"); //RESET FOCUS
	current_cam_index = 0;
	spawn_cam_index = 0;
	API.triggerServerEvent("finish_intro");
}

Event.OnUpdate.connect(function () {

	if (timer < Date.now() && timer != 0){
		timer = 0;
		if (spawn_cam_index < spawn_sequence_campos.length){
			spawn_sequence_start();
		}
		else {
			next_camera();
		}
	}

	if (isonintro == true){
	API.disableAllControlsThisFrame();
	}

	if (showtext == true) {
	API.drawText(shot_text[current_text_index], API.getScreenResolutionMaintainRatio().Width/2 - 900, API.getScreenResolutionMaintainRatio().Height/2 + 200, 1, 255, 255, 255, 255, 6, 0, true, true, 0);
	}

});