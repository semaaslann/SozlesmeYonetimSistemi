$(document).ready(function () {
    $('.select2').select2({
        placeholder: "Kişi seçiniz",
        allowClear: true
    });

    // Tarih değiştiğinde sözleşme süresini hesapla
    $('#ImzaOnayTarihi, #FinisDate').on('change', function () {
        console.log('Tarih değişti'); // Debug için
        var imzaTarihi = $('#ImzaOnayTarihi').val();
        var bitisTarihi = $('#FinisDate').val();

        if (imzaTarihi && bitisTarihi) {
            console.log('Imza Tarihi:', imzaTarihi, 'Bitiş Tarihi:', bitisTarihi); // Debug için
            var imzaDate = new Date(imzaTarihi);
            var bitisDate = new Date(bitisTarihi);

            // Tarihlerin geçerli olduğundan emin ol
            if (isNaN(imzaDate) || isNaN(bitisDate)) {
                console.log('Geçersiz tarih formatı');
                $('#SozlesmeSuresi').val('');
                return;
            }

            // Bitiş tarihi imza tarihinden önce olamaz
            if (bitisDate < imzaDate) {
                alert('Bitiş tarihi, imza/onay tarihinden önce olamaz.');
                $('#FinisDate').val('');
                $('#SozlesmeSuresi').val('');
                return;
            }

            // Gün farkını hesapla
            var timeDiff = bitisDate - imzaDate;
            var daysDiff = Math.ceil(timeDiff / (1000 * 3600 * 24));
            console.log('Hesaplanan Gün:', daysDiff); // Debug için
            $('#SozlesmeSuresi').val(daysDiff + ' gün');
        } else {
            $('#SozlesmeSuresi').val('');
        }
    });

    // Form gönderilmeden önce validasyon
    $('#frmContract').on('submit', function (e) {
        var imzaTarihi = $('#ImzaOnayTarihi').val();
        var bitisTarihi = $('#FinisDate').val();

        if (imzaTarihi && bitisTarihi) {
            var imzaDate = new Date(imzaTarihi);
            var bitisDate = new Date(bitisTarihi);

            if (isNaN(imzaDate) || isNaN(bitisDate)) {
                e.preventDefault();
                alert('Lütfen geçerli tarihler seçin.');
                return;
            }

            if (bitisDate < imzaDate) {
                e.preventDefault();
                alert('Bitiş tarihi, imza/onay tarihinden önce olamaz.');
            }
        }
    });
});