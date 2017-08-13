$(document).ready(function () {
    resourceCall("loaded");
});

$(document).ready(function () {
    $('#modsList').on('click', '.list-group-item', function () {
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
    var modsList = JSON.parse(mods);
    $("#modsList").empty();
    for (var i = 0; i < modsList.length; i++) {
        $("#modsList").append(`<a href="#" class="list-group-item moditem" data-type="${modsList[i][1]}" data-mod="${modsList[i][2]}"><span class="float-left">${modsList[i][0]}</span><span class="float-right">$${modsList[i][3]}</span></a>`);
    }
}