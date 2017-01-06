function setChoice() {
	var tbl = document.getElementById('ans');
	for(var i = 0; i < tbl.rows.length; ++i) {
		var r = tbl.rows[i];
		for(var j = 0; j < r.cells.length; ++j) {
			r.cells[j].className = i + '_' + j;
			r.cells[j].onmouseup = checkA;
		}
	}
}
function checkA(e) {
	var targ;
	if (!e) var e = window.event;
	if (e.target) targ = e.target;
	else if (e.srcElement) targ = e.srcElement;
	if (targ.nodeType == 3) // defeat Safari bug
		targ = targ.parentNode;
	var m = targ.className, i = m.indexOf('_');
	var r = m.substring(0, i), c = m.substring(i+1) - 1;
	var tbl = document.getElementById('ans');
	var row = tbl.rows[r];
	for(var i = 1; i < row.cells.length; ++i)
		row.cells[i].innerHTML = '';
	targ.innerHTML = 'X';
	var l = document.getElementsByName(r);
	var d = document.getElementsByName(-r);
	if(c < l.length) {
		for(var i = 0; i < l.length; ++i) {
			l[i].className = 'choice';
			d[i].checked = false;
		}
		l[c].className += ' hlight';
		d[c].checked = true;
	}
}
function check(x) {
	var e = x.getAttribute('name');
	var a = document.getElementsByName(e);
	var col = 0;
	for(var i = 0; i < a.length; ++i) {
		a[i].className = 'choice';
		if(a[i] == x)
			col = i;
	}
	x.className += ' hlight';
	++col; //+1 for 1st col is qid
	//not e-1, 1st row is th
	a = document.getElementById('ans').rows[e].cells;
	for(var i = 1; i < a.length; ++i)
		a[i].innerHTML = '';
	a[col].innerHTML = 'X';
	for(var i = 1; i < a.length; ++i)
		s += a[i].innerHTML;
}
function score() {
	try{
		var mark = 0;
		var n = -1;
		var a = document.getElementsByName(n);
		while(0 < a.length) {
			var t = 0, c = 0;
			for(var i = 0; i < a.length; ++i)
				if('#' < a[i].value) {
					++t;
					if(a[i].checked)
						++c;
					else
						break;
				}
			if(t == c)
				++mark;
			--n;
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
				if('#' < a[i].value) {
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