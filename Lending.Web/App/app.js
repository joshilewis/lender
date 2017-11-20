﻿'use strict';

//var lendtomeApp = angular.module('lendtome', [
//    'ngRoute',
//    'lendtomeControllers',
//    'lendtomeServices'
//]);

// Declares how the application should be bootstrapped. See: http://docs.angularjs.org/guide/module
angular.module('lendtome', [
    'ngRoute',
    //'ui.compat',
    'ui.router',
    //'app.filters',
    'lendtomeServices',
    //'app.directives',
    'lendtomeControllers',
    'firebase',
    'satellizer',
    'ui.bootstrap',
    'ui.bootstrap.collapse', 
    'ui.bootstrap.dropdownToggle'
])

    // Gets executed during the provider registrations and configuration phase. Only providers and constants can be
    // injected here. This is to prevent accidental instantiation of services before they have been fully configured.
    .config(['$routeProvider', '$locationProvider', '$authProvider', '$compileProvider', function ($routeProvider, $locationProvider, $authProvider, $compileProvider) {

        // UI States, URL Routing & Mapping. For more info see: https://github.com/angular-ui/ui-router
        // ------------------------------------------------------------------------------------------------------------

        $routeProvider
            .when('/myitems', {
                templateUrl: '/app/myitems.html',
                controller: 'userItemsController'
            })
            .when('/addisbn/:isbnnumber', {
                templateUrl: '/app/addbyisbn/addbyisbn.html',
                controller: 'addByIsbnController'
            })
            .when('/books', {
                templateUrl: '/app/books/books.html',
                controller: 'booksController'
            })
            .when('/links', {
                templateUrl: '/app/links/links.html',
                controller: 'linksController'
            })
            .when('/sentrequests', {
                templateUrl: '/app/sentrequests/sentrequests.html',
                controller: 'sentRequestsController'
            })
            .when('/receivedrequests', {
                templateUrl: '/app/receivedRequests/receivedRequests.html',
                controller: 'receivedRequestsController'
            })
            .when('/searchbooks', {
                templateUrl: '/app/searchBooks/searchBooks.html',
                controller: 'searchBooksController'
            })
            .when('/loading', {
                templateUrl: '/app/loading.html'
            })
            .when('/authenticating', {
                templateUrl: '/app/authenticating.html'
            })
            .when('/autherror', {
                templateUrl: '/app/autherror.html'
            })
            .otherwise(
            {
                templateUrl: '/app/home.html',
            });

        $locationProvider.html5Mode(true);

        $compileProvider.aHrefSanitizationWhitelist(/^\s*(https?|ftp|mailto|chrome-extension|zxing):/);

        $authProvider.facebook({
            clientId: '663064230418689',
            url: '/authentication/authenticatecallback?providerkey=facebook'
        });

        $authProvider.google({
            url: '/authentication/authenticatecallback?providerkey=google',
            clientId: '75779369919.apps.googleusercontent.com',
            redirectUri: window.location.origin + '/',
        });

        $authProvider.twitter({
            url: '/authentication/authenticatecallback?providerkey=twitter',
        });

        $authProvider.baseUrl = '/api/';
        $authProvider.loginUrl = '/signin';
        $authProvider.signupUrl = '/signin';
        $authProvider.unlinkUrl = '/auth/unlink/';
        $authProvider.tokenName = 'token';
        $authProvider.tokenPrefix = 'satellizer';
        $authProvider.authHeader = 'Authorization';
        $authProvider.authToken = '';
        $authProvider.storageType = 'localStorage';
    }])

    // Gets executed after the injector is created and are used to kickstart the application. Only instances and constants
    // can be injected here. This is to prevent further system configuration during application run time.
    .run(['$templateCache', '$rootScope', '$state', '$stateParams', '$location', '$auth',
        function ($templateCache, $rootScope, $state, $stateParams, $location, auth) {

        // <ui-view> contains a pre-rendered template for the current view
        // caching it will prevent a round-trip to a server at the first page load
        var view = angular.element('#ui-view');
        $templateCache.put(view.data('tmpl-url'), view.html());

        // Allows to retrieve UI Router state information from inside templates
        $rootScope.$state = $state;
        $rootScope.$stateParams = $stateParams;

        $rootScope.$on('$stateChangeSuccess', function (event, toState) {

            // Sets the layout name, which can be used to display different layouts (header, footer etc.)
            // based on which page the user is located
            $rootScope.layout = toState.layout;
        });

        $rootScope.$on('$routeChangeStart', function (event, next, current) {
            $rootScope.isAuthenticated = auth.isAuthenticated();
            if (next.templateUrl === '/app/home.html') return;
            if (next.templateUrl === '/app/loading.html') return;
            if (next.templateUrl === '/app/authenticating.html') return;
            if (next.templateUrl === '/app/autherror.html') return;
            if (!auth.isAuthenticated()) {
                $location.path('/');
            }
        });
    }]);