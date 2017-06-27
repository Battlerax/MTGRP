

var funcToBeCalled = "";
var args;
function setToBeCalled(func /* args */) {
	var a = Array.prototype.slice.call(arguments, 1);
	funcToBeCalled = func;
	args = a;
}
function cefLoaded() {
	if (funcToBeCalled !== "") {
		lowerthird.call(funcToBeCalled, ...args);
	}
}

var lowerthird;
API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
		
        case "watch_broadcast":
			var wbcamView = API.createCamera(args[0], args[1]);
			API.setActiveCamera(wbcamView);
			var wbres = API.getScreenResolutionMantainRatio();
			lowerthird = API.createCefBrowser(800, 700);
			API.waitUntilCefBrowserInit(lowerthird);
			var pos = resource.JsFunctions.scaleCoordsToReal({X: wbres.Width - 1200,Y: wbres.Height - 300 });
			API.setCefBrowserPosition(lowerthird, pos.X, pos.Y);
			API.loadPageCefBrowser(lowerthird, "group_manager/lsnn/LowerThird.html");
			////API.setCefDrawState(true);
	        setToBeCalled("settitle", args[2]);
			break;

		case "watch_chopper_broadcast":
			var wcCamView = API.createCamera(args[0], args[1]);
			API.attachCameraToEntity(wcCamView, args[3], args[4]);
			API.setActiveCamera(wcCamView);
		    API.callNative("0xBB7454BAFF08FE25", args[5], args[6], args[7], 0.0, 0.0, 0.0); 
			var resolution = API.getScreenResolutionMantainRatio();
			lowerthird = API.createCefBrowser(800, 700);
			API.waitUntilCefBrowserInit(lowerthird);
			var apos = resource.JsFunctions.scaleCoordsToReal({X: resolution.Width - 1200,Y: resolution.Height - 300 });
			API.setCefBrowserPosition(lowerthird, apos.X, apos.Y);
			API.loadPageCefBrowser(lowerthird, "group_manager/lsnn/LowerThirdChopper.html");
			////API.setCefDrawState(true);
			setToBeCalled("settitle", args[2]);
			break;

		case "update_chopper_cam":
			var wcCamView = API.createCamera(args[0], args[1]);
			API.attachCameraToEntity(wcCamView, args[3], args[4]);
			API.setActiveCamera(wcCamView);
		    API.callNative("0xBB7454BAFF08FE25", args[5], args[6], args[7], 0.0, 0.0, 0.0); 
			break;

		case "unwatch_broadcast":
		    API.setActiveCamera(null);
			if(lowerthird != null){
		    API.destroyCefBrowser(lowerthird);
			}
			API.callNative("0x31B73D1EA9F01DA2");
			////API.setCefDrawState(false);
		    break;
    }
});

