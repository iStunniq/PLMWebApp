var dataTable;

$(document).ready(function () {
    loadDataTable()
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({

        "ajax": {
            "url":"/Admin/Inventory/GetProducts"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "stock", "width": "15%" },
            { "data": "price", "width": "15%" },
            { "data": "brand.name", "width": "15%" },
            { "data": "category.name", "width": "15%" },
            {
                "data": "id",
                "render": function (data,type,row) {
                        return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/Inventory/Batches?id=${data}" class="btn btn-info mx-2"> 
                                <i class="bi bi-pencil-square"> </i> See Inventory
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
        text: "You can revert this in See All Products",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, Deactivate it!'
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


function Activate(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "This item will be reactivated",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes'
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