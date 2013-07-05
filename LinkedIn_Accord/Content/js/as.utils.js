
var aSalmon = { };
aSalmon.utils = {
    loadFacebook: function () {
        (function (d, s, id) {
            var js, fjs = d.getElementsByTagName(s)[0];
            if (d.getElementById(id)) return;
            js = d.createElement(s); js.id = id;
            js.src = "//connect.facebook.net/en_US/all.js#xfbml=1";
            fjs.parentNode.insertBefore(js, fjs);
        } (document, 'script', 'facebook-jssdk'));
    },
    loadTwitter: function () {
        (function (d, s, id) {
            var js, fjs = d.getElementsByTagName(s)[0];
            if (!d.getElementById(id)) {
                js = d.createElement(s);
                js.id = id; js.src = "//platform.twitter.com/widgets.js";
                fjs.parentNode.insertBefore(js, fjs);
            }
        } (document, "script", "twitter-wjs"));

    },
    loadGooglePlus: function () {
        (function () {
            var po = document.createElement('script'); po.type = 'text/javascript'; po.async = true;
            po.src = 'https://apis.google.com/js/plusone.js';
            var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(po, s);
        })();
    },
    isIpad: function () {
        if ((navigator.userAgent.match(/iPad/i))) {
            return true;
        } else {
            return false;
        }
    },
    // select parent element of styled checkbox: aSalmon.utils.styleCheckBoxes($('selector'));
    styleCheckboxes: function (objs) {
        objs.each(function (index) {
            if (!$(this).prev("a").hasClass('check_styled')) {
                $(this).before("<a href='#check_" + index + "' class='check_styled' id='check_" + index + "'></a>").css({
                    position: 'absolute',
                    left: -1,
                    top: 3,
                    zIndex: -1,
                    visibility: "hidden"
                });
                if ($(this).prop('checked')) $(this).prev().addClass('checked');
                $(this).change(function () {
                    if ($(this).prop('checked')) {
                        $(this).prev().addClass('checked');
                    } else {
                        $(this).prev().removeClass('checked');
                    }
                });
            }
        });
        // function to 'check' the styled ones and their matching checkboxes
        $(".check_styled").click(function () {
            $(this).next("input[type=checkbox]").trigger("click").trigger('change');
            return false;
        });
    },
    styleRadioButtons: function (objs) {
        objs.each(function (ind) {
            if ($(this).children("a.radio-styled").length == 0) {
                $(this).find("input[type=radio]").each(function (index) {
                    $(this).before("<a href='#radio-" + index + "' class='radio-styled' id='radio-" + index + "'></a>")
                    .css({
                        position: 'absolute',
                        left: -1,
                        top: 3,
                        zIndex: -1,
                        visibility: "hidden"
                    });
                });
                $(this).find("input[type=radio]").each(function () {
                    (this.checked) ? $(this).prev().addClass('checked') : $(this).prev().removeClass('checked');
                });
            }
        });
        // function to 'check' the styled ones and their matching checkboxes
        $(".radio-styled").click(function () {
            $(this).parents('.radio-group').find('.radio-styled').removeClass('checked');
            $(this).addClass('checked');
            $(this).next("input[type=radio]").trigger("click");
            return false;
        });
    },

    // add class "word-limit" to element and call function with max words as parameter
    wordCounter: function (maxWords) {
        $('.word-limit').each(function () {
            var length = $(this).val().split(/\b[\s,\.-:;]*/).length;
            var remaining = maxWords + 1 - length;
            // update characters  
            $(this).parent().find('.counter span').html(remaining);
            // bind on key up event  
            $(this).bind('keydown keyup', function (e) {
                var keyCode = e.which;
                // get new length of words  
                length = $(this).val().split(/\b[\s,\.-:;]*/).length;
                remaining = maxWords + 1 - length;
                // update  
                $(this).parent().find('.counter span').html(remaining);
                //console.log(remaining, keyCode);
                if (remaining > 0) {
                    $(this).parent().find('.counter span').html(remaining);
                } else if (keyCode >= 8 && keyCode <= 46) {
                    if (keyCode == 32) {
                        e.preventDefault();
                    } else {
                        return;
                    }
                } else {
                    e.preventDefault();
                }
            });
        });

    },

    detectWindowsMobile: function () {
        var ua = navigator.userAgent;
        var re = new RegExp("Windows Phone");
        if (re.exec(ua) != null) $('body').addClass('win-phone');
    },
    // pass in a function reference to be called after the document ready call in main.js
    defer: function (deferredFn) {
        if (typeof deferredFn === 'function') {
            (function () {
                var docReady = $.Deferred();
                $(docReady.resolve);
                $.when(docReady).then(function () {
                    deferredFn();
                });
            })();

        }
    }

};

$.fn.equalHeights = function (px) {
    //$('.builders').children('.clear').hide();
	$(this).each(function () {
		var currentTallest = 0;
		$(this).children().each(function (i) {
			if ($(this).outerHeight() > currentTallest) {
				currentTallest = $(this).height();
			}
		});
		//if (!px || !Number.prototype.pxToEm) currentTallest = currentTallest.pxToEm(); //use ems unless px is specified
		// for ie6, set height since min-height isn't supported
		if ($('html').hasClass('ie6') || $('html').hasClass('ie7')) {
			$(this).children().css({ 'height': currentTallest });
		} else {
			$(this).children().css({ 'min-height': currentTallest });
		}
		$(this).addClass('equalHeights');
	});
	return this;
};