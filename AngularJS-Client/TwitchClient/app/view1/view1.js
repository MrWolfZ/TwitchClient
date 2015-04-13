'use strict';

angular.module('myApp.view1', ['ngRoute'])
  .config([
    '$routeProvider', function($routeProvider) {
      $routeProvider.when('/view1', {
        templateUrl: 'view1/view1.html',
        controller: 'ConnectionController'
      });
    }
  ])
  .controller('ChatController', [
    '$scope', '$filter', '$connection', '$log',
    function ($scope, $filter, $connection, $log) {
      $scope.users = [];
      $scope.messages = [];
      $scope.maxMessageId = 0;
      var userFilter = '';
      $scope.userFilter = function(newValue) {
        if (angular.isDefined(newValue)) {
          userFilter = newValue;
        }

        return userFilter;
      };

      $scope.userDisplayLimit = 1000;

      $scope.filteredUsers = function() {
        return $filter('filter')($scope.users, userFilter);
      };

      var promise = $connection.listenOnce(function (msg) { return msg.Type === 'initialUserset'; });

      promise.then(function (msg) {
        var data = msg.Message;
        $scope.users = data.Users.map(function (user) { return user.Name; });

        $log.debug('Received initial userset for channel ' + data.ChannelName + ':');
        $log.debug('users (' + $scope.users.length + '):' + $scope.users.slice(0, 20));
      });

      var usersetUpdatesDispose = $connection.listen(function(msg) { return msg.Type === 'usersetUpdate'; }, function(msg) {
        var data = msg.Message;
        var joins = data.Joins.map(function (user) { return user.Name; });
        var parts = data.Parts.map(function (user) { return user.Name; });
        $log.debug('Received userset update for channel ' + data.ChannelName + ':');
        $log.debug('joins (' + joins.length + '):' + joins.slice(0, 20));
        $log.debug('parts (' + parts.length + '):' + parts.slice(0, 20));

        $scope.$apply(function() {
          $scope.users = $scope.users.filter(function (name) { return parts.indexOf(name) === -1; });

          joins.forEach(function (name) {
            $scope.users.push(name);
          });

          if ($scope.users.length !== data.Total) {
            $log.error('Mismatch in expected (' + data.Total + ') and actual (' + $scope.users.length + ') user count!');
          }
        });
      });

      var messagesDispose = $connection.listen(function(msg) { return msg.Type === 'message'; }, function(msg) {
        var data = msg.Message;
        data.messageId = $scope.maxMessageId;
        $scope.maxMessageId += 1;
        $log.debug('Received message.');

        $scope.$apply(function() {
          if ($scope.messages.length > 200) {
            $scope.messages = $scope.messages.splice(1, $scope.messages.length - 1);
          }

          $scope.messages.push(data);
        });
      });

      var errorsDispose = $connection.listen(function(msg) { return msg.Type === 'error'; }, function(msg) {
        var data = msg.Message;
        $log.error('Received error: ' + data.Error);
      });

      $scope.$on('$destroy', function() {
        usersetUpdatesDispose();
        messagesDispose();
        errorsDispose();
        promise.dispose();
      });
    }
  ])
  .controller('ConnectionController', [
    '$scope', '$filter', '$connection', '$window', function($scope, $filter, $connection, $window) {
      $scope.isConnected = false;
      $scope.isConnecting = false;

      $scope.$watch(function() {
        return {
          isConnected: $connection.isConnected,
          isConnecting: $connection.isConnecting
        };
      }, function(newValue) {
        $scope.isConnected = newValue.isConnected;
        $scope.isConnecting = newValue.isConnecting;
      }, true);

      $scope.username = 'valienna';
      $scope.authToken = 'oauth:yfeoxat12z5v7b6tang0rv2iwm030t';

      $scope.connect = function() {
        $connection.connect();
        $connection.send({ Username: $scope.username, AuthToken: $scope.authToken });
        $connection.send({ Type: 'join', Message: { ChannelName: 'arteezy' } });
      };

      $scope.disconnect = function() {
        $connection.disconnect();
      };

      angular.element($window).bind('beforeunload', function() {
        $scope.disconnect();
      });
    }
  ]);