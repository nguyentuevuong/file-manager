tinymce.PluginManager.add('tinyfile', function (editor) {
    function openmanager() {
        var tfm_path = "/nearticle/filemanager";
        editor.windowManager.open({
            title: 'Quản lý tập tin',
            file: tfm_path || "/scripts/tinymce/plugins/tinyfile",
            filetype: 'all',
            classes: 'tinyfile',
            width: 900,
            height: 600,
            inline: 1
        })
    }

    editor.addButton('tinyfile', {
        icon: 'browse',
        tooltip: 'Chèn tập tin',
        onclick: openmanager,
        stateSelector: 'img:not([data-mce-object])'
    });

    editor.addMenuItem('tinyfile', {
        icon: 'browse',
        text: 'Chèn tập tin',
        onclick: openmanager,
        context: 'insert',
        prependToContext: true
    })
});