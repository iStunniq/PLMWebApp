    var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({

        "ajax": {
            "url":"/Admin/Sales/GetAll"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "reservationAmount", "width": "15%" },
            { "data": "grossIncome", "width": "15%" },
            { "data": "netIncome", "width": "15%" },
            { "data": "generationDate", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/Sales/Upsert?id=${data}" class="btn btn-info mx-2"> 
                                <i class="bi bi-pencil-square"> </i> Edit 
                            </a>
                            <a onclick="Delete('/Admin/Sales/Delete/+${data}')" class="btn btn-danger mx-2"> 
                                <i class="bi bi-trash"></i> Delete
                            </a>
                        </div>
                        `
                },
                "width": "15%"
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "Reports are permanently deleted",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}