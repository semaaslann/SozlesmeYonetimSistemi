// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


const hamBurger = document.querySelector(".toggle-btn");

hamBurger.addEventListener("click", function () {
    document.querySelector("#sidebar").classList.toggle("expand");
});

document.addEventListener('DOMContentLoaded', function () {
    const dropdownLinks = document.querySelectorAll('.sidebar-link.has-dropdown');

    dropdownLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();

            // Hedef dropdown'ı bul
            const targetId = this.getAttribute('data-bs-target');
            const targetDropdown = document.querySelector(targetId);

            // Diğer tüm açık dropdownları kapat
            const allDropdowns = document.querySelectorAll('.sidebar-dropdown.show');
            allDropdowns.forEach(dropdown => {
                if (dropdown !== targetDropdown) {
                    dropdown.classList.remove('show');
                }
            });

            // Tıklanan dropdown'ı aç/kapat
            if (targetDropdown) {
                targetDropdown.classList.toggle('show');
                this.classList.toggle('collapsed');
            }
        });
    });
}); 






$(document).ready(function () {
    // Select2 yapılandırması
    $('select[name="ImzalayanKisiIds"], select[name="ParaflayanKisiIds"]').select2({
        placeholder: "Kişi seçiniz",
        allowClear: true,
        width: '100%',
        dropdownAutoWidth: true,
        minimumResultsForSearch: 1
    });

    // Select2 z-index sorunu çözümü
    $('.select2-container').css('z-index', 9999);

    // TinyMCE yapılandırması
    tinymce.init({
        selector: '#contractContent',
        plugins: 'advlist autolink lists link image charmap print preview hr anchor pagebreak',
        toolbar: 'undo redo | formatselect | bold italic | alignleft aligncenter alignright | bullist numlist outdent indent | link image',
        menubar: false,
        height: 400
    });

    // Form doğrulama
    function checkFormValidity() {
        var isValid = true;
        $('[required]').not('#UserId').each(function () {
            if (!$(this).val()) {
                isValid = false;
            }
        });
        if (!$('select[name="ImzalayanKisiIds"]').val() || $('select[name="ImzalayanKisiIds"]').val().length === 0) {
            isValid = false;
            $('[data-valmsg-for="ImzalayanKisiIds"]').text('En az bir imzalayan kişi seçmelisiniz.');
        } else {
            $('[data-valmsg-for="ImzalayanKisiIds"]').text('');
        }
        if (!$('select[name="ParaflayanKisiIds"]').val() || $('select[name="ParaflayanKisiIds"]').val().length === 0) {
            isValid = false;
            $('[data-valmsg-for="ParaflayanKisiIds"]').text('En az bir paraflayan kişi seçmelisiniz.');
        } else {
            $('[data-valmsg-for="ParaflayanKisiIds"]').text('');
        }
        return isValid;
    }

    $('input, select, textarea').not('#UserId').on('change keyup', checkFormValidity);
    $('select[name="ImzalayanKisiIds"], select[name="ParaflayanKisiIds"]').on('change', checkFormValidity);

    $('#frmContract').submit(function (e) {
        tinymce.triggerSave();
        if (!$(this).valid() || !checkFormValidity()) {
            e.preventDefault();
            Swal.fire({
                icon: 'error',
                title: 'Hata',
                text: 'Zorunlu alanları doldurmalısınız!'
            });
        }
    });

    checkFormValidity();
});
