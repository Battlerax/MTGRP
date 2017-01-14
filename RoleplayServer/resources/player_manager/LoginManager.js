var login_view = API.createCamera(new Vector3(682.7154, 1063.229, 353.9427), new Vector3(-3.673188, -10.46521, -18.79283));
API.pointCameraAtPosition(login_view, new Vector3(718.9848, 1194.599, 325.2131));

API.onServerEventTrigger.connect(function (eventName, args) {

    if (eventName == "onPlayerConnectedEx") {

        //Set camera to login_view
        API.setActiveCamera(login_view);

        //Show CEF browser
    }
    else if (eventName == "login_finished") {
        API.setActiveCamera(null);
    }
});