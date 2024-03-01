using System.Runtime.CompilerServices;

namespace Compiler.Runtime;

public class Parser {
    private Queue<Token> tokenQueue;
    
    public Parser(Queue<Token> tokenQueue) {
        this.tokenQueue = tokenQueue;
    } 

    public List<Inst> parse() {
        List<Inst> instructions = new List<Inst>();
        while (tokenQueue.Count > 0) {
            switch(tokenQueue.Peek().type) {
                case TokenType.mov_Inst:
                case TokenType.add_Inst:
                case TokenType.sub_Inst:
                case TokenType.mult_Inst:
                case TokenType.div_Inst:
                case TokenType.and_Inst:
                case TokenType.or_Inst:
                case TokenType.xor_Inst:
                case TokenType.not_Inst:
                case TokenType.nor_Inst:
                case TokenType.sllv_Inst:
                case TokenType.srav_Inst:
                    instructions.Add(parseRegInst());
                    break;
                case TokenType.movI_Inst:
                case TokenType.addI_Inst:
                case TokenType.subI_Inst:
                case TokenType.multI_Inst:
                case TokenType.divI_Inst:
                case TokenType.andI_Inst:
                case TokenType.orI_Inst:
                case TokenType.xorI_Inst:
                case TokenType.sll_Inst:
                case TokenType.sra_Inst:
                    instructions.Add(parseImmInst());
                    break;
                case TokenType.lb_Inst:
                case TokenType.lw_Inst:
                case TokenType.sb_Inst:
                case TokenType.sw_Inst:
                    instructions.Add(parseMemInst());
                    break;
                case TokenType.jmpL_Reg_Inst:
                case TokenType.jmpRet_Inst:
                    instructions.Add(parseJmpRegInst());
                    break;
                case TokenType.jmp_Inst:
                case TokenType.jmpL_Inst:
                    instructions.Add(parseJmpLabelInst());
                    break;
                case TokenType.bEq_Inst:
                case TokenType.bNe_Inst:
                case TokenType.bLt_Inst:
                case TokenType.bGt_Inst:
                    instructions.Add(parseJmpBranchInst());
                    break;
                case TokenType.identifier:
                    instructions.Add(parseLabelInst());
                    break;
                case TokenType.halt_Inst:
                    instructions.Add(parseInterruptInst());
                    break;
                case TokenType.printw_Int_Inst:
                case TokenType.printw_Hex_Inst:
                case TokenType.printw_Bin_Inst:
                    instructions.Add(parseInterruptRegInst());
                    break;
                case TokenType.EOF:
                    return instructions;
                    throw new Exception(String.Format("Line {0}: {1} is not a valid instruction name",
                        tokenQueue.Peek().lineCount, tokenQueue.Peek().lexeme
                    ));
            }
        }
        return instructions;
    }
    /**
     * Parsing the form:
     * Opcode reg, reg
     */
    private RegInst parseRegInst() {
        Token opcodeToken = consume(tokenQueue.Peek().type);
        Token reg1Token = parseRegister();
        consume(TokenType.comma);
        Token reg2Token = parseRegister();

        return new RegInst(
            reg1Token.lexeme,
            reg2Token.lexeme,
            opcodeToken.lexeme
        );
    }

    /**
     * Parsing the form:
     * Opcode reg, imm
     */
    private ImmInst parseImmInst() {
        Token opcodeToken = consume(tokenQueue.Peek().type);
        Token regToken = parseRegister();
        consume(TokenType.comma);
        Token integerToken = consume(TokenType.integer);

        return new ImmInst(
            regToken.lexeme,
            Int32.Parse(integerToken.lexeme),
            opcodeToken.lexeme
        );
    }

    /**
     * Parsing the form:
     * Opcode reg, imm[reg]
     */
    private MemInst parseMemInst() {
        Token opcodeToken = consume(tokenQueue.Peek().type);
        Token regToken = parseRegister();
        consume(TokenType.comma);
        Token offsetToken = consume(TokenType.integer);
        consume(TokenType.startBracket);
        Token memRegToken = parseRegister();
        consume(TokenType.endBracket);

        return new MemInst(
            regToken.lexeme,
            memRegToken.lexeme,
            Int32.Parse(offsetToken.lexeme),
            opcodeToken.lexeme
        );
    }

    /**
     * Parsing the form:
     * Opcode reg
     */
    private JmpRegInst parseJmpRegInst() {
        Token opcodeToken = consume(tokenQueue.Peek().type);
        Token regToken = parseRegister();

        return new JmpRegInst(
            regToken.lexeme, opcodeToken.lexeme
        );
    }

    /**
     * Parsing the form:
     * Opcode labelIdentifier
     */
    private JmpLabelInst parseJmpLabelInst() {
        Token opcodeToken = consume(tokenQueue.Peek().type);
        Token labelToken = consume(TokenType.identifier);

        return new JmpLabelInst(
            labelToken.lexeme, opcodeToken.lexeme
        );
    }

    /**
     * Parsing the form:
     * Opcode reg, reg, labelIdentifier
     */
    private JmpBranchInst parseJmpBranchInst() {
        Token opcodeToken = consume(tokenQueue.Peek().type);
        Token reg1Token = parseRegister();
        consume(TokenType.comma);
        Token reg2Token = parseRegister();
        consume(TokenType.comma);
        Token labelToken = consume(TokenType.identifier);

        return new JmpBranchInst(
            reg1Token.lexeme,
            reg2Token.lexeme,
            labelToken.lexeme,
            opcodeToken.lexeme
        );
    }

    /**
     * Parsing the form:
     *
     * labelIdentifier:
     */
    private LabelInst parseLabelInst() {
        Token labelToken = consume(TokenType.identifier);
        consume(TokenType.colon);

        return new LabelInst(
            labelToken.lexeme,
            "label"
        );
    }

    /**
     * Parsing the form:
     *
     * Opcode
     */
    private InterruptInst parseInterruptInst() {
        Token commandToken = consume(tokenQueue.Peek().type);

        return new InterruptInst(
            commandToken.lexeme,
            "interrupt"
        );
    }

    /**
     * Parsing the form:
     *
     * Opcode reg
     */
    private InterruptRegInst parseInterruptRegInst() {
        Token commandToken = consume(tokenQueue.Peek().type);
        Token regToken = parseRegister();

        return new InterruptRegInst(
            commandToken.lexeme,
            regToken.lexeme,
            "interrupt"
        );
    }

    private Token parseRegister() {
        switch(tokenQueue.Peek().type) {
            case TokenType.rZERO_Reg:
            case TokenType.r1_Reg:
            case TokenType.r2_Reg:
            case TokenType.r3_Reg:
            case TokenType.r4_Reg:
            case TokenType.r5_Reg:
            case TokenType.r6_Reg:
            case TokenType.r7_Reg:
            case TokenType.r8_Reg:
            case TokenType.r9_Reg:
            case TokenType.r10_Reg:
            case TokenType.r11_Reg:
            case TokenType.r12_Reg:
            case TokenType.r13_Reg:
            case TokenType.r14_Reg:
            case TokenType.r15_Reg:
            case TokenType.r16_Reg:
            case TokenType.rSP_Reg:
            case TokenType.rFP_Reg: 
            case TokenType.rRET_Reg:
            case TokenType.rHI_Reg: 
            case TokenType.rLO_Reg: 
                return consume(tokenQueue.Peek().type);
            default:
                throw new Exception(String.Format("Line {0}: {1} is not a valid register name", 
                    tokenQueue.Peek().lineCount, 
                    tokenQueue.Peek().lexeme)
                );
        }
    }

    private Token consume(TokenType currTokenType) {
        Token expectedToken = tokenQueue.Peek();
        if (expectedToken.type == currTokenType) {
            return tokenQueue.Dequeue();
        } else {
            throw new Exception(String.Format("Line {0}: {1} does not match with the expected token {2}", 
                expectedToken.lineCount.ToString(), currTokenType.ToString(), expectedToken.type.ToString()
            ));
        }
    }
}