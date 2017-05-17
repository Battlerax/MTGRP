$(document).ready(function () {
    $(".tile").click(function () {
        var file = $(this).data("file");
       
        //Load app: 
        $("#app-browser").attr("src", "apps/" + file);
        $("#apps-list").css("display", "none"); //Set the apps list as hidden.
        $("#app-content").css("display", "block"); //Show the content.
    });

    $("#navigation").click(function () {
        //Unload app: 
        $("#app-browser").attr("src", "");
        $("#apps-list").css("display", "block"); //Set the apps list as shown.
        $("#app-content").css("display", "none"); //Hide the content.
    });
});