#ifndef _DYN_QUEUE
#define _DYN_QUEUE
#include <stdio.h>
#include <math.h>
#include <stdlib.h>
#include <time.h>
#include <string.h>

#ifndef SIZE
#define SIZE 80
#endif

#ifndef _STRING
#define _STRING
typedef char STRING[SIZE];
#endif

#ifndef _BOOLEAN
#define _BOOLEAN
typedef enum {FALSE,TRUE}BOOLEAN;
#endif

#ifndef N
#define N 15
#endif

typedef struct{
	void** arr;
	int num_of_items;
}DynQueue, *DynQueuePtr;

void initDynQueue(DynQueuePtr);
BOOLEAN isEmptyDynQueue(DynQueuePtr);
void *removeFromDynQueue(DynQueuePtr);
void insertDynQueue(DynQueuePtr,void *);
void printDynQueue(DynQueuePtr,void(*prn)(void*));
void emptyDynQueue(DynQueuePtr,void (*freeCast)(void*));
#endif