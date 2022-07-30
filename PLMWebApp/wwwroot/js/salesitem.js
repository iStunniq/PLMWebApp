var dataTable;

$(document).ready(function () {
    loadDataTable()
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({

        "ajax": {
            "url": "/Admin/Sales/GetAll2?id=" + reportId
        },
        "columns": [
            { "data": "shippingDate", "width": "15%" },
            { "data": "id", "width": "5%" },
            { "data": "applicationUser.email", "width": "20%" },
            { "data": "phone", "width": "15%" },
            { "data": "orderTotal", "width": "10%" },
            { "data": "baseTotal", "width": "10%" },
            {
                "data": "id",
                "render": function (data, type, row) {
                    return row["orderTotal"] - row["baseTotal"]
                },
                "width": "10%"
            },
            {
                "data": "id",
                "render": function (data,type,row) {
                    if (row["viewed"]) {
                        return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/Sales/Details?reservationId=${data}&oid=${reportId}" class="btn btn-info mx-2"> 
                                <i class="bi bi-pencil-square"> </i> Details
                            </a>
                        </div>
                        `
                    }
                    else {
                        return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/Sales/Details?reservationId=${data}&oid=${reportId}" class="btn btn-success mx-2"> 
                                <i class="bi bi-plus"> </i> New
                            </a>
                        </div>
                        `
                    }
                },
                "width": "5%"
            }
        ]
    });
}
