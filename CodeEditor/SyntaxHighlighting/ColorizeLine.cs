using CPDev.STComp05;
using FastColoredTextBoxNS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Speech.Synthesis.TtsEngine;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml;

namespace CodeEditor
{
    public partial class Form1 : Form
    {
        public TokenizerLineState TokenizeSingleLine(int lineIndex, TokenizerLineState beginState, FastColoredTextBox textBox)
        {
            string s1 = String.Format("Uruchamiam tokenizer dla linii: {0}, stanPocz: {1}", lineIndex, beginState);

            // Utwórz zakres dla całej linii
            FastColoredTextBoxNS.Range range = textBox.GetLine(lineIndex);

            range.ClearStyle(ttIdentifierStyle, ttImmConstantStyle, ttKeywordStyle, ttInvalidStyle, ttOperatorStyle, ttDelimiterStyle, ttCommentStyle, ttUnknownStyle, ttDirectiveStyle, ttWhiteSpaceStyle, ttVarLocDescStyle, ttILLabelStyle, ttVCBlockStyle);

            List<TokenizerLineState> vls;
            if (beginState == TokenizerLineState.tlsUndefined)
            {
                if (lineStateDictionary.TryGetValue(textBox, out vls))
                {
                    if (lineIndex <= 0)
                        beginState = TokenizerLineState.tlsDefault;
                    else
                        beginState = TokenizeSingleLine(lineIndex - 1, vls[lineIndex - 1], textBox);
                }
                else
                {
                    throw new InvalidOperationException("This should not happen");
                }
            }

            TokenList tokenlist;
            beginState = TokenizeLineFromState(range.Text, beginState, lineIndex, out tokenlist);

            foreach (var token in tokenlist.Lista)
            {
                Place tokenStart = new Place(token.Pozycja, token.LiniaKodu);
                Place tokenEnd = new Place(token.Pozycja + token.Tekst.Length, token.LiniaKodu);
                Range tokenRange = new Range(fastColoredTextBox1, tokenStart, tokenEnd);

                switch (token.Typ)
                {
                    case CPDev.STComp05.STTokenType.ttIdentifier:
                        tokenRange.SetStyle(ttIdentifierStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttImmConstant:
                        tokenRange.SetStyle(ttImmConstantStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttKeyword:
                        tokenRange.SetStyle(ttKeywordStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttInvalid:
                        tokenRange.SetStyle(ttInvalidStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttOperator:
                        tokenRange.SetStyle(ttOperatorStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttDelimiter:
                        tokenRange.SetStyle(ttDelimiterStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttComment:
                        tokenRange.SetStyle(ttCommentStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttUnknown:
                        tokenRange.SetStyle(ttUnknownStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttDirective:
                        tokenRange.SetStyle(ttDirectiveStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttWhiteSpace:
                        tokenRange.SetStyle(ttWhiteSpaceStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttVarLocDesc:
                        tokenRange.SetStyle(ttVarLocDescStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttILLabel:
                        tokenRange.SetStyle(ttILLabelStyle);
                        break;
                    case CPDev.STComp05.STTokenType.ttVCBlock:
                        tokenRange.SetStyle(ttVCBlockStyle);
                        break;
                    case STTokenType.ttDirectiveVMASM:
                        tokenRange.SetStyle(ttDirVMASMStyle);
                        break;
                    case STTokenType.ttDirectiveSpecial:
                        tokenRange.SetStyle(ttDirSpecStyle);
                        break;
                    default:
                        break;

                }

            }
            System.Diagnostics.Debug.WriteLine(s1 + String.Format(", stanKońc: {0}, tokenów: {1}", beginState, tokenlist.Lista.Count));
            return beginState;
        }

        public TokenizerLineState TokenizeLineFromState(string TextM, TokenizerLineState beginState, int lineOffset, out TokenList tokenlist)
        {
            tokenlist = null;
            switch (beginState)
            {
                case TokenizerLineState.tlsUndefined:
                    throw new NotImplementedException("This should not happen");
                case TokenizerLineState.tlsDefault:
                    beginState = DefaultTokenizer(TextM, lineOffset, out tokenlist);
                    break;
                case TokenizerLineState.tlsComment:
                case TokenizerLineState.tlsDirective:
                case TokenizerLineState.tlsVMAsm:
                case TokenizerLineState.tlsSpecialProc:
                case TokenizerLineState.tlsVerifDirect:
                    beginState = EndCommentTokenize(TextM, lineOffset, out tokenlist, beginState);
                    break;
            }
            return beginState;
        }

        private TokenizerLineState EndCommentTokenize(string TextM, int LineOffset, out TokenList tokenlist, TokenizerLineState beginState)
        {
            tokenlist = new TokenList();
            int nowPos = 0;
            bool Chdone = false;
            while (nowPos < TextM.Length)
            {
                Chdone = false;
                if (TextM[nowPos] == '*')
                {
                    int ePos = nowPos + 1;
                    if(ePos < TextM.Length && TextM[ePos] == ')')
                    {
                        nowPos = ePos + 1;
                        Chdone = true;
                        STTokenType tpToken;
                        tpToken = ChangeStateToToken(beginState);
                        BasicToken bt = new BasicToken(TextM.Substring(0, nowPos), tpToken, 0);
                        bt.LiniaKodu = LineOffset;
                        tokenlist.Lista.Add(bt);
                        break;
                    }
                }
                nowPos++;
            }
            TokenizerLineState endLineState;
            if (nowPos < TextM.Length)
            {
                string rest = TextM.Substring(nowPos);
                TokenList subTokens;
                endLineState = DefaultTokenizer(rest, LineOffset, out subTokens);
                foreach (BasicToken bt in subTokens.Lista)
                {
                    bt.Pozycja += nowPos;
                    tokenlist.Lista.Add(bt);
                }
            }
            else
            {
                if (Chdone)
                    endLineState = TokenizerLineState.tlsDefault;
                else
                {
                    STTokenType tpToken;
                    tpToken = ChangeStateToToken(beginState);
                    BasicToken bt = new BasicToken(TextM, tpToken, 0);
                    bt.LiniaKodu = LineOffset;
                    tokenlist.Lista.Add(bt);
                    endLineState = beginState;
                }
            }
            return endLineState;
        }

        private static STTokenType ChangeStateToToken(TokenizerLineState beginState)
        {
            STTokenType tpToken;
            switch (beginState)
            {
                case TokenizerLineState.tlsVerifDirect:
                    tpToken = STTokenType.ttVCBlock;
                    break;
                case TokenizerLineState.tlsComment:
                    tpToken = STTokenType.ttComment;
                    break;
                case TokenizerLineState.tlsDirective:
                    tpToken = STTokenType.ttDirective;
                    break;
                case TokenizerLineState.tlsVMAsm:
                    tpToken = STTokenType.ttDirectiveVMASM;
                    break;
                case TokenizerLineState.tlsSpecialProc:
                    tpToken = STTokenType.ttDirectiveSpecial;
                    break;
                default:
                    //This not happen
                    tpToken = STTokenType.ttIdentifier;
                    break;
            }

            return tpToken;
        }

        private TokenizerLineState DefaultTokenizer(string TextM, int LineOffset, out TokenList tokenlist)
        {
            TokenizerLineState finalState = TokenizerLineState.tlsDefault;
            tokenlist = new TokenList();
            int nowPos = 0;
            int StartAt;
            bool Chdone;
            // TODO: Tę wartość TokenizerOptions trzeba przenieść ze skojarzonego edytora, aby dopasować kolorowanie składni do obsługiwanych cech
            int TokenizerOptions = 0;
            StringBuilder sb = new StringBuilder(1);
            while (nowPos < TextM.Length)
            {
                Chdone = false;
                StartAt = nowPos;
                /**>> Białe spacje <<**/
                while (nowPos < TextM.Length && STCharConsts.IsSpaceChar(TextM[nowPos]))
                    nowPos++;
                if (nowPos - StartAt > 0)
                {
                    sb.Append(TextM, StartAt, nowPos - StartAt);
                    BasicToken iiiii = new BasicToken(sb.ToString(), STTokenType.ttWhiteSpace, StartAt);
                    iiiii.LiniaKodu = LineOffset;
                    tokenlist.Lista.Add(iiiii);
                    sb.Remove(0, sb.Length);
                    Chdone = true;
                }
                StartAt = nowPos;
                /**>> Elementary numbers <<**/
                if (nowPos < TextM.Length && ("0123456789".IndexOf(TextM[nowPos]) != -1))
                {
                    bool has_exp = false;
                    bool has_exp_sign = false;
                    bool has_dot = false;
                    bool continue_loop = true;
                    while (nowPos < TextM.Length && continue_loop && "0123456789eE.-+_".IndexOf(TextM[nowPos]) != -1)
                    {
                        switch (TextM[nowPos])
                        {
                            case 'E':
                            case 'e':
                                {
                                    if (has_exp)
                                        continue_loop = false;
                                    else
                                    {
                                        has_exp = true;
                                        goto default;
                                    }
                                }
                                break;
                            case '+':
                            case '-':
                                {
                                    if (!has_exp)
                                        continue_loop = false;
                                    else if (((TextM[nowPos - 1] == 'E') ||
                                        (TextM[nowPos - 1] == 'e')) && !has_exp_sign)
                                    {
                                        has_exp_sign = true;
                                        goto default;
                                    }
                                    else
                                        continue_loop = false;
                                }
                                break;
                            case '.':
                                {
                                    if (nowPos + 1 < TextM.Length && TextM[nowPos + 1] == '.')
                                        continue_loop = false;
                                    else
                                    {
                                        if (has_exp)
                                            continue_loop = false;
                                        else if (has_dot)
                                            continue_loop = false;
                                        else
                                        {
                                            has_dot = true;
                                            goto default;
                                        }
                                    }
                                }
                                break;
                            default:
                                nowPos++;
                                break;
                        }
                    }
                    if (nowPos - StartAt > 0)
                    {
                        sb.Append(TextM, StartAt, nowPos - StartAt);
                        tokenlist.AddAutoDetect(sb.ToString(), StartAt, LineOffset, TokenizerOptions);
                        sb.Remove(0, sb.Length);
                        Chdone = true;
                    }
                }
                StartAt = nowPos;
                /**>> Elementary words (identifiers and numbers) <<**/
                while (nowPos < TextM.Length && STCharConsts.IsLetter(TextM[nowPos]))
                    nowPos++;
                if (nowPos - StartAt > 0)
                {
                    sb.Append(TextM, StartAt, nowPos - StartAt);
                    tokenlist.AddAutoDetect(sb.ToString(), StartAt, LineOffset, TokenizerOptions);
                    sb.Remove(0, sb.Length);
                    Chdone = true;
                }
                StartAt = nowPos;
                /**>> Potencjalne operatory <<**/
                while (nowPos < TextM.Length && STCharConsts.IsOpChar(TextM[nowPos]))
                    nowPos++;
                if (nowPos - StartAt > 0)
                {
                    sb.Append(TextM, StartAt, nowPos - StartAt);
                    string loc = sb.ToString();
                    if (STCharConsts.IsOperator(loc))
                    {
                        if (loc.Equals("+") || loc.Equals("-"))
                        {
                            bool stilloper = true;
                            if (nowPos < TextM.Length && STCharConsts.IsNumber(TextM[nowPos]) && TextM[nowPos] != '_')
                            {
                                BasicToken PrvToken = null;
                                STTokenType[] allowGrp = new STTokenType[3];
                                allowGrp[0] = STTokenType.ttOperator;
                                allowGrp[1] = STTokenType.ttKeyword;
                                allowGrp[2] = STTokenType.ttDelimiter;
                                STTokenType[] ignoreGrp = new STTokenType[3];
                                ignoreGrp[0] = STTokenType.ttWhiteSpace;
                                ignoreGrp[1] = STTokenType.ttComment;
                                ignoreGrp[2] = STTokenType.ttDirective;
                                if (tokenlist.Lista.Count == 0 || IsPrevTokenTypeEx(tokenlist, tokenlist.Lista.Count - 1, allowGrp, ignoreGrp, out PrvToken))
                                {
                                    if (PrvToken == null || (PrvToken.Typ != STTokenType.ttDelimiter) || !PrvToken.Tekst.Equals(")", StringComparison.Ordinal))
                                    {
                                        stilloper = false;
                                        nowPos++;
                                        while (nowPos < TextM.Length && STCharConsts.IsNumber(TextM[nowPos]))
                                            nowPos++;
                                        sb.Remove(0, sb.Length);
                                        sb.Append(TextM, StartAt, nowPos - StartAt);
                                        tokenlist.AddAutoDetect(sb.ToString(), StartAt, LineOffset, TokenizerOptions);
                                    }
                                }
                            }
                            if (stilloper)
                            {
                                BasicToken bbbt = tokenlist.AddNewTokenType(loc, STTokenType.ttOperator, StartAt);
                                bbbt.LiniaKodu = LineOffset;
                            }
                        }
                        else
                        {
                            BasicToken bbbt = tokenlist.AddNewTokenType(loc, STTokenType.ttOperator, StartAt);
                            bbbt.LiniaKodu = LineOffset;
                        }
                    }
                    else if ((TokenizerOptions & STTokenizer.toEnableCppCommentStyle) != 0 && loc.StartsWith("//", StringComparison.Ordinal))
                    {
                        sb.Remove(0, sb.Length);
                        int comment = 1;
                        while (nowPos < TextM.Length && comment > 0)
                        {
                            switch (TextM[nowPos])
                            {
                                case '\r': //#13#10, #13
                                    if (nowPos + 1 < TextM.Length && TextM[nowPos + 1] == '\n')
                                        nowPos++;
                                    comment--;
                                    break;
                                case '\n': //#10
                                    comment--;
                                    break;
                                case '\0':
                                    comment--;
                                    break;
                            }
                            nowPos++;
                        }
                        sb.Append(TextM, StartAt, nowPos - StartAt);
                        BasicToken bbt = new BasicToken(sb.ToString(), STTokenType.ttComment, StartAt);
                        bbt.LiniaKodu = LineOffset;
                        if ((TokenizerOptions & STTokenizer.toIgnoreComments) == 0)
                            tokenlist.Lista.Add(bbt);
                    }
                    else
                    {
                        //backtracking 
                        do
                        {
                            nowPos--;
                            sb.Remove(sb.Length - 1, 1);
                            loc = sb.ToString();
                        }
                        while (!STCharConsts.IsOperator(loc) && nowPos >= 0 && nowPos > StartAt);

                        if (STCharConsts.IsOperator(loc))
                        {
                            BasicToken bbbt = tokenlist.AddNewTokenType(loc, STTokenType.ttOperator, StartAt);
                            bbbt.LiniaKodu = LineOffset;
                        }
                        else
                        {
                            BasicToken bbbt = tokenlist.AddNewTokenType(loc, STTokenType.ttInvalid, StartAt);
                            bbbt.LiniaKodu = LineOffset;
                        }
                    }
                    sb.Remove(0, sb.Length);
                    Chdone = true;
                }
                StartAt = nowPos;
                /**>> Komentarze <<**/
                while (nowPos + 1 < TextM.Length && TextM[nowPos] == '(' && TextM[nowPos + 1] == '*') //komentarz
                {
                    int DirectiveVariant = 0;
                    nowPos += 2;
                    if (nowPos < TextM.Length)
                    {
                        if (TextM[nowPos] == '$')
                        {
                            DirectiveVariant = 1;
                            int exPos = nowPos + 1;
                            while (exPos < TextM.Length && STCharConsts.IsLetter(TextM[exPos]))
                                exPos++;
                            string dirName = TextM.Substring(nowPos + 1, exPos - nowPos - 1);
                            if (dirName == "VMASM")
                                DirectiveVariant = 3;
                        }
                        if (TextM[nowPos] == '@' && ((TokenizerOptions & STTokenizer.toEnableParsingVCGBlocks) != 0))
                            DirectiveVariant = 2;
                        if (TextM[nowPos] == '#')
                            DirectiveVariant = 4;
                    }
                    int comment = 1;
                    while (nowPos < TextM.Length && comment > 0)
                    {
                        if ((TokenizerOptions & STTokenizer.toEnableNestedComments) != 0)
                        {
                            if (TextM[nowPos] == '(' && nowPos + 1 < TextM.Length && TextM[nowPos + 1] == '*')
                                comment++;
                        }
                        if (TextM[nowPos] == '*' && nowPos + 1 < TextM.Length && TextM[nowPos + 1] == ')')
                            comment--;
                        nowPos++;
                    }
                    if (nowPos < TextM.Length)
                        nowPos++;
                    sb.Append(TextM, StartAt, nowPos - StartAt);
                    switch (DirectiveVariant)
                    {
                        case 1:
                            {
                                BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttDirective, StartAt);
                                bbt.LiniaKodu = LineOffset;
                            }
                            break;
                        case 2:
                            {
                                BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttVCBlock, StartAt);
                                bbt.LiniaKodu = LineOffset;
                            }
                            break;
                        case 3:
                            {
                                BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttDirectiveVMASM, StartAt);
                                bbt.LiniaKodu = LineOffset;
                            }
                            break;
                        case 4:
                            {
                                BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttDirectiveSpecial, StartAt);
                                bbt.LiniaKodu = LineOffset;
                            }
                            break;
                        default:
                            {
                                BasicToken bbt = new BasicToken(sb.ToString(), STTokenType.ttComment, StartAt);
                                bbt.LiniaKodu = LineOffset;
                                tokenlist.Lista.Add(bbt);
                            }
                            break;
                    }
                    sb.Remove(0, sb.Length);
                    Chdone = true;
                    if (comment > 0 && nowPos >= TextM.Length)
                    {
                        switch (DirectiveVariant)
                        {
                            case 1:
                                finalState = TokenizerLineState.tlsDirective;
                                break;
                            case 2:
                                finalState = TokenizerLineState.tlsVerifDirect;
                                break;
                            case 3:
                                finalState = TokenizerLineState.tlsVMAsm;
                                break;
                            case 4:
                                finalState = TokenizerLineState.tlsSpecialProc;
                                break;
                            default:
                                finalState = TokenizerLineState.tlsComment;
                                break;
                        }
                    }
                }
                /**>> Nawiasy <<**/
                StartAt = nowPos;
                if (nowPos < TextM.Length && TextM[nowPos] == '(')
                {
                    BasicToken bbt = tokenlist.AddNewTokenType("(", STTokenType.ttDelimiter, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    nowPos++;
                    Chdone = true;
                }
                if (nowPos < TextM.Length && TextM[nowPos] == ')')
                {
                    BasicToken bbt = tokenlist.AddNewTokenType(")", STTokenType.ttDelimiter, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    nowPos++;
                    Chdone = true;
                }
                if (nowPos < TextM.Length && TextM[nowPos] == '[')
                {
                    BasicToken bbt = tokenlist.AddNewTokenType("[", STTokenType.ttDelimiter, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    nowPos++;
                    Chdone = true;
                }
                if (nowPos < TextM.Length && TextM[nowPos] == ']')
                {
                    BasicToken bbt = tokenlist.AddNewTokenType("]", STTokenType.ttDelimiter, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    nowPos++;
                    Chdone = true;
                }
                /* Pojedyncze znaki */
                if (nowPos < TextM.Length && (TextM[nowPos] == ';' || TextM[nowPos] == ','))
                {
                    sb.Append(TextM, nowPos, 1);
                    BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttDelimiter, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    sb.Remove(0, sb.Length);
                    nowPos++;
                    Chdone = true;
                }
                if (((TokenizerOptions & STTokenizer.toUseLRBracketsAsDelim) != 0) && (nowPos < TextM.Length) && (TextM[nowPos] == '{' || TextM[nowPos] == '}'))
                {
                    sb.Append(TextM, nowPos, 1);
                    BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttDelimiter, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    sb.Remove(0, sb.Length);
                    nowPos++;
                    Chdone = true;
                }
                if (nowPos < TextM.Length && TextM[nowPos] == '%')
                {
                    StartAt = nowPos;
                    nowPos++;
                    while (nowPos < TextM.Length && !STCharConsts.IsEndVarLocDesc(TextM[nowPos]))
                        nowPos++;
                    if (nowPos - StartAt > 0)
                    {
                        sb.Append(TextM, StartAt, nowPos - StartAt);
                        BasicToken bt = new BasicToken(sb.ToString(), STTokenType.ttVarLocDesc, StartAt);
                        bt.LiniaKodu = LineOffset;
                        sb.Remove(0, sb.Length);
                        tokenlist.Lista.Add(bt);
                        Chdone = true;
                    }
                    StartAt = nowPos;
                }
                /**>> Teksty <<**/
                StartAt = nowPos;
                if (nowPos < TextM.Length && (TextM[nowPos] == '\'' || TextM[nowPos] == '\"'))
                {
                    char beginStringCharacter = TextM[nowPos];
                    sb.Append(beginStringCharacter);
                    nowPos++;
                    while (nowPos < TextM.Length && TextM[nowPos] != beginStringCharacter)
                    {
                        char az = TextM[nowPos];
                        sb.Append(az);
                        if (az == '$')
                        {
                            nowPos++;
                            char ay = '\u0000';
                            if (nowPos < TextM.Length)
                            {
                                ay = TextM[nowPos];
                                sb.Append(ay);
                            }
                        }
                        nowPos++;
                    }
                    if (nowPos < TextM.Length && TextM[nowPos] == beginStringCharacter)
                    {
                        sb.Append(beginStringCharacter);
                        nowPos++;
                    }

                    BasicToken bbt = new BasicToken(sb.ToString(), STTokenType.ttImmConstant, StartAt);
                    bbt.BuildInTypeName = beginStringCharacter == '\'' ? "STRING" : "WSTRING";
                    bbt.LiniaKodu = LineOffset;
                    tokenlist.Lista.Add(bbt);

                    sb.Remove(0, sb.Length);
                    Chdone = true;
                }

                /**>> Liczby dziesiętne, separatory pól, wielokropek  <<**/
                if (nowPos < TextM.Length && TextM[nowPos] == '.')
                {
                    if ((nowPos + 1 < TextM.Length) && (TextM[nowPos + 1] == '.'))
                    {
                        BasicToken bbt = tokenlist.AddNewTokenType("..", STTokenType.ttDelimiter, nowPos);
                        bbt.LiniaKodu = LineOffset;
                        nowPos += 2;
                        Chdone = true;
                    }
                    else
                    {
                        BasicToken tok_1;
                        if (tokenlist.Lista.Count > 0)
                        {
                            tok_1 = tokenlist.Lista[tokenlist.Lista.Count - 1];
                            if (tok_1.IsIntConst())
                            {
                                //parsuj dalej aż do napotkania końca liczby rzeczywistej
                                bool has_exponent = false;
                                bool has_exp_sign = false;
                                bool do_this = true;
                                bool loc_valid = true;
                                string valid_chars = "_1234567890eE-+";
                                StartAt = nowPos - tok_1.Tekst.Length;
                                nowPos++;
                                while (nowPos < TextM.Length && do_this && valid_chars.IndexOf(TextM[nowPos]) >= 0)
                                {
                                    if (TextM[nowPos] == 'E' || TextM[nowPos] == 'e')
                                    {
                                        if (has_exponent)
                                        {
                                            do_this = false;
                                            loc_valid = false;
                                        }
                                        else
                                            has_exponent = true;
                                    }
                                    if (TextM[nowPos] == '-' || TextM[nowPos] == '+')
                                    {
                                        if (has_exponent)
                                        {
                                            if (has_exp_sign)
                                                do_this = false;
                                            else
                                            {
                                                has_exp_sign = true;
                                                //?? zabezpieczenie przed np 1.23E+ 
                                            }
                                        }
                                        else
                                            do_this = false;
                                    }

                                    if (do_this)
                                        nowPos++;
                                }
                                //nowPos++;
                                if (nowPos - StartAt > 0)
                                {
                                    sb.Append(TextM, StartAt, nowPos - StartAt);
                                    tok_1.Tekst = sb.ToString();
                                    sb.Remove(0, sb.Length);
                                    if (loc_valid)
                                    {
                                        tok_1.Typ = STTokenType.ttImmConstant;
                                        tok_1.BuildInTypeName = "REAL";
                                    }
                                    else
                                        tok_1.Typ = STTokenType.ttInvalid;
                                    Chdone = true;
                                }
                                StartAt = nowPos;
                            }
                            else
                            {
                                int sub = 1;
                                while (tokenlist.Lista.Count >= sub)
                                {
                                    tok_1 = tokenlist.Lista[tokenlist.Lista.Count - sub];
                                    if (tok_1.Typ == STTokenType.ttComment || tok_1.Typ == STTokenType.ttWhiteSpace)
                                        sub++;
                                    else
                                        break;
                                }
                                if (tokenlist.Lista.Count >= sub)
                                {
                                    if (tok_1.Typ == STTokenType.ttIdentifier)
                                    {
                                        sb.Append(TextM, nowPos, 1);
                                        BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttOperator, nowPos);
                                        bbt.LiniaKodu = LineOffset;
                                        sb.Remove(0, sb.Length);
                                        nowPos++;
                                        Chdone = true;
                                    }
                                    else if (tok_1.Typ == STTokenType.ttDelimiter)
                                    {
                                        sb.Append(TextM, nowPos, 1);
                                        if (tok_1.Tekst.Equals("]") || tok_1.Tekst.Equals(")"))
                                        {
                                            BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttOperator, nowPos);
                                            bbt.LiniaKodu = LineOffset;
                                        }
                                        else
                                        {
                                            BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                                            bbt.LiniaKodu = LineOffset;
                                        }
                                        sb.Remove(0, sb.Length);
                                        nowPos++;
                                        Chdone = true;
                                    }
                                    else
                                    {
                                        sb.Append(TextM, nowPos, 1);
                                        BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                                        bbt.LiniaKodu = LineOffset;
                                        sb.Remove(0, sb.Length);
                                        nowPos++;
                                        Chdone = true;
                                    }
                                }
                                else
                                {
                                    sb.Append(TextM, nowPos, 1);
                                    BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                                    bbt.LiniaKodu = LineOffset;
                                    sb.Remove(0, sb.Length);
                                    nowPos++;
                                    Chdone = true;
                                }
                            }
                        }
                        else
                        {
                            sb.Append(TextM, nowPos, 1);
                            BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                            bbt.LiniaKodu = LineOffset;
                            sb.Remove(0, sb.Length);
                            nowPos++;
                            Chdone = true;
                        }
                    }
                }
                /**>> Liczby o innej podstawie oraz stałe <<**/
                if (nowPos < TextM.Length && TextM[nowPos] == '#')
                {
                    BasicToken tok_1;
                    if (tokenlist.Lista.Count > 0)
                    {
                        tok_1 = tokenlist.Lista[tokenlist.Lista.Count - 1];
                        if (tok_1.IsIntConst())
                        {
                            StringBuilder Bvalid_chars = new StringBuilder();
                            int conv_base;
                            try
                            {
                                conv_base = Math.Abs(Convert.ToInt32(tok_1.Tekst));
                            }
                            catch (System.FormatException)
                            {
                                conv_base = 0;
                            }
                            bool limited = false;
                            if (conv_base > 36)
                            {
                                conv_base = 36;
                                limited = true;
                            }
                            for (int cc = 0; cc < conv_base; cc++)
                            {
                                if (cc > 9)
                                {
                                    Bvalid_chars.Append(Convert.ToChar(cc - 10 + 0x41));
                                    Bvalid_chars.Append(Convert.ToChar(cc - 10 + 0x61));
                                }
                                else
                                    Bvalid_chars.Append(Convert.ToString(cc));
                            }

                            if (Bvalid_chars.Length > 0)
                                Bvalid_chars.Append('_');

                            StartAt = nowPos - tok_1.Tekst.Length;
                            nowPos++;
                            string valid_chars = Bvalid_chars.ToString();
                            while (nowPos < TextM.Length && valid_chars.IndexOf(TextM[nowPos]) >= 0)
                                nowPos++;
                            if (nowPos - StartAt > 0)
                            {
                                sb.Append(TextM, StartAt, nowPos - StartAt);
                                tok_1.Tekst = sb.ToString();
                                sb.Remove(0, sb.Length);
                                if (limited || (nowPos < TextM.Length && STCharConsts.IsLetter(TextM[nowPos])))
                                    tok_1.Typ = STTokenType.ttInvalid;
                                else
                                {
                                    tok_1.Typ = STTokenType.ttImmConstant;
                                    if ((TokenizerOptions & STTokenizer.toUseDINTAsDefaultINT) != 0)
                                        tok_1.BuildInTypeName = "DINT";
                                    else
                                        tok_1.BuildInTypeName = "INT";
                                }
                                Chdone = true;
                            }
                            StartAt = nowPos;
                        }
                        else if (tok_1.Typ == STTokenType.ttIdentifier || (tok_1.Typ == STTokenType.ttKeyword && STCharConsts.IsBasicBuildType(tok_1.Tekst)))
                        {
                            tok_1.BuildInTypeName = tok_1.Tekst;
                            StartAt = nowPos - tok_1.Tekst.Length;
                            nowPos++;
                            if (nowPos < TextM.Length)
                            {
                                switch (TextM[nowPos])
                                {
                                    case '\'':
                                        {
                                            int rrr;
                                            nowPos++;
                                            do
                                            {
                                                if (nowPos < TextM.Length)
                                                    rrr = STCharConsts.IsEndStringChar(TextM[nowPos], nowPos + 1 < TextM.Length ? TextM[nowPos + 1] : '\u0000', '\'');
                                                else
                                                    rrr = 0;
                                                nowPos += rrr;
                                            }
                                            while (nowPos < TextM.Length && (rrr > 0));
                                            if (nowPos < TextM.Length)
                                                nowPos++;
                                        }
                                        break;
                                    case '"':
                                        {
                                            int rrr;
                                            nowPos++;
                                            do
                                            {
                                                if (nowPos < TextM.Length)
                                                    rrr = STCharConsts.IsEndStringChar(TextM[nowPos], nowPos + 1 < TextM.Length ? TextM[nowPos + 1] : '\u0000', '"');
                                                else
                                                    rrr = 0;
                                                nowPos += rrr;
                                            }
                                            while (nowPos < TextM.Length && (rrr > 0));
                                            if (nowPos < TextM.Length)
                                                nowPos++;
                                        }
                                        break;
                                    default:
                                        while (nowPos < TextM.Length && !STCharConsts.IsEndConstChar(TextM[nowPos]))
                                            nowPos++;
                                        break;
                                }
                            }
                            if (nowPos - StartAt > 0)
                            {
                                sb.Append(TextM, StartAt, nowPos - StartAt);
                                tok_1.Tekst = sb.ToString();
                                sb.Remove(0, sb.Length);
                                tok_1.Typ = STTokenType.ttImmConstant;
                                Chdone = true;
                            }
                            StartAt = nowPos;
                        }
                        else
                        {
                            sb.Append(TextM, nowPos, 1);
                            BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                            bbt.LiniaKodu = LineOffset;
                            sb.Remove(0, sb.Length);
                            nowPos++;
                            Chdone = true;
                        }
                    }
                    else
                    {
                        sb.Append(TextM, nowPos, 1);
                        BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                        bbt.LiniaKodu = LineOffset;
                        sb.Remove(0, sb.Length);
                        nowPos++;
                        Chdone = true;
                    }

                }
                /**>> Liczby o innej podstawie oraz stałe <<**/
                if (nowPos < TextM.Length && TextM[nowPos] == '\\')
                {
                    if ((TokenizerOptions & STTokenizer.toBackslashAsKeyword) != 0)
                    {
                        sb.Append(TextM, StartAt, 1);
                        BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttKeyword, nowPos);
                        bbt.LiniaKodu = LineOffset;
                        sb.Remove(0, sb.Length);
                        nowPos++;
                        Chdone = true;
                    }
                }
                /**>> Inne <<**/
                if (!Chdone)
                {
                    sb.Append(TextM, StartAt, 1);
                    BasicToken bbt = tokenlist.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    sb.Remove(0, sb.Length);
                    nowPos++;
                }
            }
            return finalState;
        }

        public static bool IsPrevTokenTypeEx(TokenList tt, int PosFrom, STTokenType[] reqTokenGrp, STTokenType[] ignoreTokGrp, out BasicToken prevToken)
        {
            prevToken = null;
            while (PosFrom >= 0 && Array.IndexOf<STTokenType>(ignoreTokGrp, tt.Lista[PosFrom].Typ) != -1)
                PosFrom--;
            if (PosFrom >= 0)
            {
                prevToken = tt.Lista[PosFrom];
                return Array.IndexOf<STTokenType>(reqTokenGrp, prevToken.Typ) != -1;
            }
            else
                return false;
        }
    }
}
