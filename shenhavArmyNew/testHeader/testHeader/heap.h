#define _CRT_SECURE_NO_WARNINGS
#pragma once
#ifndef _INC_HEAP
#define _INC_HEAP
#include <stdio.h>
#include<conio.h>
#include<stdlib.h>
//hufman.h
#include<time.h>
#include<string.h>
#ifndef _BOOLEAN
#define _BOOLEAN
typedef enum { FALSE, TRUE }BOOLEAN;
#endif // !_BOOLEAN
#ifndef N
#define N 80
#endif //!N

#ifndef _STRING
#define _STRING
typedef char STRING[N];
#endif // !_STRING

typedef struct
{
	void**ar;
	int size;
}heap, *heapPtr;

typedef struct hufmanNode
{
	int frequancy;
	struct hufmanNode*left;
	struct hufmanNode*right;

}hufmanNode, *hufmanNodePtr;
void initHeap(heapPtr);
void heapifyUp(heapPtr, int, int(*cmp)(void*, void*), void(*updateIndex)(void **, void **, void * , int , int ));
void addToHeap(heapPtr, void*, int(*cmp)(void*, void*), void(*updateIndex)(void **, void **, void *, int, int), void(*prn)(void *));
void heapifyDown(heapPtr, int, int(*cmp)(void*, void*), void(*updateIndex)(void **, void **, void *, int, int));
void *subtructHeap(heapPtr, int(*cmp)(void*, void*), void(*updateIndex)(void **, void **, void *, int, int));
#endif
