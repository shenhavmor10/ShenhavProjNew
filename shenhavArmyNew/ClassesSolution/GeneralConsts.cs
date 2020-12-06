using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassesSolution
{
    public static class GeneralConsts
    {
        public const string SPACEBAR = " ";
        public const char ASTERIX = '*';
        public const string EMPTY_STRING = "";
        public const string EQUAL_SIGN = "=";
        public const int NOT_FOUND_STRING = -1;
        public const char TAB_SPACE = '\t';
        public const int ONGOING_THREAD = 0;
        public const int ERROR = 1;
        public const int FINISHED_SUCCESFULLY = 2;
        public const int TIMEOUT_JOIN = 20000;
        public const string NEW_LINE = "\n\r";
        public const string MEMORY_ALLOCATION = "Memory Allocation";
        public const string MEMORY_FREE = "Memory Release (Free)";
        public const string MEMORY_MANAGEMENT = "Memory Management (Allocates And Frees)";
        public const string mallocPattern = "malloc";
        public const string callocPattern = "calloc";
        public const string allocPattern = "alloc";
        public const string reallocPattern = "realloc";
        public const string freePattern = "free";
        public static string [] arrayMemoryPatterns = { mallocPattern, callocPattern, allocPattern, reallocPattern, freePattern };
    }
}
