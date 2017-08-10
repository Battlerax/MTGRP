$(document).ready(function () {
    resourceCall("loaded");
});

$(document).ready(function () {
    $(".list-group-item").click(function () {
        $(".list-group-item").removeClass("active");
        $(this).addClass("active");
    });
});

function addTypes(types) {
    var typesList = JSON.parse(types);
    for (var i = 0; i < typesList.length; i++) {
        $("#modtypeSelect").append(`<li><a href="#" onclick="getModsList('${typesList[i]}');">${typesList[i]}</a></li>`);
    }
}

function getModsList(type) {
    resourceCall("callServerEvent", "MODDING_GETMODS", type);
}

function showMods(mods) {
    var modsList = JSON.parse(types);
    for (var i = 0; i < typesList.length; i++) {
        $("#modtypeSelect").append(`<li><a href="#" onclick="getModsList('${typesList[i]}');">${typesList[i]}</a></li>`);
    }
}