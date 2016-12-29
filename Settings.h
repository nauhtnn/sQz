#ifndef _SETTINGS_H_
#define _SETTINGS_H_

class Settings {
public:
	size_t nQuest;
	size_t nChoices;
	bool bChoiceSort;
	bool bQuestSort;
	bool bDIV;
	
	Settings();
	bool read(list<char*>::iterator& i);
};

#endif //_SETTINGS_H_