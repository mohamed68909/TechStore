var dtble;
$(document).ready(function () {
    loaddata();
});

function loaddata() {
    dtble = $("#mytable").DataTable({
        "ajax": {
            "url":"/Admin/Order/GetData"
        },
        "columns": [
            { "data": "id" },
            { "data": "name"},
            { "data": "phoneNumber" },
            { "data": "applicationUser.email" },
            { "data": "orderStatus" },
            { "data": "totalPrice" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="d-flex justify-content-center gap-2">
                            <a href="/Admin/Order/Details?orderid=${data}" class="btn btn-outline-info btn-action" title="View Details">
                                <i class="fas fa-eye"></i>
                            </a>
                            <button onclick="DeleteItem('/Admin/Order/Delete?id=${data}')" class="btn btn-outline-danger btn-action" title="Delete Order">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    `
                }
            }

            

        ]
    });
}

function DeleteItem(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: "DELETE",
                url: url,
                headers: {
                    "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dtble.ajax.reload();                        
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })            
        }
    })
}

