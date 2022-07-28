var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({

        "ajax": {
            "url":"/Admin/Userlist/GetAll"
        },
        "columns": [
            { "data": "email", "width": "20%" },
            { "data": "firstName", "width": "15%" },
            { "data": "lastName", "width": "15%" },
            { "data": "phone", "width": "15%" },
            { "data": "roleName", "width": "15%" },
            { "data": "warnings", "width": "10%" },
            {
                "data": "email",
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/Userlist/Update?email=${data}" class="btn btn-info mx-2"> 
                                <i class="bi bi-pencil-square"> </i> View
                            </a>
                        </div>
                        `
                },
                "width": "10%"
            }
        ]
    });
}