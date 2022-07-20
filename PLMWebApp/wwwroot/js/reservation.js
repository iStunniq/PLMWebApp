var dataTable;

$(document).ready(function () {
    var url = window.location.search;

    //if (url.includes("pending")) {
    //    loadDataTable("pending");
    //} else {
    //    if (url.includes("inprocess")) {
    //        loadDataTable("inprocess");
    //    } else {
    //        if (url.includes("completed")) {
    //            loadDataTable("completed");
    //        } else {
    //            loadDataTable("all");
    //        }
    //    }
    //}

    if (url.includes("pending")) {
        loadDataTable("pending");
    } else if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    } else if (url.includes("completed")) {
        loadDataTable("completed");
    } else if (url.includes("approval")) {
        loadDataTable("approval");
    } else if (url.includes("shipped")) {
        loadDataTable("shipped");
    } else {
        loadDataTable("all");
    }
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({

        "ajax": {
            "url":"/Admin/Reservation/GetAll?status=" + status
        },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "firstName", "width": "15%" },
            { "data": "lastName", "width": "15%" },
            { "data": "phone", "width": "15%" },
            { "data": "applicationUser.email", "width": "15%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "orderTotal", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/Reservation/Details?reservationId=${data}" class="btn btn-info mx-2"> 
                                <i class="bi bi-pencil-square"> </i> Details 
                            </a>
                        </div>
                        `
                },
                "width": "5%"
            }
        ]
    });
}
