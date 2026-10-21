// Họ và tên: Trần Phương Thảo (Nhóm trưởng) - Mã sinh viên: 23103100226
// Nội dung thực hiện: Script giao diện dùng chung - toast tự ẩn, hộp thoại xác nhận,
// xem trước ảnh upload, tự gửi form lọc khi đổi lựa chọn.
(function () {
    'use strict';

    // Toast tự ẩn sau 5 giây
    document.querySelectorAll('.pk-toast').forEach(function (t) {
        setTimeout(function () { t.style.transition = 'opacity .4s'; t.style.opacity = '0'; setTimeout(function () { t.remove(); }, 400); }, 5000);
    });
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-close-toast]');
        if (btn) btn.closest('.pk-toast').remove();
    });

    // Hộp thoại xác nhận: <button data-confirm-url=".." data-confirm-title=".." data-confirm-text=".." data-confirm-reason="true">
    var modalEl = document.getElementById('modalXacNhan');
    if (modalEl && window.bootstrap) {
        var modal = new bootstrap.Modal(modalEl);
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-confirm-url]');
            if (!btn) return;
            e.preventDefault();
            document.getElementById('formXacNhan').action = btn.getAttribute('data-confirm-url');
            document.getElementById('xacNhanTieuDe').textContent = btn.getAttribute('data-confirm-title') || 'Xác nhận';
            document.getElementById('xacNhanNoiDung').textContent = btn.getAttribute('data-confirm-text') || 'Bạn có chắc chắn muốn thực hiện thao tác này?';
            document.getElementById('xacNhanNut').textContent = btn.getAttribute('data-confirm-btn') || 'Đồng ý';
            document.getElementById('xacNhanLyDo').classList.toggle('d-none', btn.getAttribute('data-confirm-reason') !== 'true');
            document.getElementById('lyDoHuy').value = '';
            modal.show();
        });
    }

    // Xem trước ảnh trước khi upload: <input type="file" data-preview="#imgId">
    document.querySelectorAll('input[type=file][data-preview]').forEach(function (input) {
        input.addEventListener('change', function () {
            var img = document.querySelector(input.getAttribute('data-preview'));
            if (img && input.files && input.files[0]) {
                img.src = URL.createObjectURL(input.files[0]);
                img.classList.remove('d-none');
            }
        });
    });

    // Tự gửi form lọc khi đổi select có data-auto-submit
    document.querySelectorAll('[data-auto-submit]').forEach(function (el) {
        el.addEventListener('change', function () { el.form.submit(); });
    });
})();
