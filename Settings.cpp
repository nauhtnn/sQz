#include <list>
#include <cstring>
#include <regex>
#include <cstdlib>
using namespace std;
#include "Settings.h"

#define MAX_N_QUESTS	(999)
#define DEFT_N_CHOICES	(4)

#define D_FUNC(x) x

Settings::Settings() {
	nQuest = MAX_N_QUESTS;
	nChoices = DEFT_N_CHOICES;
	bQuestSort = true;
	bChoiceSort = true;
	bDIV = true;
}

bool Settings::read(list<char*>::iterator& i) {
	D_FUNC(__func__);
	size_t nStLen = 0; //to check setting/statement
	const char* n = "\\\\[0-9]+", *q = "\\\\[qQ]", *c = "\\\\[cC]";
	char* buf = *i;
	size_t nc = 0;
	bool bQuSort = true, bChSort = true;
	regex r(n);
	cmatch m;
	regex_search(buf, m, r);
	if(0 < m.size()) {
		const char* f = m[0].first, *s = m[0].second;
		nStLen += s - f + 1; //1 is guardband
		nc = 0;
		while(++f < s)
			nc = 10 * nc + (*f) - '0';
	}
	r = regex(q);
	regex_search(buf, m, r);
	if(0 < m.size()) {
		bQuSort = false;
		nStLen += 2 + 1; //1 is guardband
	}
	r = regex(c);
	regex_search(buf, m, r);
	if(0 < m.size()) {
		bChSort = false;
		nStLen += 2 + 1; //1 is guardband
	}
	if(nStLen < strlen(buf)) {
		D_FUNC("end readS");
		return false; //statement
	}
	++i;
	if(1 < nc)
		nChoices = nc;
	bQuestSort = bQuSort;
	bChoiceSort = bChSort;
	D_FUNC(__func__);
	return true; //settings
}
