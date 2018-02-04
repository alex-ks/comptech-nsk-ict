function CompileCode(code) {
    var requester = new XMLHttpRequest();
    requester.open('POST', 'http://217.79.61.87:8080', false);
    requester.send(JSON.stringify({ source: code }));
    return requester.responseText;
}

function PostSession(compiledSession){
    var requester = new XMLHttpRequest();
    requester.open('POST', 'http://localhost:5000/api/session', false);
    requester.setRequestHeader("Content-type", "application/json");
    requester.send(compiledSession);
    return requester.responseText.slice(1, -1);
}

function GetId() {
    var url = new URL(window.location.href);
    return url.searchParams.get('id');
}

function isSessionFinished(session) {
    const ops = session.operationStatus;
    return ops.every(x => x.status === 2) || ops.some(x => x.status === 3 || x.status === 4);
}

function updateValuesTable(table) {
    const tableNode = document.getElementById('table');
    tableNode.innerHTML = '';

    Object.keys(table).forEach(k => {
        if (k[0] === '@') {
            return;
        }
        
        const v = table[k].value;

        var iDiv = document.createElement('div');
        iDiv.style = 'white-space: nowrap; overflow: hidden; text-overflow: ellipsis;';
        iDiv.textContent = `${k} = ${v}`;
        tableNode.appendChild(iDiv);
    });
}

function setStatus(session) {
    const statusNode = document.getElementById('status');
    const ops = session.operationStatus;
    if (ops.every(x => x.status === 2)) {
        statusNode.textContent = 'completed';
        return;
    }

    if (ops.some(x => x.status === 3 || x.status === 4)) {
        statusNode.textContent = 'failed';
        return;
    }

    statusNode.textContent = 'in progress';
}

function fetchSessionData() {
    var xhr = new XMLHttpRequest();
  
    xhr.onreadystatechange = function() {
      if (xhr.readyState !== XMLHttpRequest.DONE) {
          return;
      }

      var sessionStatus = null;
      if (this.status == 200 && this.responseText) {
        sessionStatus = JSON.parse(this.responseText); 
        updateValuesTable(sessionStatus.mnemonicsTable);
        setStatus(sessionStatus);
      }
      
      if (!sessionStatus || !isSessionFinished(sessionStatus)) {
        setTimeout(fetchSessionData, 1000);
      }
    }

    xhr.open('GET', 'http://localhost:5000/api/session/' + GetId(), true);
    xhr.send();
  }

function initSessionPage(SessionId){
    var id = GetId();
    var idElement = document.getElementById('sessionId');
    idElement.textContent = id;

    fetchSessionData(id);
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
    var id = PostSession(compiled);

    console.log("Compiled session: ", compiled);
    window.location.href = 'session.html?id=' + id;
}
