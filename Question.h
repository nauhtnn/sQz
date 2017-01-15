#ifndef _QUEST_H_
#define _QUEST_H_

using namespace std;
#include "Settings.h"

enum QuestType {
	Single = 1,
	Multiple = 2,
	Insertion = 4,
	Selection = 8,
	Matching = 16
};
enum ContentType {
	Raw = 1,
	Image = 2,
	Audio = 4,
	Video = 8
};

class Question {
public:
	char* stmt; //statement
	size_t nChoices;
	char** choices;
	bool* keys;
	bool bChoiceSort;
	QuestType qType;
	ContentType cType;
	Question();
	~Question();
	void read(list<char*>::iterator& i, list<char*>::iterator e,
		Settings* deftSt);
	void write(ofstream& os, int idx);
	void wrt(ofstream& os, char* idx);
	void wrtChoices(ofstream& os, char* idx);
	void print();
};

#endif //_QUEST_H_