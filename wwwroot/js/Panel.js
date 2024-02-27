var connection = new signalR.HubConnectionBuilder()
    .withUrl("/adminHub") // URL of your SignalR hub
    .build();


window.onload = function () {
    connection.start().then(function () {
        
    }).catch(function (err) {
        console.error("SignalR connection error: " + err);
    });
}

function polacz() {
    var inputs = document.querySelectorAll('.searchbox');

    // Initialize an array to store the input contents
    var inputContents = [];

    // Loop through each input element and push its value to the inputContents array
    inputs.forEach(function (input) {
        inputContents.push(input.value);
    });

    connection.invoke("SetPassword", defaultPassword)
    connection.invoke("ADConnection", domainController, login, password)
}

function deleteUser() {
    // Find the checked checkbox in the userList table
    var checkedCheckbox = document.querySelector('input[type="checkbox"].chatCheckbox:checked');

    // If a checkbox is checked, extract its value and handle the deletion
    if (checkedCheckbox) {
        var userId = checkedCheckbox.value;
        // Handle the deletion, for example:
        connection.invoke("DeleteUser", userId);
    } else {
        // Handle case where no checkbox is checked
        console.log("No user selected for deletion.");
    }
}

function deleteChat() {
    // Find the checked radio button in the chatList table
    var checkedRadio = document.querySelector('input[type="radio"].chatRadio:checked');

    // If a radio button is checked, extract its value and handle the deletion
    if (checkedRadio) {
        var chatId = checkedRadio.value;
        // Invoke the DeleteChat method with the chat ID
        connection.invoke("DeleteChat", chatId);
    } else {
        // Handle case where no radio button is checked
        console.log("No chat selected for deletion.");
    }
}

function resetUser() {
    // Find the checked checkbox in the userList table
    var checkedCheckbox = document.querySelector('input[type="checkbox"].chatCheckbox:checked');

    // If a checkbox is checked, extract its value and handle the reset
    if (checkedCheckbox) {
        var userId = checkedCheckbox.value;
        // Invoke the ResetUserPassword method with the user ID
        connection.invoke("ResetUserPassword", userId);
    } else {
        // Handle case where no checkbox is checked
        console.log("No user selected for password reset.");
    }
}

function backup() {
    var selectedRadio = document.querySelector('input[name="selectedBackup"]:checked');

    // If a radio button is selected, extract its value and invoke the RestoreBackup method
    if (selectedRadio) {
        var file = selectedRadio.value;
        connection.invoke("RestoreBackup", file);
    } else {
        // Handle case where no radio button is selected
        console.log("No backup selected.");
    }
}

connection.on("UpdateUserList", function (users) {

    var userListTable = document.getElementById("userList");

    // Clear existing rows
    userListTable.innerHTML = "";

    var headerRow = document.createElement("tr");
    headerRow.className = "header";

    var headerCell = document.createElement("th");
    headerCell.style.width = "100%";
    headerCell.textContent = "UŻYTKOWNICY";

    headerRow.appendChild(headerCell);
    userListTable.appendChild(headerRow);

    usersArray = Object.values(users);

    // Add user rows with radioboxes to userListTable
    usersArray.forEach(function (user) {
        // Create userRow for userListTable
        var userRow = document.createElement("tr");
        userRow.setAttribute("data-userid", user.id);

        var profilePictureCell = document.createElement("td");
        var profilePicture = document.createElement("img");
        profilePicture.src = user.profilePicturePath || "/resources/profile.png"; // Use profile picture path or default path if null
        profilePicture.alt = "Profile Picture";

        var nameCell = document.createElement("td");
        nameCell.textContent = user.name + " " + user.surname;

        var radioCell = document.createElement("td");
        var radio = document.createElement("input");
        radio.type = "radio";
        radio.name = "selectedUser";
        radio.value = user.id;
        radio.className = "userRadio"; // Add a class for styling if needed

        userRow.appendChild(profilePictureCell);
        profilePictureCell.appendChild(profilePicture);
        userRow.appendChild(nameCell);
        userRow.appendChild(radioCell);
        radioCell.appendChild(radio);
        userListTable.appendChild(userRow);
    });
});

connection.on("UpdateChatsList", function (chats) {

    var chatListTable = document.getElementById("chatList");

    // Clear existing rows
    chatListTable.innerHTML = "";

    // Add header row to the table
    var headerRow = document.createElement("tr");
    headerRow.className = "header";

    var headerCell = document.createElement("th");
    headerCell.style.width = "100%";
    headerCell.textContent = "NAZWA CZATU";

    headerRow.appendChild(headerCell);
    chatListTable.appendChild(headerRow);

    // Loop through each chat in the chats object
    Object.values(chats).forEach(function (chat) {
        // Create a new row for each chat
        var chatRow = document.createElement("tr");
        chatRow.setAttribute("data-chatid", chat.id);

        // Create cells to display chat name and user count
        var nameCell = document.createElement("td");
        nameCell.textContent = chat.name;

        var userCountCell = document.createElement("td");
        userCountCell.textContent = chat.uids.length;

        // Create a cell for the radio button
        var radioCell = document.createElement("td");
        var radio = document.createElement("input");
        radio.type = "radio";
        radio.name = "selectedChat";
        radio.value = chat.id;
        radio.className = "chatRadio"; // Add a class for styling if needed

        // Append cells to the chat row
        chatRow.appendChild(nameCell);
        chatRow.appendChild(userCountCell);
        chatRow.appendChild(radioCell);
        radioCell.appendChild(radio);

        // Append chat row to the table
        chatListTable.appendChild(chatRow);
    });
});

connection.on("UpdateBackupList", function (backups) {

    var backupListTable = document.getElementById("backupList");

    // Clear existing rows
    backupListTable.innerHTML = "";

    // Add header row to the table
    var headerRow = document.createElement("tr");
    headerRow.className = "header";

    var headerCell = document.createElement("th");
    headerCell.style.width = "100%";
    headerCell.textContent = "PLIKI BACKUP";

    headerRow.appendChild(headerCell);
    backupListTable.appendChild(headerRow);

    // Loop through each backup in the backups array
    for (var i = 0; i < backups.length; i++) {
        var backup = backups[i];

        // Create a new row for each backup
        var backupRow = document.createElement("tr");

        // Create a cell to display the backup filename
        var filenameCell = document.createElement("td");
        filenameCell.textContent = backup;

        // Create a cell for the radio button
        var radioCell = document.createElement("td");
        var radio = document.createElement("input");
        radio.type = "radio";
        radio.name = "selectedBackup";
        radio.value = backup;
        radio.className = "backupRadio"; // Add a class for styling if needed

        // Append cells to the backup row
        backupRow.appendChild(filenameCell);
        backupRow.appendChild(radioCell);
        radioCell.appendChild(radio);

        // Append backup row to the table
        backupListTable.appendChild(backupRow);
    }
});