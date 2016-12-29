function check() {
	var cls = this.getAttribute('class');
	window.alert(cls);
}
function checked(a) {
	for(var i = 0; i < a.length; ++i)
		if(a[i].checked ||
			a[i].getAttribute('class').indexOf('checked') != -1)
			return a[i];
	return null;
}
function score() {
	try{
		var mark = 0;
		var n = document.getElementById('n').value;
		while(n) {
			var x = checked(document.getElementsByName(n));
			if(x != null && x.value == 0)
				mark++;
			--n;
		}
		window.alert('Số câu đúng = ' + mark);
	}catch(err){
		window.alert(err.message);
	}
}
function toggle() {
	var x = document.getElementById('toggle');
	if(x != null)
		if(x.value == 'Show answer')
			showAnswer();
		else
			hideAnswer();
}
function showAnswer() {
	try {
		var n = document.getElementById('n').value;
		while(n) {
			var x = document.getElementsByName(n);
			if(x != null)
				for(var i = 0; i < x.length; ++i)
					if(x[i].value == 0)
						x[i].className = 'hlight';
			--n;
		}
		document.getElementById('toggle').value = 'Hide answer';
	}catch(err){
		window.alert(err.message);
	}
}
function hideAnswer() {
	var n = document.getElementById('n').value;
	while(n) {
		var x = document.getElementsByName(n);
		if(x != null)
			for(var i = 0; i < x.length; ++i)
				x[i].className = '';
		--n;
	}
	document.getElementById('toggle').value = 'Show answer';
}