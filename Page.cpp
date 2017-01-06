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

#define BUF_SIZE 1024

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
		"</script><link rel='stylesheet' type='text/css' href='sQz.css'></head><body>\n")
}
void Page::writeFooter(ofstream& os) {
	WRARR("</body></html>")
}
void Page::writeFormHeader(ofstream& os, size_t nQuest) {
	// const char* c = "<form><input type='hidden' id='n' value='";
	// os.write(c, strlen(c));
	// char* s = new char[(sizeof(int)*8+1)];
	// sprintf(s, "%d", nQuest);
	// os.write(s, strlen(s));
	// delete(s);
	// c = "'>\n";
	// os.write(c, strlen(c));
	WRARR("<form><div id='lp'>Settings<br><input type='button' onclick='score()' value='Score'>"\
		"<input type='button'onclick='setChoice()' value='Show answer'><div id='sht'>"\
		"<table id='ans'><caption>Answer sheet</caption>"\
		"<tr><th class='o'></th><th>A</th><th>B</th><th>C</th><th>D</th></tr>")
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
	len += sprintf(it, "</table></div></div>\n");
	os.write(buf, len);
	delete[] buf;
}
void Page::writeFormFooter(ofstream& os) {
	// const char c[] = "<input type='button' onclick='score()' value='Score'>"\
		// "<input type='button' onclick='toggle()' value='Show answer'></form>\n";
	// os.write(c, sizeof(c)-1);
}