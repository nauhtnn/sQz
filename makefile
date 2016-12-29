CC=g++
CFLAGS=-c -std=c++11
LFLAGS=-std=c++11 -o sQz
sQz: Page.o Question.o utils.o sQz.cpp Settings.o
	$(CC) $(LFLAGS) Page.o Question.o utils.o Settings.o sQz.cpp
Page.o: Page.h Page.cpp Settings.o
	$(CC) $(CFLAGS) Page.cpp
Question.o: Question.h Question.cpp Settings.o
	$(CC) $(CFLAGS) Question.cpp
Settings.o: Settings.h Settings.cpp
	$(CC) $(CFLAGS) Settings.cpp
utils.o: utils.cpp
	$(CC) $(CFLAGS) utils.cpp