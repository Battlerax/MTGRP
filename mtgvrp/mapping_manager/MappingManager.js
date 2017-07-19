API.onServerEventTrigger.connect((eventName, args) => {
    switch (eventName) {
        case "callMappingFunction": {
            var func = args[0]; //Func name.
            var realArgs = Array.prototype.slice.call(args, 1); //Cut the first parameter and get remaining.
            //myBrowser.call(func, ...realArgs); //Call the func sending the array as parameters
            break;
        }
    }
});

function callServerEvent(eventName /* Args */) {
    var args = Array.prototype.slice.call(arguments, 1);
    API.triggerServerEvent(eventName, ...args);
}