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
	bChoiceSort = true;
}

Question::~Question() {
	if(choices)
		delete choices;
}

void Question::read(chIt& i, chIt e, Settings* deftSt) {
	// if(choices) {
		// for(int ci = 0; ci < nChoices; ++ci)
			// delete[] choices[ci];
		// delete[] choices;
	// }
	SAFE_DEL_AR(stmt);
	SAFE_DEL_AR_AR(choices, nChoices);
	stmt = *i;
	const char* n = "\\\\[0-9]+", *c = "\\\\[cC]";
	regex r(n);
	cmatch m;
	regex_search(*i, m, r);
	if(0 < m.size()) {
		const char* f = m[0].first, *s = m[0].second;
		size_t nc = 0;
		while(++f < s)
			nc = 10 * nc + (*f) - '0';
		if(1 < nc)
			nChoices = nc;
		else
			nChoices = deftSt->nChoices;
		if(stmt < m[0].second)
			stmt = *i + (m[0].second - *i);
	} else
		nChoices = deftSt->nChoices;
	r = regex(c);
	regex_search(*i, m, r);
	if(0 < m.size()) {
		bChoiceSort = false;
		if(stmt < m[0].second)
			stmt = *i + (m[0].second - *i);
	}
	++i;
	choices = new char*[nChoices];
	int ci = 0;
	for(; ci < nChoices && i != e; ++ci)
		choices[ci] = *(i++);
	if(ci < nChoices) {
		for(int cj = ci; cj < nChoices; ++cj)
			choices[cj] = NULL;
		nChoices = ci;
	}
}
void Question::print() {
	cout << "Stmt_" << stmt << '_' << nChoices << " choices\n";
	for(int i = 0; i < nChoices; ++i)
		cout << '_' << choices[i] << "_\n";
}
void Question::write(ofstream& stm, int idx, bool bDIV) {
	if(bDIV)
		wrtDIV(stm, idx);
	else
		wrt(stm, idx);
}
void Question::wrt(ofstream& stm, int idx) {
	char* ix = new char[sizeof(int) * 8 + 1];
	sprintf(ix, "%d", idx);
	stm.write(ix, strlen(ix));
	stm.write(". ", sizeof(". ") - 1);
	HTMLspecialChars(stmt);
	stm.write(stmt, strlen(stmt));
	const char br[] = "<br>\n";
	stm.write(br, sizeof(br) - 1);
	const char *hdr = "<input type='radio' name='", *mid = "' value='";
	int len = strlen(hdr) + strlen(ix) + strlen(mid) + 1;
	char* header = new char[len];
	len = strlen(hdr);
	memcpy(header, hdr, len);
	memcpy(header + len, ix, strlen(ix));
	len += strlen(ix);
	delete(ix);
	memcpy(header + len, mid, strlen(mid));
	len += strlen(mid);
	header[len] = '\0';
	char j = '1';
	chList vChoices;
	for(int i = 0; i < nChoices; ++i)
		vChoices.push_back(choices[i]);
	chIt qi = vChoices.begin();
	while(!vChoices.empty()) {
		qi = vChoices.begin();
		if(bChoiceSort && 1 < vChoices.size()) {
			int pos = rand() % vChoices.size();
			while(pos--)
				++qi;
		}
		stm.write(header, strlen(header));
		char* s = *qi;
		vChoices.erase(qi);
		if(s[0] == '\\') {
			stm.write("0'>", sizeof("0'>") - 1);
			HTMLspecialChars(s);
			stm.write(s + 1, strlen(s) - 1); //+1 to skip '\'
		} else {
			stm.write(&j, sizeof(char));
			++j;
			stm.write("'>", sizeof("'>") - 1);
			HTMLspecialChars(s);
			stm.write(s, strlen(s));
		}
		stm.write(br, sizeof(br) - 1);
	}
	delete(header);
}
void Question::wrtDIV(ofstream& stm, int idx) {
	char* ix = new char[sizeof(int) * 8 + 1];
	sprintf(ix, "%d", idx);
	stm.write("<div class='stmt'>", sizeof("<div class='stmt'>")-1);
	stm.write(ix, strlen(ix));
	stm.write(". ", sizeof(". ") - 1);
	HTMLspecialChars(stmt);
	stm.write(stmt, strlen(stmt));
	stm.write("</div>", sizeof("</div>")-1);
	const char br[] = "<br>\n";
	stm.write(br, sizeof(br) - 1);
	const char *hdr = "<label onclick='check(this)'><input type='radio' name='", *mid = "' value='";
	int len = strlen(hdr) + strlen(ix) + strlen(mid) + 1;
	char* header = new char[len];
	len = strlen(hdr);
	memcpy(header, hdr, len);
	memcpy(header + len, ix, strlen(ix));
	len += strlen(ix);
	delete(ix);
	memcpy(header + len, mid, strlen(mid));
	len += strlen(mid);
	header[len] = '\0';
	char j = '1';
	chList vChoices;
	for(int i = 0; i < nChoices; ++i)
		vChoices.push_back(choices[i]);
	chIt qi = vChoices.begin();
	while(!vChoices.empty()) {
		qi = vChoices.begin();
		if(bChoiceSort && 1 < vChoices.size()) {
			int pos = rand() % vChoices.size();
			while(pos--)
				++qi;
		}
		stm.write(header, strlen(header));
		char* s = *qi;
		vChoices.erase(qi);
		if(s[0] == '\\') {
			stm.write("0'>", sizeof("0'>") - 1);
			HTMLspecialChars(s);
			stm.write(s + 1, strlen(s) - 1); //+1 to skip '\'
		} else {
			stm.write(&j, sizeof(char));
			++j;
			stm.write("'>", sizeof("'>") - 1);
			HTMLspecialChars(s);
			stm.write(s, strlen(s));
		}
		stm.write("</label>", sizeof("</label>")-1);
		stm.write(br, sizeof(br) - 1);
	}
	delete(header);
}
