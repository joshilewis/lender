﻿lendtomeServices.factory('googleBookDetails', [
    '$resource',
    function ($resource) {
        return $resource('https://www.googleapis.com/books/v1/volumes?q=isbn::isbnNumber',
        {
            maxResults: '10',
            callback: 'JSON_CALLBACK',
            key: 'AIzaSyDCQ090RphWKV_lBNzq7nthXZRo6Q8QvmU',
            fields: 'kind,items(volumeInfo/title, volumeInfo/imageLinks, volumeInfo/authors, volumeInfo/publishedDate)'
        },
        {
            get: { method: 'JSONP' }
        });
    }
]);

lendtomeControllers.controller('addByIsbnController', ['$scope', 'googleBookDetails', '$routeParams', 'userItems', '$location',
  function ($scope, bookDetails, $routeParams, userItems, $location) {
      $scope.isbnNumber = $routeParams.isbnnumber;
      $scope.bookDetails = bookDetails.get({ isbnNumber: $scope.isbnNumber });

      $scope.addNew = function () {
          $scope.userItem = {
              title: $scope.bookDetails.items[0].volumeInfo.title,
              creator: $scope.bookDetails.items[0].volumeInfo.authors[0],
              edition: $scope.bookDetails.items[0].volumeInfo.publishedDate
          };
          userItems.save($scope.userItem);
          $location.path('/myitems/');
      };


  }]);

