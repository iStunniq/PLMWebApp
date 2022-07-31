﻿    var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({

        "ajax": {
            "url":"/Admin/Sales/GetAll"
        },
        order: [[0, 'desc']],
        "columns": [
            { "data": "generationDate", "width": "15%" },
            { "data": "name", "width": "15%" },
            { "data": "reservationAmount", "width": "5%" },
            { "data": "cancelledAmount", "width": "5%" },
            { "data": "grossIncome", "width": "10%" },
            { "data": "baseCosts", "width": "10%" },
            { "data": "netIncome", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/Sales/Upsert?id=${data}" class="btn btn-info mx-2"> 
                                <i class="bi bi-pencil-square"> </i> Edit 
                            </a>
                            <a href="/Admin/Sales/SalesItems?id=${data}" class="btn btn-success mx-2">
                                <i class="bi bi-bag-check"> </i> Items
                            </a>
                            <a href="/Admin/Sales/SalesCancelled?id=${data}" class="btn btn-danger mx-2">
                                <i class="bi bi-bag-x"> </i> Items
                            </a>
                            <a onclick="Delete('/Admin/Sales/Delete/+${data}')" class="btn btn-warning mx-2"> 
                                <i class="bi bi-trash"></i> Delete
                            </a>
                        </div>
                        `
                },
                "width": "20%"
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