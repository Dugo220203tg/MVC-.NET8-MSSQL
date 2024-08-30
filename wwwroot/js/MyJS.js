﻿// Sự kiện open dropdown
$(document).ready(function () {
    // Toggle dropdown on click
    $('.dropdown-toggle').on('click', function (e) {
        e.preventDefault();
        $(this).parent().toggleClass('open');
        $(this).attr('aria-expanded', function (index, attr) {
            return attr === 'true' ? 'false' : 'true';
        });
    });

    // Close dropdown when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.dropdown').length) {
            $('.dropdown').removeClass('open');
            $('.dropdown-toggle').attr('aria-expanded', 'false');
        }
    });
});
//Cập nhật lên giỏ hành trên header

function updateCartDisplay() {
    $.ajax({
        url: '/Cart/GetCartData',
        type: 'GET',
        success: function (data) {
            // Cập nhật số lượng sản phẩm trong giỏ hàng
            $('.qty').text(data.totalQuantity);

            // Xóa danh sách sản phẩm hiện tại
            $('.cart-list').empty();

            // Thêm danh sách sản phẩm mới
            $.each(data.cardProducts, function (index, item) {
                var productHtml = `
                <div class="product-widget">
                    <div class="product-img" style="width:60px;height:60px">
                        ${(() => {
                        let firstImageUrl = '';
                        if (item.Hinh) {
                            const imageUrls = item.Hinh.split(',');
                            if (imageUrls.length > 0) {
                                firstImageUrl = imageUrls[0].trim();
                            }
                        }
                        return firstImageUrl ?
                            `<img src="/Hinh/Hinh/HangHoa/${item.MaHH}/${firstImageUrl}" alt="${item.TenHH}" style="width:60px;height:60px">` :
                            '';
                    })()}
                    </div>
                    <div class="product-body">
                        <h2 class="product-name"><a href="#">${item.tenHH}</a></h2>
                        <h4 class="product-price"><span class="qty">${item.soLuong} x</span>$${item.donGia}</h4>
                    </div>
                    <button class="delete" data-product-id="${item.maHH}">
                        <i class="fa fa-times text-danger"></i>
                    </button>
                </div>
            `;
                $('.cart-list').append(productHtml);
            });

            // Gán sự kiện click cho các nút "Xóa" mới
            $('.cart-list .delete').on('click', function () {
                var productId = $(this).data('product-id');
                var productWidget = $(this).closest('.product-widget');
                RemoveCart(productId, productWidget);
            });

            // Cập nhật tổng tiền
            $('.cart-summary h5').text(`SUBTOTAL: $${data.totalAmount}`);
        },
        error: function (xhr, status, error) {
            console.log(error);
        }
    });
}
//$('.cart-list').append(productHtml);

//// Gán sự kiện click cho các nút "Xóa"
//$('.cart-list .delete').on('click', function () {
//    var productId = $(this).data('product-id');
//    var productWidget = $(this).closest('.product-widget');
//    RemoveCart(productId, productWidget);
//});
//Thêm vào giỏ hàng
function addToCart(id, quantity, type) {
    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: { id: id, quantity: quantity, type: type },
        success: function (result) {
            if (result.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Thành công',
                    text: 'Sản phẩm đã được thêm vào giỏ hàng',
                    showConfirmButton: false,
                    timer: 1500
                });
                updateCartDisplay(); // Cập nhật hiển thị giỏ hàng
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: result.message
                });
            }
        },
        error: function (xhr, status, error) {
            Swal.fire({
                icon: 'error',
                title: 'Lỗi',
                text: error
            });
        }
    });
}

//Xóa sản phẩm trong giỏ hàng
function RemoveCart(id, element) {
    $.ajax({
        url: '/Cart/RemoveCart',
        type: 'POST',
        data: { id: id },
        success: function (result) {
            if (result.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Xóa Thành công',
                    text: 'Đã xóa sản phẩm khỏi giỏ hàng',
                    showConfirmButton: false,
                    timer: 1000
                });
                updateCartDisplay(); // Cập nhật hiển thị giỏ hàng
                $(element).closest('tr').remove(); // Xóa phần tử HTML
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: result.message
                });
            }
        },
        error: function (xhr, status, error) {
            Swal.fire({
                icon: 'error',
                title: 'Lỗi',
                text: error
            });
        }
    });
}

//Tìm kiếm
$(document).ready(function () {
    $("#searchButton").click(function () {
        let searchText = $("#searchText").val().trim(); // Trim to remove leading/trailing spaces
        if (searchText !== "") { // Check if search text is not empty
            let url = `/Product/Search?query=` + encodeURIComponent(searchText); // Encode search text
            window.location.href = url;
        } else {
            window.location.href = "/Product"; // Redirect to /Product if searchText is empty
        }
    });
});
//Hiển thị danh mục sản phẩm trên breadcrumb
document.addEventListener('DOMContentLoaded', function () {
    // Lấy phần path của URL
    var urlPath = window.location.pathname + window.location.search;
    console.log("urlPath:", urlPath);
    // Lặp qua tất cả các phần tử có class "danhmuc"
    document.querySelectorAll('.danhmuc').forEach(link => {
        // Lấy href của mỗi thẻ chứa class danhmuc
        var href = link.getAttribute('href');

        // In giá trị của biến href vào console
        console.log("Href:", href);


        if (href && href.includes(urlPath)) {
            // Lấy phần tử label trong phạm vi của thẻ <a>
            var label = link.querySelector('label');

            // In giá trị của biến label vào console
            console.log("Label:", label);

            // Kiểm tra xem label có tồn tại không trước khi lấy dữ liệu từ nó
            if (label) {
                // Lấy nội dung của label
                var labelContent = label.textContent.trim();
                // Cập nhật nội dung của phần tử <li> có class 'active' trong breadcrumb
                document.querySelector('#breadcrumb-tree li.active').textContent = labelContent;
            }
        }
    });
});

//Hiển thị Your Cart
/*
$(document).ready(function () {
    // Bắt sự kiện click vào thẻ a.dropdown-toggle
    $('a.dropdown-toggle').click(function (e) {
        // Ngăn chặn hành động mặc định của thẻ a
        e.preventDefault();
        // Đảo ngược giá trị của thuộc tính aria-expanded
        $(this).attr('aria-expanded', function (index, attr) {
            return attr === 'true' ? 'false' : 'true';
        });
        // Thêm hoặc xóa lớp 'open' cho div.dropdown
        $('div.dropdown').toggleClass('open');
    });
});
*/
$(document).ready(function () {
    // Bắt sự kiện click vào thẻ a.dropdown-toggle
    $('a.dropdown-toggle').click(function (e) {
        // Ngăn chặn hành động mặc định của thẻ a
        e.preventDefault();
        // Đảo ngược giá trị của thuộc tính aria-expanded
        $(this).attr('aria-expanded', function (index, attr) {
            return attr === 'true' ? 'false' : 'true';
        });
        // Thêm hoặc xóa lớp 'open' cho thẻ cha của thẻ a.dropdown-toggle
        $(this).closest('div.dropdown').toggleClass('open');
    });
});

// Thêm sự kiện click cho các thẻ li
var liElements = document.querySelectorAll('.section-tab-nav li');
liElements.forEach(function (li) {
    li.addEventListener('click', function () {
        // Xóa lớp "active" từ tất cả các thẻ li
        liElements.forEach(function (el) {
            el.classList.remove('active');
        });
        // Thêm lớp "active" cho thẻ li được nhấp vào
        this.classList.add('active');
    });
});

// Kiểm tra nếu có tham số loại trong URL, thêm lớp "active" cho thẻ li tương ứng
var queryParams = new URLSearchParams(window.location.search);
var loaiParam = queryParams.get('loai');
if (loaiParam) {
    var activeLi = document.querySelector('.section-tab-nav RemoveWishListli a[asp-route-loai="' + loaiParam + '"]').parentNode;
    if (activeLi) {
        activeLi.classList.add('active');
    }
}
//-----WISH-LIST-----
function updateWishList() {
    $.ajax({
        url: '/WishList/Index',
        type: 'GET',
        success: function (data) {
            // Cập nhật số lượng sản phẩm trong giỏ hàng
            $('.qty').text(data.totalQuantity);

            // Xóa danh sách sản phẩm hiện tại
            $('.wish-list').empty();

            // Thêm danh sách sản phẩm mới
            $.each(data.cardProducts, function (index, item) {
                var productHtml = `
                <div class="product-widget">
                    <div class="product-img" style="width:60px;height:60px">
                        ${(() => {
                        let firstImageUrl = '';
                        if (item.Hinh) {
                            const imageUrls = item.Hinh.split(',');
                            if (imageUrls.length > 0) {
                                firstImageUrl = imageUrls[0].trim();
                            }
                        }
                        return firstImageUrl ?
                            `<img src="/Hinh/Hinh/HangHoa/${item.MaHH}/${firstImageUrl}" alt="${item.TenHH}" style="width:60px;height:60px">` :
                            '';
                    })()}
                    </div>
                    <div class="product-body">
                        <h2 class="product-name"><a href="#">${item.tenHH}</a></h2>
                        <h4 class="product-price"><span class="qty">${item.soLuong} x</span>$${item.donGia}</h4>
                    </div>
                    <button class="delete" data-product-id="${item.MaYT}">
                        <i class="fa fa-times text-danger"></i>
                    </button>
                </div>
            `;
                $('.cart-list').append(productHtml);
            });

            // Gán sự kiện click cho các nút "Xóa" mới
            $('.wish-list .delete').on('click', function () {
                var productId = $(this).data('product-id');
                var productWidget = $(this).closest('.product-widget');
                RemoveCart(productId, productWidget);
            });

            // Cập nhật tổng tiền
            $('.cart-summary h5').text(`SUBTOTAL: $${data.totalAmount}`);
        },
        error: function (xhr, status, error) {
            console.log(error);
        }
    });
}
//$('.cart-list').append(productHtml);
function addToWishlist(id, quantity, type) {
    $.ajax({
        url: '/WishList/AddToWishList',
        type: 'POST',
        data: { id: id, quantity: quantity, type: type },
        success: function (result) {
            if (result.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Thành công',
                    text: 'Sản phẩm đã được thêm vào danh sách yêu thích',
                    showConfirmButton: false,
                    timer: 1500
                });
                updateCartDisplay(); // Cập nhật hiển thị giỏ hàng
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: result.message
                });
            }
        },
        error: function (xhr, status, error) {
            Swal.fire({
                icon: 'error',
                title: 'Lỗi',
                text: error
            });
        }
    });
}
function RemoveWishList(id, element) {
    console.log(`Removing item with ID: ${id}`); // Debugging line

    $.ajax({
        url: '/WishList/RemoveWishList',
        type: 'POST',
        data: { id: id },
        success: function (result) {
            if (result.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Xóa Thành công',
                    text: 'Đã xóa sản phẩm khỏi danh sách yêu thích',
                    showConfirmButton: false,
                    timer: 1000
                });
                $(element).closest('.product-widget').remove();
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: result.message
                });
            }
        },
        error: function (xhr, status, error) {
            Swal.fire({
                icon: 'error',
                title: 'Lỗi',
                text: error
            });
        }
    });
}

// Initialize wishlist
$(document).ready(function () {
    updateWishList();
});