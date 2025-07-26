
function initTinyMCE(textareaId) {
    tinymce.remove(); // Önceki örnekleri temizle
    tinymce.init({
    selector: '#contractContent',
    height: 1123,
    width: 794,
    plugins: [
        'advlist autolink lists link image charmap print preview anchor',
        'searchreplace visualblocks code fullscreen',
        'insertdatetime media table paste code help wordcount'
    ],
    toolbar: 'undo redo | formatselect | fontselect fontsizeselect | ' +
             'bold italic underline strikethrough | alignleft aligncenter alignright alignjustify | ' +
             'bullist numlist outdent indent | table | link image media | removeformat | code',
    menubar: 'file edit insert view format table tools help',
    content_css: '/path/to/tinymce-custom.css',
    font_formats: 'Arial=arial,helvetica,sans-serif; Times New Roman=times new roman,times,serif; Calibri=calibri,sans-serif; Verdana=verdana,geneva,sans-serif',
    fontsize_formats: '8pt 10pt 12pt 14pt 18pt 24pt 36pt',
    branding: false,
    setup: function (editor) {
        editor.on('init', function () {
            editor.getBody().style.backgroundColor = '#fff';
            editor.getBody().style.margin = '25mm';
            editor.getBody().style.maxWidth = '744px';
        });
    },
    content_style: 'body { font-family: Calibri, Arial, sans-serif; font-size: 12pt; line-height: 1.5; margin: 25mm auto; max-width: 744px; } p { margin: 0 0 12pt 0; }',
    force_br_newlines: true,
    force_p_newlines: false,
    forced_root_block: ''
});

document.getElementById('frmContract').addEventListener('submit', function () {
    tinymce.triggerSave();
});

// Dosya yükleme işlevi
document.getElementById('fileUpload').addEventListener('change', function (e) {
    const file = e.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (event) {
            const text = event.target.result;
            tinymce.get('contractContent').setContent(text); // Dosya içeriğini editöre yerleştir
        };
        reader.readAsText(file); // Yalnızca .txt dosyalarını destekler
    }
});
    // BU KISMI EKLEYİN (TinyMCE init'ten sonra)
    //document.getElementById('frmContract').addEventListener('submit', function () {
    //    tinymce.triggerSave();
    //});
    //});
}