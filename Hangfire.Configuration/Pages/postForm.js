var submitButton = document.querySelector('.workerCountSubmit');

if(submitButton) {{
    submitButton.addEventListener('click', function(){{
        var workerCountForm = document.querySelector('.workerCountForm');
        var formData = new FormData(workerCountForm);
        var reloadOnOk = false;
        var request = new XMLHttpRequest();

        request.onload = function() {{
            if (request.status != 200) {{
                alert('Error: ' + request.status + ' : ' + request.response);
            }} else if ( reloadOnOk ) {{
                window.location.reload(true);
            }} else {{
                alert(request.response);
            }}
        }};

        let jsonObject = {};
        for (const [key, value]  of formData.entries()) {{
            jsonObject[key] = value;
        }}
        request.open('POST', 'HangfireConfiguration/saveWorkerGoalCount');
        //request.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');
        request.send(JSON.stringify(jsonObject));
    }});
}}


function postForm(formId, path, reloadOnOk) {{
    var formElement = document.querySelector('#' + formId);
    var formData = new FormData(formElement);
    var request = new XMLHttpRequest();

    request.onload = function() {{
        if (request.status != 200) {{
            alert('Error: ' + request.status + ' : ' + request.response);
        }} else if ( reloadOnOk ) {{
            window.location.reload(true);
        }} else {{
            alert(request.response);
        }}
    }};

    let jsonObject = {};
    for (const [key, value]  of formData.entries()) {{
        jsonObject[key] = value;
    }}
    request.open('POST', path);
    //request.setRequestHeader('Content-Type', 'application/json;charset=UTF-8');
    request.send(JSON.stringify(jsonObject));
}}