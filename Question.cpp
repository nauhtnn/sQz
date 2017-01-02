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
#define WRARR(x) os.write(x,sizeof(x)-1);
#define WRPTR(x) os.write(x,strlen(x));
#define WRPTRN(x,n) os.write(x,n);

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
void Question::write(ofstream& os, int idx, bool bDIV) {
	if(bDIV)
		wrtDIV(os, idx);
	else
		wrt(os, idx);
}
void Question::wrt(ofstream& os, int idx) {
	char* ix = new char[sizeof(int) * 8 + 1];
	sprintf(ix, "%d", idx);
	WRPTR(ix)
	WRARR(". ")
	HTMLspecialChars(stmt);
	WRARR(stmt);
	const char br[] = "<br>\n";
	WRARR(br)
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
		WRPTR(header)
		char* s = *qi;
		vChoices.erase(qi);
		if(s[0] == '\\') {
			WRARR("0'>")
			HTMLspecialChars(s);
			os.write(s + 1, strlen(s) - 1); //+1 to skip '\'
		} else {
			os.write(&j, sizeof(char));
			++j;
			WRARR("'>")
			HTMLspecialChars(s);
			WRPTR(s)
		}
		WRARR(br)
	}
	delete(header);
}
void Question::wrtDIV(ofstream& os, int idx) {
	char* ix = new char[sizeof(int) * 8 + 1];
	sprintf(ix, "%d", idx);
	WRARR("<div class='qid'>")
	WRPTR(ix)
	WRARR("</div><div class='q'><div class='stmt'>");
	HTMLspecialChars(stmt);
	WRPTR(stmt)
	WRARR("</div>\n");
	const char *hdr = "<label><div onmouseup='check(this)' name='", *mid = "' class='choice'><span class='cid'>(";
	size_t lh = strlen(hdr) + strlen(ix) + strlen(mid) + 1; //+1 for '\0'
	char* header = new char[lh];
// <div class='qid'>1</div><div class='q'><div class='stmt'>Trong soạn thảo Word, muốn chuyển đổi giữa hai chế đó gõ: chế đó gõ chèn và chế độ gõ thay thế, ta nhấn phím nào trên bàn phím</div>
// <label><div onmouseup='check(this)' name='2' class='choice'><span class='cid'>(A)</span><input type='radio' name='-2' value='1'>Tab</div></label>
// </div>
	lh = strlen(hdr);
	memcpy(header, hdr, lh);
	memcpy(header + lh, ix, strlen(ix));
	lh += strlen(ix);
	memcpy(header + lh, mid, strlen(mid));
	lh += strlen(mid);
	header[lh] = '\0';
	hdr = ")</span><input type='radio' name='-";
	mid = "' value='";
	size_t lm = strlen(hdr) + strlen(ix) + strlen(mid) + 1; //+1 for '\0'
	char* middle = new char[lm];
	lm = strlen(hdr);
	memcpy(middle, hdr, lm);
	memcpy(middle + lm, ix, strlen(ix));
	lm += strlen(ix);
	memcpy(middle + lm, mid, strlen(mid));
	lm += strlen(mid);
	middle[lm] = '\0';
	mid = "</div></label>\n";
	char j = 'A';
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
		// os.write(header, lh);
		WRPTRN(header, lh)
		WRPTRN(&j, sizeof(char))
		WRPTRN(middle, lm)
		char* s = *qi;
		vChoices.erase(qi);
		if(s[0] == '\\') {
			char r = j - 'A' + '0';
			os.write(&r, sizeof(char));
			WRARR("'>")
			HTMLspecialChars(s);
			os.write(s + 1, strlen(s) - 1); //+1 to skip '\'
		} else {
			WRARR("#'>")
			HTMLspecialChars(s);
			WRPTR(s)
		}
		WRARR("</div></label>\n")
		++j;
	}
	WRARR("</div>")
	SAFE_DEL_AR(ix)
	SAFE_DEL_AR(header)
	SAFE_DEL_AR(middle)
}
