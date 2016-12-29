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

int main(int argc, const char* argv[]) {
	string fname = (1 < argc) ? argv[1] : "qz.txt";
	char* buf = NULL;
	size_t len = 0;
	readFile(fname.c_str(), buf, len);
	if(!buf)
		return -1;
	cleanWhSp(buf, len);
	chList vToken;
	splitStr(buf, len, '\n', vToken);
	Settings st;
	Page pg;
	pg.mSt = &st;
	chIt i = vToken.begin(), e = vToken.end();
	while(i != e && st.read(i));
	// cout << pg.nChoices << '_' << pg.bQuestSort << '_' << pg.bChoiceSort;
	// list<char*>::iterator i = vToken.begin(), e = vToken.end();
	// while(i != e) {
		// cout << *i << '\n';
		// ++i;
	// }
	
	list<Question*> vQuest;
	Question* q = new Question;
    while(i != e){
		q->read(i, e, &st);
		vQuest.push_back(q);
		//q->print();
		q = new Question;
    }
	delete(q);
	size_t pos = fname.find_last_of('.');
	fname = fname.substr(0, pos);
	fname += ".html";
	ofstream writer(fname.c_str(), ofstream::out);
	if(!writer) {
		cout << "Cannot write file " << fname << "\n";
		return -1;
	} else
		cout << "Write file " << fname << "\n";
	pg.writeHeader(writer);
	pg.writeFormHeader(writer, vQuest.size());
	list<Question*>::iterator qi = vQuest.begin();
	len = vQuest.size();
	srand(time(NULL));
	int j = 0;
	while(!vQuest.empty()) {
		qi = vQuest.begin();
		if(pg.mSt->bQuestSort) {
			pos = rand() % vQuest.size();
			while(pos--)
				++qi;
		}
		q = *qi;
		vQuest.erase(qi);
		q->write(writer, ++j);
		delete(q);
	}
	pg.writeFormFooter(writer);
	pg.writeFooter(writer);
	writer.close();
	SAFE_DEL_AR(buf);
	return 0;
}