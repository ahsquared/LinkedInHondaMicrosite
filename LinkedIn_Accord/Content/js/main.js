$(function () {
    aSalmon.hondaDreamTeam.init();
});

aSalmon.hondaDreamTeam = {
    logout: function () {
        IN.User.logout(function () { document.location = '/'; });
    },
    userType: '',
    init: function () {
        $('.linkedin-signin').css('visibility', 'visible');
        this.setupHotSpots();
        this.setupScoreboard();
        this.setupChooseMeType();
        this.setupFooter();
        this.setupClosePopup();
        this.setupConfirmation();
        this.setupAddThis();
    },
    setupConfirmation: function () {
        if ($('#confirmation').length > 0) {
            $('#main-nav').show();
        }
        if ($('#scoreboard').length > 0) {
            $.get("/Home/GetTopThree", function (results) {
                $("#scoreboard .pages #all table tbody").html("");
                count = 0;
                $.each(results, function () {
                    count = count + 1;
                    $("#scoreboard .pages #all table tbody").append("<tr><td>" + count + ".</td><td>" + this.Name + "</td><td>" + this.TotalPoints + "</td></tr>");
                });
            });
            $.get("/Home/GetTopThreeNetwork", function (results) {
                $("#scoreboard .pages #network table tbody").html("");
                count = 0;
                $.each(results, function () {
                    count = count + 1;
                    $("#scoreboard .pages #network table tbody").append("<tr><td>" + count + ".</td><td>" + this.Name + "</td><td>" + this.TotalPoints + "</td></tr>");
                });

            });
        }
    },
    setupAddThis: function () {
        //setup confirmation page only addthis toolbox
        if ($('#confirmation').length > 0) {
            addthis.toolbox('#toolbox-confirm', {
                "data_track_addressbar": true,
                ui_email_note: "Just created my Honda Accord A-Team to go in the draw to win Business Technology Products valued at $3k"
            }, {
                "description": "Just created my Honda Accord A-Team to go in the draw to win Business Technology Products valued at $3k",
                passthrough: {
                    twitter: {
                        text: "Just created my Honda Accord A-Team to go in the draw to win Business Technology Products valued at $3k"
                    }
                },
                url_transforms: {
                    shorten: {
                        twitter: 'bitly'
                    }
                },
                shorteners: {
                    bitly: {}
                },
                email_template: "ATeamShare"
            });
        }
        //setup main addthis toolboxes
        addthis.toolbox('.toolbox-main', {
            "data_track_addressbar": true
        }, {
            "description": "Share your ultimate Honda Accord A-Team and you could win Business Technology Products valued at $3k",
            passthrough: {
                twitter: {
                    text: "Share your ultimate Honda Accord A-Team and you could win Business Technology Products valued at $3k"
                }
            },
            url_transforms: {
                shorten: {
                    twitter: 'bitly'
                }
            },
            shorteners: {
                bitly: {}
            },
            email_template: "MicrositeShare"
        });

    },
    setupHotSpots: function () {
        if ($('.hot-spot').length > 0) {
            $('.hot-spot').hoverIntent(function () {
                $(this).find('.info-wrap').fadeIn();
            }, function () {
                $(this).find('.info-wrap').fadeOut();
            });
            $('.hot-spot .select').on('click', function (e) {
                e.preventDefault();
                $('#contest').hide();
                var id = $(this).data('id');
                $('#choose-me .member-type').filter(function () {
                    return $(this).hasClass(id);
                }).click();
                $('#choose-me .you-info .headshot').attr('src', aSalmon.hondaDreamTeam.myprofile.pictureUrl);
                $('#choose-me .you-info .title h2').text(aSalmon.hondaDreamTeam.myprofile.firstName + " " + aSalmon.hondaDreamTeam.myprofile.lastName);
                $('#choose-me .you-info .title p').text(aSalmon.hondaDreamTeam.myprofile.positions.values[0].title);
                aSalmon.hondaDreamTeam.showPopupMask(true);
                $('#choose-me').show();
            });
            var paper;
            var pulse_anim = Raphael.animation({ r: 35, 'stroke-opacity': 0, 'stroke-width': 2 }, 1400);
            paper = new Raphael("spot-pulses", '100%', '100%');
            $(".hot-spot").each(function () {
                var xx = parseInt($(this).css('left')) + 20;
                var yy = parseInt($(this).css('top')) + 20;
                var c = paper.circle(xx, yy, 20).attr({ stroke: "#a5884f", 'stroke-opacity': 0.6, 'stroke-width': 4 }).animate(pulse_anim.repeat(Infinity));
                //var c = paper.circle(xx, yy, 16).attr({ stroke: "#ffffff", 'stroke-width': 5 }).animate(pulse_anim.repeat(100));
            });
        }
    },
    showPopupMask: function (show) {
        if (show) {
            var h = $('.wrap').height();
            $('#popup-mask').css({
                height: h
            }).show();
        } else {
            $('#popup-mask').hide();
        }
    },
    setupScoreboard: function () {
        if ($('#scoreboard').length > 0) {
            $('#scoreboard').tabs();
        }
    },
    setupFooter: function () {
        $(window, 'body').resize(function () {
            aSalmon.hondaDreamTeam.calcMainMinHt();
        });
        $('.popup-link').on('click', function (e) {
            e.preventDefault();
            var id = $(this).data('id');
            aSalmon.hondaDreamTeam.showPopupMask(true);
            $('#' + id).show();
            $(window).scrollTop(0);
        });
    },
    calcMainMinHt: function () {
        var popupHt = 0;
        var minHt = Math.max(710, parseInt($(window).height()) - 194);
        if ($('#choose-team:visible').length > 0) {
            popupHt = parseInt($('#choose-team .popup').height());
        }
        minHt = Math.max(minHt, popupHt);
        //console.log(popupHt, minHt);
        $('.main').css('minHeight', minHt);
        if ($('.popup:visible').length > 0) {
            aSalmon.hondaDreamTeam.showPopupMask(true);
        }
    },
    setupClosePopup: function () {
        $('.close').on('click', function () {
            var parent = $(this).parents('.content');
            parent.hide();
            if (parent.attr('id') == "choose-me") {
                $('#contest').show();
            }
            $('#popup-mask').hide();
        });
    },
    setupChooseMeType: function () {
        if ($('.member-type').length > 0) {
            $('.member-type').click(function () {
                $('.member-type .tick').removeClass('active');
                $(this).find('.tick').addClass('active');
            });
            $('#select-me').on('click', function (e) {
                e.preventDefault();
                if ($('.member-type .tick.active').length > 0) {
                    // show loader
                    $(this).addClass('loader');
                    var type = $('.member-type .tick.active').siblings('h2').text().toLowerCase();
                    var id = aSalmon.hondaDreamTeam.myprofile.id;
                    var name = aSalmon.hondaDreamTeam.myprofile.firstName + " " + aSalmon.hondaDreamTeam.myprofile.lastName;
                    var imgUrl = aSalmon.hondaDreamTeam.myprofile.pictureUrl;

                    // fill the form fields and the bubbles in the confirmation page
                    aSalmon.hondaDreamTeam.fillFormAndBubbles(type, id, name, imgUrl);

                    $('#choose-team #members').find('h2').each(function () {
                        if ($(this).text().toLowerCase() == type) {
                            var member = $(this).parents('.member');
                            member.addClass('chosen me');
                            $('<img/>').appendTo(member.find('.headshot')).attr('src', aSalmon.hondaDreamTeam.myprofile.pictureUrl).attr('alt', '');
                        }
                    });
                    setTimeout(function () {
                        aSalmon.hondaDreamTeam.renderConnections(aSalmon.hondaDreamTeam.profiles);
                        $('#choose-me').hide();
                        $('#select-me').removeClass('loader');
                        $('#choose-team').show();
                        aSalmon.hondaDreamTeam.chooseTeamMembersHandler();
                        aSalmon.hondaDreamTeam.filterConnectionsHandlers();
                        aSalmon.hondaDreamTeam.calcMainMinHt();
                    }, 500);
                }

            });
        }
    },

    fillFormAndBubbles: function (type, id, name, imgUrl) {
        switch (type) {
            case "high achiever":
                $('#HighAchieverLinkedInId').val(id);
                $('#HighAchieverName').val(name);
                $('#high-achiever-filled .bubble').empty();
                $('#high-achiever-filled .member p').text(name);
                $('<img/>').appendTo($('#high-achiever-filled .bubble')).attr('src', imgUrl).attr('alt', '');
                break;
            case "diplomat":
                $('#DiplomatLinkedInId').val(id);
                $('#DiplomatName').val(name);
                $('#diplomat-filled .bubble').empty();
                $('#diplomat-filled .member p').text(name);
                $('<img/>').appendTo($('#diplomat-filled .bubble')).attr('src', imgUrl).attr('alt', '');
                break;
            case "dependable":
                $('#DependableLinkedInId').val(id);
                $('#DependableName').val(name);
                $('#dependable-filled .bubble').empty();
                $('#dependable-filled .member p').text(name);
                $('<img/>').appendTo($('#dependable-filled .bubble')).attr('src', imgUrl).attr('alt', '');
                break;
            case "original thinker":
                $('#OriginalThinkerLinkedInId').val(id);
                $('#OriginalThinkerName').val(name);
                $('#original-thinker-filled .bubble').empty();
                $('#original-thinker-filled .member p').text(name);
                $('<img/>').appendTo($('#original-thinker-filled .bubble')).attr('src', imgUrl).attr('alt', '');
                break;
            case "social butterfly":
                $('#SocialButterflyLinkedInId').val(id);
                $('#SocialButterflyName').val(name);
                $('#social-butterfly-filled .bubble').empty();
                $('#social-butterfly-filled .member p').text(name);
                $('<img/>').appendTo($('#social-butterfly-filled .bubble')).attr('src', imgUrl).attr('alt', '');
                break;
        }
    },
    renderConnections: function (connections) {
        if ($('#connections').length > 0) {
            if ($('#connections-container').length > 0) {
                return false;
            }
            // create a wrapper for all the elements so we can inject them all at once
            var connectionsContainer = $('<div id="connections-container"><div id="connections-mask"></div></div>');
            var connectionsWrap = $('<div id="connections-wrap"></div>');
            var connectionRow = '';
            // loop through all the connections
            var numConnections = connections.values.length;
            for (var i = 0; i < numConnections; i++) {
                // get the next connection
                var newConnection = connections.values[i];
                // make sure there is a picture, if not set the default one
                if (newConnection.pictureUrl == undefined) {
                    newConnection.pictureUrl = "/Content/img/blank-user-pic.png";
                }
                // make sure the connection has created at least one position,
                // if not leave it empty - nothing will be rendered
                if (newConnection.positions != undefined && newConnection.positions.values != undefined) {
                    newConnection.positions.values = [newConnection.positions.values[0]];
                }
                // generate the connection template
                var connection = ich.connection(newConnection);
                // insert into wrapper
                // not using row wrapper so we can filter and reorganize layout automatically
                connectionsWrap.append(connection);
            }
            // put all the rows and wrap in the container
            connectionsContainer.append(connectionsWrap);
            // finally insert all the html into the DOM
            $('#connections').append(connectionsContainer);
            // create ellipsis for the titles
            $('#connections-wrap').find('.connection-info').dotdotdot();

        }
    },
    renderUser: function () {
        if ($('#members').length > 0) {

        }
    },
    chooseTeamMembersHandler: function () {
        if ($('#members').length > 0) {
            $('.member').on('click', function () {
                if ($(this).hasClass('me')) {
                    return false;
                }
                var id = $(this).data('id');
                $('.member.active').removeClass('active');

                function memberAlreadySelected() {
                    var counter = 0;
                    for (var i = 0; i < 5; i++) {
                        var testId = $('.member').eq(i).data('id');
                        if (testId == id) {
                            counter++;
                        }
                    }
                    if (counter > 1) {
                        return true;
                    }
                    return false;
                }

                if (!memberAlreadySelected()) {
                    $('.connection[data-id="' + id + '"]').removeClass('active');
                }
                $(this).removeClass('chosen').addClass('active').data('id', '').find('img').remove();
                $('#connections-mask').hide();
                $('#done').fadeOut();
            });
            $('.connection:visible').on('click', function () {

                var type = $('.member.active h2').text().toLowerCase();
                var id = $(this).data('id');
                var name = $(this).data('name');
                var imgUrl = $(this).data('picture-url');

                $(this).addClass('active');
                $('.member.active').data('id', id);
                // copy pic and name to member type that is active
                $('.member.active .headshot').append($(this).find('.headshot img').clone()).parent().removeClass('active').addClass('chosen');
                // add id and name to the form for submission
                //var img = $(this).find('.headshot img');
                //var connnectionNameAndId = $(this).find('.name');
                //var id = connnectionNameAndId.attr('id');
                //var name = connnectionNameAndId.text();

                //console.log(type, id, name, imgUrl);

                // fill the form fields and the bubbles in the confirmation page
                aSalmon.hondaDreamTeam.fillFormAndBubbles(type, id, name, imgUrl);

                $('#connections-mask').show();
                if ($('.member.chosen').length == 5) {
                    if (aSalmon.hondaDreamTeam.checkForDuplicateMembers()) {
                        $('#duplicate-warning').show();
                        $('#done').hide();
                    } else {
                        $('#duplicate-warning').hide();
                        $('#done').show();
                    }
                }
            });
            $('#done').on('click', function (e) {
                e.preventDefault();
                // add chosen members to the bubbles
                $('#contest-filled').show();
                $('#choose-team').hide();
                $(window).scrollTop(0);
                aSalmon.hondaDreamTeam.attachCompletionHandlers();
                aSalmon.hondaDreamTeam.loadInviteForm();
                aSalmon.hondaDreamTeam.showPopupMask(false);
                aSalmon.hondaDreamTeam.calcMainMinHt();
                aSalmon.hondaDreamTeam.showPopupMask(false);
            });
        }
    },
    checkForDuplicateMembers: function () {
        var duplicates = false;
        var len = $('#members .member').length;
        for (var i = 0; i < len; i++) {
            var id = $('#members .member').eq(i).data('id');
            for (var j = 0; j < len; j++) {
                if (i != j && id == $('#members .member').eq(j).data('id')) {
                    //console.log(img, i, j);
                    return true;
                }
            }
        }
        return false;
    },
    filterConnectionsHandlers: function () {
        $('#all-connections').on('click', function (e) {
            e.preventDefault();
            $('#connections .connection').show();
            aSalmon.hondaDreamTeam.filterConnectionsByName();
            $('#connection-filter .active').removeClass('active');
            $(this).addClass('active');
        });
        $('#colleagues-connections').on('click', function (e) {
            e.preventDefault();
            $('#connections .connection').show();
            aSalmon.hondaDreamTeam.filterConnectionsByColleagues();
            aSalmon.hondaDreamTeam.filterConnectionsByName();
            $('#connection-filter .active').removeClass('active');
            $(this).addClass('active');
        });
        $('#location-connections').on('click', function (e) {
            e.preventDefault();
            $('#connections .connection').show();
            aSalmon.hondaDreamTeam.filterConnectionsByLocation();
            aSalmon.hondaDreamTeam.filterConnectionsByName();
            $('#connection-filter .active').removeClass('active');
            $(this).addClass('active');
        });
        $('#name-filter-btn').on('click', function (e) {
            e.preventDefault();
            $('#connections .connection').show();
            aSalmon.hondaDreamTeam.filterConnectionsByName();
        });
        $('#name-filter').on('keyup', function (e) {
            if (e.which == 13 || e.keyCode == 13) {
                return false;
            }
            $('#connections .connection').show();
            if ($('#colleagues-connections').hasClass('.active')) {
                aSalmon.hondaDreamTeam.filterConnectionsByColleagues();
            }
            if ($('#location-connections').hasClass('.active')) {
                aSalmon.hondaDreamTeam.filterConnectionsByLocation();
            }
            aSalmon.hondaDreamTeam.filterConnectionsByName();
        });
    },
    filterConnectionsByColleagues: function () {
        var companyId = aSalmon.hondaDreamTeam.myprofile.positions.values[0].company.id;
        $('#connections .connection').filter(function (index) {
            return $(this).data('company-id') != companyId;
        }).hide();
    },
    filterConnectionsByLocation: function () {
        var countryCode = aSalmon.hondaDreamTeam.myprofile.location.country.code;
        $('#connections .connection').filter(function (index) {
            return $(this).data('country') != countryCode;
        }).hide();
    },
    filterConnectionsByName: function () {
        var name = $('#name-filter').val().toLowerCase();
        $('#connections .connection:visible').filter(function (index) {
            return $(this).data('name').toLowerCase().indexOf(name) == -1;
        }).hide();
    },
    attachCompletionHandlers: function () {
        $('#invite .send-invite-btn').on('click', function (e) {
            e.preventDefault();
            // send invitations
            for (var i = 0; i < aSalmon.hondaDreamTeam.userInfo.length; i++) {
                aSalmon.hondaDreamTeam.SendMessage(i);
            }
            // submit form once invite sent
            $('#form-entry').submit();
        });
        $('#invite .cancel').on('click', function (e) {
            e.preventDefault();
            $('#form-entry').submit();
        });

        $('#reset').on('click', function (e) {
            e.preventDefault();
            $('#form-entry .team-field').val('');
            $('#contest-filled').hide();
            $('#members .member').removeClass('chosen me').find('.headshot img').remove();
            $('#members .member').data('id', '');
            $('.connection.active').removeClass('active');
            $('.connection').show();
            $('#name-filter').val('');
            $('#choose-me').show();
            aSalmon.hondaDreamTeam.showPopupMask(true);
        });
        $('#submit').on('click', function (e) {
            e.preventDefault();
            if ($('#agree').is(':checked')) {
                $('#AgreedTerms').attr("checked", "checked");
                if ($('#emails').is(':checked')) {
                    $('#Offers').attr("checked", "checked");
                }
                if ($('#post').is(':checked')) {
                    $('#PostUpdate').attr("checked", "checked");
                }
                aSalmon.hondaDreamTeam.launchInvitation();
            } else {
                $('.agree-warning').show();
            }
        });
        $('#agree').change(function () {
            if ($('#agree').prop("checked")) {
                $('.agree-warning').hide();
            } else {
                $('.agree-warning').show();
            }
        });
    },
    loadInviteForm: function () {
        aSalmon.hondaDreamTeam.userInfo = [];
        $('#connections .connection.active').each(function (index) {
            var headshot = $('#invite .connection').eq(index).find('.headshot');
            headshot.empty();
            var img = $('<img/>').attr('src', $(this).data('picture-url')).attr('alt', '');
            var name = $('<h3 class="name"></h3>').text($(this).data('name'));
            var title = $('<h4 class="title" title=""><span class="title-text"></span> at <span class="company"></span></h4>');
            title.find('.title-text').text($(this).data('title'));
            title.find('.company').text($(this).data('company'));
            var invitee = $('#invite .connection').eq(index);
            invitee.find('.headshot').append(img);
            invitee.find('.connection-info').empty().append(name).append(title);
            var userInfo = {};
            userInfo.id = $(this).data('id');
            userInfo.name = $(this).data('name');
            aSalmon.hondaDreamTeam.userInfo.push(userInfo);
        });
    },
    launchInvitation: function () {
        //$('#contest-filled').hide();
        aSalmon.hondaDreamTeam.showPopupMask(true);
        $('#invite').show();
    },
    current: "",
    userInfo: [],
    myprofile: null,
    profile: null,
    profiles: null,
    isFirst: true,
    SendMessage: function (index) {
        var message = document.getElementById('message').value;
        var subject = document.getElementById('subject').value;
        var BODY = {
            "recipients": {
                "values": [{
                    "person": {
                        "_path": "/people/" + aSalmon.hondaDreamTeam.userInfo[index].id
                    }
                }]
            },
            "subject": subject,
            "body": "Hi " + aSalmon.hondaDreamTeam.userInfo[index].name + ", \n" + message
        };
        IN.API.Raw("/people/~/mailbox")
           .method("POST")
           .body(JSON.stringify(BODY))
           .result(function (result) {
           })
           .error(function error(e) {
           });
    }

};

function loadData() {
    $('.linkedin-signin').addClass('signed-in');
    $('.linkedin-signin').css('visibility', 'hidden');
    IN.API.Profile("me")
        //.fields(["pictureUrl", "publicProfileUrl", "id", "firstName", "lastName", "location", "num-recommenders", "positions", "educations", "languages", "certifications", "connections"])
        .fields(["pictureUrl", "publicProfileUrl", "id", "firstName", "lastName", "location", "positions"])
        .result(function (result) {
            aSalmon.hondaDreamTeam.myprofile = result.values[0];
            $("#LinkedInId").val(aSalmon.hondaDreamTeam.myprofile.id).data("pictureUrl", aSalmon.hondaDreamTeam.myprofile.pictureUrl);
            $("#ProfileUrl").val(result.values[0]["publicProfileUrl"]);
            $('#main-nav .headshot').attr('src', aSalmon.hondaDreamTeam.myprofile.pictureUrl);
            var name = aSalmon.hondaDreamTeam.myprofile.firstName + " " + aSalmon.hondaDreamTeam.myprofile.lastName;
            $('#main-nav .user h2').text(name);
            $("#Name").val(name);
        });


    IN.API.Connections("me")
      //.fields(["pictureUrl", "publicProfileUrl", "distance", "id", "firstName", "lastName", "location", "num-recommenders", "positions", "educations", "languages", "certifications", "connections", "current-status-timestamp", "current-status"])
      .fields(["pictureUrl", "publicProfileUrl", "distance", "id", "firstName", "lastName", "location", "positions"])
      .params({ "count": 2500 })
      .result(function (result) {
          $('#home').css('display', 'none');
          $('#main-nav').css('display', 'block');
          $('#contest').css('display', 'block');
          aSalmon.hondaDreamTeam.profiles = result;
      });

}

