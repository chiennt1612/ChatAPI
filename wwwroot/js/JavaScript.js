function uuidv4() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function AddOption(e, v, t) {
    RemoveOption(e, v);
    var option = document.createElement("option");
    option.text = t;
    option.value = v;
    e.appendChild(option);
}

function RemoveOption(e, v) {
    for (var i = 0; i < e.length; i++) {
        if (e.options[i].value == v)
            e.remove(i);
    }
}

function strReplace(str) {
    var kt = true; var i = 0;
    while (kt && i < 1000) {
        var kt = str.indexOf("ObjectId(\"") > -1;
        str = str.replace("ObjectId(\"", "\"").replace("\")", "\"");
    }
    return str;
}

function onchangeGrp(e) {
    Member.style.display = "none";
    Msg.style.display = "block";
    tdMsg.innerHTML = "Message";

    if (e.options[e.selectedIndex].value == 74 || e.options[e.selectedIndex].value == 73 || e.options[e.selectedIndex].value == 72 || e.options[e.selectedIndex].value == 71) {
        Group.value = GroupId.value;
        Msg.value = MessageText.value;
        MessageType.value = MsgType.value;
    }
    else if (e.options[e.selectedIndex].value == 11) {
        Member.style.display = "block";
        tdMsg.innerHTML = "Tên nhóm/ D.sách member";
    } 
    else if (e.options[e.selectedIndex].value == 12) {
        tdMsg.innerHTML = "Member";
    } 
    else if (e.options[e.selectedIndex].value == 21) {
        Member.style.display = "block";
        Msg.style.display = "none";
        tdMsg.innerHTML = "D.sách member";
    } 
    else if (e.options[e.selectedIndex].value == 22) {
        Member.style.display = "none";
        Msg.style.display = "none";
        tdMsg.innerHTML = "";
    } 
    else if (e.options[e.selectedIndex].value == 31) {
        Msg.style.display = "none";
        tdMsg.innerHTML = "D.sách member";
    }
    else if (e.options[e.selectedIndex].value == 32) {
        Msg.style.display = "none";
        tdMsg.innerHTML = "";
    }
    setValMsg();
}

function FnMember(str) {
    if (str.indexOf(","))
        return str.split(",");
    else if (str.indexOf(";"))
        return str.split(";");
    else
        return str.split(" ");
}

function setValMsg() {
    switch (parseInt(SocketTask.value)) {
        case 70: // SocketTask.MessageSend
        case 74: // SocketTask.MessageEdit
            textElement.value = "{\"SocketTask\": " + SocketTask.value + ", \"Id\": \"" + Id.value + "\", \"Sender\": \"" + Username.value + "\", \"GroupId\": \"" + Group.value + "\", \"Token\": \"" +
                Token.value + "\", \"ContentType\": " + MessageType.value + ", \"Content\": \"" +
                Msg.value + "\", \"MessageId\": \"" + MessageId.value + "\"}";
            break;
        case 72: //SocketTask.MessageRead
            textElement.value = "{\"SocketTask\": " + SocketTask.value + ", \"Id\": \"" + Id.value + "\", \"Sender\": \"" + Username.value + "\", \"GroupId\": \"" + Group.value + "\", \"Token\": \"" +
                Token.value + "\", \"MessageId\": \"" + MessageId.value + "\"}";
            break;
        case 71: //SocketTask.MessageRecall
        case 73: //SocketTask.MessageDelete
            textElement.value = "{\"SocketTask\": " + SocketTask.value + ", \"Id\": \"" + Id.value + "\", \"Sender\": \"" + Username.value + "\", \"Token\": \"" +
                Token.value + "\", \"MessageId\": \"" + MessageId.value + "\"}";
            break;

        case 11: //SocketTask.CreateGroup
            textElement.value = "{\"SocketTask\": " + SocketTask.value + ", \"Id\": \"" + Id.value + "\", \"Sender\": \"" + Username.value + "\", \"Token\": \"" +
                Token.value + "\", \"GroupName\": \"" + Msg.value + "\", \"Member\": " + JSON.stringify(FnMember(Member.value)) + ", \"Avatar\": 0}";
            break;
        case 12: //SocketTask.CreateChat
            textElement.value = "{\"SocketTask\": " + SocketTask.value + ", \"Id\": \"" + Id.value + "\", \"Sender\": \"" + Username.value + "\", \"Token\": \"" +
                Token.value + "\", \"Recieve\": \"" + Msg.value + "\"}";
            break;
        case 22: //SocketTask.JoinToGroup
        case 21: //SocketTask.AddToGroup
            textElement.value = "{\"SocketTask\": " + SocketTask.value + ", \"Id\": \"" + Id.value + "\", \"Sender\": \"" + Username.value + "\", \"Token\": \"" +
                Token.value + "\", \"GroupId\": \"" + Group.value + "\", \"Member\": " + JSON.stringify(FnMember(Member.value)) + "}";
            break;
        case 31: //SocketTask.RemoveFromGroup
            textElement.value = "{\"SocketTask\": " + SocketTask.value + ", \"Id\": \"" + Id.value + "\", \"Sender\": \"" + Username.value + "\", \"Token\": \"" +
                Token.value + "\", \"GroupId\": \"" + Group.value + "\", \"Username\": \"" + Msg.value + "\"}";
            break;
        case 32: //SocketTask.LeaveFromGroup
            textElement.value = "{\"SocketTask\": " + SocketTask.value + ", \"Id\": \"" + Id.value + "\", \"Sender\": \"" + Username.value + "\", \"Token\": \"" +
                Token.value + "\", \"GroupId\": \"" + Group.value + "\"}";
            break;
    }
    
}
function setValLogin() {
    document.getElementById('txtLogin').value = "{\"SocketTask\": 1, \"Sender\": \"" + Username.value + "\", \"Token\": \"" + Token.value + "\"}";
}
let webSocket;
let url = `wss://${location.host}/api/stream`;
// connecting
let connect2Chat = document.getElementById("connect2Chat");
let stateLabel = document.getElementById('stateLabel');
let connectButton = document.getElementById('connectButton');
let Username = document.getElementById('Username');
let Token = document.getElementById('Token');
let SocketTask = document.getElementById('SocketTask');
// chatting
let chatting = document.getElementById("chatting");
let UsernameMsg = document.getElementById('UsernameMsg');
let TokenMsg = document.getElementById('TokenMsg');
let MessageType = document.getElementById('MessageType');
let textElement = document.getElementById("txtSendMsg");
let Member = document.getElementById('Member');

let closeButton = document.getElementById('closeButton');
let sendMessage = document.getElementById('sendmessage');
let ulElement = document.getElementById('chatMessages');

let Id = document.getElementById('Id');
let Group = document.getElementById('Group');

let MessageId = document.getElementById('MessageId');
let MsgType = document.getElementById('MsgType');
let GroupId = document.getElementById('GroupId');
let MessageText = document.getElementById('MessageText');

let tdMsg = document.getElementById('tdMsg');

function socketOpen(msg) {
    webSocket = new WebSocket(url);
    webSocket.onopen = function (event) {
        console.log("Opened");
        if (msg) if (msg != "") webSocket.send(msg);
        updateState();
    };
    webSocket.onclose = function (event) {
        console.log("Closed");
        updateState();
    };
    webSocket.onerror = function (event) {
        console.log("Error");
        updateState();
    };
    webSocket.onmessage = function (message) {
        ulElement.innerHTML = ulElement.innerHTML += `<li>${message.data}</li>`
        try {
            var obj = JSON.parse(message.data);
            switch (obj.TaskType) {
                case 1:
                    var msg = JSON.parse(strReplace(obj.Msg));
                    Id.value = obj._id;
                    Group.length = 0;
                    for (var i = 0; i < msg.GroupId.length; i++) {
                        AddOption(Group, msg.GroupId[i], msg.GroupId[i]);
                    }
                    break;
                case 11:
                case 12:
                case 21:
                case 22:
                    AddOption(Group, obj.GroupId, obj.GroupId);
                    break;
                case 31:
                    var msg1 = strReplace(obj.Msg);
                    msg = JSON.parse(msg1);
                    if (UsernameMsg.value == msg.Username) RemoveOption(Group, msg.GroupId);
                    break;
                case 32:
                    RemoveOption(Group, obj.GroupId);
                    break;                
                case 70:
                    msg1 = strReplace(obj.Msg);
                    msg = JSON.parse(msg1);
                    MessageId.value = msg.MessageId;
                    MsgType.value = msg.ContentType;
                    GroupId.value = msg.GroupId;
                    MessageText.value = msg.Content;
                    break;
                case 74:
                    break;
                case 71:
                    break;
                case 72:
                    break;
                case 73:
                    break;
            }
        }
        catch (e) {
            console.log(e.message);
        }
        console.log(message);
    }
}

function updateState() {
    function disable() {
        connect2Chat.style.display = "block";
        chatting.style.display = "none";
        sendMessage.readonly = true;
        connectButton.readonly = false;
        closeButton.readonly = true;
    }
    function enable() {
        connect2Chat.style.display = "none";
        chatting.style.display = "block";
        sendMessage.readonly = false;
        connectButton.readonly = true;
        closeButton.readonly = false;
    }

    if (!webSocket) {
        disable();
    } else {
        switch (webSocket.readyState) {
            case WebSocket.CLOSED:
                stateLabel.innerHTML = "Closed";
                disable();
                break;
            case WebSocket.CLOSING:
                stateLabel.innerHTML = "Closing...";
                disable();
                break;
            case WebSocket.CONNECTING:
                stateLabel.innerHTML = "Connecting...";
                disable();
                break;
            case WebSocket.OPEN:
                stateLabel.innerHTML = "Open";
                enable();
                break;
            default:
                stateLabel.innerHTML = "Unknown WebSocket State: " + htmlEscape(webSocket.readyState);
                disable();
                break;
        }
    }
}

(function () {
    //Token.value = uuidv4();
    sendMessage.addEventListener("click", function () {
        if (!webSocket || webSocket.readyState !== WebSocket.OPEN) {
            alert("socket not connected");
            socketOpen(webSocket, url);
        }
        else {
            let text = textElement.value;
            console.log('Sending text: ' + text);
            webSocket.send(text);
            textElement.value = '';
            Msg.value = '';
        }

    });

    closeButton.addEventListener("click", function () {
        if (!webSocket || webSocket.readyState !== WebSocket.OPEN) {
            console.log("Closed");
            return;
        }
        webSocket.close(1000, "Closing from client");
    });

    connectButton.addEventListener("click", function () {
        if (webSocket) {
            if (webSocket.readyState === WebSocket.OPEN) {
                console.log("Opened");
                return;
            }
        }
        UsernameMsg.value = Username.value;
        TokenMsg.value = Token.value;
        ulElement.innerHTML = "";

        let text = "{\"SocketTask\": 1, \"Sender\": \"" + Username.value + "\", \"Token\": \"" + Token.value + "\"}";
        socketOpen(text);
    });

    connectButton.click();
}());