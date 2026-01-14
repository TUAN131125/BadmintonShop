document.addEventListener("DOMContentLoaded", function () {
    // ================= LIVE SEARCH LOGIC =================
    const searchInput = document.getElementById('searchInput');
    const suggestionsBox = document.getElementById('searchSuggestions');
    const spinner = document.getElementById('searchSpinner');
    let searchTimeout;

    // Hàm định dạng tiền tệ VND
    const formatCurrency = (amount) => {
        return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
    };

    if (searchInput && suggestionsBox) {

        // Sự kiện khi người dùng gõ phím
        searchInput.addEventListener('input', function () {
            const query = this.value.trim();

            // Xóa lệnh tìm kiếm cũ nếu người dùng vẫn đang gõ liên tục
            clearTimeout(searchTimeout);

            // Nếu xóa hết chữ hoặc ít hơn 2 ký tự -> Ẩn bảng gợi ý
            if (query.length < 2) {
                suggestionsBox.classList.remove('show');
                suggestionsBox.innerHTML = '';
                if (spinner) spinner.classList.add('d-none');
                return;
            }

            // Hiện icon loading
            if (spinner) spinner.classList.remove('d-none');

            // Chờ 300ms sau khi ngừng gõ mới gọi API (Debounce)
            searchTimeout = setTimeout(() => {
                fetch(`/api/products/search-suggestions?query=${encodeURIComponent(query)}`)
                    .then(response => response.json())
                    .then(data => {
                        renderSuggestions(data, query);
                    })
                    .catch(err => {
                        console.error("Search error:", err);
                    })
                    .finally(() => {
                        // Tắt icon loading dù thành công hay thất bại
                        if (spinner) spinner.classList.add('d-none');
                    });
            }, 300);
        });

        // Hàm vẽ HTML kết quả
        function renderSuggestions(products, query) {
            if (!products || products.length === 0) {
                suggestionsBox.innerHTML = '<div class="p-3 text-center text-muted small">No products found.</div>';
            } else {
                // Tạo danh sách HTML
                const htmlItems = products.map(p => `
                    <a href="${p.url}" class="suggestion-item">
                        <img src="${p.image}" alt="${p.name}" class="suggestion-thumb">
                        <div class="suggestion-info">
                            <span class="suggestion-name">${highlightMatch(p.name, query)}</span>
                            <div class="suggestion-price">${formatCurrency(p.price)}</div>
                        </div>
                    </a>
                `).join('');

                // Thêm nút "Xem tất cả"
                const viewAllHtml = `
                    <a href="/Product/Index?q=${encodeURIComponent(query)}" class="suggestion-view-all">
                        View all results for "${query}" <i class="bi bi-arrow-right"></i>
                    </a>
                `;

                suggestionsBox.innerHTML = htmlItems + viewAllHtml;
            }

            // Hiển thị bảng
            suggestionsBox.classList.add('show');
        }

        // Hàm bôi đậm từ khóa tìm kiếm (Optional - cho đẹp)
        function highlightMatch(text, query) {
            const regex = new RegExp(`(${query})`, 'gi');
            return text.replace(regex, '<span class="text-primary fw-bold">$1</span>');
        }

        // Sự kiện: Click ra ngoài thì ẩn bảng gợi ý
        document.addEventListener('click', function (e) {
            if (!searchInput.contains(e.target) && !suggestionsBox.contains(e.target)) {
                suggestionsBox.classList.remove('show');
            }
        });
    }
});