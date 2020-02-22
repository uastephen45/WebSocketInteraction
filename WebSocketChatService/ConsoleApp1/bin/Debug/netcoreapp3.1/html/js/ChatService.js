var ChatServiceApp = angular.module('ChatServiceApp', ['ngRoute']);

// configure our routes
ChatServiceApp.config(function ($routeProvider) {
    $routeProvider

        // route for the home page
        .when('/Home', {
            templateUrl: 'views/home.html'
        })
        .when('/FrankinStory', {
            templateUrl: 'views/FrankinStory.html'
        })
        .when('/CaH', {
            templateUrl: 'views/CaH.html'
        });
})

ChatServiceApp.controller('HomeController', function ($scope, $routeParams, $location, $http, $rootScope) {
    $scope.Rooms = [];
    $scope.Init = function () {
        $http.get("/api/ChatRooms").then(function (p) {
            $scope.Rooms = p.data;
        }, function (p) {
            console.log('Failed');
        });
    };
    $scope.GoToChat = function (chat) {
        $rootScope.RoomId = chat.instance;
        $location.path('/'+chat.topic);
    };

    $scope.CreateNew = function (roomType) {
        $http.get("/api/CreateNewChatRoom?roomType=" + roomType).then(function (p) {
            $rootScope.RoomId = p.data;//this is the id to the chatroom
            $location.path('/' + roomType);
        }, function (p) {
            console.log('Failed');
        });
    };

    $scope.Init();
});
ChatServiceApp.controller('CaHController', function ($scope, $routeParams, $location,$rootScope,$http) {

    var socket = [];
    $scope.Messages = [];
    $scope.Message = "";
    $scope.Validated = false;
    $scope.lightson = false;
    $scope.myturn = false;
    $scope.LoggedIn = false;
    $scope.Colors = [];
    $scope.selectedColor = "";
    $scope.UsernameDisabled = false;
    $scope.myWhiteCards = [];
    $scope.PossWinnerWhiteCards = [];
    $scope.Username = "";
    $scope.showMyWhite = false;
    $scope.showPossWhite = false;
    $scope.players = [];
    $scope.blackCard = "";
    $scope.gamecontext = [];
    $scope.currentplayer ="";

    $scope.updatefromsidecar = function (data) {
        var switchVal = data.substring(7,10);
        switch (switchVal) {
            //Start the game
            case "STG":
                console.log("Start the game");
                var context = JSON.parse(data.replace("SideCarSTG", ""));
                $scope.PossWinnerWhiteCards = [];
                $scope.players = context.players;
                $scope.blackCard = context.BlackCard; 
                $scope.gamecontext = context; 

                $scope.gamecontext.players.forEach(function (p) {
                    if (p.myTurn) {
                        $scope.currentplayer = p.Username;
                    }
                });
                context.players.forEach(function (p) {
                    if (p.Username == $scope.Username) {
                        if (p.myTurn == true) {
                            $scope.myturn = true;
                            $scope.Message = "Wait For Everyone To Play Their White Cards";
                            $scope.showMyWhite = false;
                        } else {
                            $scope.myturn = false;
                        }
                    }
                    if ($scope.myturn == false) {
                        $scope.Message = "Select A White Card To Play";
                        $scope.showMyWhite = true;
                        $scope.showPossWhite = false;
                        $scope.PossWinnerWhiteCards = [];
                    }
                });

                break;
            // Select the Winner
            case "STW":
                console.log("Select the Winner");
                break;
            // Pick Your White Card It's time to select a winner 
            case "PWC":
                console.log("Possible White Card");
                var posscardcontext = JSON.parse(data.replace("SideCarPWC", ""));
                $scope.PossWinnerWhiteCards.push(posscardcontext);
                if ($scope.myturn) {
                    $scope.Message = "Select A White Card To Win";
                } else {
               

                    $scope.Message = "Waiting For "+$scope.currentplayer+" To Pick A Card";
                }
                $scope.showPossWhite = true;
                break;
            // New White Card 
            case "HWC":
                var cardcontext = JSON.parse(data.replace("SideCarHWC", ""));
                $scope.myWhiteCards.push(cardcontext);
                break;
            // Wait For White Cards. 
            case "WFC":
                console.log("Wait For White Cards.");
                break;
            // Announce Winning White Card 
            case "WWC":
                var winningCard = JSON.parse(data.replace("SideCarWWC", ""));
                winningCard.content = "Winner Is: " + winningCard.content;
                $scope.PossWinnerWhiteCards = [];
                $scope.PossWinnerWhiteCards.push(winningCard);
                break;


        }
    };
    $scope.update = function (eventdata) {
        if (eventdata.data.substring(0,7)=="SideCar") {
            $scope.updatefromsidecar(eventdata.data);
            return;
        }
        var obj = JSON.parse(eventdata.data);
        if ($scope.Username == obj.userName) {
            obj.class = "sent msg";
        } else {
            obj.class = "rcvd msg";
        }       
        $scope.Messages.push(obj);
    };
    $scope.Login = function () {
        $scope.LoggedIn = true;

        $scope.CurrentText = "User Logged In";
        $scope.SendMemo();
 
    };
    
    $scope.sendWhiteCard = function (cardValue) {
        $scope.myWhiteCards.splice($scope.myWhiteCards.indexOf(cardValue), 1);           
        $scope.Message = "Wait for the other players";
        $scope.showMyWhite = false;
        var message = {
            "content": "SideCarWCD" + cardValue.content,
            "userName": $scope.Username,
            "dateTime": "11/16/1991",
            "userColor": "red"
        };

        socket.send(JSON.stringify(message));
    };
    $scope.selectWinningWhiteCard = function (cardValue) {

        if (!$scope.myturn) {
            return;
        } 


        var message = {
            "content": "SideCarWCS" + cardValue.content,
            "userName": $scope.Username,
            "dateTime": "11/16/1991",
            "userColor": "red"
        };

        socket.send(JSON.stringify(message));
    };




    $scope.SendMemo = function () {
        if (!$scope.Username || $scope.Username == "") {
            console.log('give a username first');
        }
        if ($scope.UsernameDisabled == false) {
            $scope.UsernameDisabled = true;
        }
        var message = {
            "content": $scope.CurrentText,
            "userName": $scope.Username,
            "dateTime": "11/16/1991",
            "userColor": "red"
        };

        socket.send(JSON.stringify(message));
        $scope.CurrentText = "";
    };
    $scope.init = function () {
        $scope.Message = "Loading";
        socket = new WebSocket("ws://" + location.host + "/api/CaHService?roomid=" + $rootScope.RoomId);
        socket.onopen = (function (event) {
            $scope.Validated = true;
            $scope.Message = "Access Approved";
            $scope.$apply();
        });
        socket.addEventListener('message', function (event) {
            $scope.update(event);
            $scope.$apply();
        });

        $http.get("/api/Colors?roomid=" + $rootScope.RoomId).then(function (pasdfj) {
            $scope.Colors = pasdfj.data;
        }, function (p) {
            console.log('Failed');
        });

        socket.onclose = function (event) {
            $scope.Validated = false;
            $scope.Message = "Connection Closed";
            $scope.$apply();
        };
    };
    $scope.init();
});


ChatServiceApp.controller('FrankinStoryController', function ($scope, $routeParams, $location, $rootScope, $http) {

    var socket = [];
    $scope.Messages = [];
    $scope.Message = "";
    $scope.Validated = false;
    $scope.lightson = false;
    $scope.myturn = false;
    $scope.LoggedIn = false;
    $scope.Colors = [];
    $scope.selectedColor = "";
    $scope.UsernameDisabled = false;
    $scope.myWhiteCards = [];
    $scope.PossWinnerWhiteCards = [];
    $scope.Username = "";
    $scope.showMyWhite = false;
    $scope.showPossWhite = false;
    $scope.players = [];
    $scope.blackCard = "";
    $scope.gamecontext = [];
    $scope.currentplayer = "";

    $scope.updatefromsidecar = function (data) {
        var switchVal = data.substring(7, 10);
        switch (switchVal) {
            //Start the game
            case "STG":
                console.log("Start the game");
                var context = JSON.parse(data.replace("SideCarSTG", ""));
                $scope.PossWinnerWhiteCards = [];
                $scope.players = context.players;
                $scope.blackCard = context.BlackCard;
                $scope.gamecontext = context;

                $scope.gamecontext.players.forEach(function (p) {
                    if (p.myTurn) {
                        $scope.currentplayer = p.Username;
                    }
                });
                context.players.forEach(function (p) {
                    if (p.Username == $scope.Username) {
                        if (p.myTurn == true) {
                            $scope.myturn = true;
                            $scope.Message = "Wait For Everyone To Play Their White Cards";
                            $scope.showMyWhite = false;
                        } else {
                            $scope.myturn = false;
                        }
                    }
                    if ($scope.myturn == false) {
                        $scope.Message = "Select A White Card To Play";
                        $scope.showMyWhite = true;
                        $scope.showPossWhite = false;
                        $scope.PossWinnerWhiteCards = [];
                    }
                });

                break;
            // Select the Winner
            case "STW":
                console.log("Select the Winner");
                break;
            // Pick Your White Card It's time to select a winner 
            case "PWC":
                console.log("Possible White Card");
                var posscardcontext = JSON.parse(data.replace("SideCarPWC", ""));
                $scope.PossWinnerWhiteCards.push(posscardcontext);
                if ($scope.myturn) {
                    $scope.Message = "Select A White Card To Win";
                } else {


                    $scope.Message = "Waiting For " + $scope.currentplayer + " To Pick A Card";
                }
                $scope.showPossWhite = true;
                break;
            // New White Card 
            case "HWC":
                var cardcontext = JSON.parse(data.replace("SideCarHWC", ""));
                $scope.myWhiteCards.push(cardcontext);
                break;
            // Wait For White Cards. 
            case "WFC":
                console.log("Wait For White Cards.");
                break;
            // Announce Winning White Card 
            case "WWC":
                var winningCard = JSON.parse(data.replace("SideCarWWC", ""));
                winningCard.content = "Winner Is: " + winningCard.content;
                $scope.PossWinnerWhiteCards = [];
                $scope.PossWinnerWhiteCards.push(winningCard);
                break;


        }
    };
    $scope.update = function (eventdata) {
        if (eventdata.data.substring(0, 7) == "SideCar") {
            $scope.updatefromsidecar(eventdata.data);
            return;
        }
        var obj = JSON.parse(eventdata.data);
        if ($scope.Username == obj.userName) {
            obj.class = "sent msg";
        } else {
            obj.class = "rcvd msg";
        }
        $scope.Messages.push(obj);
    };
    $scope.Login = function () {
        $scope.LoggedIn = true;

        $scope.CurrentText = "User Logged In";
        $scope.SendMemo();

    };

    $scope.sendWhiteCard = function (cardValue) {
        $scope.myWhiteCards.splice($scope.myWhiteCards.indexOf(cardValue), 1);
        $scope.Message = "Wait for the other players";
        $scope.showMyWhite = false;
        var message = {
            "content": "SideCarWCD" + cardValue.content,
            "userName": $scope.Username,
            "dateTime": "11/16/1991",
            "userColor": "red"
        };

        socket.send(JSON.stringify(message));
    };
    $scope.selectWinningWhiteCard = function (cardValue) {

        if (!$scope.myturn) {
            return;
        }


        var message = {
            "content": "SideCarWCS" + cardValue.content,
            "userName": $scope.Username,
            "dateTime": "11/16/1991",
            "userColor": "red"
        };

        socket.send(JSON.stringify(message));
    };




    $scope.SendMemo = function () {
        if (!$scope.Username || $scope.Username == "") {
            console.log('give a username first');
        }
        if ($scope.UsernameDisabled == false) {
            $scope.UsernameDisabled = true;
        }
        var message = {
            "content": $scope.CurrentText,
            "userName": $scope.Username,
            "dateTime": "11/16/1991",
            "userColor": "red"
        };

        socket.send(JSON.stringify(message));
        $scope.CurrentText = "";
    };
    $scope.init = function () {
        $scope.Message = "Loading";
        socket = new WebSocket("ws://" + location.host + "/api/FrankinStoryService?roomid=" + $rootScope.RoomId);
        socket.onopen = (function (event) {
            $scope.Validated = true;
            $scope.Message = "Access Approved";
            $scope.$apply();
        });
        socket.addEventListener('message', function (event) {
            $scope.update(event);
            $scope.$apply();
        });

        $http.get("/api/Colors?roomid=" + $rootScope.RoomId).then(function (pasdfj) {
            $scope.Colors = pasdfj.data;
        }, function (p) {
            console.log('Failed');
        });

        socket.onclose = function (event) {
            $scope.Validated = false;
            $scope.Message = "Connection Closed";
            $scope.$apply();
        };
    };
    $scope.init();
});