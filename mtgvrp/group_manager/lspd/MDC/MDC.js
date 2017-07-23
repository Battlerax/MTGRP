var mdcBrowser = null;

API.onResourceStart.connect(function () {
    
});

//From server
API.onServerEventTrigger.connect(function (eventName, args) {
 
    switch (eventName) {
        case "showMDC":
            var res = API.getScreenResolutionMantainRatio();
	        var pos = resource.JsFunctions.scaleCoordsToReal({X: res.Width, Y: res.Height});
            mdcBrowser = API.createCefBrowser(pos.X, pos.Y);
        
            API.waitUntilCefBrowserInit(mdcBrowser);
            API.setCefBrowserPosition(mdcBrowser, 0, 0);
            API.loadPageCefBrowser(mdcBrowser, "group_manager/lspd/MDC/MDC.html");
            //API.setCefDrawState(true);
            API.setCanOpenChat(false);
            API.showCursor(true);
            API.waitUntilCefBrowserLoaded(mdcBrowser);
            
            break;

        case "add911":

            //number, time, info
            mdcBrowser.call("html_add911", args[0], args[1], args[2], args[3]);
            break;

        case "addBolo":

            //boloId, officer, time, priority, info
            mdcBrowser.call("html_addBolo", args[0], args[1], args[2], args[3], args[4]);
            break;

        case "remove911":

            break;

        case "MDC_SHOW_CITIZEN_INFO":
            mdcBrowser.call("html_showcitizeninfo", args[0], args[1], args[2], args[3], args[4]);
            break;

        case "MDC_SHOW_VEHICLE_INFO":
            mdcBrowser.call("html_showvehicleinfo", args[0], args[1], args[2]);
            break;

        case "MDC_UPDATE_CRIMES":
            mdcBrowser.call("html_updatecrimes", args[0]);
            break;
    }
});

//From HTML 

function MdcLoaded() {
    API.triggerServerEvent("requestMdcInformation");
    mdcBrowser.call("setImageBackground");
}

function client_updateMdcAnnoucement(text) {
    API.triggerServerEvent("server_updateMdcAnnouncement", text);
}

function client_removeBolo(boloId) {
    API.triggerServerEvent("server_removeBolo", boloId);
}

function client_sendBoloToServer(info, priority) {
    API.triggerServerEvent("server_createBolo", info, priority);
}

function client_mdc_close() {
    API.destroyCefBrowser(mdcBrowser);
    API.showCursor(false);
    //API.setCefDrawState(false);
    API.setCanOpenChat(true);
    API.triggerServerEvent("server_mdc_close");
}

function MDC_SearchForCitizen(name, phone) {
    API.triggerServerEvent("MDC_SearchForCitizen", name, phone);
}

function MDC_SearchForVehicle(lic) {
    API.triggerServerEvent("MDC_SearchForVehicle", lic);
}

function MDC_RequestNextCrimesPage(number) {
    API.triggerServerEvent("MDC_RequestNextCrimesPage", number);
}