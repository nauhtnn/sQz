#ifndef _DEF_H_
#define _DEF_H_
typedef list<char*> chList;
typedef chList::iterator chIt;

void readFile(const char* fname, char*& buf, size_t& len);
bool in(char c, const char* a, size_t n);
void cleanWhSp(char*& buf, size_t& len);
void splitStr(char* buf, size_t len, char c, list<char*>& v);
void HTMLspecialChars(std::string& data);
void HTMLspecialChars(char*& s);
#endif //_DEF_H_