
API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
		
        case "watch_broadcast":
			var cam_view = API.createCamera(args[0], args[1]);
			API.setActiveCamera(cam_view);
			var resolution = API.getScreenResolution();
			lowerthird = API.createCefBrowser(resolution.Width, resolution.Height);
			API.waitUntilCefBrowserInit(lowerthird);
			API.setCefBrowserPosition(lowerthird, 0, 0);
			API.loadPageCefBrowser(lowerthird, "group_manager/lsnn/LowerThird.html");
			document.getElementById('headline').textContent = args[2];
			break;

		case "unwatch_broadcast":
		    API.setActiveCamera(null);
		    API.destroyCefBrowser(lowerthird);
		    break;

		case "cam_carry":
			API.disableControlThisFrame(21);
			API.disableControlThisFrame(22);

		case "cam_drop":
			API.enableControlThisFrame(21);
			API.enableControlThisFrame(22);
    }
});

