//-------------- Button Navigation Code --------------
activeView = $('#view_all');
function show(id) {
    activeView.css("display", "none");
    $(id).css("display", "block");
    activeView = $(id);
    $('#error_box').css("display", "none");
}

//-------------- Create Mapping Functions --------------
function createMappingRequest() {
    if ($('#createPropLink').val() == "") {
        return send_error("You left the property link field blank. For no propery, input 0.");
    }
    if ($('#createDim').val() == "") {
        return send_error("You left the dimension field blank. For all dimensions, input 0.");
    }
    if ($('#createPastebinLink').val() == "") {
        return send_error("You left the pastebin link field empty.");
    }
    if ($('#createDesc').val() == "") {
        return send_error("You left the description field blank. Please input a valuable description.");
    }

    resourceCall("callServerEvent", "requestCreateMapping", $('#createPropLink').val(), $('#createDim').val(), $('#createPastebinLink').val(), $('#createDesc').val())
}

//-------------- View Mapping Functions --------------
function searchForMappingRequest() {
    if ($('#viewID').val() == "") {
        return send_error("You left the mapping ID empty.");
    }

    alert("Searched for request ID " + $('#viewID').val());
}

function saveMappingRequest() {
    if ($('#viewID').val() == "") {
        return send_error("You left the mapping ID empty.");
    }

    alert("Saved mapping request ID " + $('#viewID').val());
}

function deleteMappingRequest() {
    if ($('#viewID').val() == "") {
        return send_error("You left the mapping ID empty.");
    }

    alert("Deleted mapping request ID " + $('#viewID').val());
}

function toggleMappingLoaded() {
    if ($('#viewID').val() == "") {
        return send_error("You left the mapping ID empty.");
    }
    alert("Toggled loaded");
}

function toggleMappingActive() {
    if ($('#viewID').val() == "") {
        return send_error("You left the mapping ID empty.");
    }
    alert("Toggled active");
}

function requestMappingCode() {
    if ($('#viewID').val() == "") {
        return send_error("You left the mapping ID empty.");
    }

    alert("Requested code for mapping request ID " + $('#viewID').val());
}

function viewMappingRequest(id) {
    alert("Asked to view request " + id);
}

//-------------- Error Message Functions --------------
function send_error(error) {
    $('#error_box').css("display", "block");
    $('#error_msg').html(error);
}

//-------------- Pagination Functions --------------
function lastPage() {
    //resourceCall("requestLastPage");
    alert("Selected last page");
}

function nextPage() {
    //resourceCall("requestNextPage");
    alert("Selected next page");
}

function viewPage(page) {
    //resourceCal("requestPage", page);
    alert("Selected page " + page);
}

function addMappingRequest(id, description, createdBy, active)
{
    if (active == true) {
        $('#mapping_table_body').append('<tr class="info" id=' + id + ' onclick="viewMappingRequest(' + id + ' )"> \
                <td style="color: black;">' + id + '</td> \
                <td style="color: black;">' + description + '</td> \
                <td style="color: black;">' + createdBy + '</td> \
                <td style="color: green">Yes</td>');
    }
    else {
        $('#mapping_table_body').append('<tr class="info" id=' + id + ' onclick="viewMappingRequest(' + id + ' )"> \
                <td style="color: black;">' + id + '</td> \
                <td style="color: black;">' + description + '</td> \
                <td style="color: black;">' + createdBy + '</td> \
                <td style="color: red">No</td>');
    }
}

function createPagination(minPage, middlePage, maxPage) {
    $('#pagination').empty();
    $('#pagination').append('<span class="item" onclick="lastPage()" id="last_page">&lt;</span><span class="item current">'+ minPage + '</span>');
    for (i = minPage+1; i <= maxPage; i++){
        $('#pagination').append('<span class="item" id="' + i + '" onclick="viewPage(' + i + ')">' + i + '</span>')
        if (i == middlePage && middlePage != maxPage) {
            $('#pagination').append('<span class="item spaces">...</span>')
            $('#pagination').append('<span class="item" id="' + (maxPage - 1) + '" onclick="viewPage(' + (maxPage - 1) + ')">' + (maxPage - 1) + '</span>')
            $('#pagination').append('<span class="item" id="' + maxPage + '" onclick="viewPage(' + maxPage + ')">' + maxPage + '</span>')
            break;
        }
    }
    $('#pagination').append('<span class="item" onclick="nextPage()" id="next_page">&gt;</span>');
}