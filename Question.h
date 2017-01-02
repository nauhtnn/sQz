#ifndef _QUEST_H_
#define _QUEST_H_

using namespace std;
#include "Settings.h"

class Question {
public:
	char* stmt; //statement
	size_t nChoices;
	char** choices;
	bool bChoiceSort;
	Question();
	~Question();
	void read(list<char*>::iterator& i, list<char*>::iterator e,
		Settings* deftSt);
	void write(ofstream& stm, int idx, bool DIV = true);
	void wrt(ofstream& stm, int idx);
	void wrtDIV(ofstream& stm, int idx);
	void print();
};

#endif //_QUEST_H_