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

        //Add to cart.
        addToCart($(this).data("name"), modType, modId, $(this).data("price"));
    });
});

function addToCart(name, type, id, price) {
    //If existing, replace.
    var item = $(`.shoppingitem[data-type='${type}']`);
    if (item.length) {
        item.data("mod", id);
        item.data("price", price);
        item.find(".shoppingName").text(name);
        item.find(".shoppingPrice").text(price);
        calculateTotal();
        return;
    }

    //Else add
    $("#shoppingCart").append(`<a href="#" class="list-group-item moditem shoppingitem" data-type="${type}" data-mod="${id}" data-price="${price}">
<span class="float-left"><span class="shoppingName">${name}</span></span>
<span class="float-right">$<span class="shoppingPrice">${price}</span><button type="button" class="btn btn-danger btn-xs" style="margin-left: 5px;">X</button></span>
</a>`);
    calculateTotal();
}

function calculateTotal() {
    var total = 0;;
    $(".shoppingitem").each(function(index) {
        var price = parseInt($(this).data("price"));
        total += price;
    });
    $("#totalPrice").text("$" + total);
}

function addTypes(types) {
    var typesList = JSON.parse(types);
    for (var i = 0; i < typesList.length; i++) {
        $("#modtypeSelect").append(`<li><a href="#" onclick="getModsList('${typesList[i]}');">${typesList[i]}</a></li>`);
    }
    $("#modtypeSelect").append(`<li><a href="#" onclick="getColor('primarycolor', 'Primary Color');">Primary Color</a></li>`);
    $("#modtypeSelect").append(`<li><a href="#" onclick="getColor('secondarycolor', 'Secondary Color');">Secondary Color</a></li>`);
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
    $("#modsListGroup").css("display", "block");
    $("#modsSelectColor").css("display", "none");
    for (var i = 0; i < modsList.length; i++) {
        $("#modsList").append(`<a href="#" class="list-group-item moditem" data-name="${modsList[i][0]}" data-type="${modsList[i][1]}" data-mod="${modsList[i][2]}" data-price="${modsList[i][3]}"><span class="float-left">${modsList[i][0]}</span><span class="float-right">$${modsList[i][3]}</span></a>`);
    }
}