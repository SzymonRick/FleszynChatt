var connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub") // URL of your SignalR hub
    .build();

let currentChatId;
let currentUserId;
let currentMessageCount = 0;
let usersArray;
let contactsArray;

connection.on("SetUserId", function(id){
    currentUserId = id;
});

connection.on("OpenChat", function (id) {
    openChat(id);
});

function openChat(chatId) {
    currentMessageCount = 0;
    contactsArray.forEach(function (contact) {
        if (contact.id = currentChatId) {
            connection.invoke("UpdateActiveTime", contact.id, currentUserId);
        }
    });
    currentChatId = chatId;
    var chatMessagesDiv = document.querySelector('.chatMessages');
    chatMessagesDiv.innerHTML = "";
    contactsArray.forEach(function (contact) {
        if (contact.id = currentChatId) {
            connection.invoke("UpdateActiveTime", contact.id, currentUserId);
        }
    });
    connection.invoke("GetChatMessages", currentChatId, currentMessageCount)
}

function sendMessage(sender = null) {
    if (sender != null) {
        var messageBox = document.getElementById("messagebox");
        var message = messageBox.value.trim();

        var fileInput = document.getElementById("fileInput");
        var file = fileInput.files[0];
        var fileBytes = null;

        if (message === "" && !file) {
            alert("Please enter a message or select a file.");
            return;
        }

        connection.invoke("UpdateActiveTime", currentChatId, currentUserId);

        if (file) {
            var reader = new FileReader();

            reader.onload = function (e) {
                fileBytes = new Uint8Array(e.target.result);
                sendWithFile(message, fileBytes);
            };

            reader.readAsArrayBuffer(file);
        } else {
            sendWithFile(sender, currentChatId, message, fileBytes);
        }
    }
}

var modal = document.getElementById('chatCreator');

// When the user clicks anywhere outside of the modal, close it
window.onclick = function (event) {
    if (event.target == modal) {
        modal.style.display = "none";
    }
}

connection.on("UpdateContactList", function (contacts) {

    var contactListTable = document.getElementById("contactList");

    // Clear existing rows
    contactListTable.innerHTML = "";

    contactsArray = Object.values(contacts);
    // Add buttons and last message for each contact
    contactsArray.forEach(function (contact) {
        connection.invoke("JoinChatGroup", contact.id)
        var contactButton = document.createElement("button");
        contactButton.textContent = contact.name;
        contactButton.addEventListener("click", function () {
            if (currentChatId != contact.id) {
                openChat(contact.id);;
            }
        });

        var lastMessageSpan = document.createElement("span");
        lastMessageSpan.textContent = contact.lastMessage;

        // Check if the message was sent after the last active time
        if (contact.lastMessageTime > contact.lastActiveTime) {
            // If so, make the last message string bold
            lastMessageSpan.style.fontWeight = "bold";
        }

        var tableRow = document.createElement("tr");
        tableRow.id = contact.id; // Set id attribute to contact id

        var buttonCell = document.createElement("td");
        var messageCell = document.createElement("td");

        buttonCell.appendChild(contactButton);
        messageCell.appendChild(lastMessageSpan);

        tableRow.appendChild(buttonCell);
        tableRow.appendChild(messageCell);

        contactListTable.appendChild(tableRow);
    });
});

connection.on("UpdateMessages", function (messages) {

    var chatMessagesDiv = document.querySelector('.chatMessages');

    const messagesArray = Object.values(messages);

    // Loop through each message and create HTML elements
    messagesArray.forEach(function (message) {
        currentMessageCount++;
        var messageDiv = document.createElement('div');
        messageDiv.classList.add('message');

        var senderSpan = document.createElement('span');
        senderSpan.classList.add('sender');
        senderSpan.textContent = message.item1 + ': ';

        var contentSpan = document.createElement('span');
        contentSpan.classList.add('wiadomosc');
        contentSpan.textContent = message.item2.messageText;

        var timestampSpan = document.createElement('span');
        timestampSpan.classList.add('timestamp');

        // Format the sendDate to remove the "T" from the datetime string
        var sendDate = new Date(message.item2.sendDate);
        var formattedDate = sendDate.toLocaleString(); // Change the formatting as needed

        timestampSpan.textContent = formattedDate;

        // Prepend spans to messageDiv instead of appending them
        messageDiv.appendChild(senderSpan);
        messageDiv.appendChild(timestampSpan);
        messageDiv.appendChild(contentSpan);
        

        // Prepend messageDiv to chatMessagesDiv instead of appending it
        chatMessagesDiv.prepend(messageDiv);
    });
});

connection.on("ReceiveMessage", function (message) {
    if (message.item2.recipientID == currentChatId) {
        currentMessageCount++;
        var chatMessagesDiv = document.querySelector('.chatMessages');

        var messageDiv = document.createElement('div');
        messageDiv.classList.add('message');

        var senderSpan = document.createElement('span');
        senderSpan.classList.add('sender');
        senderSpan.textContent = message.item1 + ': ';

        var contentSpan = document.createElement('span');
        contentSpan.textContent = message.item2.messageText;

        var timestampSpan = document.createElement('span');
        timestampSpan.classList.add('timestamp');

        // Format the sendDate to remove the "T" from the datetime string
        var sendDate = new Date(message.item2.sendDate);
        var formattedDate = sendDate.toLocaleString(); // Change the formatting as needed

        timestampSpan.textContent = formattedDate;

        // Prepend spans to messageDiv instead of appending them
        messageDiv.appendChild(senderSpan);
        messageDiv.appendChild(timestampSpan);
        messageDiv.appendChild(contentSpan);


        // Prepend messageDiv to chatMessagesDiv instead of appending it
        chatMessagesDiv.append(messageDiv);
    }

        var contactListTable = document.getElementById("contactList");
        var rows = contactListTable.getElementsByTagName("tr");

        for (var i = 0; i < rows.length; i++) {
            var row = rows[i];
            var id = row.getAttribute("id");
            var secondTd = row.getElementsByTagName("td")[1]; // Second <td> element

            // Your condition based on row id to update second td content
            if (id == message.item2.recipientID) {
                var contentSpan = document.createElement("span");
                if (message.item2.recipientID != currentChatId) {
                    contentSpan.style.fontWeight = "bold";
                }
                contentSpan.textContent = message.item2.messageText;

                // Clear the existing content and append the new <span> element
                secondTd.innerHTML = "";
                secondTd.appendChild(contentSpan);
            }
        }

    if (message.item2.senderID != currentUserId) {
        var audio = document.getElementById("alertSound");
        audio.play();
    }

});

connection.on("UpdateUserList", function (users) {

    var userListTable = document.getElementById("userList");
    var userListTable2 = document.getElementById("chatCreatorList");

    // Clear existing rows
    userListTable.innerHTML = "";
    userListTable2.innerHTML = "";

    // Add header row to both tables
    var headerRow = document.createElement("tr");
    headerRow.classList.add("header");

    var nameHeader = document.createElement("th");
    nameHeader.style.width = "60%";
    nameHeader.textContent = "Name";

    var usernameHeader = document.createElement("th");
    usernameHeader.style.width = "40%";
    usernameHeader.textContent = "Username";

    headerRow.appendChild(nameHeader);
    headerRow.appendChild(usernameHeader);

    userListTable.appendChild(headerRow);
    userListTable2.appendChild(headerRow.cloneNode(true)); // Clone header row for userListTable2

    var usersArray = Object.values(users);

    // Add user rows and checkboxes to userListTable and userListTable2
    usersArray.forEach(function (user) {
        // Create userRow for userListTable
        var userRow1 = document.createElement("tr");
        userRow1.setAttribute("data-userid", user.id);
        userRow1.addEventListener("click", function () {
            selectUser(user.id);
        });

        var nameCell1 = document.createElement("td");
        nameCell1.textContent = user.name + " " + user.surname;

        var usernameCell1 = document.createElement("td");
        usernameCell1.textContent = user.username;

        userRow1.appendChild(nameCell1);
        userRow1.appendChild(usernameCell1);
        userListTable.appendChild(userRow1);

        // Create userRow for userListTable2
        var userRow2 = document.createElement("tr");
        userRow2.setAttribute("data-userid", user.id);

        var nameCell2 = document.createElement("td");
        nameCell2.textContent = user.name + " " + user.surname;

        var usernameCell2 = document.createElement("td");
        usernameCell2.textContent = user.username;

        userRow2.appendChild(nameCell2);
        userRow2.appendChild(usernameCell2);

        // Add checkbox to userListTable2
        var checkboxCell = document.createElement("td");
        var checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.value = user.id;
        checkbox.className = "chatCheckbox";

        checkboxCell.appendChild(checkbox);
        userRow2.appendChild(checkboxCell);
        userListTable2.appendChild(userRow2); // Append userRow2 with checkbox to userListTable2
    });
});

function sendWithFile(sender, recipeint,message, fileBytes) {
    // Call the SendMessage function on the server
    connection.invoke("SendMessage", sender, recipeint, message, fileBytes).then(function () {

        // Clear the textarea after sending the message
        document.getElementById("messagebox").value = "";
        document.getElementById("fileInput").value = ""; // Reset file input
    }).catch(function (err) {
        console.error("Error invoking SendMessage: " + err);
    });
}

function listUsers() {
    // Declare variables
    var input, filter, table, tr, td, i, txtValue;
    input = document.getElementById("searchbox");
    filter = input.value.toUpperCase();
    table = document.getElementById("userList");
    tr = table.getElementsByTagName("tr");

    // Loop through all table rows, and hide those who don't match the search query
    for (i = 0; i < tr.length; i++) {
        td = tr[i].getElementsByTagName("td")[0];
        if (td) {
            txtValue = td.textContent || td.innerText;
            if (txtValue.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
}

function listUsers2() {
    // Declare variables
    var input, filter, table, tr, td, i, txtValue;
    input = document.getElementById("searchbox2");
    filter = input.value.toUpperCase();
    table = document.getElementById("chatCreatorList");
    tr = table.getElementsByTagName("tr");

    // Loop through all table rows, and hide those who don't match the search query
    for (i = 0; i < tr.length; i++) {
        td = tr[i].getElementsByTagName("td")[0];
        if (td) {
            txtValue = td.textContent || td.innerText;
            if (txtValue.toUpperCase().indexOf(filter) > -1) {
                tr[i].style.display = "";
            } else {
                tr[i].style.display = "none";
            }
        }
    }
}

function createChat(userId) {
    var usersIDArray = [];
    var checkboxes = document.getElementsByClassName("chatCheckbox");

    // Loop through all checkboxes
    for (var i = 0; i < checkboxes.length; i++) {
        if (checkboxes[i].checked) {
            var userIdValue = parseInt(checkboxes[i].value); // Convert to integer
            if (!usersIDArray.includes(userIdValue)) {
                usersIDArray.push(userIdValue);
            }
        }
    }

    // Convert userId to integer
    userId = parseInt(userId);

    // Add the userId parameter if it's not already in the array
    if (!usersIDArray.includes(userId)) {
        usersIDArray.push(userId);
    }

    // Get the value of the input field
    var chatName = document.getElementById("chatNamer").value;

    // Check if chatName is empty
    if (!chatName.trim()) {
        alert("Please enter a chat name.");
        return; // Exit the function if chatName is empty
    }

    // Check if the usersIDArray contains at least two elements
    if (usersIDArray.length < 2) {
        alert("Please select at least one user that isnt you to create a chat.");
        return; // Exit the function if there are fewer than two users
    }

    // Now usersIDArray contains at least two IDs of checked checkboxes and the userId parameter
    modal.style.display = "none";

    connection.invoke("CreateChat", chatName, usersIDArray);
}

function resizeContainer() {
    var windowHeight = window.innerHeight; // Get the height of the browser window
    var windowWidth = window.innerWidth;   // Get the width of the browser window

    var container = document.getElementById("container");
    container.style.height = windowHeight + "px"; // Set container height to window height
    container.style.width = windowWidth + "px";   // Set container width to window width
}

function selectUser(id) {
    connection.invoke("SelectUserContact", id, currentUserId);
}

connection.on("Update", function () {
    connection.invoke("SendUserData");
    connection.invoke("SendContactsData");
});

window.onload = function () {
    resizeContainer;

    connection.start().then(function () {
        connection.invoke("SendUserData");
        connection.invoke("SendContactsData");
    }).catch(function (err) {
        console.error("SignalR connection error: " + err);
    });

    var firstContactButton = document.querySelector('.chatTab');
    if (firstContactButton) {
        firstContactButton.click();
    }
}

window.onunload = function (event) {
    connection.invoke("UpdateActiveTime", currentChatId, currentUserId);
};

window.onresize = resizeContainer;