#include <iostream>
#include <fstream>
#include <list>
#include <cstring>
#include <regex>
#include <cstdlib>
using namespace std;
#include "Settings.h"
#include "Page.h"
#include "Question.h"
#include "utils.h"

#define MAX_COLUMN 2

int main(int argc, const char* argv[]) {
	cout << "sQz version 0.0.2\n";
	// string fname = (1 < argc) ? argv[1] : "qz.txt";
	char* fname = new char[BUF_SIZE], fi = 0;
	sprintf(fname, "qz%i.txt", ++fi);
	char* buf = NULL;
	size_t len = 0;
	// readFile(fname.c_str(), buf, len);
	// if(!buf)
		// return -1;
	readFile(fname, buf, len);
	while(buf) {
		cleanWhSp(buf, len);
		chList vToken;
		splitStr(buf, len, '\n', vToken);
		Settings st;
		Page pg;
		pg.mSt = &st;
		chIt i = vToken.begin(), e = vToken.end();
		while(i != e && st.read(i));
		
		list<Question*> vQuest;
		Question* q = new Question;
		while(i != e){
			q->read(i, e, &st);
			vQuest.push_back(q);
			q = new Question;
		}
		delete(q);
		// size_t pos = fname.find_last_of('.');
		// fname = fname.substr(0, pos);
		// fname += ".html";
		size_t pos = 0;
		sprintf(fname, "qz%i.html", fi);
		ofstream os(fname, ofstream::out);
		if(!os) {
			cout << "Cannot write file " << fname << "\n";
			return -1;
		} else
			cout << "Write file " << fname << "\n";
		pg.writeHeader(os);
		pg.writeFormHeader(os, vQuest.size());
		list<Question*>::iterator qi = vQuest.begin();
		len = vQuest.size();
		srand(time(NULL));
		int j = 0, column = MAX_COLUMN;
		while(!vQuest.empty()) {
			if(column == MAX_COLUMN) {
				WRARR("<div class='cl1'></div>")
				column = 0;
			}
			qi = vQuest.begin();
			if(pg.mSt->bQuestSort) {
				pos = rand() % vQuest.size();
				while(pos--)
					++qi;
			}
			q = *qi;
			vQuest.erase(qi);
			q->write(os, ++j);
			delete(q);
			++column;
		}
		pg.writeFormFooter(os);
		pg.writeFooter(os);
		os.close();
		SAFE_DEL_AR(buf);
		
		sprintf(fname, "qz%i.txt", ++fi);
		readFile(fname, buf, len);
	}
	SAFE_DEL_AR(fname);
	return 0;
}