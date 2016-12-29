#include <iostream>
#include <fstream>
#include <cstring>
#include <list>
using namespace std;
void readFile(const char* fname, char*& buf, size_t& len) {
	ifstream is(fname, ifstream::binary);
	if(!is)
		cout << "Cannot read file " << fname << "\n";
	else {
		cout << "Read file " << fname << "\n";
		is.seekg(0, is.end);
		len = is.tellg();
		buf = new char[len];
		is.seekg(0, is.beg);
		is.read(buf, len);
		if (!is) {
			len = is.gcount();
			char* b = buf;
			buf = new char[len];
			memcpy(buf, b, len);
			delete[] b;
			cout << "\tError! Only " << len << " could be read\n";
		}
	}
	is.close();
}
bool in(char c, const char* a, size_t n) {
	for(int i = 0; i < n; ++i)
		if(c == a[i])
			return true;
	return false;
}

//WARNING: the following function may cause leak memory
void cleanWhSp(char*& buf, size_t& len) {
	const char whSp[] = {' ', '\t', 13, '\n'};//include 13
	char* s = new char[len], *j = s;
	char* i = buf, *e = buf + len;
	while(i < e && in(*i, whSp, sizeof(whSp)))
		++i;//truncate front
	while(i < e) {
		do *j++ = *i++;
		while(i < e && !in(*i, whSp, sizeof(whSp)));
		if(i < e) {
			char* h = i;
			do ++i;//truncate middle
			while(i < e && in(*i, whSp, sizeof(whSp)));
			if(i < e) {//truncate end
				bool nl = false;
				while(h < i && !nl)
					if(*h++ == '\n')
						nl = true;
				if(nl)
					*j++ = '\n';
				else
					*j++ = ' ';
			}
		}
	}
	len = j - s;
	buf = new char[len + 1];//add last character = '\0'
	memcpy(buf, s, len);
	buf[len] = '\0';
}

void splitStr(char* buf, size_t len, char c, list<char*>& v) {
	//buf must be cleaned before, or crash will occur
	char* i = buf, *e = buf + len;
	v.push_back(i++);
	while(i < e) {
		while(*i++ != c && i < e);//order is ok
		if(i < e) {
			*(i - 1) = '\0';
			v.push_back(i++);
		}
	}
}

void HTMLspecialChars(std::string& data) {
	if(data.find('&') == string::npos &&
		data.find('\"') == string::npos &&
		data.find('\'') == string::npos &&
		data.find('<') == string::npos &&
		data.find('>') == string::npos)
		return;
    string buffer;
    buffer.reserve(data.size() + 10);
    for(size_t pos = 0; pos != data.size(); ++pos) {
        switch(data[pos]) {
			case '&':
				buffer.append("&amp;");
				break;
			case '\"':
				buffer.append("&quot;");
				break;
			case '\'':
				buffer.append("&apos;");
				break;
			case '<':
				buffer.append("&lt;");
				break;
			case '>':
				buffer.append("&gt;");
				break;
			default:
				buffer.append(&data[pos], 1);
				break;
        }
    }
    data.swap(buffer);
}

void HTMLspecialChars(char*& s) {
    size_t len = 0, olen = 0;
	for(char* i = s; *i != '\0'; ++i) {
		switch(*i) {
			case '&':
				len += sizeof("&amp;") - 1;
				break;
			case '\"':
				len += sizeof("&quot;") - 1;
				break;
			case '\'':
				len += sizeof("&apos;") - 1;
				break;
			case '<':
				len += sizeof("&lt;") - 1;
				break;
			case '>':
				len += sizeof("&gt;") - 1;
				break;
			default:
				++len;
        }
		++olen;
	}
	if(len == olen)
		return;
    char* buf = new char[len + 1], *e = s + olen;
    for(char* i = s, *j = buf; i < e;++i)
		switch(*i) {
			case '&': 
				strcpy(j, "&amp;");
				j += sizeof("&amp'") - 1;
				break;
			case '\"':
				strcpy(j, "&quot;");
				j += sizeof("&quot;") - 1;
				break;
			case '\'':
				strcpy(j, "&apos;");
				j += sizeof("&apos;") - 1;
				break;
			case '<':
				strcpy(j, "&lt;");
				j += sizeof("&lt;") - 1;
				break;
			case '>':
				strcpy(j, "&gt;");
				j += sizeof("&gt;") - 1;
				break;
			default:
				*j++ = *i;
		}
	buf[len] = '\0';
    s = buf;
}
