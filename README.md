# UPDATE
I'm strongly considering rewriting this project in C++.
Why? A couple of reasons.
1. I'm currently employed at a firm that uses C++ as their primary language.
2. There's more opportunity to explore different backends. Most notably, LLVM.

So, I may archive this project soon.

# IN PROGRESS
# What is this?
This is my attempt at writing a compiler for a simple C-like imperative language that targets my custom [runtime.](https://github.com/lar9482/AssemblySimulator)

This is more or less based on the compilers course at Cornell University.

Side node:
- Compilers at Cornell is regarded as one of the most difficult CS courses there, which I'm starting to realize why :)

# Goals for the compiler
- [x] Lexical Analysis
- [x] Syntactic Analysis
- [x] Type checking
- [x] IR generation
- [ ] Converting IR to assembly language in the runtime
- [ ] Optimization somewhere?

# BACKLOG(TODO features for later)
1. Implement support for global variables.
