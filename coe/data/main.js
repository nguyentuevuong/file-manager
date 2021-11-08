Element.prototype.resize = function (w, h) {
    if (this.tagName != 'IMG' || (!w && !h)) { return; }
    var img = this,
        c = document.createElement("canvas"),
        cx = c.getContext("2d"),
        ima = new Image();

    ima.src = img.src;

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
    return c.toDataURL();
};

File.prototype.getContent = function () {
    var reader = new FileReader(),
        file = {
            name: this.name,
            size: this.size
        };
    reader.onload = function (event) {
        file.data = event.target.result;
    }
    reader.readAsDataURL(this);
    return file;
};

var rn = {
    setPath: function (subPath) {
        var currentPath = rn.getPath() || '\\';
        if (subPath == '..') {
            if (currentPath.indexOf('\\') != currentPath.lastIndexOf('\\')) {
                currentPath = currentPath.substr(0, currentPath.lastIndexOf('\\'));
            }
        } else {
            currentPath += '\\' + subPath;
        }
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
        if (currentPath.indexOf('\\') != currentPath.lastIndexOf('\\')) {
            document.getElementById('btnback').style.opacity = 1;
        } else {
            document.getElementById('btnback').style.opacity = 0.3;
        }

        if (rn.autoupload) {
            document.getElementById('floatbtnupload').className = 'hidden-sm-up';
        }

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
                        var uploadfile = {
                            type: 1,
                            name: _file.name,
                            thumb: _img.resize(160, 160),
                            data: event.target.result
                        };
                        rn.addfile(uploadfile);
                        rn.files.push(uploadfile);
                    }
                }
                reader.readAsDataURL(file);
                reader.onprogress = function (data) {
                    if (data.lengthComputable) {
                        var progress = parseInt(((data.loaded / data.total) * 100), 10);
                        console.log(progress);
                    }
                }
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
                card.setAttribute('id', file.id);
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

                file.id = 'file_' + String.fromCharCode(65 + Math.floor(Math.random() * 26)) + '_' + new Date().getTime();

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
                    if (file.type) {
                        file.checked = (checkbox.checked = !checkbox.checked);
                        if (file.checked) {
                            this.className = this.className + ' checked';
                        } else {
                            this.className = this.className.replace(' checked', '');
                        }
                        rn.showButton();
                    }
                    else {
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
                                    if (resp) {
                                        file.data = undefined;
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
                                        if (resp) {
                                            uploadicon.remove();
                                            file.data = undefined;
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

        setTimeout(function (toastr) { toastr.remove(); }, 10000, tsuccess);
    },
    'breadcrumb': function () {
        var currentPath = rn.getPath().split('\\'),
            breadcrumb = document.getElementById('breadcrumb');
        breadcrumb.innerHTML = '<li class="breadcrumb-item">Root</li>';
        for (var i in currentPath) {
            if (currentPath[i]) {
                var li = document.createElement('li');
                li.className = 'breadcrumb-item';
                li.innerText = currentPath[i];
                breadcrumb.appendChild(li);
            }
        }
    }
};

// Check for the various File API support.
if (window.File && window.FileReader && window.FileList && window.Blob) {
    // Great success! All the File APIs are supported.
} else {
    alert('The File APIs are not fully supported in this browser.');
}

document.addEventListener('dragover', function (e) {
    e.stopPropagation();
    e.preventDefault();
    e.dataTransfer.dropEffect = 'copy';
});

document.addEventListener('drop', function (e) {
    e.stopPropagation();
    e.preventDefault();
    rn.addfiles(e.dataTransfer.files);
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

    document.getElementById('btnopen').addEventListener('click', function (event) {
        document.getElementById('uploadinput').click();
    });

    document.getElementById('uploadinput').addEventListener('change', function (event) {
        rn.addfiles(this.files);
    });

    document.getElementById('btndelete').addEventListener('click', function (event) {
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
        var folderName = prompt('Nhập tên thư mục:');
        if (folderName) {
            rn.addfolder(folderName, function (resp) {
                rn.getfiles('');
            });
        };
    });

    document.getElementById('btnback').addEventListener('click', function (event) {
        var currentPath = rn.getPath();
        if (currentPath.indexOf('\\') != currentPath.lastIndexOf('\\')) {
            rn.setPath('..');
            rn.getfiles('');
        }
    });

    document.getElementById('btnadd2App').addEventListener('click', function (event) {
        
    });
});