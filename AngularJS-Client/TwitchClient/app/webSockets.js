angular.module('webSockets', [])
  .service('guid', function() {
    function s4() {
      return Math.floor((1 + Math.random()) * 0x10000)
        .toString(16)
        .substring(1);
    }

    return function() {
      return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
        s4() + '-' + s4() + s4() + s4();
    };
  })
  .factory('websocketUrl', [
    'guid', '$log', function(guid, $log) {
      var defaultHost = 'localhost';
      var defaultPort = '8006';
      var id = guid();

      if (window.sessionStorage) {
        var stored = window.sessionStorage.getItem('websocketId');
        if (stored) {
          $log.debug('Using stored id: ' + stored);
          id = stored;
        } else {
          $log.debug('Using new id: ' + id);
          window.sessionStorage.setItem('websocketId', id);
        }
      }

      return {
        host: defaultHost,
        port: defaultPort,
        url: function() {
          return 'ws://' + this.host + ':' + this.port + '/' + id;
        }
      };
    }
  ])
  .service('$connection', [
    '$q', '$timeout', 'websocketUrl', '$log', '$rootScope', function($q, $timeout, websocketUrl, $log, $rootScope) {
      var me = {};
      var listeners = [];

      me.isConnected = false;
      me.isConnecting = false;

      listeners.removeOne = function(listener) {
        var index = listeners.indexOf(listener);
        if (index !== -1) {
          listeners.splice(index, 1);
        }
      };

      var correlationId = 1;
      me.nextCorrelationId = function() {
        return correlationId += 1;
      };

      me.queuedMessages = [];

      me.listen = function(predicate, handler) {
        var listener = { p: predicate, h: handler };
        listeners.push(listener);
        return function() { listeners.removeOne(listener); };
      };

      me.listenOnce = function(predicate, timeout) {
        var deferred = $q.defer();
        var handler = function(data) { deferred.resolve(data); };
        var listener = { p: predicate, h: handler };
        listeners.push(listener);
        var promise = deferred.promise;
        if (timeout) {
          var p = $timeout(function() {
            deferred.reject('timeout');
          }, timeout);

          promise.finally(function() {
            $timeout.cancel(p);
          });
        }

        promise.finally(function() {
          listeners.removeOne(listener);
        });

        promise.dispose = function() {
          deferred.reject('diposed');
        };

        return promise;
      };

      var onopen = function() {
        $rootScope.$apply(function() {
          me.isConnected = true;
          me.isConnecting = false;
        });

        me.queuedMessages.forEach(me.send);
        me.queuedMessages = [];
      };

      var onclose = function() {
        $rootScope.$apply(function() {
          me.isConnected = false;
          me.isConnecting = false;
        });
        me.ws = null;
      };

      var onmessage = function(msg) {
        var obj = JSON.parse(msg.data);
        listeners.filter(function(l) {
          return l.p(obj);
        }).forEach(function(l) {
          l.h(obj);
        });
      };

      var onerror = function() {
        $log.error('onerror');
        $rootScope.$apply(function() {
          me.isConnecting = false;
          me.isConnected = false;
        });
      };

      me.send = function(obj) {
        if (!me.isConnected) {
          me.queuedMessages.push(obj);
        } else {
          me.ws.send(JSON.stringify(obj));
        }
      };

      var setHandlers = function(w) {
        w.onopen = onopen;
        w.onclose = onclose;
        w.onmessage = onmessage;
        w.onerror = onerror;
      };

      me.disconnect = function() {
        if (!me.isConnecting && me.isConnected) {
          me.isConnecting = true;
          $log.debug('disconnecting...');
          me.ws.close();
        }
      };

      me.connect = function() {
        if (!me.isConnecting && !me.isConnected) {
          me.isConnecting = true;
          $log.debug('connecting...');
          me.ws = new WebSocket(websocketUrl.url());
          setHandlers(me.ws);
        }
      };

      return me;
    }
  ]);