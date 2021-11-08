var rn = {
    setPath: function (subPath) {
        var currentPath = rn.getPath() || '\\';
        if (subPath == '..') {
            if (currentPath != '') {
                currentPath = currentPath.substr(0, currentPath.lastIndexOf('\\'));
            }
        } else {
            currentPath += '\\' + subPath;
        }
        currentPath = currentPath.replace(/\\+/g, '\\');
        localStorage.setItem('__rnpath__', currentPath);
    },
    getPath: function () {
        return localStorage.getItem('__rnpath__') || '\\';
    },
    files: [],
    'ajax': function (options) {
        options = options || {};

        options.data = options.data || {};
        options.url = options.url || location.pathname;
        options.success = options.success || function (resp) { };
        options.error = options.error || function (resp) { };

        var xhttp = new XMLHttpRequest();
        xhttp.open('POST', options.url, true);
        xhttp.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200 && typeof options.success === 'function') {
                try {
                    options.success(JSON.parse(this.responseText));
                } catch (ex) {
                    options.success(this.responseText);
                }
            } else {
                options.error({ status: this.status, text: this.statusText });
            }
        };
        xhttp.setRequestHeader('Content-type', 'application/x-www-form-urlencoded');
        if (typeof options.data === 'object') {
            var parseString = [];
            for (var key in options.data) {
                parseString.push(encodeURIComponent(key) + '=' + encodeURIComponent(options.data[key]));
            }
            xhttp.send(parseString.join('&'));
        } else {
            xhttp.send(options.data);
        }
    },
    'addfolder': function (folderName, callback) {
        rn.ajax({
            data: {
                mode: 'addfolder',
                fileName: folderName,
                currentPath: rn.getPath()
            },
            success: function (resp) {
                if (callback && typeof callback == 'function') {
                    callback(resp);
                }
            }
        });
    },
    'delete': function (fileName, callback) {
        rn.ajax({
            data: {
                mode: 'delete',
                fileName: fileName,
                currentPath: rn.getPath()
            },
            success: function (resp) {
                if (callback && typeof callback == 'function') {
                    callback(resp);
                }
            }
        });
    },
    'upload': function (fileName, fileData, thumb, callback) {
        rn.ajax({
            data: {
                fileName: fileName,
                fileData: fileData,
                currentPath: rn.getPath(),
                mode: (typeof thumb == 'boolean' && thumb) ? 'uploadthumb' : 'upload'
            },
            success: function (resp) {
                if (typeof thumb == 'function') {
                    thumb(resp);
                } else if (typeof callback == 'function') {
                    callback(resp);
                }
            }
        });
    },
    'getfiles': function (filter) {
        var currentPath = rn.getPath();
        rn.ajax({
            data: {
                mode: 'get',
                filter: filter || '*.*',
                currentPath: rn.getPath()
            },
            success: function (resp) {
                rn.files = resp;
                document.getElementById('filezone').innerHTML = '';
                rn.files.forEach(function (file) {
                    file.id = 'file_' + String.fromCharCode(65 + Math.floor(Math.random() * 26)) + '_' + new Date().getTime();
                    rn.addfile(file);
                });
            }
        });
        rn.breadcrumb();
    },
    'download': function (fileName) {
        rn.ajax({
            data: {
                mode: 'download',
                fileName: fileName,
                currentPath: rn.getPath()
            },
            success: function (resp) {
                if (resp) {
                    var blob = new Blob(resp, { type: "octet/stream" }),
                        url = window.URL.createObjectURL(blob);

                    var a = document.createElement("a");
                    a.href = url;
                    a.download = fileName;
                    a.click();
                    window.URL.revokeObjectURL(url);
                }
            }
        });
    },
    'addfiles': function (files) {
        var filezone = document.getElementById('filezone');
        for (var i = 0, file; file = files[i]; i++) {
            if (file.type.match(/image.*/)) {
                if (file.size > 2097152) {
                    rn.toastr('Tập tin vừa tải lên có dung lượng vượt quá 2MB!');
                    return;
                }
                var reader = new FileReader();
                reader.file = file;
                reader.onload = function (event) {
                    var _file = this.file;
                    if (filezone) {
                        var _img = document.createElement('img');
                        _img.src = event.target.result;

                        _img.resize(300, 300, function (result) {
                            var uploadfile = {
                                type: 1,
                                name: _file.name,
                                thumb: result,
                                data: event.target.result,
                                url: ''
                            };
                            rn.addfile(uploadfile);
                            rn.files.push(uploadfile);
                        });
                    }
                }
                reader.readAsDataURL(file);
            } else {
                rn.toastr('Tập tin vừa tải lên không phải là ảnh!');
            }
        }
    },
    'addfile': function (file) {
        if (file && typeof file === 'object') {
            var filezone = document.getElementById('filezone');
            if (filezone) {

                var card = document.createElement('div');

                file.id = 'file_' + String.fromCharCode(65 + Math.floor(Math.random() * 26)) + '_' + new Date().getTime();

                card.className = 'card' + (file.checked ? ' checked' : '');

                var img = document.createElement('img');
                img.className = 'img-fluid img-responsive';
                img.src = file.thumb;

                card.appendChild(img);
                var block = document.createElement('div');
                block.className = 'card-block';
                var title = document.createElement('h4');
                title.className = 'card-title';
                title.innerHTML = file.name;
                block.appendChild(title);
                card.appendChild(block);

                var checkboxContainer = document.createElement('fieldset');
                checkboxContainer.setAttribute('class', 'form-group');

                var checkbox = document.createElement('input');
                checkbox.setAttribute('id', file.id);
                checkbox.setAttribute('type', 'checkbox');
                checkbox.setAttribute('class', 'filled-in');
                checkbox.checked = file.checked || false;

                if (!file.type) {
                    checkboxContainer.addEventListener('click', function (event) {
                        file.checked = checkbox.checked = !(checkbox.checked || false);
                        if (file.checked) {
                            card.className = card.className + ' checked';
                        } else {
                            card.className = card.className.replace(' checked', '');
                        }
                        rn.showButton();
                        event.stopPropagation();
                        event.preventDefault();
                    });
                }

                checkbox.addEventListener('change', function (event) {
                    file.checked = this.checked;
                    if (file.checked) {
                        card.className = card.className + ' checked';
                    } else {
                        card.className = card.className.replace(' checked', '');
                    }
                    rn.showButton();
                });

                card.addEventListener('click', function (event) {
                    file.checked = (checkbox.checked = !checkbox.checked);
                    if (file.checked) {
                        this.className = this.className + ' checked';
                    } else {
                        this.className = this.className.replace(' checked', '');
                    }
                    rn.showButton();
                });

                card.addEventListener('dblclick', function (event) {
                    if (!file.type) {
                        rn.setPath(file.name);
                        rn.getfiles('');
                    }
                });

                var label = document.createElement('label');
                label.setAttribute('for', file.id);

                checkboxContainer.appendChild(checkbox);
                checkboxContainer.appendChild(label);

                if (file.data) {
                    if (rn.autoupload) {
                        rn.upload(file.name, file.data, false, function (resp) {
                            if (resp) {
                                rn.upload(file.name, file.thumb, true, function (resp) {
                                    if (resp.status) {
                                        file.url = resp.url;
                                        file.data = undefined;
                                    } else {
                                        file.url = file.data;
                                    }
                                });
                            }
                        });
                    } else {
                        var uploadicon = document.createElement('i');
                        uploadicon.className = 'fa fa-upload';
                        uploadicon.id = file.id + '_upload_icon';
                        card.appendChild(uploadicon);
                        uploadicon.addEventListener('click', function (event) {
                            event.stopPropagation();
                            event.preventDefault();
                            rn.upload(file.name, file.data, false, function (resp) {
                                if (resp) {
                                    rn.upload(file.name, file.thumb, true, function (resp) {
                                        if (resp.status) {
                                            uploadicon.remove();
                                            file.url = resp.url;
                                            file.data = undefined;
                                        } else {
                                            file.url = file.data;
                                        }
                                    });
                                }
                            });
                        });
                    }
                }

                card.appendChild(checkboxContainer);

                filezone.appendChild(card);
                rn.showButton();
            }
        }
    },
    'showButton': function () {
        var _checked = rn.files.filter(function (file) { return file.checked; }).length > 0;
        if (_checked) {
            document.getElementById('btnupload').style.opacity = 1;
            document.getElementById('btndelete').style.opacity = 1;
            document.getElementById('btnadd2App').style.opacity = 1;
        }
        else {
            document.getElementById('btnupload').style.opacity = 0.3;
            document.getElementById('btndelete').style.opacity = 0.3;
            document.getElementById('btnadd2App').style.opacity = 0.3;
        }
    },
    'toastr': function (text) {
        var toastr = document.getElementById('toast-container');
        var tsuccess = document.createElement('div');
        tsuccess.className = 'toast toast-success';

        var tmessage = document.createElement('div');
        tmessage.className = 'toast-message';
        tmessage.innerText = text;

        tsuccess.appendChild(tmessage);
        toastr.appendChild(tsuccess);

        tsuccess.addEventListener('click', function (event) {
            this.remove();
        });

        setTimeout(function (toastr) { toastr.remove(); }, 6000, tsuccess);
    },
    'breadcrumb': function () {
        var currentPath = rn.getPath().split('\\'),
            breadcrumb = document.getElementById('breadcrumb');

        breadcrumb.innerHTML = "";
        var backli = document.createElement('li');
        backli.className = 'breadcrumb-item';
        backli.innerHTML = '<i class="fa fa fa-arrow-circle-left"></i>';
        if (rn.getPath() == '\\') {
            backli.style.opacity = '0.3';
        }
        backli.onclick = function (event) {
            event.preventDefault();
            event.stopPropagation();
            event.stopImmediatePropagation();
            rn.setPath('..');
            rn.getfiles();
        };
        breadcrumb.appendChild(backli);

        var rootli = document.createElement('li');
        rootli.className = 'breadcrumb-item';
        rootli.innerText = 'Root';
        breadcrumb.appendChild(rootli);

        for (var i in currentPath) {
            if (currentPath[i]) {
                var li = document.createElement('li');
                li.className = 'breadcrumb-item';
                li.innerText = currentPath[i];
                breadcrumb.appendChild(li);
            }
        }
    },
    latin_map: {
        "Á": "A", "Ă": "A", "Ắ": "A", "Ặ": "A", "Ằ": "A", "Ẳ": "A", "Ẵ": "A", "Ǎ": "A", "Â": "A", "Ấ": "A", "Ậ": "A", "Ầ": "A", "Ẩ": "A", "Ẫ": "A", "Ä": "A", "Ǟ": "A", "Ȧ": "A", "Ǡ": "A", "Ạ": "A", "Ȁ": "A", "À": "A", "Ả": "A", "Ȃ": "A", "Ā": "A", "Ą": "A", "Å": "A", "Ǻ": "A", "Ḁ": "A", "Ⱥ": "A", "Ã": "A", "Ꜳ": "AA", "Æ": "AE", "Ǽ": "AE", "Ǣ": "AE", "Ꜵ": "AO", "Ꜷ": "AU", "Ꜹ": "AV", "Ꜻ": "AV", "Ꜽ": "AY",
        "Ḃ": "B", "Ḅ": "B", "Ɓ": "B", "Ḇ": "B", "Ƀ": "B", "Ƃ": "B",
        "Ć": "C", "Č": "C", "Ç": "C", "Ḉ": "C", "Ĉ": "C", "Ċ": "C", "Ƈ": "C", "Ȼ": "C",
        "Ď": "D", "Ḑ": "D", "Ḓ": "D", "Ḋ": "D", "Ḍ": "D", "Ɗ": "D", "Ḏ": "D", "ǲ": "D", "ǅ": "D", "Đ": "D", "Ƌ": "D", "Ǳ": "DZ", "Ǆ": "DZ",
        "É": "E", "Ĕ": "E", "Ě": "E", "Ȩ": "E", "Ḝ": "E", "Ê": "E", "Ế": "E", "Ệ": "E", "Ề": "E", "Ể": "E", "Ễ": "E", "Ḙ": "E", "Ë": "E", "Ė": "E", "Ẹ": "E", "Ȅ": "E", "È": "E", "Ẻ": "E", "Ȇ": "E", "Ē": "E", "Ḗ": "E", "Ḕ": "E", "Ę": "E", "Ɇ": "E", "Ẽ": "E", "Ḛ": "E", "Ꝫ": "ET",
        "Ḟ": "F", "Ƒ": "F",
        "Ǵ": "G", "Ğ": "G", "Ǧ": "G", "Ģ": "G", "Ĝ": "G", "Ġ": "G", "Ɠ": "G", "Ḡ": "G", "Ǥ": "G",
        "Ḫ": "H", "Ȟ": "H", "Ḩ": "H", "Ĥ": "H", "Ⱨ": "H", "Ḧ": "H", "Ḣ": "H", "Ḥ": "H", "Ħ": "H",
        "Í": "I", "Ĭ": "I", "Ǐ": "I", "Î": "I", "Ï": "I", "Ḯ": "I", "İ": "I", "Ị": "I", "Ȉ": "I", "Ì": "I", "Ỉ": "I", "Ȋ": "I", "Ī": "I", "Į": "I", "Ɨ": "I", "Ĩ": "I", "Ḭ": "I",
        "Ꝺ": "D", "Ꝼ": "F", "Ᵹ": "G", "Ꞃ": "R", "Ꞅ": "S", "Ꞇ": "T", "Ꝭ": "IS",
        "Ĵ": "J", "Ɉ": "J",
        "Ḱ": "K", "Ǩ": "K", "Ķ": "K", "Ⱪ": "K", "Ꝃ": "K", "Ḳ": "K", "Ƙ": "K", "Ḵ": "K", "Ꝁ": "K", "Ꝅ": "K",
        "Ĺ": "L", "Ƚ": "L", "Ľ": "L", "Ļ": "L", "Ḽ": "L", "Ḷ": "L", "Ḹ": "L", "Ⱡ": "L", "Ꝉ": "L", "Ḻ": "L", "Ŀ": "L", "Ɫ": "L", "ǈ": "L", "Ł": "L", "Ǉ": "LJ",
        "Ḿ": "M", "Ṁ": "M", "Ṃ": "M", "Ɱ": "M",
        "Ń": "N", "Ň": "N", "Ņ": "N", "Ṋ": "N", "Ṅ": "N", "Ṇ": "N", "Ǹ": "N", "Ɲ": "N", "Ṉ": "N", "Ƞ": "N", "ǋ": "N", "Ñ": "N", "Ǌ": "NJ",
        "Ó": "O", "Ŏ": "O", "Ǒ": "O", "Ô": "O", "Ố": "O", "Ộ": "O", "Ồ": "O", "Ổ": "O", "Ỗ": "O", "Ö": "O", "Ȫ": "O", "Ȯ": "O", "Ȱ": "O", "Ọ": "O", "Ő": "O", "Ȍ": "O", "Ò": "O", "Ỏ": "O", "Ơ": "O", "Ớ": "O", "Ợ": "O", "Ờ": "O", "Ở": "O", "Ỡ": "O", "Ȏ": "O", "Ꝋ": "O", "Ꝍ": "O", "Ō": "O", "Ṓ": "O", "Ṑ": "O", "Ɵ": "O", "Ǫ": "O", "Ǭ": "O", "Ø": "O", "Ǿ": "O", "Õ": "O", "Ṍ": "O", "Ṏ": "O", "Ȭ": "O", "Ƣ": "OI", "Ꝏ": "OO", "Ɛ": "E", "Ɔ": "O", "Ȣ": "OU",
        "Ṕ": "P", "Ṗ": "P", "Ꝓ": "P", "Ƥ": "P", "Ꝕ": "P", "Ᵽ": "P", "Ꝑ": "P",
        "Ꝙ": "Q", "Ꝗ": "Q",
        "Ŕ": "R", "Ř": "R", "Ŗ": "R", "Ṙ": "R", "Ṛ": "R", "Ṝ": "R", "Ȑ": "R", "Ȓ": "R", "Ṟ": "R", "Ɍ": "R", "Ɽ": "R", "Ꜿ": "C", "Ǝ": "E",
        "Ś": "S", "Ṥ": "S", "Š": "S", "Ṧ": "S", "Ş": "S", "Ŝ": "S", "Ș": "S", "Ṡ": "S", "Ṣ": "S", "Ṩ": "S",
        "Ť": "T", "Ţ": "T", "Ṱ": "T", "Ț": "T", "Ⱦ": "T", "Ṫ": "T", "Ṭ": "T", "Ƭ": "T", "Ṯ": "T", "Ʈ": "T", "Ŧ": "T", "Ɐ": "A", "Ꞁ": "L", "Ɯ": "M", "Ʌ": "V", "Ꜩ": "TZ",
        "Ú": "U", "Ŭ": "U", "Ǔ": "U", "Û": "U", "Ṷ": "U", "Ü": "U", "Ǘ": "U", "Ǚ": "U", "Ǜ": "U", "Ǖ": "U", "Ṳ": "U", "Ụ": "U", "Ű": "U", "Ȕ": "U", "Ù": "U", "Ủ": "U", "Ư": "U", "Ứ": "U", "Ự": "U", "Ừ": "U", "Ử": "U", "Ữ": "U", "Ȗ": "U", "Ū": "U", "Ṻ": "U", "Ų": "U", "Ů": "U", "Ũ": "U", "Ṹ": "U", "Ṵ": "U",
        "Ꝟ": "V", "Ṿ": "V", "Ʋ": "V", "Ṽ": "V", "Ꝡ": "VY",
        "Ẃ": "W", "Ŵ": "W", "Ẅ": "W", "Ẇ": "W", "Ẉ": "W", "Ẁ": "W", "Ⱳ": "W",
        "Ẍ": "X", "Ẋ": "X",
        "Ý": "Y", "Ŷ": "Y", "Ÿ": "Y", "Ẏ": "Y", "Ỵ": "Y", "Ỳ": "Y", "Ƴ": "Y", "Ỷ": "Y", "Ỿ": "Y", "Ȳ": "Y", "Ɏ": "Y", "Ỹ": "Y",
        "Ź": "Z", "Ž": "Z", "Ẑ": "Z", "Ⱬ": "Z", "Ż": "Z", "Ẓ": "Z", "Ȥ": "Z", "Ẕ": "Z", "Ƶ": "Z",
        "Ĳ": "IJ", "Œ": "OE",
        "ᴀ": "A", "ᴁ": "AE",
        "ʙ": "B", "ᴃ": "B",
        "ᴄ": "C",
        "ᴅ": "D",
        "ᴇ": "E",
        "ꜰ": "F",
        "ɢ": "G", "ʛ": "G",
        "ʜ": "H", "ɪ": "I", "ʁ": "R", "ᴊ": "J", "ᴋ": "K", "ʟ": "L", "ᴌ": "L", "ᴍ": "M", "ɴ": "N",
        "ᴏ": "O", "ɶ": "OE", "ᴐ": "O", "ᴕ": "OU",
        "ᴘ": "P", "ʀ": "R", "ᴎ": "N", "ᴙ": "R",
        "ꜱ": "S", "ᴛ": "T", "ⱻ": "E", "ᴚ": "R", "ᴜ": "U",
        "ᴠ": "V", "ᴡ": "W", "ʏ": "Y", "ᴢ": "Z",
        "á": "a", "ă": "a", "ắ": "a", "ặ": "a", "ằ": "a", "ẳ": "a", "ẵ": "a", "ǎ": "a", "â": "a", "ấ": "a", "ậ": "a", "ầ": "a", "ẩ": "a", "ẫ": "a", "ä": "a", "ǟ": "a", "ȧ": "a", "ǡ": "a", "ạ": "a", "ȁ": "a", "à": "a", "ả": "a", "ȃ": "a", "ā": "a", "ą": "a", "ᶏ": "a", "ẚ": "a", "å": "a", "ǻ": "a", "ḁ": "a", "ⱥ": "a", "ã": "a", "ꜳ": "aa", "æ": "ae", "ǽ": "ae", "ǣ": "ae", "ꜵ": "ao", "ꜷ": "au", "ꜹ": "av", "ꜻ": "av", "ꜽ": "ay",
        "ḃ": "b", "ḅ": "b", "ɓ": "b", "ḇ": "b", "ᵬ": "b", "ᶀ": "b", "ƀ": "b", "ƃ": "b", "ɵ": "o", "ć": "c", "č": "c", "ç": "c", "ḉ": "c", "ĉ": "c", "ɕ": "c", "ċ": "c", "ƈ": "c", "ȼ": "c", "ď": "d", "ḑ": "d", "ḓ": "d", "ȡ": "d", "ḋ": "d", "ḍ": "d", "ɗ": "d", "ᶑ": "d", "ḏ": "d", "ᵭ": "d", "ᶁ": "d", "đ": "d", "ɖ": "d", "ƌ": "d", "ı": "i", "ȷ": "j", "ɟ": "j", "ʄ": "j", "ǳ": "dz", "ǆ": "dz",
        "é": "e", "ĕ": "e", "ě": "e", "ȩ": "e", "ḝ": "e", "ê": "e", "ế": "e", "ệ": "e", "ề": "e", "ể": "e", "ễ": "e", "ḙ": "e", "ë": "e", "ė": "e", "ẹ": "e", "ȅ": "e", "è": "e", "ẻ": "e", "ȇ": "e", "ē": "e", "ḗ": "e", "ḕ": "e", "ⱸ": "e", "ę": "e", "ᶒ": "e", "ɇ": "e", "ẽ": "e", "ḛ": "e", "ꝫ": "et", "ḟ": "f", "ƒ": "f", "ᵮ": "f", "ᶂ": "f", "ǵ": "g", "ğ": "g", "ǧ": "g", "ģ": "g", "ĝ": "g", "ġ": "g", "ɠ": "g", "ḡ": "g", "ᶃ": "g", "ǥ": "g", "ḫ": "h", "ȟ": "h", "ḩ": "h", "ĥ": "h", "ⱨ": "h", "ḧ": "h", "ḣ": "h", "ḥ": "h", "ɦ": "h", "ẖ": "h", "ħ": "h", "ƕ": "hv", "í": "i", "ĭ": "i", "ǐ": "i", "î": "i", "ï": "i", "ḯ": "i", "ị": "i", "ȉ": "i", "ì": "i", "ỉ": "i", "ȋ": "i", "ī": "i", "į": "i", "ᶖ": "i", "ɨ": "i", "ĩ": "i", "ḭ": "i", "ꝺ": "d", "ꝼ": "f", "ᵹ": "g", "ꞃ": "r", "ꞅ": "s", "ꞇ": "t", "ꝭ": "is", "ǰ": "j", "ĵ": "j", "ʝ": "j", "ɉ": "j", "ḱ": "k", "ǩ": "k", "ķ": "k", "ⱪ": "k", "ꝃ": "k", "ḳ": "k", "ƙ": "k", "ḵ": "k", "ᶄ": "k", "ꝁ": "k", "ꝅ": "k", "ĺ": "l", "ƚ": "l", "ɬ": "l", "ľ": "l", "ļ": "l", "ḽ": "l", "ȴ": "l", "ḷ": "l", "ḹ": "l", "ⱡ": "l", "ꝉ": "l", "ḻ": "l", "ŀ": "l", "ɫ": "l", "ᶅ": "l", "ɭ": "l", "ł": "l", "ǉ": "lj", "ſ": "s", "ẜ": "s", "ẛ": "s", "ẝ": "s", "ḿ": "m", "ṁ": "m", "ṃ": "m", "ɱ": "m", "ᵯ": "m", "ᶆ": "m", "ń": "n", "ň": "n", "ņ": "n", "ṋ": "n", "ȵ": "n", "ṅ": "n", "ṇ": "n", "ǹ": "n", "ɲ": "n", "ṉ": "n", "ƞ": "n", "ᵰ": "n", "ᶇ": "n", "ɳ": "n", "ñ": "n", "ǌ": "nj", "ó": "o", "ŏ": "o", "ǒ": "o", "ô": "o", "ố": "o", "ộ": "o", "ồ": "o", "ổ": "o", "ỗ": "o", "ö": "o", "ȫ": "o", "ȯ": "o", "ȱ": "o", "ọ": "o", "ő": "o", "ȍ": "o", "ò": "o", "ỏ": "o", "ơ": "o", "ớ": "o", "ợ": "o", "ờ": "o", "ở": "o", "ỡ": "o", "ȏ": "o", "ꝋ": "o", "ꝍ": "o", "ⱺ": "o", "ō": "o", "ṓ": "o", "ṑ": "o", "ǫ": "o", "ǭ": "o", "ø": "o", "ǿ": "o", "õ": "o", "ṍ": "o", "ṏ": "o", "ȭ": "o", "ƣ": "oi", "ꝏ": "oo", "ɛ": "e", "ᶓ": "e", "ɔ": "o", "ᶗ": "o", "ȣ": "ou", "ṕ": "p", "ṗ": "p", "ꝓ": "p", "ƥ": "p", "ᵱ": "p", "ᶈ": "p", "ꝕ": "p", "ᵽ": "p", "ꝑ": "p", "ꝙ": "q", "ʠ": "q", "ɋ": "q", "ꝗ": "q", "ŕ": "r", "ř": "r", "ŗ": "r", "ṙ": "r", "ṛ": "r", "ṝ": "r", "ȑ": "r", "ɾ": "r", "ᵳ": "r", "ȓ": "r", "ṟ": "r", "ɼ": "r", "ᵲ": "r", "ᶉ": "r", "ɍ": "r", "ɽ": "r", "ↄ": "c", "ꜿ": "c", "ɘ": "e", "ɿ": "r", "ś": "s", "ṥ": "s", "š": "s", "ṧ": "s", "ş": "s", "ŝ": "s", "ș": "s", "ṡ": "s", "ṣ": "s", "ṩ": "s", "ʂ": "s", "ᵴ": "s", "ᶊ": "s", "ȿ": "s", "ɡ": "g", "ᴑ": "o", "ᴓ": "o", "ᴝ": "u", "ť": "t", "ţ": "t", "ṱ": "t", "ț": "t", "ȶ": "t", "ẗ": "t", "ⱦ": "t", "ṫ": "t", "ṭ": "t", "ƭ": "t", "ṯ": "t", "ᵵ": "t", "ƫ": "t", "ʈ": "t", "ŧ": "t", "ᵺ": "th", "ɐ": "a", "ᴂ": "ae", "ǝ": "e", "ᵷ": "g", "ɥ": "h", "ʮ": "h", "ʯ": "h", "ᴉ": "i", "ʞ": "k", "ꞁ": "l", "ɯ": "m", "ɰ": "m", "ᴔ": "oe", "ɹ": "r", "ɻ": "r", "ɺ": "r", "ⱹ": "r", "ʇ": "t", "ʌ": "v", "ʍ": "w", "ʎ": "y", "ꜩ": "tz", "ú": "u", "ŭ": "u", "ǔ": "u", "û": "u", "ṷ": "u", "ü": "u", "ǘ": "u", "ǚ": "u", "ǜ": "u", "ǖ": "u", "ṳ": "u", "ụ": "u", "ű": "u", "ȕ": "u", "ù": "u", "ủ": "u", "ư": "u", "ứ": "u", "ự": "u", "ừ": "u", "ử": "u", "ữ": "u", "ȗ": "u", "ū": "u", "ṻ": "u", "ų": "u", "ᶙ": "u", "ů": "u", "ũ": "u", "ṹ": "u", "ṵ": "u", "ᵫ": "ue", "ꝸ": "um", "ⱴ": "v", "ꝟ": "v", "ṿ": "v", "ʋ": "v", "ᶌ": "v", "ⱱ": "v", "ṽ": "v", "ꝡ": "vy", "ẃ": "w", "ŵ": "w", "ẅ": "w", "ẇ": "w", "ẉ": "w", "ẁ": "w", "ⱳ": "w", "ẘ": "w", "ẍ": "x", "ẋ": "x", "ᶍ": "x", "ý": "y", "ŷ": "y", "ÿ": "y", "ẏ": "y", "ỵ": "y", "ỳ": "y", "ƴ": "y", "ỷ": "y", "ỿ": "y", "ȳ": "y", "ẙ": "y", "ɏ": "y", "ỹ": "y", "ź": "z", "ž": "z", "ẑ": "z", "ʑ": "z", "ⱬ": "z", "ż": "z", "ẓ": "z", "ȥ": "z", "ẕ": "z", "ᵶ": "z", "ᶎ": "z", "ʐ": "z", "ƶ": "z", "ɀ": "z", "ﬀ": "ff", "ﬃ": "ffi", "ﬄ": "ffl", "ﬁ": "fi", "ﬂ": "fl", "ĳ": "ij", "œ": "oe", "ﬆ": "st", "ₐ": "a", "ₑ": "e", "ᵢ": "i", "ⱼ": "j", "ₒ": "o", "ᵣ": "r", "ᵤ": "u", "ᵥ": "v", "ₓ": "x"
    }
};

// Check for the various File API support.
if (window.File && window.FileReader && window.FileList && window.Blob) {
    // Great success! All the File APIs are supported.
} else {
    alert('The File APIs are not fully supported in this browser.');
}

document.addEventListener('dragover', function (event) {
    event.preventDefault();
    event.stopPropagation();
    event.stopImmediatePropagation();

    event.dataTransfer.dropEffect = 'copy';
});

document.addEventListener('drop', function (event) {
    event.preventDefault();
    event.stopPropagation();
    event.stopImmediatePropagation();

    rn.addfiles(event.dataTransfer.files);
});

var resizeFunc = function (event) {
    var style = document.getElementById('rnstyle');
    var wiw = document.getElementById('uploadzone').offsetWidth - 30;
    style.innerText = '.container-files .card{ width: calc(' + (100 / parseInt(wiw / 200)) + '% - 4px); margin: 2px;}';
};

window.addEventListener('load', resizeFunc);

window.addEventListener('resize', resizeFunc);

window.addEventListener('load', function () {
    // init first load files
    rn.getfiles('');

    if (rn.autoupload) {
        document.getElementById('floatbtnupload').style.display = 'none';
    }

    document.getElementById('btnopen').addEventListener('click', function (event) {
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();
        document.getElementById('uploadinput').click();
    });

    document.getElementById('uploadinput').addEventListener('change', function (event) {
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();

        rn.addfiles(this.files);
    });

    document.getElementById('btndelete').addEventListener('click', function (event) {
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();

        document.getElementById('filezone').innerHTML = '';
        var fileNames = [];
        rn.files = rn.files.filter(function (file) {
            if (file.checked && !file.data) {
                // delete file by ajax
                rn.delete(file.name, function (resp) {
                    rn.toastr('Xóa thành công ' + (!file.type ? 'thư mục' : 'tệp tin') + ': ' + file.name);
                });
            }
            return !file.checked;
        });

        rn.files.forEach(function (file) {
            rn.addfile(file);
        });
    });

    document.getElementById('btnupload').addEventListener('click', function (event) {
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();

        rn.files.forEach(function (file, i) {
            if (file.checked && file.data) {
                rn.upload(file.name, file.thumb, true, function (resp) {
                    if (resp) {
                        rn.upload(file.name, file.data, false, function (resp) {
                            file.data = undefined;
                            document.getElementById(file.id + '_upload_icon').remove();
                        });
                    }
                });
            }
        });
    });

    document.getElementById('btnaddfolder').addEventListener('click', function (event) {
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();

        var folderName = (prompt('Nhập tên thư mục:') || '').StripDiacritics().replace(/\s/g, '_').trim();
        if (folderName) {
            rn.addfolder(folderName, function (resp) {
                rn.getfiles('');
            });
        };
    });

    document.getElementById('btnadd2App').addEventListener('click', function (event) {
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();

        var files = rn.files.filter(function (item) { return item.type && item.checked; });
        if (top.tinyfile && top.tinyfile.fillTo) {
            var img = [];
            files.map(function (item) { img.push(item.name); });
            top.tinyfile.fillTo.value = img.join(', ');
            var closeBtn = window.top.parent.document.getElementsByClassName('close-button')[0];
            if (closeBtn) {
                closeBtn.click();
            }
            return;
        }

        if (top.tinyMCE && top.tinyMCE.activeEditor) {
            var img = '';
            files.map(function (item) {
                img += '<img src="' + item.url + '" alt="' + item.name + '" />';
            });
            top.tinyMCE.activeEditor.execCommand('mceInsertContent', false, img);

            var closeBtn = window.top.parent.document.getElementsByClassName('mce-close')[0];
            if (closeBtn) {
                closeBtn.click();
            }
            return;
        }
    });
});

String.prototype.StripDiacritics = function () {
    return this.replace(/[^A-Za-z0-9\[\] ]/g, function (a) { return rn.latin_map[a] || a })
};

Element.prototype.resize = function (w, h, callback) {
    if (this.tagName != 'IMG' || (!w && !h)) { return; }
    var img = this,
        c = document.createElement("canvas"),
        cx = c.getContext("2d"),
        ima = new Image();
    ima.onload = function () {
        var pl = 0, pt = 0, rw = 0, rh = 0, rt1 = ima.width / ima.height;

        c.width = (rw = (w = w || (h * rt1)));
        c.height = (rh = (h = h || (w / rt1)));

        var rt2 = w / h;
        if (rt1 < rt2) {
            rw = h * rt1;
            pl = (w - rw) / 2;
        } else if (rt1 > rt2) {
            rh = w / rt1;
            pt = (h - rh) / 2;
        }

        cx.drawImage(img, pl, pt, rw, rh);
        if (callback && typeof callback == 'function') {
            callback(c.toDataURL());
        }
    };
    ima.src = img.src;
};