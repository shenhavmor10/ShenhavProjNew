#ifndef _GRAPH
#define _GRAPH
#define _CRT_SECURE_NO_WARNINGS
#include<limits.h>
#include <stdio.h>
#include <time.h>
#include <stdlib.h>
#include<string.h>
#include <conio.h>

#ifndef _BOOLEAN
#define _BOOLEAN
typedef enum {FALSE,TRUE}BOOLEAN;
#endif

#ifndef N
#define N 100
#endif

#ifndef _STRING
#define _STRING
typedef char STRING[N];
#endif

typedef struct
{
	void ***adjacentMat;
	void **verticesArr;
	int size;
}graph,*graphPtr;


void initGraph(graphPtr);
void addGraphVertex(graphPtr,void *);
int getOffsetByVertex(graphPtr,void *vName,int (*cmp)(void*,void*));
BOOLEAN addGraphEdge(graphPtr,void *vName1,void * vName2,void *edge ,int (*cmp)(void*,void*));
void* removeGraphEdge(graphPtr,void *vName1,void * vName2,int (*cmp)(void*,void*));
BOOLEAN isGraphEdge(graphPtr,void *vName1,void * vName2,int (*cmp)(void*,void*));

#endif