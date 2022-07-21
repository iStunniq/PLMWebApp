var dataTable;

$(document).ready(function () {
    if (window.location.toString().includes("SeeAll")) {
        loadDataTable("GetAll2");
    } else {
        loadDataTable("GetAll");
    }
});

function loadDataTable(get) {
    dataTable = $('#tblData').DataTable({

        "ajax": {
            "url": "/Admin/Inventory/" + get + "?id=" + productid
        },
        "columns": [    
            { "data": "id", "width": "15%" },
            { "data": "expiry", "width": "15%" },
            { "data": "basePrice", "width": "15%" },
            { "data": "stock", "width": "15%" },
            {
                "data": "product.id",
                "render": function (data,type,row) {
                        return `
                        <div class="w-75 btn-group" role="group">
                            <a href="/Admin/Inventory/Upsert?prodid=${data}&batchid=${row.id}" class="btn btn-info mx-2"> 
                                <i class="bi bi-pencil-square"> </i> Update
                            </a>
                        </div>
                        `
                },
                "width": "15%"
            }
        ]
    });
}