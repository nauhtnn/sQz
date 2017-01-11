#include <iostream>
#include <fstream>
#include <list>
#include <cstring>
#include <regex>
#include <cstdlib>
using namespace std;
#include "utils.h"
#include "Settings.h"
#include "Page.h"

Page::Page() {
	mSt = NULL;
}
void Page::writeHeader(ofstream& os) {
	if(mSt->bDIV)
		writeHdrDIV(os);
	else
		writeHdr(os);
}
void Page::writeHdr(ofstream& os) {
	WRARR("<!DOCTYPE html><html><head><meta charset='utf-8'/><script src='sQz.js'>"\
		"</script></head><body>\n")
}
void Page::writeHdrDIV(ofstream& os) {
	WRARR("<!DOCTYPE html><html><head><meta charset='utf-8'/><script src='sQz.js'>"\
		"</script><link rel='stylesheet' type='text/css' href='sQz.css'></head>"\
		"<body onload='setCell()'>\n")
}
void Page::writeFooter(ofstream& os) {
	WRARR("</body></html>")
}
void Page::writeFormHeader(ofstream& os, size_t nQuest) {
	WRARR("<form><div id='lp'><div class='tit2'>Phiếu trả lời</div><div id='sht'>"\
		"<table id='ans'><tr><th class='o i'></th><th>A</th><th>B</th><th>C</th>"\
		"<th>D</th></tr>")
	//if 0 < nQuest
	char* buf = new char[BUF_SIZE * nQuest], *it;
	const char* fmt = "<tr><td>%d</td><td></td><td></td><td></td><td></td></tr>\n";
	it = buf;
	size_t len = 0;
	for(size_t i = 0; i < nQuest;) { //&& it < e
		int h = sprintf(it, fmt, ++i);
		it += h;
		len += h;
	}
	len += sprintf(it, "</table></div><input type='button'class='btn btn1'"\
			"onclick='score()'value='Submit'><input type='button'"\
			"class='btn'onclick='showAnswer()'value='Answer'>"\
			"</div><div class='bp'></div>");
	os.write(buf, len);
	delete[] buf;
}
void Page::writeFormFooter(ofstream& os) {
	// const char c[] = "<input type='button' onclick='score()' value='Score'>"\
		// "<input type='button' onclick='toggle()' value='Show answer'></form>\n";
	// os.write(c, sizeof(c)-1);
}