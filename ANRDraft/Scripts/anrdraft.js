﻿
$(function () {
    // Reference the auto-generated proxy for the hub.
    var draft = $.connection.draftHub;
    var $currentPack = $('#currentPack');
    var $selectedCardsElement = $('#selectedCards');
    var selectedCards = {};
    // the function that updates the pack
    var updatePack = function (draft) {

        //$currentPack.hide();
        draft.server.getCurrentPack(draftname, playername).done(function (cards) {
            
            $currentPack.empty();
            $.each(cards, function (i, card) {
                var $img = $(document.createElement('img'));
                var ID = card.ID;
                $img.attr('id', ID);
                $img.attr('src', card.Data.ImageUri);
                $img.attr('alt', card.Data.Title);
                $img.attr('class', "cardimage");
                //$img.dblclick(function () {
                //    $currentPack.fadeOut(150, function () {
                //        draft.server.trySelectCard(draftname, playername, ID).done(
                //        function () {
                //            addCardToSelectedList(card);
                //            showSelectedList();
                //            updatePack(draft);

                //        }
                //        );
                //    });
                //});
                var tapped = false
                $img.on("click", function (e) {
                    if (!tapped) { //if tap is not set, set up single tap
                        $img.toggleClass("hovered");
                        if (screen.width <= 480) { $('html,body').animate({ scrollTop: $img.offset().top - ($(window).height() - $img.outerHeight(true)) / 2 }, '150'); }
                        tapped = setTimeout(function () {
                            tapped = null
                        }, 300);   //wait 300ms then run single click code
                    } else {    //tapped within 300ms of last tap. double tap
                        clearTimeout(tapped); //stop single tap callback
                        tapped = null
                        $currentPack.fadeOut(150, function () {
                            $img.toggleClass("hovered");
                            draft.server.trySelectCard(draftname, playername, ID).done(
                            function () {
                                addCardToSelectedList(card);
                                showSelectedList();
                                updatePack(draft);
                            });
                        });
                    }
                    e.preventDefault()
                });
                $currentPack.prepend($img);
            });
            $('#message').focus();
            $currentPack.fadeIn(150);
        });
    };

    
    var addCardToSelectedList = function (card) {
        var added = false;
        if (card.Data.Type in selectedCards) {
            var arrOfType = selectedCards[card.Data.Type];
            for (var i = 0; i < arrOfType.length; i++) {
                if (arrOfType[i].Title === card.Data.Title) {
                    arrOfType[i].Number++;
                    added = true;
                    break;
                }
            }
            if (!added) {
                arrOfType.push({ Title: card.Data.Title, Number: 1 });
            }
        } else {
            selectedCards[card.Data.Type] = [{Title: card.Data.Title, Number: 1}]
        }
    };

    var showSelectedList = function () {
        
        $selectedCardsElement.empty();
        var types = Object.keys(selectedCards).sort();
        for (var j = 0; j < types.length; j++) {
            var ofType = selectedCards[types[j]];
            var $typecontainer = $(document.createElement('tr'));
            //$typecontainer.addClass('table-info');
            $typecontainer.append("<td colspan = 2><b>" + types[j] + 'S </b></td>');
            $selectedCardsElement.append($typecontainer);
            for (var i = 0; i < ofType.length; i++) {
                $item = $(document.createElement('tr'));
                $item.append('<td>' + ofType[i].Number + '</td>');
                $item.append('<td>' + ofType[i].Title + '</td>');
                $selectedCardsElement.append($item);
            }
        }
    }

    var updateSelected = function (draft) {
        draft.server.getSelectedCards(draftname, playername).done(
            function (cards) {
                selectedCards = [];
                $.each(cards, function (i, card) {
                    addCardToSelectedList(card);
                });
                showSelectedList();
            });
    };
    // Create a function that the hub can call back to display messages.
    draft.client.addNewMessageToPage = function (name, message) {
        // Add the message to the page.
        $('#discussion').prepend('<li><strong>' + htmlEncode(name)
            + '</strong>: ' + htmlEncode(message) + '</li>');
    };
    $("#message").focus(function() {
        $(this).data("hasfocus", true);
    });

    $("#message").blur(function() {
        $(this).data("hasfocus", false);
    });


    //create a notification when a new round has started
    draft.client.newRound = function () {
        alert('a new round has started!');
        updatePack(draft);
    };
    //
    draft.client.newPack = function (name) {
        if (playername === name) {
            updatePack(draft);
        }
    };
    // Set initial focus to message input box.
    $('#message').focus();
    var init = function () {
        draft.server.joinDraft(draftname);
        updatePack(draft);
        updateSelected(draft);
        $('#sendmessage').click(function () {
            // Call the Send method on the hub.
            draft.server.newChatMessage(draftname, playername, $('#message').val());
            // Clear text box and reset focus for next comment.
            $('#message').val('').focus();
        });
        $('#message').keyup(function(ev) {
            // 13 is ENTER
            if (ev.which === 13 && $("#message").data("hasfocus")) {
                draft.server.newChatMessage(draftname, playername, $('#message').val());
                // Clear text box and reset focus for next comment.
                $('#message').val('').focus();
            }
        });
    };
    // Start the connection.
    $.connection.hub.start().done(function () {
        init();
    });
       // $.connect.hub.reconnected(init());

    
});
// This optional function html-encodes messages for display in the page.
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}

