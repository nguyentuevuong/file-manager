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

angular.module('FileApp', [])
    .directive('ngCss', ['$window', function ($window) {
        return {
            restrict: 'E',
            template: '.container-files .card{ width: calc({{cardWidth}}% - 4px); margin: 2px;}',
            link: function link($scope, $element, $attrs) {
                debugger;
                angular.element($window).bind('resize', function () {
                    $scope.cardWidth = (100 / parseInt($window.innerWidth / 200));
                    debugger;
                    scope.$digest();
                });
            }
        };
    }])
    .directive("ngFile", [function () {
        return {
            scope: {
                ngFile: "="
            },
            link: function ($scope, $element, $attrs) {
                $element.bind("change", function (changeEvent) {
                    var files = [];
                    for (var i = 0, $file; $file = changeEvent.target.files[i]; i++) {

                        var reader = new FileReader();
                        reader.file = {
                            name: $file.name,
                            size: $file.size
                        }
                        reader.onload = function (event) {
                            files.push({
                                name: this.file.name,
                                size: this.file.size,
                                data: event.target.result
                            });
                            $scope.$apply(function () {
                                $scope.ngFile = files;
                            });
                        };

                        reader.readAsDataURL($file);
                    }
                });
            }
        }
    }])
    .controller('ManagerController', ['$http', '$scope', '$window', function ($http, $scope, $window) {
        //$scope.files;

        $scope.abc = function () {

        }

        angular.element($window).on('resize', function () {
            $scope.cardWidth = $window.innerWidth / 200;
            $scope.$apply();
        });
    }]);