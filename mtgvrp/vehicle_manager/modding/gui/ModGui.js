$(document).ready(function () {
    resourceCall("loaded");
});

$(document).ready(function () {
    $('#modsList').on('click', '.list-group-item', function () {
        $(".list-group-item").removeClass("active");
        $(this).addClass("active");

        var modType = $(this).data("type");
        var modId = $(this).data("mod");
        resourceCall("putmod", modType, modId);
    });
});

function addTypes(types) {
    var typesList = JSON.parse(types);
    for (var i = 0; i < typesList.length; i++) {
        $("#modtypeSelect").append(`<li><a href="#" onclick="getModsList('${typesList[i]}');">${typesList[i]}</a></li>`);
    }
    $("#modtypeSelect").append(`<li><a href="#" onclick="getColor('primarycolor', 'Primary Color');">Primary Color</a></li>`);
    $("#modtypeSelect").append(`<li><a href="#" onclick="getColor('secondarycolor', 'Secondary Color');">Secondary Color</a></li>`);
    $("#modtypeSelect").append(`<li><a href="#" onclick="getColor('modcolor1', 'Mod Color 1');">Mod Color 1</a></li>`);
    $("#modtypeSelect").append(`<li><a href="#" onclick="getColor('modcolor2', 'Mod Color 2');">Mod Color 2</a></li>`);
    $("#modtypeSelect").append(`<li><a href="#" onclick="getColor('tyresmoke', 'Tyre Smoke');">Tyre Smoke (VIP)</a></li>`);
    $("#modtypeSelect").append(`<li><a href="#" onclick="getColor('neoncolor', 'Neon Color');">Neon Color (VIP)</a></li>`);
}


function getModsList(type) {
    resourceCall("callServerEvent", "MODDING_GETMODS", type);
}

function updateColor(clr) {
    resourceCall("updateColor", curColorType, clr.rgb[0], clr.rgb[1], clr.rgb[2]);
}

var curColorType;
function getColor(type, name) {
    $("#modsListGroup").css("display", "none");
    $("#modsSelectColor").css("display", "block");
    $("#colorTitle").text(name);
    curColorType = type;
}

function showMods(mods) {
    var modsList = JSON.parse(mods);
    $("#modsList").empty();
    for (var i = 0; i < modsList.length; i++) {
        $("#modsList").append(`<a href="#" class="list-group-item moditem" data-type="${modsList[i][1]}" data-mod="${modsList[i][2]}"><span class="float-left">${modsList[i][0]}</span><span class="float-right">${modsList[i][3]}</span></a>`);
    }
}