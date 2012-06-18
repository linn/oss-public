// See http://en.wikipedia.org/wiki/C_preprocessor#Quoting_macro_arguments
// for use of QUOTEME to put quote's round macros.
//
#define QUOTEME_(x) #x
#define QUOTEME(x) QUOTEME_(x)

#ifdef LOGGING

  #define LOG(str) log(str)
  #define TIMESTAMP(str) timestamp(str)

#else

  #define LOG(str) //
  #define TIMESTAMP(str) //

#endif

#define DAVAAR 1
