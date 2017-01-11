#ifndef _DEF_H_
#define _DEF_H_
typedef list<char*> chList;
typedef chList::iterator chIt;

#define BUF_SIZE (1024)

#define SAFE_DEL(p) if(p){delete(p);p=NULL}
#define SAFE_DEL_AR(p) if(p){delete[]p;p=NULL;}
#define SAFE_DEL_AR_AR(p, n)\
if(p){for(int pi=0;pi<n;++pi)delete[]p[pi];delete[]p;p=NULL;}

#define D_FUNC(x) cout<<x<<'\n';

#define WRARR(x) os.write(x,sizeof(x)-1);
#define WRPTR(x) os.write(x,strlen(x));
#define WRPTRN(x,n) os.write(x,n);

void readFile(const char* fname, char*& buf, size_t& len);
bool in(char c, const char* a, size_t n);
void cleanWhSp(char*& buf, size_t& len);
void splitStr(char* buf, size_t len, char c, list<char*>& v);
void HTMLspecialChars(std::string& data);
void HTMLspecialChars(char*& s);
#endif //_DEF_H_