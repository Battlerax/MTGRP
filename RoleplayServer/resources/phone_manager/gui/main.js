$(document).ready(function () {
    $(".tile").click(function () {
        var file = $(this).data("file");
       
        //Load app: 
        $("#app-content").load("apps/" + file);
        $("#apps-list").css("display", "none"); //Set the apps list as hidden.
        $("#app-content").css("display", "block"); //Show the div.
    });
});