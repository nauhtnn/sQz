#include <cstdlib>
#include <iostream>
#include <fstream>
#include <list>
#include <regex>
#include <cstring>
using namespace std;
#include "utils.h"
#include "Question.h"
#include "Page.h"

Question::Question() {
	stmt = NULL;
	nChoices = 0;
	choices = NULL;
	keys = NULL;
	bChoiceSort = true;
	qType = QuestType::Single;
	cType = ContentType::Raw;
}

Question::~Question() {
	SAFE_DEL_AR(choices)
	SAFE_DEL_AR(keys)
}

void Question::read(chIt& i, chIt e, Settings* deftSt) {
	SAFE_DEL_AR(stmt);
	SAFE_DEL_AR_AR(choices, nChoices);
	stmt = *i;
	const char* n = "\\\\[0-9]+", *c = "\\\\[cC]";
	regex r(n);
	cmatch m;
	// regex_search(*i, m, r);
	// if(0 < m.size()) {
		// const char* f = m[0].first, *s = m[0].second;
		// size_t nc = 0;
		// while(++f < s)
			// nc = 10 * nc + (*f) - '0';
		// if(1 < nc)
			// nChoices = nc;
		// else
			// nChoices = deftSt->nChoices;
		// if(stmt < m[0].second)
			// stmt = *i + (m[0].second - *i);
	// } else
		nChoices = deftSt->nChoices;
	// r = regex(c);
	// regex_search(*i, m, r);
	// if(0 < m.size()) {
		// bChoiceSort = false;
		// if(stmt < m[0].second)
			// stmt = *i + (m[0].second - *i);
	// }
	++i;
	choices = new char*[nChoices];
	keys = new bool[nChoices];
	memset(keys, 0, nChoices * sizeof(bool));
	int ci = 0, keyC = 0;
	for(; ci < nChoices && i != e; ++ci) {
		choices[ci] = *(i++);
		if(choices[ci][0] == '\\') {
			keys[ci] = true;
			++keyC;
			choices[ci] += 1;
			// cleanFront(choices[ci]);
		}
	}
	if(ci < nChoices) {
		for(int cj = ci; cj < nChoices; ++cj)
			choices[cj] = NULL;
		nChoices = ci;
	}
	if(1 < keyC && qType == QuestType::Single)
		qType = QuestType::Multiple;
}
void Question::print() {
	cout << "Stmt_" << stmt << '_' << nChoices << " choices\n";
	for(int i = 0; i < nChoices; ++i)
		cout << '_' << choices[i] << "_\n";
}
void Question::write(ofstream& os, int idx) {
	char* ix = new char[sizeof(int) * 8 + 1];
	sprintf(ix, "%d", idx);
	wrt(os, ix);
	SAFE_DEL_AR(ix)
}
void wrtImg(ofstream& os, char* s) {
	const char* img ="\\{[a-zA-Z0-9\\. :]+\\}";
	regex r(img);
	cmatch m;
	regex_search(s, m, r);
	if(0 < m.size()) {
		char* t = s;
		t += m[0].first - s;
		*t = '\0';
		t += m[0].second - 1 - t;
		*t = '\0';
		cout << s;
		cout << "<img src='" << m[0].first + 1 << "'>";
		cout << m[0].second;
		// WRPTR(s)
		// WRARR("Image 1")
		// WRPTR(m[0].second)
		// WRARR("<img src='")
		// WRPTR(m[0].first + 1)
		// WRARR("'>")
	}
	else
		cout << "____";
}
void Question::wrt(ofstream& os, char* idx) {
	WRARR("<div class='cl'><div class='qid'>")
	WRPTR(idx)
	WRARR("</div><div class='q'><div class='stmt'>")
	HTMLspecialChars(stmt);
	WRPTR(stmt)
	// wrtImg(os, stmt);
	WRARR("</div>\n")
	if(qType == QuestType::Single ||
		qType == QuestType::Multiple)
		wrtChoices(os, idx);
}
void Question::wrtChoices(ofstream& os, char* idx) {
	const char *hdr = "<div name='", *mid = "'class='c'><span class='cid'>(";
	size_t lh = strlen(hdr) + strlen(idx) + strlen(mid) + 1; //+1 for '\0'
	char* header = new char[lh];
	lh = strlen(hdr);
	memcpy(header, hdr, lh);
	memcpy(header + lh, idx, strlen(idx));
	lh += strlen(idx);
	memcpy(header + lh, mid, strlen(mid));
	lh += strlen(mid);
	header[lh] = '\0';
	if(qType == QuestType::Single)
		hdr = ")</span><input type='radio' name='-";
	else //QuestType::Multiple
		hdr = ")</span><input type='checkbox' name='-";
	mid = "' value='";
	size_t lm = strlen(hdr) + strlen(idx) + strlen(mid) + 1; //+1 for '\0'
	char* middle = new char[lm];
	lm = strlen(hdr);
	memcpy(middle, hdr, lm);
	memcpy(middle + lm, idx, strlen(idx));
	lm += strlen(idx);
	memcpy(middle + lm, mid, strlen(mid));
	lm += strlen(mid);
	middle[lm] = '\0';
	char j = 'A';
	chList vChoices;
	list<bool> vKeys;
	for(int i = 0; i < nChoices; ++i) {
		vChoices.push_back(choices[i]);
		vKeys.push_back(keys[i]);
	}
	chIt qi = vChoices.begin();
	list<bool>::iterator ki = vKeys.begin();
	while(!vChoices.empty()) {
		qi = vChoices.begin();
		ki = vKeys.begin();
		if(bChoiceSort && 1 < vChoices.size()) {
			int pos = rand() % vChoices.size();
			while(pos--) {
				++qi;
				++ki;
			}
		}
		WRPTRN(header, lh)
		WRPTRN(&j, sizeof(char))
		WRPTRN(middle, lm)
		if(*ki) {
			char r = j - 'A' + '0';
			os.write(&r, sizeof(char));
		} else {
			char r = '#';
			os.write(&r, sizeof(char));
		}
		WRARR("'>")
		HTMLspecialChars(*qi);
		WRPTR(*qi)
		WRARR("</div>\n")
		vChoices.erase(qi);
		vKeys.erase(ki);
		++j;
	}
	WRARR("</div></div>")
	SAFE_DEL_AR(header)
	SAFE_DEL_AR(middle)
}