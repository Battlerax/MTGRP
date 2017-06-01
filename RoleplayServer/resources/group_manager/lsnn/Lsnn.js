
API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
		
        case "watch_broadcast":
			var cam_view = API.createCamera(args[0], args[1]);
			API.setActiveCamera(cam_view);
			var resolution = API.getScreenResolution();
			lowerthird = API.createCefBrowser(resolution.Width, resolution.Height);
			API.waitUntilCefBrowserInit(lowerthird);
			API.setCefBrowserPosition(lowerthird, 800, 700);
			API.loadPageCefBrowser(lowerthird, "group_manager/lsnn/LowerThird.html");
			lowerthird.call("settitle", args[2]);
			break;

		case "watch_chopper_broadcast":
			var cam_view = API.createCamera(args[0], args[1]);
			API.attachCameraToEntity(cam_view, args[3], args[4]);
			API.setActiveCamera(cam_view);
			var resolution = API.getScreenResolution();
			lowerthird = API.createCefBrowser(resolution.Width, resolution.Height);
			API.waitUntilCefBrowserInit(lowerthird);
			API.setCefBrowserPosition(lowerthird, 800, 700);
			API.loadPageCefBrowser(lowerthird, "group_manager/lsnn/LowerThirdChopper.html");
			lowerthird.call("settitle", args[2]);
			break;

		case "unwatch_broadcast":
		    API.setActiveCamera(null);
		    API.destroyCefBrowser(lowerthird);
		    break;
    }
});

