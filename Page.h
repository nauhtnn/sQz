#ifndef _PAGE_H_
#define _PAGE_H_

#include "Settings.h"

class Page {
public:
	Settings* mSt;
	
	Page();
	void writeHeader(ofstream& stm);
	void writeHdr(ofstream& stm);
	void writeHdrDIV(ofstream& stm);
	void writeFooter(ofstream& stm);
	void writeFormHeader(ofstream& stm, size_t nQuest);
	void writeFormFooter(ofstream& stm);
};

#endif //_PAGE_H_