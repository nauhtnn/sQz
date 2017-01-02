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
void Page::writeHeader(ofstream& stm) {
	if(mSt->bDIV)
		writeHdrDIV(stm);
	else
		writeHdr(stm);
}
void Page::writeHdr(ofstream& stm) {
	const char c[] = "<!DOCTYPE html><html><head><meta charset='utf-8'/><script src='sQz.js'>"\
		"</script></head><body>\n";
	stm.write(c, sizeof(c)-1);
}
void Page::writeHdrDIV(ofstream& stm) {
	const char c[] = "<!DOCTYPE html><html><head><meta charset='utf-8'/><script src='sQz.js'>"\
		"</script><link rel='stylesheet' type='text/css' href='sQz.css'></head><body>\n";
	stm.write(c, sizeof(c)-1);
}
void Page::writeFooter(ofstream& stm) {
	const char c[] = "</body></html>";
	stm.write(c, sizeof(c)-1);
}
void Page::writeFormHeader(ofstream& stm, size_t nQuest) {
	// const char* c = "<form><input type='hidden' id='n' value='";
	// stm.write(c, strlen(c));
	// char* s = new char[(sizeof(int)*8+1)];
	// sprintf(s, "%d", nQuest);
	// stm.write(s, strlen(s));
	// delete(s);
	// c = "'>\n";
	// stm.write(c, strlen(c));
	stm.write("<form>\n", sizeof("<form>\n") - 1);
}
void Page::writeFormFooter(ofstream& stm) {
	const char c[] = "<input type='button' onclick='score()' value='Score'>"\
		"<input type='button' onclick='toggle()' value='Show answer'></form>\n";
	stm.write(c, sizeof(c)-1);
}