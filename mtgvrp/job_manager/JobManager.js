/// <reference path="../types-gt-mp/index.d.ts" />
"use strict";
var cornerStart = null;
var cornerUp = null;
var cornerDown = null;
var cornerStartPos = null;
var xAdd = 1;
var yAdd = 1;
API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case "create_job_zone":
            var player = API.getLocalPlayer();
            cornerStart = API.createMarker(0, API.getEntityPosition(player), new Vector3(), new Vector3(), new Vector3(1.0, 1.0, 1.0), 255, 255, 0, 255);
            cornerUp = API.createMarker(0, API.getEntityPosition(player).Add(new Vector3(xAdd, 0.0, 0.0)), new Vector3(), new Vector3(), new Vector3(1.0, 1.0, 1.0), 0, 255, 0, 255);
            cornerDown = API.createMarker(0, API.getEntityPosition(player).Add(new Vector3(0.0, yAdd, 0.0)), new Vector3(), new Vector3(), new Vector3(1.0, 1.0, 1.0), 255, 0, 0, 255);
            cornerStartPos = API.getEntityPosition(player);
            break;
        case "cancel_job_zone":
            API.deleteEntity(cornerStart);
            API.deleteEntity(cornerUp);
            API.deleteEntity(cornerDown);
            cornerStart = null;
            cornerUp = null;
            cornerDown = null;
            xAdd = 1;
            yAdd = 1;
            break;
        case "finish_job_zone":
            API.triggerServerEvent("finish_job_zone_create", cornerStartPos, xAdd, yAdd);
            API.deleteEntity(cornerStart);
            API.deleteEntity(cornerUp);
            API.deleteEntity(cornerDown);
            cornerStart = null;
            cornerUp = null;
            cornerDown = null;
            xAdd = 1;
            yAdd = 1;
            break;
    }
});
API.onKeyDown.connect(function (Player, args) {
    if (cornerUp != null && cornerDown != null) {
        if (args.KeyCode == Keys.Up) {
            xAdd++;
            API.setEntityPosition(cornerUp, cornerStartPos.Add(new Vector3(xAdd, 0.0, 0.0)));
        }
        else if (args.KeyCode == Keys.Down) {
            xAdd--;
            API.setEntityPosition(cornerUp, cornerStartPos.Add(new Vector3(xAdd, 0.0, 0.0)));
        }
        else if (args.KeyCode == Keys.Left) {
            yAdd++;
            API.setEntityPosition(cornerDown, cornerStartPos.Add(new Vector3(0, yAdd, 0.0)));
        }
        else if (args.KeyCode == Keys.Right) {
            yAdd--;
            API.setEntityPosition(cornerDown, cornerStartPos.Add(new Vector3(0.0, yAdd, 0.0)));
        }
    }
});
