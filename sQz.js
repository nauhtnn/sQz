function setCell() {
	var tbl = document.getElementById('ans');
	for(var i = 1; i < tbl.rows.length; ++i) { //1, not 0
		var r = tbl.rows[i];
		for(var j = 1; j < r.cells.length; ++j) { //1, not 0
			r.cells[j].className = i + '_' + j;
			r.cells[j].onmouseup = checkCell;
		}
	}
	// countdown();
}
function caller(e) {
	var targ;
	if (!e) var e = window.event;
	if (e.target) targ = e.target;
	else if (e.srcElement) targ = e.srcElement;
	if (targ.nodeType == 3) // defeat Safari bug
		targ = targ.parentNode;
	return targ;
}
function checkCell(e) {
	var targ = caller(e);
	var m = targ.className, i = m.indexOf('_');
	var r = m.substring(0, i), c = m.substring(i+1) - 1;
	var tbl = document.getElementById('ans');
	var row = tbl.rows[r];
	var l = document.getElementsByName(r);
	if(c < l.length) {
		targ.innerHTML = 'X';
		l[c].getElementsByTagName('input')[0].checked = true;
		var cid = l[c].getElementsByTagName('span')[0];
		if(cid.className.indexOf('ck') < 0)
			cid.className += ' ck';
		for(var i = 0; i < l.length; ++i) {
			if(!l[i].getElementsByTagName('input')[0].checked) {
				l[i].getElementsByTagName('span')[0].className = 'cid';
				row.cells[i + 1].innerHTML = '';
			}
		}
	}
}
// function check(x) {
	// var e = x.getAttribute('name');
	// var a = document.getElementsByName(e);
	// var col = 0;
	// for(var i = 0; i < a.length; ++i) {
		// a[i].className = 'choice';
		// if(a[i] == x)
			// col = i;
	// }
	// x.className += ' hlight';
	// ++col; //+1 for 1st col is qid
	// //not e-1, 1st row is th
	// a = document.getElementById('ans').rows[e].cells;
	// for(var i = 1; i < a.length; ++i)
		// a[i].innerHTML = '';
	// a[col].innerHTML = 'X';
	// for(var i = 1; i < a.length; ++i)
		// s += a[i].innerHTML;
// }
var bAnswer = false;
function score(e) {
	try{
	var targ = caller(e);
	if(targ.value != 'Submit')
		return;
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
	targ.value = mark + '/' + (-n - 1);
	bAnswer = true;
	window.alert('Số câu đúng = ' + mark);
	}catch(err) {
		window.alert(err.message);
	}
}
function showAnswer() {
	// var x = document.getElementById('toggle');
	// if(x != null)
		// if(x.value == 'Show answer')
			// showAnswer();
		// else
			// hideAnswer();
	if(bAnswer)
		showAnswer1();
	else
		window.alert('Nộp bài trước khi xem đáp án');
}
function showAnswer1() {
	try {
		var n = -1;
		var a = document.getElementsByName(n);
		while(0 < a.length) {
			var b = document.getElementsByName(-n);
			for(var i = 0; i < a.length; ++i)
				if('#' < a[i].value) {
					b[i].className += ' ch';
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
// function hideAnswer() {
	// var n = document.getElementById('n').value;
	// while(n) {
		// var x = document.getElementsByName(n);
		// if(x != null)
			// for(var i = 0; i < x.length; ++i)
				// x[i].className = '';
		// --n;
	// }
	// document.getElementById('toggle').value = 'Show answer';
// }
// function countdown() {
	// var s = Date.parse(new Date());
	// var tr = setInterval(function(){
		// var d = Date.parse(new Date()) - s;
		// var c = document.getElementById('clock');
		// c.innerHTML = d + ' s';
	// }, 5000);
// }