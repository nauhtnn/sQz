function check(x) {
	var e = x.getAttribute('name');
	var a = document.getElementsByName(e);
	for(var i = 0; i < a.length; ++i)
		a[i].className = 'choice';
	x.className += ' hlight';
}
// function checked(a) {
	// for(var i = 0; i < a.length; ++i)
		// if(a[i].checked)
			// return a[i];
	// return null;
// }
function score() {
	try{
		var mark = 0;
		var n = 6;
		var a = document.getElementsByName(n);
		while(0 < a.length) {
			var t = 0, c = 0;
			for(var i = 0; i < a.length; ++i)
				if(0 < a[i].value) {
					++t;
					if(a[i].checked)
						++c;
					else
						break;
				}
			if(t == c)
				++mark;
			++n;
			a = document.getElementsByName(n);
		}
		window.alert('Số câu đúng = ' + mark);
	}catch(err){
		window.alert(err.message);
	}
}
function toggle() {
	// var x = document.getElementById('toggle');
	// if(x != null)
		// if(x.value == 'Show answer')
			// showAnswer();
		// else
			// hideAnswer();
		showAnswer();
}
function showAnswer() {
	try {
		var n = -1;
		var a = document.getElementsByName(n);
		while(0 < a.length) {
			var b = document.getElementsByName(-n);
			for(var i = 0; i < a.length; ++i)
				if(0 < a[i].value) {
					b[i].className += ' hlight';
					// if(a[i].checked)
					// else
				}
			--n;
			a = document.getElementsByName(n);
		}
		// document.getElementById('toggle').value = 'Hide answer';
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