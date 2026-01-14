document.addEventListener("DOMContentLoaded", function () {
    console.log("Cart.js loaded successfully!");

    // ===================== KHAI BÁO BIẾN =====================
    const miniCart = document.getElementById("miniCart");
    const cartToggle = document.getElementById("cartToggle");
    const cartCountBadge = document.getElementById("cartCount");

    // ===================== MINI CART TOGGLE =====================
    function toggleMiniCart() {
        if (!miniCart) return;
        miniCart.classList.toggle("d-none");
    }

    function closeMiniCart() {
        if (!miniCart) return;
        miniCart.classList.add("d-none");
    }

    if (cartToggle) {
        cartToggle.addEventListener("click", function (e) {
            e.stopPropagation();
            toggleMiniCart();
        });
    }

    document.addEventListener("click", function (e) {
        if (miniCart && cartToggle) {
            if (!miniCart.contains(e.target) && !cartToggle.contains(e.target)) {
                closeMiniCart();
            }
        }
    });

    // ===================== AJAX ADD TO CART =====================
    document.addEventListener("submit", function (e) {
        const form = e.target;

        // Chỉ xử lý nếu form có class "add-to-cart-form"
        if (!form.classList.contains("add-to-cart-form")) {
            return;
        }

        e.preventDefault();
        console.log("AJAX Add to cart triggered");

        const btn = form.querySelector("button[type='submit']");
        const originalText = btn ? btn.innerHTML : "";

        // Hiệu ứng Loading (Tiếng Anh)
        if (btn) {
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Adding...';
        }

        fetch(form.action, {
            method: "POST",
            body: new FormData(form)
        })
            .then(response => {
                if (!response.ok) throw new Error("Network response was not ok");
                return response.json();
            })
            .then(data => {
                // Cập nhật Badge số lượng
                if (cartCountBadge) {
                    cartCountBadge.innerText = data.count;
                }

                // Cập nhật nội dung Mini Cart và mở nó ra
                if (miniCart) {
                    miniCart.innerHTML = data.html;
                    miniCart.classList.remove("d-none");
                }

                // Hiệu ứng thành công trên nút (Tiếng Anh)
                if (btn) {
                    btn.innerHTML = '<i class="bi bi-check2"></i> Added';
                    // Xóa class cũ, thêm class xanh lá
                    btn.classList.remove("btn-primary", "btn-dark");
                    btn.classList.add("btn-success");

                    // Trả về trạng thái cũ sau 2s
                    setTimeout(() => {
                        btn.innerHTML = originalText;
                        btn.classList.remove("btn-success");
                        // Bạn có thể trả về class gốc tùy ý, ví dụ btn-dark nếu ở trang Detail
                        btn.disabled = false;
                    }, 2000);
                }

                // LƯU Ý: KHÔNG CÓ DÒNG ALERT NÀO Ở ĐÂY NỮA
            })
            .catch(error => {
                console.error("Lỗi thêm giỏ hàng:", error);
                // Vẫn giữ alert lỗi để debug nếu server chết
                alert("An error occurred. Please try again.");
                if (btn) {
                    btn.innerHTML = originalText;
                    btn.disabled = false;
                }
            });
    });

    // ===================== ĐỒNG BỘ KHI BACK TRANG =====================
    window.addEventListener('pageshow', function (event) {
        var historyTraversal = event.persisted ||
            (typeof window.performance != "undefined" &&
                window.performance.navigation.type === 2);

        if (historyTraversal) {
            console.log("Back button detected! Refreshing cart...");
            fetch('/Cart/GetCartSummary') // Đảm bảo bạn đã thêm hàm này vào CartController
                .then(res => res.json())
                .then(data => {
                    if (cartCountBadge) cartCountBadge.innerText = data.count;
                    if (miniCart) miniCart.innerHTML = data.html;
                })
                .catch(e => console.log("Sync error:", e));
        }
    });
});