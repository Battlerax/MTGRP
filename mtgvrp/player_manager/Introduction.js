//Spawn Sequence Camera Positions & Rotations
var spawn_sequence_campos = [new Vector3(-379.8545, -2652.042, 118.9725), new Vector3(-382.2806, -2585.594, 112.1685), 
new Vector3(459.716, -674.2458, 27.24684), new Vector3(428.7231, -674.6559, 29.15647)]
var spawn_sequence_camrot = [new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, -92.13364), new Vector3(0, 0, -92.13364)]

//Introduction Camera Shots
var camera_pos = []
var camera_rot = [new Vector3(0, 0, 0), new Vector3(0, 0, -92.13364)]

//Current Index
var current_cam_index = 0;
var spawn_cam_index = 0;

API.onServerEventTrigger.connect(function (eventName, args) {
    if (eventName == "start_introduction") {
		API.sendChatMessage("Starting introduction");
		API.setHudVisible(false);
		API.startAudio("audio_resources/introduction.mp3", true);
		spawn_sequence_start();
	}
});

function spawn_sequence_start() {
	while (spawn_cam_index < spawn_sequence_campos.length){
		API.triggerServerEvent("bus_driving_bridge");
		API.callNative("0xBB7454BAFF08FE25", -379.8545, -2652.042, 118.9725, 0.0, 0.0, 0.0); 
		var current_cam = API.createCamera(spawn_sequence_campos[spawn_cam_index], spawn_sequence_camrot[spawn_cam_index]);
		API.setActiveCamera(current_cam);
		spawn_cam_index++;
		var next_cam = API.createCamera(spawn_sequence_campos[spawn_cam_index], spawn_sequence_camrot[spawn_cam_index]);
		spawn_cam_index++;
		API.interpolateCameras(current_cam, next_cam, 5000, false, false);
		API.sleep(5000);
	}

	next_camera();
}

function next_camera() {
	while (current_cam_index < camera_pos.length){
		var current_cam = API.createCamera(camera_pos[current_cam_index], new Vector3(0, 0, 0));
		API.setActiveCamera(current_cam);
		current_cam_index++;
		var next_cam = API.createCamera(camera_pos[current_cam_index], new Vector3(0, 0, 0));
		current_cam_index++;
		API.interpolateCameras(current_cam, next_cam, 10000, false, false);
		API.sleep(10000);
	}

	stop_introduction();
}

function stop_introduction(){
	API.sendChatMessage("Stopping introduction");
	API.setHudVisible(true);
	API.stopAudio();
	API.setActiveCamera(null);
}


API.onUpdate.connect(function () {
	if (current_cam_index < camera_pos.length){
		API.disableAllControlsThisFrame();
	}
});
