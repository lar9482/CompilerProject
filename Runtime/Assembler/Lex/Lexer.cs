using System.Text.RegularExpressions;

namespace Compiler.Runtime;

public class Lexer {
    private readonly Regex matchIdentifier;
    private readonly Regex matchWhitespace;
    private readonly Regex matchOneSymbol;
    private readonly Regex matchIntegers;

    private int lineCounter;

    public Lexer() {
        this.matchIdentifier = new Regex(@"\b^[a-zA-Z]{1}[a-zA-Z0-9_]*\b");
        this.matchWhitespace = new Regex(@"\b^\n|\t|\s|\r\b");
        this.matchOneSymbol = new Regex(@"^[,:\[\]]");
        this.matchIntegers = new Regex(@"^-?\b\d+\b");
        this.lineCounter = 1;
    }

    public Queue<Token> lexProgram(string programText) {
        Queue<Token> tokens = new Queue<Token>();

        Token lastTokenSeen = new Token("", 0, TokenType.halt_Inst);
        while (programText.Length > 0) {
            Tuple<string, string> longestMatchWithType = scanLongestMatch(programText);
            string matchedLexeme = longestMatchWithType.Item1;
            string matchType = longestMatchWithType.Item2;

            switch(matchType) {
                case "matchIdentifier":
                    lastTokenSeen = resolveWordLexeme(matchedLexeme);
                    tokens.Enqueue(lastTokenSeen);
                    break;
                case "matchOneSymbol":
                    lastTokenSeen = resolveOneSymbolLexeme(matchedLexeme); 
                    tokens.Enqueue(lastTokenSeen);
                    break;
                case "matchIntegers":
                    lastTokenSeen = new Token(matchedLexeme, lineCounter, TokenType.integer);
                    tokens.Enqueue(lastTokenSeen);
                    break;
                case "matchWhitespace":
                    if (matchedLexeme == "\n")
                        lineCounter++;
                    break;
                default:
                    throw new InvalidOperationException(
                        String.Format("Lexer: {0} is not a recognizable lexeme", matchedLexeme)
                    );
            }
            programText = programText.Remove(0, matchedLexeme.Length);
        }

        // A halt instruction is automatically added if the source file didn't have one.
        if (lastTokenSeen.type != TokenType.halt_Inst) {
            tokens.Enqueue(new Token("halt", lineCounter+1, TokenType.halt_Inst));
        }

        //This EOF token will ensure that the parser can determine when to terminate
        tokens.Enqueue(new Token("EOF",  lineCounter+2, TokenType.EOF));
        return tokens;
    }

    private Tuple<string, string> scanLongestMatch(string programText) {
        Dictionary<string, string> matches = new Dictionary<string, string>();
        matches.Add("matchIdentifier", matchIdentifier.Match(programText).Value);
        matches.Add("matchOneSymbol", matchOneSymbol.Match(programText).Value);
        matches.Add("matchIntegers", matchIntegers.Match(programText).Value);
        matches.Add("matchWhitespace", matchWhitespace.Match(programText).Value);

        int longestMatchLength = 0;
        string longestMatch = "";
        string matchType = "";

        foreach(KeyValuePair<string, string> regexProgramMatch in matches) {
            if (regexProgramMatch.Value.Length > longestMatchLength) {
                longestMatch = regexProgramMatch.Value;
                matchType = regexProgramMatch.Key;
                longestMatchLength = regexProgramMatch.Value.Length;
            }
        }

        return Tuple.Create<string, string>(longestMatch, matchType);
    }

    private Token resolveOneSymbolLexeme(string lexeme) {
        switch(lexeme) {
            case ",":
                return new Token(lexeme, lineCounter, TokenType.comma);
            case ":":
                return new Token(lexeme, lineCounter, TokenType.colon);
            case "[":
                return new Token(lexeme, lineCounter, TokenType.startBracket);
            case "]":
                return new Token(lexeme, lineCounter, TokenType.endBracket);
            default:
                throw new Exception(String.Format("The one symbol lexeme {0} is unrecognizable", lexeme));
        }
    }

    private Token resolveWordLexeme(string lexeme) {
        switch(lexeme) {
            case "rZERO":
                return new Token(lexeme, lineCounter, TokenType.rZERO_Reg);
            case "r1":
                return new Token(lexeme, lineCounter, TokenType.r1_Reg);
            case "r2":
                return new Token(lexeme, lineCounter, TokenType.r2_Reg);
            case "r3":
                return new Token(lexeme, lineCounter, TokenType.r3_Reg);
            case "r4":
                return new Token(lexeme, lineCounter, TokenType.r4_Reg);
            case "r5":
                return new Token(lexeme, lineCounter, TokenType.r5_Reg);
            case "r6":
                return new Token(lexeme, lineCounter, TokenType.r6_Reg);
            case "r7":
                return new Token(lexeme, lineCounter, TokenType.r7_Reg);
            case "r8":
                return new Token(lexeme, lineCounter, TokenType.r8_Reg);
            case "r9":
                return new Token(lexeme, lineCounter, TokenType.r9_Reg);
            case "r10":
                return new Token(lexeme, lineCounter, TokenType.r10_Reg);
            case "r11":
                return new Token(lexeme, lineCounter, TokenType.r11_Reg);
            case "r12":
                return new Token(lexeme, lineCounter, TokenType.r12_Reg);
            case "r13":
                return new Token(lexeme, lineCounter, TokenType.r13_Reg);
            case "r14":
                return new Token(lexeme, lineCounter, TokenType.r14_Reg);
            case "r15":
                return new Token(lexeme, lineCounter, TokenType.r15_Reg);
            case "r16":
                return new Token(lexeme, lineCounter, TokenType.r16_Reg);
            case "rSP":
                return new Token(lexeme, lineCounter, TokenType.rSP_Reg); 
            case "rFP":
                return new Token(lexeme, lineCounter, TokenType.rFP_Reg);   
            case "rRET":
                return new Token(lexeme, lineCounter, TokenType.rRET_Reg); 
            case "rHI":
                return new Token(lexeme, lineCounter, TokenType.rHI_Reg); 
            case "rLO":
                return new Token(lexeme, lineCounter, TokenType.rLO_Reg); 
            case "mov":
                return new Token(lexeme, lineCounter, TokenType.mov_Inst);
            case "add":
                return new Token(lexeme, lineCounter, TokenType.add_Inst);
            case "sub":
                return new Token(lexeme, lineCounter, TokenType.sub_Inst);
            case "mult":
                return new Token(lexeme, lineCounter, TokenType.mult_Inst);
            case "div":
                return new Token(lexeme, lineCounter, TokenType.div_Inst);
            case "and":
                return new Token(lexeme, lineCounter, TokenType.and_Inst);
            case "or":
                return new Token(lexeme, lineCounter, TokenType.or_Inst);
            case "xor":
                return new Token(lexeme, lineCounter, TokenType.xor_Inst);    
            case "not":
                return new Token(lexeme, lineCounter, TokenType.not_Inst);
            case "nor":
                return new Token(lexeme, lineCounter, TokenType.nor_Inst);
            case "sllv":
                return new Token(lexeme, lineCounter, TokenType.sllv_Inst);
            case "srav":
                return new Token(lexeme, lineCounter, TokenType.srav_Inst);
            case "movI":
                return new Token(lexeme, lineCounter, TokenType.movI_Inst);
            case "addI":
                return new Token(lexeme, lineCounter, TokenType.addI_Inst);
            case "subI":
                return new Token(lexeme, lineCounter, TokenType.subI_Inst);
            case "multI":
                return new Token(lexeme, lineCounter, TokenType.multI_Inst);
            case "divI":
                return new Token(lexeme, lineCounter, TokenType.divI_Inst);
            case "andI":
                return new Token(lexeme, lineCounter, TokenType.andI_Inst);
            case "orI":
                return new Token(lexeme, lineCounter, TokenType.orI_Inst);
            case "xorI":
                return new Token(lexeme, lineCounter, TokenType.xorI_Inst);
            case "sll":
                return new Token(lexeme, lineCounter, TokenType.sll_Inst);
            case "sra":
                return new Token(lexeme, lineCounter, TokenType.sra_Inst);
            case "bEq":
                return new Token(lexeme, lineCounter, TokenType.bEq_Inst);
            case "bNe":
                return new Token(lexeme, lineCounter, TokenType.bNe_Inst);
            case "bLt":
                return new Token(lexeme, lineCounter, TokenType.bLt_Inst);
            case "bGt":
                return new Token(lexeme, lineCounter, TokenType.bGt_Inst);
            case "jmp":
                return new Token(lexeme, lineCounter, TokenType.jmp_Inst);
            case "jmpL":
                return new Token(lexeme, lineCounter, TokenType.jmpL_Inst);
            case "jmpL_Reg":
                return new Token(lexeme, lineCounter, TokenType.jmpL_Reg_Inst);
            case "jmpReg":
                return new Token(lexeme, lineCounter, TokenType.jmpRet_Inst);
            case "lb":
                return new Token(lexeme, lineCounter, TokenType.lb_Inst);
            case "lw":
                return new Token(lexeme, lineCounter, TokenType.lw_Inst);
            case "sb":
                return new Token(lexeme, lineCounter, TokenType.sb_Inst);
            case "sw":
                return new Token(lexeme, lineCounter, TokenType.sw_Inst);
            case "halt":
                return new Token(lexeme, lineCounter, TokenType.halt_Inst);
            case "printw_int": 
                return new Token(lexeme, lineCounter, TokenType.printw_Int_Inst);
            case "printw_hex": 
                return new Token(lexeme, lineCounter, TokenType.printw_Hex_Inst);
            case "printw_bin": 
                return new Token(lexeme, lineCounter, TokenType.printw_Bin_Inst);
            default:
                return new Token(lexeme, lineCounter, TokenType.identifier);
        }
    }
}