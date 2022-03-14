var provider = document.getElementById('databaseProvider')
if(provider){
	provider.addEventListener('change', function(){
		if(provider.value==='redis'){
			document.getElementById('database').style.display='none';
			document.getElementById('patchuser').style.display='none';
			document.getElementById('applicationUser').style.display='none';
		} else {
			document.getElementById('database').style.display='block';
			document.getElementById('patchuser').style.display='block';
			document.getElementById('applicationUser').style.display='block';
		}
	});
}

var forms = document.querySelectorAll('form');

forms.forEach(function(form) {

    var button = form.querySelector('button');
    
    button.addEventListener('click', function(){
        var formData = new FormData(form);
        
        var reloadOnOk = false;
        if (form.attributes['data-reload'])
            reloadOnOk = !!form.attributes['data-reload'].value;
        
        var request = new XMLHttpRequest();
        request.onload = function() {
            if (request.status !== 200) {
                alert('Error: ' + request.status + ' : ' + request.response);
            } else if ( reloadOnOk ) {
                window.location.reload();
            } else {
                alert(request.response);
            }
        };
        
        var jsonObject = {};
        for (const [key, value]  of formData.entries()) {
            jsonObject[key] = value;
        }
        
        request.open('POST', form.action);
        request.send(JSON.stringify(jsonObject));
    });
});