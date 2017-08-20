$(document).ready(function () {
    resourceCall("loaded");
});

var isVIP = false;

$(document).ready(function () {
    $('#modsList').on('click', '.list-group-item', function () {
        $(".list-group-item").removeClass("active");
        $(this).addClass("active");

        var test = $(this).data("vip");
        if (test === true && isVIP === false) {
            showError("This modification is restricated to VIP only.");
            return;
        }

        var modType = $(this).data("type");
        var modId = $(this).data("mod");
        resourceCall("putmod", modType, modId);

        //Add to cart.
        addToCart($(this).data("name"), modType, modId, $(this).data("price"));
    });

    $("#clearCartButton").click(function() {
        $(".shoppingitem").each(function (index) {
            resetMod($(this).data("type"), $(this), true);
        });
    });

    $("#purchaseButton").click(function() {
        var items = [];
        $(".shoppingitem").each(function (index) {
            items.push([$(this).data("type"), $(this).data("mod")]);
        });
        resourceCall("callServerEvent", "MODDONG_PURCHASE_ITEMS", JSON.stringify(items));
    });
});

function showError(string) {
    $("#vipNeededMsg").text(string);
    $("#vipNeededMsg").fadeIn();
    setTimeout(function () {
            if ($("#vipNeededMsg").text() !== string)
            return;

            $("#vipNeededMsg").fadeOut();
        },
        5000);
}

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
<span class="float-right">
    $<span class="shoppingPrice">${price}</span>
    <button type="button" class="btn btn-danger btn-xs" style="margin-left: 5px;" onclick="resetMod(${type}, this);">X</button>
</span>
</a>`);
    calculateTotal();
}

function resetMod(type, item, isBase = false) {
    if (isBase)
        $(item).remove();
    else
        $(item).parent().parent().remove();
    resourceCall("resetModType", parseInt(type));
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

function addTypes(types, isvip) {
    isVIP = isvip;

    var typesList = JSON.parse(types);
    for (var i = 0; i < typesList.length; i++) {
        var vip = "";
        if (typesList[i][1] === "true") {
            vip = " (VIP)";
        }
        $("#modtypeSelect").append(`<li><a href="#" onclick="getModsList('${typesList[i][0]}');">${typesList[i][0]}${vip}</a></li>`);
    }
    $("#modtypeSelect").append(`<li><a href="#" onclick="getModsList('windowtint');">Window Tint</a></li>`);
    $("#modtypeSelect").append(`<li><a href="#" onclick="getColor('primarycolor', 'Primary Color');">Primary Color</a></li>`);
    $("#modtypeSelect").append(`<li><a href="#" onclick="getColor('secondarycolor', 'Secondary Color');">Secondary Color</a></li>`);
    $("#modtypeSelect").append(`<li><a href="#" onclick="getColor('tyresmoke', 'Tyre Smoke');">Tyre Smoke (VIP)</a></li>`);
    $("#modtypeSelect").append(`<li><a href="#" onclick="getColor('neoncolor', 'Neon Color');">Neon Color (VIP)</a></li>`);
}


function getModsList(type) {
    if (type === "windowtint") {
        $("#modsList").empty();
        $("#modsListGroup").css("display", "block");
        $("#modsSelectColor").css("display", "none");

        $("#modsList").append(`
<a href="#" class="list-group-item moditem" data-name="None Window Tint" data-type="104" data-mod="0" data-price="200" data-vip="false">
    <span class="float-left">None</span>
    <span class="float-right">$200</span>
</a>`);

        $("#modsList").append(`
<a href="#" class="list-group-item moditem" data-name="Pure Black Window Tint" data-type="104" data-mod="1" data-price="200" data-vip="false">
    <span class="float-left">Pure Black</span>
    <span class="float-right">$200</span>
</a>`);

        $("#modsList").append(`
<a href="#" class="list-group-item moditem" data-name="Dark Smoke Window Tint" data-type="104" data-mod="2" data-price="200" data-vip="false">
    <span class="float-left">Dark Smoke</span>
    <span class="float-right">$200</span>
</a>`);

        $("#modsList").append(`
<a href="#" class="list-group-item moditem" data-name="Light Smoke Window Tint" data-type="104" data-mod="3" data-price="200" data-vip="false">
    <span class="float-left">Light Smoke</span>
    <span class="float-right">$200</span>
</a>`);

        $("#modsList").append(`
<a href="#" class="list-group-item moditem" data-name="Stock Window Tint" data-type="104" data-mod="4" data-price="200" data-vip="false">
    <span class="float-left">Stock</span>
    <span class="float-right">$200</span>
</a>`);

        $("#modsList").append(`
<a href="#" class="list-group-item moditem" data-name="Limo Window Tint" data-type="104" data-mod="5" data-price="200" data-vip="true">
    <span class="float-left">Limo (VIP)</span>
    <span class="float-right">$200</span>
</a>`);

        $("#modsList").append(`
<a href="#" class="list-group-item moditem" data-name="Green Window Tint" data-type="104" data-mod="6" data-price="200" data-vip="true">
    <span class="float-left">Green (VIP)</span>
    <span class="float-right">$200</span>
</a>`);
        return;
    }

    resourceCall("callServerEvent", "MODDING_GETMODS", type);
}

function updateColor(clr) {
    if (curColorType === "primarycolor") {
        addToCart("Primary Color", 100, Math.round(clr.rgb[0]) + "|" + Math.round(clr.rgb[1]) + "|" + Math.round(clr.rgb[2]), 100);
    }
    else if (curColorType === "secondarycolor") {
        addToCart("Secondary Color", 101, Math.round(clr.rgb[0]) + "|" + Math.round(clr.rgb[1]) + "|" + Math.round(clr.rgb[2]), 100);
    }
    else if (curColorType === "tyresmoke") {
        if (isVIP === false) {
            showError("This modification is restricated to VIP only.");
            return;
        }
        addToCart("Tyre Smoke Color", 102, Math.round(clr.rgb[0]) + "|" + Math.round(clr.rgb[1]) + "|" + Math.round(clr.rgb[2]), 500);
    }
    else if (curColorType === "neoncolor") {
        if (isVIP === false) {
            showError("This modification is restricated to VIP only.");
            return;
        }
        addToCart("Neons", 103, Math.round(clr.rgb[0]) + "|" + Math.round(clr.rgb[1]) + "|" + Math.round(clr.rgb[2]), 300);
    }

    resourceCall("updateColor", curColorType, clr.rgb[0], clr.rgb[1], clr.rgb[2]);
}

var curColorType;
function getColor(type, name) {
    $("#modsListGroup").css("display", "none");
    $("#modsSelectColor").css("display", "block");
    $("#colorTitle").text(name);
    resourceCall("updateCurrentColor", type);
    curColorType = type;
}

function updateColorPicker(r, g, b) {
    document.getElementById('colorTool').jscolor.fromRGB(r, g, b);
}

function showMods(mods) {
    var modsList = JSON.parse(mods);
    $("#modsList").empty();
    $("#modsListGroup").css("display", "block");
    $("#modsSelectColor").css("display", "none");
    for (var i = 0; i < modsList.length; i++) {
        var vip = "";
        if (modsList[i][4] === "true") {
            vip = " (VIP)";
        }
        $("#modsList").append(`<a href="#" class="list-group-item moditem" data-name="${modsList[i][0]}" data-type="${modsList[i][1]}" data-mod="${modsList[i][2]}" data-price="${modsList[i][3]}" data-vip="${modsList[i][4]}"><span class="float-left">${modsList[i][0]}${vip}</span><span class="float-right">$${modsList[i][3]}</span></a>`);
    }
}