'use strict';

// Declare app level module which depends on views, and components
angular.module('myApp', [
        'ngRoute',
        'myApp.view1',
        'myApp.view2',
        'myApp.version',
        'webSockets',
        'luegg.directives'
    ]).
    config([
        '$routeProvider', function($routeProvider) {
            $routeProvider.otherwise({ redirectTo: '/view1' });
        }
    ])
    .directive('resizeToWindowDimensions', [
        '$window', 'debounce', function ($window, debounce) {
            var w = angular.element($window);
            var debouncedResize = debounce(function () { }, 16, false);

            function link(scope, element) {
                scope.getWindowDimensions = function() {
                    return { 'h': $window.innerHeight, 'w': $window.innerWidth };
                };

                scope.$watch(scope.getWindowDimensions, function(newValue) {
                    var newHeight = (newValue.h + parseInt(scope.heightMod || 0));
                    var newWidth = (newValue.w + parseInt(scope.widthMod || 0));
                    if (angular.isUndefined(scope.adjustHeight) || scope.adjustHeight) {
                        element.css('height', newHeight + 'px');
                    }
                    if (angular.isUndefined(scope.adjustWidth) || scope.adjustWidth) {
                        element.css('width', newWidth + 'px');
                    }
                }, true);
                w.bind('resize', function () {
                    // when the promise is resolved, $scope.apply is automatically called
                    debouncedResize();
                });
            }

            return {
                restrict: 'A',
                scope: {
                    adjustHeight: '=',
                    heightMod: '=',
                    adjustWidth: '=',
                    widthMod: '='
                },
                link: link
            };
        }
    ])

    // Create an AngularJS service called debounce
    .factory('debounce', [
        '$timeout', '$q', function($timeout, $q) {
            // The service is actually this function, which we call with the func
            // that should be debounced and how long to wait in between calls
            return function debounce(func, wait, immediate) {
                var timeout;
                // Create a deferred object that will be resolved when we need to
                // actually call the func
                var deferred = $q.defer();
                return function() {
                    var context = this, args = arguments;
                    var later = function () {
                        timeout = null;
                        if (!immediate) {
                            deferred.resolve(func.apply(context, args));
                            deferred = $q.defer();
                        }
                    };
                    var callNow = immediate && !timeout;
                    if (!timeout) {
                        timeout = $timeout(later, wait);
                    }
                    if (callNow) {
                        deferred.resolve(func.apply(context, args));
                        deferred = $q.defer();
                    }
                    return deferred.promise;
                };
            };
        }
    ]);