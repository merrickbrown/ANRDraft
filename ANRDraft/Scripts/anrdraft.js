
$(function () {
    // Reference the auto-generated proxy for the hub.
    var draft = $.connection.draftHub;
    var $currentPack = $('#currentPack');
    var $selectedCards = $('#selectedCards');
    var arrySelectedCards = [];
    var updatePack = function (draft) {
        draft.server.getCurrentPack(draftname, playername).done(function (cards) {
            $currentPack.empty();
            $.each(cards, function (i, card) {
                var $img = $(document.createElement('img'));
                var ID = card.ID;
                $img.attr('id', ID);
                $img.attr('src', card.Data.ImageUri);
                $img.attr('alt', card.Data.Title);
                $img.attr('class', "cardimage");
                $img.dblclick(function () {
                    $currentPack.hide();
                    draft.server.trySelectCard(draftname, playername, ID).done(
                    function () {
                        addCardToSelectedList(card);
                        showSelectedList();
                        updatePack(draft);
                        $currentPack.show();
                    }
                    );
                });
                $currentPack.prepend($img);
            });
            $('#message').focus();
        });
    };

    var addCardToSelectedList = function (card) {
        var added = false;
        for (var i = 0; i < arrySelectedCards.length; i++) {
            if (arrySelectedCards[i].Title === card.Data.Title) {
                arrySelectedCards[i].Number++;
                added = true;
                break;
            }
        }
        if (!added) {
            arrySelectedCards.push({ Title: card.Data.Title, Number: 1 });
        }
    };

    var showSelectedList = function () {
        $selectedCards.empty();
        for (var i = 0; i < arrySelectedCards.length; i++) {
            var $item = $(document.createElement('tr'));
            $item.append('<td>' + arrySelectedCards[i].Number + '</td>');
            $item.append('<td>' + arrySelectedCards[i].Title + '</td>');
            $selectedCards.append($item);
        }

    }

    var updateSelected = function (draft) {
        draft.server.getSelectedCards(draftname, playername).done(
            function (cards) {
                arrySelectedCards = [];
                $.each(cards, function (i, card) {
                    addCardToSelectedList(card);
                });
                showSelectedList();
            });
    };
    // Create a function that the hub can call back to display messages.
    draft.client.addNewMessageToPage = function (name, message) {
        // Add the message to the page.
        $('#discussion').append('<li><strong>' + htmlEncode(name)
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