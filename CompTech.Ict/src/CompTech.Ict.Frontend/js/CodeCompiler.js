function CompileCode(code) {
    var requester = new XMLHttpRequest();
    requester.open('POST', 'http://217.79.61.87:8080', false);
    requester.send(JSON.stringify({ source: code }));
    return requester.responseText;
}

function GetTextareaValue(id) {
    return document.getElementById(id).value;
}

function SetTextareaValue(id, text) {
    document.getElementById(id).value = text;
}

function GetInputValue(id) {
    return document.getElementById(id).value;
}

function HandleCompile() {
    var text = GetTextareaValue("source");
    var compiled = CompileCode(text);
    SetTextareaValue("compiled", compiled);
}
