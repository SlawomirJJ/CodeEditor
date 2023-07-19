using System;
using System.Collections.Generic;
using System.Text;
//using CPDev.Public;
//using ConfigManager_ = CPDev.SADlg.ConfigManager;

namespace CPDev.STComp05
{
    public class STTokenizer
    {
        /// <summary>
        /// Opcje tokenizacji
        /// </summary>
        public int TokenizerOptions;
        /// <summary>
        /// Lista b≥ÍdÛw ostrzeøeÒ podpowiedzi
        /// </summary>
        public LocationList Errors;
        /// <summary>
        /// Use '\\' symbol as keyword for special processing. Otherwise it is treated as invalid.
        /// </summary>
        public const int toBackslashAsKeyword = 0x0001;
        /// <summary>
        /// Parametr ignorujπcy dodawanie bia≥ych spacji do wynikowej listy
        /// </summary>
        public const int toIgnoreWhiteSpace = 0x0002;
        /// <summary>
        /// Parametr ignorujπcy dodawanie komentarzy do wynikowej listy
        /// </summary>
        public const int toIgnoreComments = 0x0004;
        /// <summary>
        /// Uzywa nawiasÛw { } jako delimiterÛw tablic
        /// </summary>
        public const int toUseLRBracketsAsDelim = 0x0008;
        /// <summary>
        /// Uaktywnia parsowanie dwÛch znakÛw slash (//) jako komentarz do koÒca linii (z C++)
        /// </summary>
        public const int toEnableCppCommentStyle = 0x0010;
        /// <summary>
        /// Uaktywnia zagnieødøone komentarze
        /// </summary>
        public const int toEnableNestedComments = 0x0020;
        /// <summary>
        /// Enables parsing of Verification Condition Blocks in code
        /// </summary>
        public const int toEnableParsingVCGBlocks = 0x0040;
        /// <summary>
        /// Allows to keep all identifiers in upper case letters (for compability with previous releases of CPDev)
        /// </summary>
        public const int toKeepIdentsInUpcase = 0x0080;
        /// <summary>
        /// Allows to use all non-typed INTs to be DINTs.
        /// </summary>
        public const int toUseDINTAsDefaultINT = 0x0100;
        /// <summary>
        /// Ignores ';' after special keywords like END_IF, END_WHILE
        /// </summary>
        public const int toIgnoreSemicolonAtEnd = 0x0200;
        /// <summary>
        /// Allows to put new line characters in the middle of string
        /// </summary>
        public const int toAllowNewLineInStringConstant = 0x0400;
        /// <summary>
        /// Domyúlny pusty konstruktor
        /// </summary>
        public STTokenizer()
        {
            TokenizerOptions = 0;
            Errors = new LocationList(-1, -1, -1);
        }

        public STTokenizer(int TokOpts) : this()
        {
            TokenizerOptions = TokOpts;
        }

        public static bool IsInArray(STTokenType tt, STTokenType[] arr)
        {
            bool fnd = false;
            int i = 0;
            while (i < arr.Length && !fnd)
            {
                if (arr[i] == tt)
                    fnd = true;
                else
                    i++;
            }
            return fnd;
        }

        public static bool IsPrevTokenType(TokenList tt, int PosFrom, STTokenType[] reqTokenGrp, STTokenType[] ignoreTokGrp)
        {
            BasicToken local;
            return IsPrevTokenTypeEx(tt, PosFrom, reqTokenGrp, ignoreTokGrp, out local);
            /*while (PosFrom >= 0 && IsInArray(tt.Lista[PosFrom].Typ, ignoreTokGrp))
                PosFrom--;
            if (PosFrom >= 0)
            {
                return IsInArray(tt.Lista[PosFrom].Typ, reqTokenGrp);
            }
            else
                return false;*/
        }

        public static bool IsPrevTokenTypeEx(TokenList tt, int PosFrom, STTokenType[] reqTokenGrp, STTokenType[] ignoreTokGrp, out BasicToken prevToken)
        {
            prevToken = null;
            while (PosFrom >= 0 && IsInArray(tt.Lista[PosFrom].Typ, ignoreTokGrp))
                PosFrom--;
            if (PosFrom >= 0)
            {
                prevToken = tt.Lista[PosFrom];
                return IsInArray(prevToken.Typ, reqTokenGrp);
            }
            else
                return false;
        }

        /// <summary>
        /// Dzieli ≥aÒcuch na fragmenty
        /// </summary>
        /// <param name="iStream">ciπg bajtÛw do podzielenia</param>
        public TokenList TokenizeSTStream(System.IO.Stream iStream)
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(iStream, Encoding.UTF8);
            string rdd = sr.ReadToEnd();
            return TokenizeSTStream(rdd.ToCharArray(), 1);
        }

        /// <summary>
        /// Dzieli ≥aÒcuch na fragmenty
        /// </summary>
        /// <param name="TextM">ciπg znakÛw do parsowania</param>
        /// <param name="LineOffset">Obecny numer linii</param>
        /// <returns>Lista tokenÛw po przetworzeniu</returns>
        public TokenList TokenizeSTStream(char[] TextM, int LineOffset)
        {
            TokenList ret = new TokenList();
            ret.SourceText = TextM;
            int nowPos = 0;
            int StartAt;
            bool Chdone;
            bool STCO_DNL_IN_LITERIAL = ((this.TokenizerOptions & STTokenizer.toAllowNewLineInStringConstant) == 0);

            StringBuilder sb = new StringBuilder(1);
            while (nowPos < TextM.Length)
            {
                Chdone = false;
                StartAt = nowPos;
                /**>> Bia≥e spacje <<**/
                while (nowPos < TextM.Length && STCharConsts.IsSpaceChar(TextM[nowPos]))
                    nowPos++;
                if (nowPos - StartAt > 0)
                {
                    sb.Append(TextM, StartAt, nowPos - StartAt);
                    BasicToken iiiii = new BasicToken(sb.ToString(), STTokenType.ttWhiteSpace, StartAt);
                    iiiii.LiniaKodu = LineOffset;
                    CalcLineOffset(iiiii.Tekst, ref LineOffset);
                    if ((TokenizerOptions & toIgnoreWhiteSpace) == 0)
                        ret.Lista.Add(iiiii);
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
                        ret.AddAutoDetect(sb.ToString(), StartAt, LineOffset, TokenizerOptions);
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
                    ret.AddAutoDetect(sb.ToString(), StartAt, LineOffset, TokenizerOptions);
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
                                if (ret.Lista.Count == 0 || IsPrevTokenTypeEx(ret, ret.Lista.Count - 1, allowGrp, ignoreGrp, out PrvToken))
                                {
                                    if (PrvToken == null || (PrvToken.Typ != STTokenType.ttDelimiter) || !PrvToken.Tekst.Equals(")", StringComparison.Ordinal))
                                    {
                                        stilloper = false;
                                        nowPos++;
                                        while (nowPos < TextM.Length && STCharConsts.IsNumber(TextM[nowPos]))
                                            nowPos++;
                                        sb.Remove(0, sb.Length);
                                        sb.Append(TextM, StartAt, nowPos - StartAt);
                                        ret.AddAutoDetect(sb.ToString(), StartAt, LineOffset, TokenizerOptions);
                                    }
                                }
                            }
                            if (stilloper)
                            {
                                BasicToken bbbt = ret.AddNewTokenType(loc, STTokenType.ttOperator, StartAt);
                                bbbt.LiniaKodu = LineOffset;
                            }
                        }
                        else
                        {
                            BasicToken bbbt = ret.AddNewTokenType(loc, STTokenType.ttOperator, StartAt);
                            bbbt.LiniaKodu = LineOffset;
                        }
                    }
                    else if (loc.StartsWith("//", StringComparison.Ordinal))
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
                        CalcLineOffset(bbt.Tekst, ref LineOffset);
                        if ((TokenizerOptions & toIgnoreComments) == 0)
                            ret.Lista.Add(bbt);
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
                            BasicToken bbbt = ret.AddNewTokenType(loc, STTokenType.ttOperator, StartAt);
                            bbbt.LiniaKodu = LineOffset;
                        }
                        else
                        {
                            BasicToken bbbt = ret.AddNewTokenType(loc, STTokenType.ttInvalid, StartAt);
                            bbbt.LiniaKodu = LineOffset;
                            CalcLineOffset(bbbt.Tekst, ref LineOffset);
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
                            DirectiveVariant = 1;
                        if (TextM[nowPos] == '@' && ((this.TokenizerOptions & toEnableParsingVCGBlocks) != 0))
                            DirectiveVariant = 2;
                    }
                    int comment = 1;
                    while (nowPos < TextM.Length && comment > 0)
                    {
                        //if (CPD_Objs.HasBitSet(TokenizerOptions, STTokenizer.toEnableNestedComments))
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
                                BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttDirective, StartAt);
                                bbt.LiniaKodu = LineOffset;
                                CalcLineOffset(bbt.Tekst, ref LineOffset);
                            }
                            break;
                        case 2:
                            {
                                BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttVCBlock, StartAt);
                                bbt.LiniaKodu = LineOffset;
                                CalcLineOffset(bbt.Tekst, ref LineOffset);
                            }
                            break;
                        default:
                            {
                                BasicToken bbt = new BasicToken(sb.ToString(), STTokenType.ttComment, StartAt);
                                bbt.LiniaKodu = LineOffset;
                                CalcLineOffset(bbt.Tekst, ref LineOffset);
                                if ((TokenizerOptions & toIgnoreComments) == 0)
                                    ret.Lista.Add(bbt);
                            }
                            break;
                    }
                    sb.Remove(0, sb.Length);
                    Chdone = true;
                }
                /**>> Nawiasy <<**/
                StartAt = nowPos;
                if (nowPos < TextM.Length && TextM[nowPos] == '(')
                {
                    BasicToken bbt = ret.AddNewTokenType("(", STTokenType.ttDelimiter, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    nowPos++;
                    Chdone = true;
                }
                if (nowPos < TextM.Length && TextM[nowPos] == ')')
                {
                    BasicToken bbt = ret.AddNewTokenType(")", STTokenType.ttDelimiter, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    nowPos++;
                    Chdone = true;
                }
                if (nowPos < TextM.Length && TextM[nowPos] == '[')
                {
                    BasicToken bbt = ret.AddNewTokenType("[", STTokenType.ttDelimiter, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    nowPos++;
                    Chdone = true;
                }
                if (nowPos < TextM.Length && TextM[nowPos] == ']')
                {
                    BasicToken bbt = ret.AddNewTokenType("]", STTokenType.ttDelimiter, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    nowPos++;
                    Chdone = true;
                }
                /* Pojedyncze znaki */
                if (nowPos < TextM.Length && (TextM[nowPos] == ';' || TextM[nowPos] == ','))
                {
                    sb.Append(TextM, nowPos, 1);
                    BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttDelimiter, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    sb.Remove(0, sb.Length);
                    nowPos++;
                    Chdone = true;
                }
                if (((this.TokenizerOptions & toUseLRBracketsAsDelim) != 0) && (nowPos < TextM.Length) && (TextM[nowPos] == '{' || TextM[nowPos] == '}'))
                {
                    sb.Append(TextM, nowPos, 1);
                    BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttDelimiter, nowPos);
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
                        ret.Lista.Add(bt);
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
                    while (nowPos + 1 < TextM.Length && TextM[nowPos] == beginStringCharacter && TextM[nowPos + 1] == beginStringCharacter)
                    {
                        sb.Append(beginStringCharacter);
                        nowPos += 2;
                    }
                    while (nowPos < TextM.Length && TextM[nowPos] != beginStringCharacter)
                    {
                        char az = TextM[nowPos];
                        if (az == '$')
                        {
                            nowPos++;
                            char ay = '\u0000';
                            if (nowPos < TextM.Length)
                                ay = TextM[nowPos];
                            switch (ay)
                            {
                                case '$':
                                case '\'':
                                case '\"':
                                    sb.Append(ay);
                                    break;
                                case 'L':
                                case 'l':
                                    sb.Append('\n');
                                    break;
                                case 'N':
                                case 'n':
                                    sb.Append("\r\n");
                                    break;
                                case 'P':
                                case 'p':
                                    sb.Append('\u000B');
                                    break;
                                case 'R':
                                case 'r':
                                    sb.Append('\r');
                                    break;
                                case 't':
                                case 'T':
                                    sb.Append('\t');
                                    break;
                                default:
                                    {
                                        nowPos++;
                                        char ax = '\u0000';
                                        if (nowPos < TextM.Length)
                                            ax = TextM[nowPos];

                                        if (STUtils.IsHexChar(ay) && STUtils.IsHexChar(ax))
                                        {
                                            sb.Append(STUtils.DCharToChar(ay, ax));
                                        }
                                        else
                                        {
                                            sb.Append(az);
                                            nowPos -= 2;
                                            if (nowPos <= 0)
                                                nowPos = 0;
                                            if (STUtils.IsHexChar(ay))
                                                //Errors.ReportWarning(LineOffset, String.Format(Messages.Tokenizer_W001, az, STUtils.NameAsciiChar(ay), STUtils.NameAsciiChar(ax)));
                                                throw new ArgumentException("Error");
                                            else
                                                //Errors.ReportWarning(LineOffset, String.Format(Messages.Tokenizer_W002, az, STUtils.NameAsciiChar(ay)));
                                                throw new ArgumentException("Error");
                                        }
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            if (az == '\r')
                            {
                                if (nowPos + 1 < TextM.Length && TextM[nowPos + 1] == '\n')
                                {
                                    nowPos++;
                                    az = '\n';
                                }
                                LineOffset++;
                                if (STCO_DNL_IN_LITERIAL)
                                {
                                    //Errors.ReportError(LineOffset - 1, Messages.Tokenizer_E001);
                                    throw new Exception("Error");
                                }
                                else
                                {
                                    if (nowPos + 1 < TextM.Length && TextM[nowPos + 1] == '\n')
                                    {
                                        nowPos++;
                                        az = '\n';
                                        sb.Append('\r');
                                    }
                                }
                            }
                            else if (az == '\n')
                            {
                                LineOffset++;
                                if (STCO_DNL_IN_LITERIAL)
                                {
                                    //Errors.ReportError(LineOffset - 1, Messages.Tokenizer_E001);
                                    throw new Exception("Error");

                                }
                            }
                            sb.Append(az);
                        }
                        nowPos++;
                        while (nowPos + 1 < TextM.Length && TextM[nowPos] == beginStringCharacter && TextM[nowPos + 1] == beginStringCharacter)
                        {
                            sb.Append(beginStringCharacter);
                            nowPos += 2;
                        }
                    }
                    nowPos++;
                    sb.Append(beginStringCharacter);
                    {
                        BasicToken bbt = new BasicToken(sb.ToString(), STTokenType.ttImmConstant, StartAt);
                        bbt.BuildInTypeName = beginStringCharacter == '\'' ? "STRING" : "WSTRING";
                        bbt.LiniaKodu = LineOffset;
                        ret.Lista.Add(bbt); // AddNewTokenType(sb.ToString(), STTokenType.ttString, StartAt);
                    }
                    sb.Remove(0, sb.Length);
                    Chdone = true;
                }

                /**>> Liczby dziesiÍtne, separatory pÛl, wielokropek  <<**/
                if (nowPos < TextM.Length && TextM[nowPos] == '.')
                {
                    if ((nowPos + 1 < TextM.Length) && (TextM[nowPos + 1] == '.'))
                    {
                        BasicToken bbt = ret.AddNewTokenType("..", STTokenType.ttDelimiter, nowPos);
                        bbt.LiniaKodu = LineOffset;
                        nowPos += 2;
                        Chdone = true;
                    }
                    else
                    {
                        BasicToken tok_1;
                        if (ret.Lista.Count > 0)
                        {
                            tok_1 = ret.Lista[ret.Lista.Count - 1];
                            if (tok_1.IsIntConst())
                            {
                                //parsuj dalej aø do napotkania koÒca liczby rzeczywistej
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
                                while (ret.Lista.Count >= sub)
                                {
                                    tok_1 = ret.Lista[ret.Lista.Count - sub];
                                    if (tok_1.Typ == STTokenType.ttComment || tok_1.Typ == STTokenType.ttWhiteSpace)
                                        sub++;
                                    else
                                        break;
                                }
                                if (ret.Lista.Count >= sub)
                                {
                                    if (tok_1.Typ == STTokenType.ttIdentifier)
                                    {
                                        sb.Append(TextM, nowPos, 1);
                                        BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttOperator, nowPos);
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
                                            BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttOperator, nowPos);
                                            bbt.LiniaKodu = LineOffset;
                                        }
                                        else
                                        {
                                            BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                                            bbt.LiniaKodu = LineOffset;
                                        }
                                        sb.Remove(0, sb.Length);
                                        nowPos++;
                                        Chdone = true;
                                    }
                                    else
                                    {
                                        sb.Append(TextM, nowPos, 1);
                                        BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                                        bbt.LiniaKodu = LineOffset;
                                        CalcLineOffset(bbt.Tekst, ref LineOffset);
                                        sb.Remove(0, sb.Length);
                                        nowPos++;
                                        Chdone = true;
                                    }
                                }
                                else
                                {
                                    sb.Append(TextM, nowPos, 1);
                                    BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                                    bbt.LiniaKodu = LineOffset;
                                    CalcLineOffset(bbt.Tekst, ref LineOffset);
                                    sb.Remove(0, sb.Length);
                                    nowPos++;
                                    Chdone = true;
                                }
                            }
                        }
                        else
                        {
                            sb.Append(TextM, nowPos, 1);
                            BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                            bbt.LiniaKodu = LineOffset;
                            CalcLineOffset(bbt.Tekst, ref LineOffset);
                            sb.Remove(0, sb.Length);
                            nowPos++;
                            Chdone = true;
                        }
                    }
                }
                /**>> Liczby o innej podstawie oraz sta≥e <<**/
                if (nowPos < TextM.Length && TextM[nowPos] == '#')
                {
                    BasicToken tok_1;
                    if (ret.Lista.Count > 0)
                    {
                        tok_1 = ret.Lista[ret.Lista.Count - 1];
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
                                    if ((this.TokenizerOptions & STTokenizer.toUseDINTAsDefaultINT) != 0)
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
                            BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                            bbt.LiniaKodu = LineOffset;
                            CalcLineOffset(bbt.Tekst, ref LineOffset);
                            sb.Remove(0, sb.Length);
                            nowPos++;
                            Chdone = true;
                        }
                    }
                    else
                    {
                        sb.Append(TextM, nowPos, 1);
                        BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                        bbt.LiniaKodu = LineOffset;
                        CalcLineOffset(bbt.Tekst, ref LineOffset);
                        sb.Remove(0, sb.Length);
                        nowPos++;
                        Chdone = true;
                    }

                }
                /**>> Liczby o innej podstawie oraz sta≥e <<**/
                if (nowPos < TextM.Length && TextM[nowPos] == '\\')
                {
                    //if (CPD_Objs.HasBitSet(this.TokenizerOptions, STTokenizer.toBackslashAsKeyword))
                    {
                        sb.Append(TextM, StartAt, 1);
                        BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttKeyword, nowPos);
                        bbt.LiniaKodu = LineOffset;
                        CalcLineOffset(bbt.Tekst, ref LineOffset);
                        sb.Remove(0, sb.Length);
                        nowPos++;
                        Chdone = true;
                    }
                }
                /**>> Inne <<**/
                if (!Chdone)
                {
                    sb.Append(TextM, StartAt, 1);
                    BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    CalcLineOffset(bbt.Tekst, ref LineOffset);
                    sb.Remove(0, sb.Length);
                    nowPos++;
                }
            }
            return ret;
        }



        public TokenList TokenizeILStream(char[] TextM, int LineOffset)
        {
            TokenList ret = new TokenList();
            int nowPos = 0;
            int StartAt;
            bool Chdone;
            StringBuilder sb = new StringBuilder(1);
            bool STCO_DNL_IN_LITERIAL = ((this.TokenizerOptions & STTokenizer.toAllowNewLineInStringConstant) == 0);

            nowPos = 0;
            while (nowPos < TextM.Length)
            {
                Chdone = false;
                StartAt = nowPos;
                /**>> Bia≥e spacje <<**/
                while (nowPos < TextM.Length && STCharConsts.IsSpaceChar(TextM[nowPos]))
                    nowPos++;
                if (nowPos - StartAt > 0)
                {
                    sb.Append(TextM, StartAt, nowPos - StartAt);
                    BasicToken iiiii = new BasicToken(sb.ToString(), STTokenType.ttWhiteSpace, StartAt);
                    iiiii.LiniaKodu = LineOffset;
                    CalcLineOffset(iiiii.Tekst, ref LineOffset);
                    if ((TokenizerOptions & toIgnoreWhiteSpace) == 0)
                        ret.Lista.Add(iiiii);
                    sb.Remove(0, sb.Length);
                    Chdone = true;
                }
                StartAt = nowPos;

                /**>> Elementarne s≥owa <<**/
                while (nowPos < TextM.Length && STCharConsts.IsLetter(TextM[nowPos]))
                    nowPos++;
                //sprawdzam czy jest slowem kluczowym
                if (nowPos - StartAt > 0)
                {
                    sb.Append(TextM, StartAt, nowPos - StartAt);
                    //sprawdzam czy jest nalezy do KeyWords                        
                    string lstr = sb.ToString();
                    ret.nit = ret.Lista.Count - 1;
                    bool IsPrevImp = ret.PrevImportant();
                    if ((lstr.Equals("R", StringComparison.CurrentCultureIgnoreCase) && IsPrevImp && (ret.it.Typ != STTokenType.ttKeyword))
                        || (lstr.Equals("S", StringComparison.CurrentCultureIgnoreCase) && IsPrevImp && (ret.it.Typ != STTokenType.ttKeyword)))
                    {
                        BasicToken iiiii = new BasicToken(lstr, STTokenType.ttKeyword, StartAt);
                        iiiii.LiniaKodu = LineOffset;
                        CalcLineOffset(iiiii.Tekst, ref LineOffset);
                        ret.Lista.Add(iiiii);
                    }
                    else
                    {
                        ret.AddAutoDetect(sb.ToString(), StartAt, LineOffset, this.TokenizerOptions | STTokenizer.toKeepIdentsInUpcase);
                    }

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
                                if (ret.Lista.Count == 0 || IsPrevTokenTypeEx(ret, ret.Lista.Count - 1, allowGrp, ignoreGrp, out PrvToken))
                                {
                                    if (PrvToken == null || (PrvToken.Typ != STTokenType.ttDelimiter) || !PrvToken.Tekst.Equals(")", StringComparison.Ordinal))
                                    {
                                        stilloper = false;
                                        nowPos++;
                                        while (nowPos < TextM.Length && STCharConsts.IsNumber(TextM[nowPos]))
                                            nowPos++;
                                        sb.Remove(0, sb.Length);
                                        sb.Append(TextM, StartAt, nowPos - StartAt);
                                        ret.AddAutoDetect(sb.ToString(), StartAt, LineOffset, TokenizerOptions | STTokenizer.toKeepIdentsInUpcase);
                                    }
                                }
                            }
                            if (stilloper)
                            {
                                BasicToken bbbt = ret.AddNewTokenType(loc, STTokenType.ttOperator, StartAt);
                                bbbt.LiniaKodu = LineOffset;
                            }
                        }
                        else
                        {
                            //Console.WriteLine("OPERATOR");
                            BasicToken bbbt = ret.AddNewTokenType(loc, STTokenType.ttOperator, StartAt);
                            bbbt.LiniaKodu = LineOffset;
                        }
                    }
                    else if (loc.StartsWith("//", StringComparison.Ordinal))
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
                        CalcLineOffset(bbt.Tekst, ref LineOffset);
                        if ((TokenizerOptions & toIgnoreComments) == 0)
                            ret.Lista.Add(bbt);
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
                            BasicToken bbbt = ret.AddNewTokenType(loc, STTokenType.ttOperator, StartAt);
                            bbbt.LiniaKodu = LineOffset;
                        }
                        else
                        {
                            BasicToken bbbt = ret.AddNewTokenType(loc, STTokenType.ttInvalid, StartAt);
                            bbbt.LiniaKodu = LineOffset;
                            CalcLineOffset(bbbt.Tekst, ref LineOffset);
                        }
                    }
                    sb.Remove(0, sb.Length);
                    Chdone = true;
                }
                StartAt = nowPos;


                /* := */
                if (nowPos < TextM.Length && TextM[nowPos] == ':' && TextM[nowPos + 1] == '=')
                {
                    nowPos += 2;
                    if (nowPos - StartAt > 0)
                    {
                        sb.Append(TextM, StartAt, nowPos - StartAt);
                        BasicToken iiiii = new BasicToken(sb.ToString(), STTokenType.ttOperator, StartAt);
                        iiiii.LiniaKodu = LineOffset;
                        CalcLineOffset(iiiii.Tekst, ref LineOffset);

                        ret.Lista.Add(iiiii);
                        sb.Remove(0, sb.Length);
                        Chdone = true;
                    }
                }
                else if (nowPos < TextM.Length && TextM[nowPos] == ':')
                {
                    sb.Append(TextM, nowPos, 1);
                    BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttOperator, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    sb.Remove(0, sb.Length);
                    nowPos++;
                    Chdone = true;
                }
                StartAt = nowPos;
                /**>> Nawiasy <<**/
                StartAt = nowPos;

                if (nowPos < TextM.Length && TextM[nowPos] == '[')
                {
                    BasicToken bbt = ret.AddNewTokenType("[", STTokenType.ttDelimiter, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    nowPos++;
                    Chdone = true;
                }
                if (nowPos < TextM.Length && TextM[nowPos] == ']')
                {
                    BasicToken bbt = ret.AddNewTokenType("]", STTokenType.ttDelimiter, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    nowPos++;
                    Chdone = true;
                }

                /**>> Komentarze <<**/
                if (nowPos < TextM.Length && TextM[nowPos] == '/' && nowPos + 1 < TextM.Length && TextM[nowPos + 1] == '/') //komentarz
                {
                    if ((TokenizerOptions & toEnableCppCommentStyle) != 0)
                    {
                        nowPos += 2;
                        while (nowPos + 1 < TextM.Length && TextM[nowPos] != '\n' && TextM[nowPos] != '\r') //komentarz
                            nowPos++;
                        if (nowPos - StartAt > 0)
                        {
                            sb.Append(TextM, StartAt, nowPos - StartAt);
                            BasicToken iiiii = new BasicToken(sb.ToString(), STTokenType.ttComment, StartAt);
                            iiiii.LiniaKodu = LineOffset;
                            CalcLineOffset(iiiii.Tekst, ref LineOffset);
                            ret.Lista.Add(iiiii);
                            sb.Remove(0, sb.Length);
                            Chdone = true;
                        }
                    }
                }
                StartAt = nowPos;
                /**>> Komentarze <<**/
                while (nowPos < TextM.Length && TextM[nowPos] == '(' && TextM[nowPos + 1] == '*') //komentarz
                {
                    bool isDirective = false;
                    nowPos += 2;
                    if (nowPos < TextM.Length)
                    {
                        if (TextM[nowPos] == '$')
                            isDirective = true;
                    }
                    int comment = 1;
                    while (nowPos < TextM.Length && comment > 0)
                    {
                        //if (CPD_Objs.HasBitSet(TokenizerOptions, STTokenizer.toEnableNestedComments))
                        {
                            if (TextM[nowPos] == '(' && TextM[nowPos + 1] == '*')
                                comment++;
                        }
                        if (TextM[nowPos] == '*' && TextM[nowPos + 1] == ')')
                            comment--;
                        nowPos++;
                    }
                    if (nowPos < TextM.Length)
                        nowPos++;
                    sb.Append(TextM, StartAt, nowPos - StartAt);
                    if (isDirective)
                    {
                        BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttDirective, StartAt);
                        bbt.LiniaKodu = LineOffset;
                        CalcLineOffset(bbt.Tekst, ref LineOffset);
                    }
                    else
                    {
                        BasicToken bbt = new BasicToken(sb.ToString(), STTokenType.ttComment, StartAt);
                        bbt.LiniaKodu = LineOffset;
                        CalcLineOffset(bbt.Tekst, ref LineOffset);
                        if ((TokenizerOptions & toIgnoreComments) == 0)
                            ret.Lista.Add(bbt);
                    }
                    sb.Remove(0, sb.Length);
                    Chdone = true;
                }

                /* Pojedyncze znaki */
                if (nowPos < TextM.Length &&
                    (TextM[nowPos] == ';' || TextM[nowPos] == ',' || TextM[nowPos] == '(' || TextM[nowPos] == ')'))
                {
                    sb.Append(TextM, nowPos, 1);
                    BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttDelimiter, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    sb.Remove(0, sb.Length);
                    nowPos++;
                    Chdone = true;
                }
                if (nowPos < TextM.Length && TextM[nowPos] == '%')
                {
                    StartAt = nowPos;
                    nowPos++;
                    while (nowPos < TextM.Length && !STCharConsts.IsSpaceChar(TextM[nowPos]))
                        nowPos++;
                    if (nowPos - StartAt > 0)
                    {
                        sb.Append(TextM, StartAt, nowPos - StartAt);
                        BasicToken bt = new BasicToken(sb.ToString(), STTokenType.ttVarLocDesc, StartAt);
                        bt.LiniaKodu = LineOffset;
                        sb.Remove(0, sb.Length);
                        ret.Lista.Add(bt);
                        Chdone = true;
                    }
                    StartAt = nowPos;
                }
                StartAt = nowPos;
                /**>> Teksty <<**/
                StartAt = nowPos;
                if (nowPos < TextM.Length && TextM[nowPos] == '\'')
                {
                    nowPos++;
                    while (nowPos + 1 < TextM.Length && TextM[nowPos] == '\'' && TextM[nowPos + 1] == '\'')
                    {
                        sb.Append("'");
                        nowPos += 2;
                    }
                    while (nowPos < TextM.Length && TextM[nowPos] != '\'')
                    {
                        char az = TextM[nowPos];
                        if (az == '$')
                        {
                            nowPos++;
                            char ay = '\u0000';
                            if (nowPos < TextM.Length)
                                ay = TextM[nowPos];
                            switch (ay)
                            {
                                case '$':
                                case '\'':
                                    sb.Append(ay);
                                    break;
                                case 'L':
                                case 'l':
                                    sb.Append('\n');
                                    break;
                                case 'N':
                                case 'n':
                                    sb.Append("\r\n");
                                    break;
                                case 'P':
                                case 'p':
                                    sb.Append('\u000B');
                                    break;
                                case 'R':
                                case 'r':
                                    sb.Append('\r');
                                    break;
                                case 't':
                                case 'T':
                                    sb.Append('\t');
                                    break;
                                default:
                                    {
                                        nowPos++;
                                        char ax = '\u0000';
                                        if (nowPos < TextM.Length)
                                            ax = TextM[nowPos];

                                        if (STUtils.IsHexChar(ay) && STUtils.IsHexChar(ax))
                                        {
                                            sb.Append(STUtils.DCharToChar(ay, ax));
                                        }
                                        else
                                        {
                                            sb.Append(az);
                                            nowPos -= 2;
                                            if (nowPos <= 0)
                                                nowPos = 0;
                                            if (STUtils.IsHexChar(ay))
                                                //Errors.ReportWarning(LineOffset, String.Format(Messages.Tokenizer_W001, az, STUtils.NameAsciiChar(ay), STUtils.NameAsciiChar(ax)));
                                                throw new Exception("Error");
                                            else
                                                //Errors.ReportWarning(LineOffset, String.Format(Messages.Tokenizer_W002, az, STUtils.NameAsciiChar(ay)));
                                                throw new Exception("Error");
                                        }
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            if (STCO_DNL_IN_LITERIAL)
                            {
                                //string LBRK_Error = Messages.Tokenizer_E001;
                            }
                            if (az == '\r')
                            {
                                if (nowPos + 1 < TextM.Length && TextM[nowPos + 1] == '\n')
                                {
                                    nowPos++;
                                    az = '\n';
                                }
                                LineOffset++;
                                if (STCO_DNL_IN_LITERIAL)
                                {
                                   //Errors.ReportError(LineOffset - 1, Messages.Tokenizer_E001);
                                    throw new Exception("Error");
                                }
                                else
                                {
                                    if (nowPos + 1 < TextM.Length && TextM[nowPos + 1] == '\n')
                                    {
                                        nowPos++;
                                        az = '\n';
                                        sb.Append('\r');
                                    }
                                }
                            }
                            else if (az == '\n')
                            {
                                LineOffset++;
                                if (STCO_DNL_IN_LITERIAL)
                                {
                                    //Errors.ReportError(LineOffset - 1, Messages.Tokenizer_E001);
                                    throw new Exception("Error");
                                }
                            }
                            sb.Append(az);
                        }
                        nowPos++;
                        while (nowPos + 1 < TextM.Length && TextM[nowPos] == '\'' && TextM[nowPos + 1] == '\'')
                        {
                            sb.Append("'");
                            nowPos += 2;
                        }
                    }
                    nowPos++;
                    {
                        BasicToken bbt = new BasicToken(sb.ToString(), STTokenType.ttImmConstant, StartAt);
                        bbt.BuildInTypeName = "STRING";
                        bbt.LiniaKodu = LineOffset;
                        ret.Lista.Add(bbt); // AddNewTokenType(sb.ToString(), STTokenType.ttString, StartAt);
                    }
                    sb.Remove(0, sb.Length);
                    Chdone = true;
                }
                /**>> Liczby dziesiÍtne, separatory pÛl, wielokropek  <<**/
                if (nowPos < TextM.Length && TextM[nowPos] == '.')
                {
                    if ((nowPos + 1 < TextM.Length) && (TextM[nowPos + 1] == '.'))
                    {
                        BasicToken bbt = ret.AddNewTokenType("..", STTokenType.ttDelimiter, nowPos);
                        bbt.LiniaKodu = LineOffset;
                        nowPos += 2;
                        Chdone = true;
                    }
                    else
                    {
                        BasicToken tok_1;
                        if (ret.Lista.Count > 0)
                        {
                            tok_1 = ret.Lista[ret.Lista.Count - 1];
                            if (tok_1.IsIntConst())
                            {
                                //parsuj dalej aø do napotkania koÒca liczby rzeczywistej
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
                            else if (tok_1.Typ == STTokenType.ttIdentifier)
                            {
                                sb.Append(TextM, nowPos, 1);
                                BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttOperator, nowPos);
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
                                    BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttOperator, nowPos);
                                    bbt.LiniaKodu = LineOffset;
                                }
                                else
                                {
                                    BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                                    bbt.LiniaKodu = LineOffset;
                                }
                                sb.Remove(0, sb.Length);
                                nowPos++;
                                Chdone = true;
                            }
                            else
                            {
                                sb.Append(TextM, nowPos, 1);
                                BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                                bbt.LiniaKodu = LineOffset;
                                CalcLineOffset(bbt.Tekst, ref LineOffset);
                                sb.Remove(0, sb.Length);
                                nowPos++;
                                Chdone = true;
                            }
                        }
                        else
                        {
                            sb.Append(TextM, nowPos, 1);
                            BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                            bbt.LiniaKodu = LineOffset;
                            CalcLineOffset(bbt.Tekst, ref LineOffset);
                            sb.Remove(0, sb.Length);
                            nowPos++;
                            Chdone = true;
                        }
                    }
                }
                /**>> Liczby o innej podstawie oraz sta≥e <<**/
                if (nowPos < TextM.Length && TextM[nowPos] == '#')
                {
                    BasicToken tok_1;
                    if (ret.Lista.Count > 0)
                    {
                        tok_1 = ret.Lista[ret.Lista.Count - 1];
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
                                if (limited || STCharConsts.IsLetter(TextM[nowPos]))
                                    tok_1.Typ = STTokenType.ttInvalid;
                                else
                                {
                                    tok_1.Typ = STTokenType.ttImmConstant;
                                    if ((this.TokenizerOptions & STTokenizer.toUseDINTAsDefaultINT) != 0)
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
                            while (nowPos < TextM.Length && !STCharConsts.IsEndConstChar(TextM[nowPos]))
                                nowPos++;
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
                            BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                            bbt.LiniaKodu = LineOffset;
                            CalcLineOffset(bbt.Tekst, ref LineOffset);
                            sb.Remove(0, sb.Length);
                            nowPos++;
                            Chdone = true;
                        }
                    }
                    else
                    {
                        sb.Append(TextM, nowPos, 1);
                        BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                        bbt.LiniaKodu = LineOffset;
                        CalcLineOffset(bbt.Tekst, ref LineOffset);
                        sb.Remove(0, sb.Length);
                        nowPos++;
                        Chdone = true;
                    }

                }
                /**>> Inne <<**/
                if (!Chdone)
                {
                    sb.Append(TextM, StartAt, 1);
                    BasicToken bbt = ret.AddNewTokenType(sb.ToString(), STTokenType.ttInvalid, nowPos);
                    bbt.LiniaKodu = LineOffset;
                    CalcLineOffset(bbt.Tekst, ref LineOffset);
                    sb.Remove(0, sb.Length);
                    nowPos++;
                }
            }

            return ret;
        }


        public static void CalcLineOffset(string StrChk, ref int LineOffset)
        {
            if (string.IsNullOrEmpty(StrChk))
                return;

            int idxo = 0;
            char[] iter = StrChk.ToCharArray();
            while (idxo < iter.Length)
            {
                if (iter[idxo] == '\r')
                {
                    if (idxo + 1 < iter.Length && iter[idxo + 1] == '\n')
                    {
                        idxo++;
                    }
                    LineOffset++;
                }
                else if (iter[idxo] == '\n')
                {
                    LineOffset++;
                }
                idxo++;
            }
        }

        public static void MergeDotOperToIdent(TokenList lst)
        {
            if (lst.AdjustToZeroItem())
            {
                while (lst.NextTo())
                {
                    if (lst.it.Typ == STTokenType.ttOperator && lst.it.Tekst.Equals("."))
                    {
                        if (lst.NextImportant())
                        {
                            string str_past = null;
                            if (lst.it.Typ == STTokenType.ttIdentifier)
                                str_past = lst.it.Tekst;
                            if (!String.IsNullOrEmpty(str_past) && lst.PrevImportant() && lst.PrevImportant())
                            {
                                if (lst.it.Typ == STTokenType.ttIdentifier)
                                {
                                    lst.it.Tekst = String.Format("{0}.{1}", lst.it.Tekst, str_past);
                                    BasicToken ba, bb;
                                    lst.NextImportant();
                                    ba = lst.it;
                                    lst.NextImportant();
                                    bb = lst.it;
                                    lst.Lista.Remove(ba);
                                    lst.Lista.Remove(bb);
                                    lst.nit -= 2;
                                }
                                else
                                {
                                    lst.NextImportant();
                                    lst.NextImportant();
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void RemoveCommentsAndWhitespaces(TokenList tls)
        {
            tls.Lista.RemoveAll(rmCmtAndWs);
        }

        private static bool rmCmtAndWs(BasicToken t)
        {
            return t.Typ == STTokenType.ttComment || t.Typ == STTokenType.ttWhiteSpace;
        }

        public static bool TokenAcceptClass(TokenList Tokeny, LocationList Errors)
        {

            List<BasicToken> btl = Tokeny.Lista.FindAll(invalidTokenFound);
            if (btl.Count > 0)
            {
                foreach (BasicToken bt in btl)
                {
                    if (bt.Typ == STTokenType.ttInvalid)
                        //Errors.ReportError(bt.LiniaKodu, String.Format(Messages.Tokenizer_E002, bt.Tekst, bt.Pozycja));
                        throw new Exception("Error");
                    if (bt.Typ == STTokenType.ttUnknown)
                        //Errors.ReportError(bt.LiniaKodu, String.Format(Messages.Tokenizer_E003, bt.Tekst, bt.Pozycja));
                        throw new Exception("Error");
                }
                return false;
            }
            return true;
        }

        private static bool invalidTokenFound(BasicToken bt)
        {
            return bt.Typ == STTokenType.ttInvalid || bt.Typ == STTokenType.ttUnknown;
        }
    }

    [Serializable]
    public class BasicToken : IDisposable
    {
        /// <summary>
        /// Typ jednostki leksykalnej
        /// </summary>
        public STTokenType Typ;
        /// <summary>
        /// Pozycja poczπtku jednostki w strumieniu
        /// </summary>
        public int Pozycja;
        /// <summary>
        /// Ciπg znakÛw wykrytej jednostki
        /// </summary>
        public string Tekst;
        /// <summary>
        /// Linia kodu w ktÛrej ten token wystÍpuje
        /// </summary>
        public int LiniaKodu;
        /// <summary>
        /// Typ tokenu (gdy jest to wielkoúÊ sta≥a)
        /// </summary>
        public string BuildInTypeName;

        /// <summary>
        /// Konstruktor tokena
        /// </summary>
        /// <param name="Tekst">Fragment strumienia</param>
        /// <param name="Typ">Rodzaj tokena</param>
        /// <param name="Pozycja">Offset w strumieniu</param>
        public BasicToken(string Tekst, STTokenType Typ, int Pozycja)
        {
            this.Tekst = Tekst;
            this.Typ = Typ;
            this.Pozycja = Pozycja;
        }

        /// <summary>
        /// Konstruktor kopiujπcy
        /// </summary>
        /// <param name="CopyFrom">èrÛd≥o danych</param>
        public BasicToken(BasicToken CopyFrom)
        {
            Tekst = CopyFrom.Tekst;
            Typ = CopyFrom.Typ;
            Pozycja = CopyFrom.Pozycja;
            BuildInTypeName = CopyFrom.BuildInTypeName;
            LiniaKodu = CopyFrom.LiniaKodu;
        }

        /// <summary>
        /// Czy jest to sta≥a tekstowa
        /// </summary>
        public bool IsStringConst()
        {
            return BuildInTypeName.Equals("STRING", StringComparison.OrdinalIgnoreCase) || BuildInTypeName.Equals("WSTRING", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Czy jest to sta≥a ca≥kowita
        /// </summary>
        public bool IsIntConst()
        {
            string pv;
            if (BuildInTypeName == null)
                return false;
            //if (BuildInTypeName.StartsWith(STParser.fDefNamespace))
                //pv = BuildInTypeName.Substring(STParser.fDefNamespace.Length + 1);
            else
                pv = BuildInTypeName;
            return STUtils.IsInStringArray(pv, STCharConsts.STIntegerTypes, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Czy jest to sta≥a rzeczywista
        /// </summary>
        public bool IsRealConst()
        {
            return STUtils.IsInStringArray(BuildInTypeName, STCharConsts.STRealTypes, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Czy jest to sta≥a liczbowa
        /// </summary>
        public bool IsNumConst()
        {
            return IsIntConst() || IsRealConst();
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            this.Tekst = null;
            this.BuildInTypeName = null;            
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    [Serializable]
    public class TokenList : IDisposable
    {
        /// <summary>
        /// Lista tokenÛw
        /// </summary>
        public List<BasicToken> Lista;
        /// <summary>
        /// WartoúÊ bieøπcego indeksu w liúcie
        /// </summary>
        public BasicToken it;
        /// <summary>
        /// Indeks bieøπcego tokenu w liúcie
        /// </summary>
        public int nit;
        /// <summary>
        /// A source from tokens went
        /// </summary>
        public char[] SourceText;

        /// <summary>
        /// Inicjuje listÍ tokenÛw
        /// </summary>
        public TokenList()
        {
            Lista = new List<BasicToken>();
            it = null;
            nit = -1;
        }

        public TokenList(TokenList src) : this()
        {
            this.SourceText = src.SourceText;
        }
        /// <summary>
        /// Dodaje do listy tokenÛw z detekcjπ typu
        /// </summary>
        /// <param name="tx">Tekst do rozpoznania</param>
        /// <param name="st">Offset w strumieniu wejúciowym</param>
        /// <param name="line">Linia kodu</param>
        /// <param name="TokenizerFlags">Flags which allow special treatment by this and remainig functions</param>
        public void AddAutoDetect(string tx, int st, int line, int TokenizerFlags)
        {
            BasicToken t = new BasicToken(tx, STTokenType.ttUnknown, st);
            if ((TokenizerFlags & STTokenizer.toKeepIdentsInUpcase) != 0)
                t.Tekst = tx.ToUpper();
            t.LiniaKodu = line;
            if (Lista.Count > 0)
            {
                int aa = Lista.Count - 1;
                while (aa >= 0 && (Lista[aa].Typ == STTokenType.ttComment || Lista[aa].Typ == STTokenType.ttWhiteSpace || Lista[aa].Typ == STTokenType.ttDirective))
                    aa--;
                if (aa >= 0)
                    STCharConsts.DetermineType(ref t, Lista[aa], TokenizerFlags);
                else
                    STCharConsts.DetermineType(ref t, null, TokenizerFlags);
            }
            else
                STCharConsts.DetermineType(ref t, null, TokenizerFlags);
            Lista.Add(t);
        }

        public BasicToken AddNewTokenType(string tx, STTokenType typ, int start)
        {
            BasicToken btt = new BasicToken(tx, typ, start);
            Lista.Add(btt);
            return btt;
        }

        /// <summary>
        /// WyodrÍbnia (poprzez kopiowanie pod-listÍ tokenÛw)
        /// </summary>
        /// <param name="From">Indeks elementu od ktÛrego trzeba rozpoczπÊ kopiowanie</param>
        /// <param name="To">Indeks ostatniego elementu ktÛry znajdzie siÍ w wynikowej liúcie</param>
        public TokenList CloneSubTokens(int From, int To)
        {
            TokenList slf = new TokenList(this);
            while (From <= To)
            {
                BasicToken nbt = new BasicToken(Lista[From]);
                slf.Lista.Add(nbt);
                From++;
            }
            return slf;
        }

        /// <summary>
        /// Przechodzi do nastÍpnego indeksu
        /// </summary>
        public bool NextTo()
        {
            nit++;
            if (nit < Lista.Count)
            {
                it = Lista[nit];
                return true;
            }
            else
                return false;
        }

        public bool NextTo(STTokenType stTyp)
        {
            if (NextTo())
            {
                if (it.Typ != stTyp)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        public bool NextTo(STTokenType stTyp, string what)
        {
            if (NextTo(stTyp))
            {
                if (it.Tekst.Equals(what, StringComparison.CurrentCultureIgnoreCase))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public bool PrevTo()
        {
            nit--;
            if (nit >= 0 && nit < Lista.Count)
            {
                it = Lista[nit];
                return true;
            }
            else
            {
                nit = 0;
                return false;
            }
        }

        public string PeekNextTekst()
        {
            int ii = nit + 1;
            if (ii < Lista.Count)
            {
                return Lista[ii].Tekst;
            }
            else
                return String.Empty;
        }

        public BasicToken GetSafeListItem(int i)
        {
            if (i >= 0 && i < Lista.Count)
                return Lista[i];
            else
                return null;
        }

        public bool AdjustToZeroItem()
        {
            nit = 0;
            if (nit < Lista.Count)
            {
                it = Lista[nit];
                return true;
            }
            else
            {
                it = null;
                return false;
            }
        }
        /// <summary>
        /// WyodrÍbnia fragment listy aø do napotkania jednego z ogranicznikÛw napotkanych w argumentach, jednoczeúnie przesuwa wskaünik tokenÛw
        /// </summary>
        /// <param name="Find">zwraca true gdy znaleziono jeden z ogranicznikÛw, false gdy skoÒczy≥y siÍ tokeny</param>
        /// <param name="Delims">tablica wymaganych ogranicznikÛw</param>
        /// <returns>Nowa lista zawierajπca kopie istniejπcych tokenÛw</returns>
        public TokenList ExtractPartTo(out bool Find, params string[] Delims)
        {
            Find = true;
            TokenList ListaWyjsciowa = new TokenList(this);
            while (Find && !STUtils.IsInStringArray(it.Tekst, Delims, StringComparison.CurrentCulture))
            {
                ListaWyjsciowa.Lista.Add(new BasicToken(it));
                Find = NextTo();
            }
            return ListaWyjsciowa;
        }

        public string ToTextStr(bool WantsSpaces)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Lista.Count; i++)
            {
                sb.Append(Lista[i].Tekst);
                if (WantsSpaces)
                    sb.Append(' ');
            }
            return sb.ToString();
        }

        [Obsolete]
        public bool IsSimpleSingleIdentifier()
        {
            if (Lista.Count == 1)
                return Lista[0].Typ == STTokenType.ttIdentifier;
            else
            {
                foreach (BasicToken bt in Lista)
                {
                    if (bt.Typ == STTokenType.ttDelimiter)
                    {
                        if (!bt.Tekst.Equals("."))
                            return false;
                    }
                    else if (bt.Typ != STTokenType.ttIdentifier)
                        return false;
                }
                return true;
            }
        }

        public bool IsSimpleSingleIdentifierEx()
        {
            if (Lista.Count == 1)
                return Lista[0].Typ == STTokenType.ttIdentifier;
            else
            {
                foreach (BasicToken bt in Lista)
                {
                    if (bt.Typ == STTokenType.ttOperator)
                    {
                        if (!bt.Tekst.Equals("."))
                            return false;
                    }
                    else if (bt.Typ != STTokenType.ttIdentifier)
                        return false;
                }
                return true;
            }
        }

        public bool GetCurrentIdentifier(out string os)
        {
            StringBuilder current_ident = new StringBuilder();
            current_ident.Append(it.Tekst);
            if (!NextTo())
            {
                os = current_ident.ToString();
                return true;
            }
            if (it.Tekst.Equals("."))
            {
                while (it.Tekst.Equals("."))
                {
                    current_ident.Append('.');
                    if (!NextTo() || it.Typ != STTokenType.ttIdentifier)
                    {
                        os = current_ident.ToString();
                        return false;
                    }
                    current_ident.Append(it.Tekst);
                    if (!NextTo())
                    {
                        os = current_ident.ToString();
                        return true;
                    }
                }
            }
            os = current_ident.ToString();
            return true;
        }

        public bool PrevImportant()
        {
            bool ret = PrevTo();
            while (ret && (it.Typ == STTokenType.ttComment || it.Typ == STTokenType.ttWhiteSpace))
                ret = PrevTo();
            return ret;
        }

        /// <summary>
        /// NastÍpny znaczπcy (rÛøny od komentarza i spacji) token w liúcie
        /// </summary>
        /// <returns><code>false</code> gdy skoÒczy≥a siÍ lista tokenÛw</returns>
        public bool NextImportant()
        {
            bool ret = NextTo();
            while (ret && (it.Typ == STTokenType.ttComment || it.Typ == STTokenType.ttWhiteSpace))
                ret = NextTo();
            return ret;
        }

        /// <summary>
        /// Ustawia wskazywany token na pierwszy znaczπcy w liúcie (rÛøny od komentarza i spacji)
        /// </summary>
        /// <returns><code>true</code> gdy operacja zakoÒczona sukcesem</returns>
        public bool StartImportant()
        {
            bool ret = true;
            AdjustToZeroItem();
            while (ret && (it.Typ == STTokenType.ttComment || it.Typ == STTokenType.ttWhiteSpace))
                ret = NextTo();
            return ret;
        }

        /// <summary>
        /// Przechodzi do nastÍpnego tokenu o zadanym typie
        /// </summary>
        /// <param name="stopAt">Typ tokenu do zatrzymania</param>
        /// <returns><code>true</code> gdy znaleziony token jest w≥aúciwego typu, <code>false</code> gdy skoÒczy≥y siÍ tokeny</returns>
        public bool SeekTo(STTokenType stopAt)
        {
            bool ret = NextTo();
            while (ret && stopAt != it.Typ)
                ret = NextTo();
            return ret;
        }

        public bool SeekToEx(out STTokenType stoped, params STTokenType[] stopAt)
        {
            bool ret = NextTo();
            stoped = STTokenType.ttUnknown;
            while (ret && !STUtils.IsInTTArray(stopAt, stoped = it.Typ))
                ret = NextTo();
            return ret;
        }

        /// <summary>
        /// Przesuwa bieøπce wskazanie do jednego z napotkanych s≥Ûw kluczowych
        /// </summary>
        /// <param name="StopKeywords">Tablica s≥Ûw kluczowych na ktÛrych naleøy zaprzestaÊ wykonywanie</param>
        /// <returns><code>true</code> gdy znaleziono dopasowanie</returns>
        public bool SeekToKeywords(params string[] StopKeywords)
        {
            bool ret = SeekTo(STTokenType.ttKeyword);
            while (ret && !STUtils.IsInStringArray(it.Tekst, StopKeywords, StringComparison.InvariantCultureIgnoreCase))
                ret = SeekTo(STTokenType.ttKeyword);
            return ret;
        }

        /// <summary>
        /// WyodrÍbnia tokeny z wewnÍtrznej listy do listy docelowej
        /// </summary>
        /// <param name="Dest">Lista docelowa</param>
        /// <param name="FromIndex">Indeks poczπtkowy</param>
        /// <param name="ToIndex">Indeks koÒcowy</param>
        public void GetTokensRange(TokenList Dest, int FromIndex, int ToIndex)
        {
            if (ToIndex > Lista.Count)
                ToIndex = Lista.Count;
            for (int i = FromIndex; i < ToIndex; i++)
                Dest.Lista.Add(Lista[i]);
        }

        public BasicToken PeekNextToken()
        {
            int ii = nit + 1;
            if (ii < Lista.Count)
            {
                return Lista[ii];
            }
            else
                return null;

        }

        public bool SetNit(int newpos)
        {
            nit = newpos;
            if ((nit >= 0) && (nit < Lista.Count))
            {
                it = Lista[nit];
                return true;
            }
            else
                return false;
        }

        public string GetSourceFromOffs(int fromOffs, int toOffs, string linePrefix, char markIndicator, bool adjToLine)
        {
            if (SourceText == null)
                return null;
            if (fromOffs < 0)
                fromOffs = 0;
            if (toOffs > SourceText.Length)
                toOffs = SourceText.Length;
            int beginOffs = fromOffs;
            int endOffs = toOffs;
            if (adjToLine)
            {
                while ((beginOffs > 0) && (SourceText[beginOffs] != '\n' && SourceText[beginOffs] != '\r'))
                    beginOffs--;
                if (SourceText[beginOffs] == '\n' || SourceText[beginOffs] == '\r')
                    beginOffs++;
                while ((endOffs < SourceText.Length) && (SourceText[endOffs] != '\n' && SourceText[endOffs] != '\r'))
                    endOffs++;
            }
            StringBuilder rslt = new StringBuilder();
            rslt.Append(SourceText, beginOffs, endOffs - beginOffs);
            if (!String.IsNullOrEmpty(linePrefix) || markIndicator != '\0')
            {
                int eoffs = 0;
                int walker = beginOffs;
                while (walker < endOffs)
                {
                    switch (SourceText[walker])
                    {
                        case '\r':
                            if ((walker + 1 < endOffs) && (SourceText[walker + 1] == '\n'))
                            {
                                walker++;
                                eoffs++;
                            }
                            rslt.Insert(eoffs + 1, linePrefix);
                            eoffs += linePrefix.Length;
                            break;

                        case '\n':
                            rslt.Insert(eoffs + 1, linePrefix);
                            eoffs += linePrefix.Length;
                            break;
                    }
                    if (walker == fromOffs)
                    {
                        rslt.Insert(eoffs, markIndicator);
                        eoffs++;
                    }
                    if (walker == toOffs)
                    {
                        rslt.Insert(eoffs, markIndicator);
                        eoffs++;
                    }
                    walker++;
                    eoffs++;
                }
                if (walker == toOffs)
                {
                    rslt.Append(markIndicator);
                    eoffs++;
                }
            }
            if (!String.IsNullOrEmpty(linePrefix))
                rslt.Insert(0, linePrefix);

            return rslt.ToString();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this.Lista != null)
                    {
                        this.Lista.Clear();
                        this.Lista = null;
                    }
                    this.it = null;
                    this.nit = 0;
                    this.SourceText = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        ~TokenList()
        {
            Dispose();
        }
    }

    public enum STTokenType
    {
        /// <summary>
        /// ST Identyfikator
        /// </summary>
        ttIdentifier,
        /// <summary>
        /// ST wartoúÊ sta≥a bezpoúrednia
        /// </summary>
        ttImmConstant,
        /// <summary>
        /// ST s≥owo kluczowe / typ podstawowy
        /// </summary>
        ttKeyword,
        /// <summary>
        /// ST b≥Ídna jednostka
        /// </summary>
        ttInvalid,
        /// <summary>
        /// ST operator
        /// </summary>
        ttOperator,
        /// <summary>
        /// ST ogranicznik
        /// </summary>
        ttDelimiter,
        /// <summary>
        /// ST komentarz
        /// </summary>
        ttComment,
        /// <summary>
        /// ST fragment nie ustalony
        /// </summary>
        ttUnknown,
        /// <summary>
        /// ST dyrektywa kompilatora
        /// </summary>
        ttDirective,
        /// <summary>
        /// ST bia≥a spacja
        /// </summary>
        ttWhiteSpace,
        /// <summary>
        /// ST okreúlacz lokalizacji zmiennej
        /// </summary>
        ttVarLocDesc,

        /// <summary>
        /// IL etykieta
        /// </summary>
        ttILLabel,
        /// <summary>
        /// Dyrektywa weryfikacji
        /// </summary>
        ttVCBlock,
    }

    public static class STCharConsts
    {
        public static string LIdents = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";
        public static string LNumbers = "0123456789_";
        //public static string[] STOperators = { "**", /*"NOT",*/ "*", "/", /*"MOD",*/ "+", "-", "<", ">", "<=", ">=", "=", "<>", "&", /*"AND", "XOR", "OR",*/ ":=", "=>", ":", "@", "%" };
        public static string[] STOperators = { "**", "NOT", "*", "/", "MOD", "+", "-", "<", ">", "<=", ">=", "=", "<>", "&", "AND", "XOR", "OR", ":=", "=>", ":", "@", "%" };
        public static string STOpChars = "+-*/:=<>&@"; // bez %
        public static char[] STSpaceChars = { '\0', '\t', '\n', '\r', ' ' };
        public static char[] STTypedConstStop = { '\0', '\t', '\n', '\r', ' ', '(', ')', '[', ']', ';', ',', '{', '}' };
        public static char[] STVarLocDescStop = { '\0', '\t', '\n', '\r', ' ', '(', ')', '[', ']', ':', ';', ',' };
        public static string VMASMHexDigs = "0123456789aAbBcCDdEefF_";
        public static string[] STKeyWords = {
            "AND", "ARRAY", "AT", "BY", "CASE", "CONST", "CONSTANT", "DO",
            "ELSE", "ELSIF", "END_CASE", "END_CONST", "END_IF",
            "END_FOR", "END_FUNCTION", "END_FUNCTION_BLOCK",
            "END_PROGRAM", "END_REPEAT", "END_VAR", "END_WHILE",
            "EXIT", "FALSE", "FOR", "FUNCTION", "FUNCTION_BLOCK", "F_EDGE",
            "IF", "MOD", "NOT", "OF", "OR", "PROGRAM", "REPEAT", "RETAIN",
            "RETURN", "R_EDGE", "THEN", "TO", "TRUE", "UNTIL",
            "VAR", "VAR_ACCES", "VAR_EXTERNAL", "VAR_GLOBAL",
            "VAR_INPUT", "VAR_IN_OUT", "VAR_OUTPUT",
            "WHILE", "XOR", "TYPE", "END_TYPE", "STRUCT",
            "END_STRUCT" };
        public static string[] STBuildKnownTypes = {
            "BOOL", "BYTE", "DINT", "DATE", "DATE_AND_TIME",
            "DT", "DWORD", "INT", "LINT", "LREAL", "LWORD", "REAL",
            "SINT", "STRING", "TIME", "TIME_OF_DAY", "TOD", "WORD", "WSTRING" };

        public static string[] implFunc = {
            "ADD", "SUB", "MUL", "DIV", "MOD", "EXPT", "ABS",
            "SQRT", "LN", "LOG", "EXP", "SIN", "COS", "TAN",
            "ASIN", "ACOS", "ATAN", "TRUNC", "ROUND", /*"CONCAT",*/
            "LEN" };

        public static string[] STIntegerTypes = { "SINT", "INT", "DINT", "LINT", "BYTE", "WORD", "DWORD", "LWORD" }; //Do not modify order
        public static string[] STRealTypes = { "REAL", "LREAL" }; //Do not modify order

        public static string[] KeywordsAsOperators = { "AND", "OR", "XOR", "MOD", "NOT" };

        public static string[] ExprTreeSpecialProcessingOperators = { "[]", "@", "." };
        /// <summary>
        /// Czy znak moøe byÊ literπ: 0-9, A-Z, a-z i _
        /// </summary>
        /// <param name="lt">znak</param>
        /// <returns>true gdy jest z zakresu: 0-9, A-Z, a-z lub _</returns>
        public static bool IsLetter(char lt)
        {
            return LIdents.IndexOf(lt) >= 0;
        }
        /// <summary>
        /// Czy jest jednym ze znakÛw: +, -, *, /, :, =, &lt;, &gt;, &amp;, @
        /// </summary>
        /// <param name="lt">znak</param>
        /// <returns>true gdy lt jest: +, -, *, /, :, =, &lt;, &gt;, &amp; lub @</returns>
        public static bool IsOpChar(char lt)
        {
            return STOpChars.IndexOf(lt) >= 0;
        }

        /// <summary>
        /// Sprawdza czy tekst moøe byÊ liczbπ ca≥kowitπ dziesiÍtnπ jÍzyka ST 
        /// </summary>
        /// <param name="s">≥aÒcuch znakÛw</param>
        /// <param name="aa">rezultat</param>
        /// <returns>true gdy pomyúlnie skonwertowano</returns>
        public static bool IsIntNumber(string s, out int aa)
        {
            aa = 0;
            int p;
            p = s.IndexOf('_');
            while (p >= 0)
            {
                s = s.Remove(p, 1);
                p = s.IndexOf('_');
            }
            if (Int32.TryParse(s, System.Globalization.NumberStyles.Integer, STUtils.getSTDecimalFormatter(), out aa))
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Sprawdza czy tekst moøe byÊ liczbπ zmiennoprzecinkowπ dziesiÍtnπ jÍzyka ST 
        /// </summary>
        /// <param name="bt">token</param>
        /// <param name="aa">rezultat</param>
        /// <returns>true gdy pomyúlnie skonwertowano</returns>
        public static bool IsFloatNumber(BasicToken bt)
        {
            double wynik = 0;
            string s = bt.Tekst;
            int p;
            p = s.IndexOf('_');
            while (p >= 0)
            {
                s = s.Remove(p, 1);
                p = s.IndexOf('_');
            }
            if (Double.TryParse(s, System.Globalization.NumberStyles.Float, STUtils.getSTDecimalFormatter(), out wynik))
            {
                wynik = Math.Abs(wynik);
                if (wynik != 0 && (wynik > Single.MaxValue || wynik < Single.Epsilon))
                    bt.BuildInTypeName = "LREAL";
                else
                    bt.BuildInTypeName = "REAL";
                return true;
            }
            else
                return false;
        }

        public static void DetermineType(ref BasicToken ret, BasicToken PrevToken, int TokenizerOptions)
        {
            if (IsKeyWord(ret.Tekst))
            {
                ret.Tekst = ret.Tekst.ToUpper();
                if (IsOperator(ret.Tekst))
                {
                    if (STUtils.IsInStringArray(ret.Tekst, KeywordsAsOperators, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (ret.Tekst.Equals("NOT", StringComparison.CurrentCultureIgnoreCase))
                            ret.Typ = STTokenType.ttOperator;
                        else
                        {
                            if ((PrevToken == null) || (PrevToken.Typ == STTokenType.ttOperator) || (PrevToken.Typ == STTokenType.ttKeyword) ||
                                (PrevToken.Typ == STTokenType.ttDelimiter &&
                                    (PrevToken.Tekst.Equals(",", StringComparison.Ordinal) || PrevToken.Tekst.Equals("(", StringComparison.Ordinal))
                                ))
                            {
                                ret.Typ = STTokenType.ttKeyword;
                            }
                            else
                            {
                                ret.Typ = STTokenType.ttOperator;
                            }
                        }
                    }
                    else
                        ret.Typ = STTokenType.ttOperator;
                    return;
                }
                else
                {
                    if (STUtils.IsInStringArray(ret.Tekst, KeywordsAsOperators, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (ret.Tekst.Equals("NOT", StringComparison.CurrentCultureIgnoreCase))
                            ret.Typ = STTokenType.ttOperator;
                        else
                        {
                            if ((PrevToken == null) || (PrevToken.Typ == STTokenType.ttOperator))
                            {
                                ret.Typ = STTokenType.ttKeyword;
                            }
                            else
                            {
                                ret.Typ = STTokenType.ttOperator;
                            }
                        }
                    }
                    else if (ret.Tekst.Equals("TRUE", StringComparison.CurrentCultureIgnoreCase) || ret.Tekst.Equals("FALSE", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ret.Typ = STTokenType.ttImmConstant;
                        ret.BuildInTypeName = "BOOL";
                    }
                    else
                    {
                        ret.Typ = STTokenType.ttKeyword;
                    }
                    return;
                }
            }
            char[] lst = ret.Tekst.ToCharArray();
            if (lst[0] == '_')
            {
                ret.Typ = STTokenType.ttIdentifier;
                return;
            }
            int lisnumber; //int number
            /* ###
            for (int i = 0; i < lst.Length; i++)
            { <L>
                isnumber &= LNumbers.IndexOf(lst[i]) >= 0;
            }*/
            if (IsIntNumber(ret.Tekst, out lisnumber))
            {
                ret.Typ = STTokenType.ttImmConstant;
                if ((TokenizerOptions & STTokenizer.toUseDINTAsDefaultINT) != 0)
                    ret.BuildInTypeName = "DINT";
                else
                    ret.BuildInTypeName = "INT";
                AlignTypeToContent(ref ret);
                return;
            }
            /**<L>*/
            //float dummy = 0.0f;
            if (IsFloatNumber(ret))
            {
                ret.Typ = STTokenType.ttImmConstant;
                AlignTypeToContent(ref ret);
                return;
            }
            if (IsOperator(ret.Tekst))
            {
                ret.Typ = STTokenType.ttOperator;
                return;
            }
            bool isnumber = true; //identyfikator
            for (int i = 0; i < lst.Length; i++)
                isnumber &= LIdents.IndexOf(lst[i]) >= 0;
            if (isnumber)
            {
                ret.Typ = STTokenType.ttIdentifier;
                return;
            }
            ret.Typ = STTokenType.ttUnknown;
            return;
        }

        public static bool AlignTypeToContent(ref BasicToken bt)
        {
            //if (bt.Typ != STTokenType.ttImmConstant)
            return false;
            /*if (bt.IsIntConst())
            {
                
            }
            else if (bt.IsRealConst())
            {
                float dummy = 0.0f;
                if (IsFloatNumber(ret.Tekst, ref dummy))
                {

                }
                else
                    return false;
            }
            else return false; */
        }

        public static bool GetRangeForValue(int Val, bool BaseInt, bool CanUnsigned, out string R)
        {
            const string typSINT = "SINT";
            const string typBYTE = "BYTE";
            const string typINT = "INT";
            const string typWORD = "WORD";
            const string typDINT = "DINT";
            const string typDWORD = "DWORD";
            //            const string typLINT = "LINT";
            //            const string typLWORD = "LWORD";

            if (Val >= -128 && Val <= 127)
            {
                if (BaseInt)
                    R = typINT;
                else R = typSINT;
                return true;
            }
            if (CanUnsigned && (Val >= 0 && Val <= 255))
            {
                if (BaseInt)
                    R = typINT;
                else
                    R = typBYTE;
                return true;
            }
            if (Val >= Int16.MinValue && Val <= Int16.MaxValue)
            {
                R = typINT;
                return true;
            }
            if (CanUnsigned && (Val >= 0 && Val <= 65535))
            {
                R = typWORD;
                return true;
            }
            if (Val >= Int32.MinValue && Val <= Int32.MaxValue)
            {
                R = typDINT;
                return true;
            }
            if (Val > 0)
            {
                R = typDWORD;
                return true;
            }
            //****************//
            R = "";
            return false;
        }

        public static bool IsNumber(char lt)
        {
            return LNumbers.IndexOf(lt) >= 0;
        }

        public static bool IsOperator(string s)
        {
            bool fnd = false;
            int i = 0;
            while (i < STOperators.Length && !fnd)
            {
                if (STOperators[i].Equals(s, StringComparison.CurrentCultureIgnoreCase))
                    fnd = true;
                else
                    i++;
            }
            return fnd;
        }

        public static bool IsKeyWord(string s)
        {
            /*
            bool fnd = false;
            int i = 0;
            s = s.ToUpper();
            while (i < STKeyWords.Length && !fnd)
            {
                if (STKeyWords[i].CompareTo(s) == 0)
                    fnd = true;
                else
                    i++;
            }
            */
            s = s.ToUpper();
            bool fnd = Array.IndexOf<string>(STKeyWords, s) != -1;
            return fnd || IsBasicBuildType(s);
        }

        public static bool IsBasicBuildType(string s)
        {
            /*
            bool fnd = false;
            int i = 0;
            while (i < STBuildKnownTypes.Length && !fnd)
            {
                if (STBuildKnownTypes[i].CompareTo(s) == 0)
                    fnd = true;
                else
                    i++;
            }
            return fnd; */
            return Array.IndexOf<string>(STBuildKnownTypes, s) != -1;
        }

        public static bool IsSpaceChar(char c)
        {
            /*
            bool fnd = false;
            int i = 0;
            while (i < STSpaceChars.Length && !fnd)
            {
                if (STSpaceChars[i] == c)
                    fnd = true;
                else
                    i++;
            }
            return fnd; 
            */
            return Array.IndexOf<char>(STSpaceChars, c) != -1;
        }

        public static bool IsEndConstChar(char c)
        {
            /*
            bool fnd = false;
            int i = 0;
            while (i < STTypedConstStop.Length && !fnd)
            {
                if (STTypedConstStop[i] == c)
                    fnd = true;
                else
                    i++;
            }
            return fnd;
            */
            return Array.IndexOf<char>(STTypedConstStop, c) != -1;
        }

        /*        public static bool IsImplConstFunc(string fun)
                {
                    bool fnd = false;
                    int i = 0;
                    while (i < implFunc.Length && !fnd)
                    {
                        if (implFunc[i].CompareTo(fun) == 0)
                            fnd = true;
                        else
                            i++;
                    }
                    return fnd;
                }*/

        public static bool IsValidVMASMHex(string s)
        {
            for (int i = 0; i < s.Length; i++)
                if (VMASMHexDigs.IndexOf(s[i]) == -1)
                    return false;
            return true;
        }

        public static bool IsEndVarLocDesc(char c)
        {
            /*
            bool fnd = false;
            int i = 0;
            while (i < STVarLocDescStop.Length && !fnd)
            {
                if (STVarLocDescStop[i] == c)
                    fnd = true;
                else
                    i++;
            }
            return fnd;
            */
            return Array.IndexOf<char>(STVarLocDescStop, c) != -1;
        }

        public static bool IsValidIdentificatorName(string IdentName)
        {
            if (IdentName == null || IdentName.Length == 0)
                return false;
            char[] lst = IdentName.ToCharArray();
            if (lst[0] == '_' || STCharConsts.LNumbers.IndexOf(lst[0]) == -1)
            {
                for (int i = 1; i < lst.Length; i++)
                    if (!STCharConsts.IsLetter(lst[i]))
                        return false;
                if (STCharConsts.IsBasicBuildType(IdentName) ||
                    STCharConsts.IsKeyWord(IdentName) ||
                    STUtils.IsInStringArray(IdentName, STCharConsts.KeywordsAsOperators, StringComparison.InvariantCultureIgnoreCase))
                    return false;

                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// Sprawdza czy znak jest w dozwolonym zakresie: litery, cyfry, podkreúlenie oraz . + = - ' \ " % $
        /// </summary>
        /// <param name="lt"></param>
        /// <returns></returns>
        public static bool ReadAheadChars(char lt)
        {
            const string EChars = ".+=-'\"%$:";
            return LIdents.IndexOf(lt) >= 0 || EChars.IndexOf(lt) >= 0;
        }


        /// <summary>
        /// Checks if character ends a String or Unicode string
        /// </summary>
        /// <param name="current">Current character</param>
        /// <param name="nextOne">Next character</param>
        /// <param name="strDelim">String delimiter</param>
        /// <returns>Indicator of the last one character</returns>
        public static int IsEndStringChar(char current, char nextOne, char strDelim)
        {
            if (current == '$' && nextOne == strDelim)
                return 2;
            if (current == strDelim)
                return 0;
            return 1;
        }
    }

    /************************************
     * Klasa do zastosowaÒ pomocniczych *
     ************************************/
    /// <summary>
    /// Klasa do zastosowaÒ pomocniczych
    /// </summary>
    public static class STUtils
    {
        /*
        public static string STTokenTypeToString(STTokenType we)
        {
            switch (we)
            {
                case STTokenType.ttIdentifier: return Messages.Utils_H001;
                case STTokenType.ttImmConstant: return Messages.Utils_H002;
                case STTokenType.ttInvalid: return Messages.Utils_H003;
                case STTokenType.ttKeyword: return Messages.Utils_H004;
                case STTokenType.ttUnknown: return Messages.Utils_H005;
                case STTokenType.ttOperator: return Messages.Utils_H006;
                case STTokenType.ttDelimiter: return Messages.Utils_H007;
                case STTokenType.ttComment: return Messages.Utils_H008;
                case STTokenType.ttWhiteSpace: return Messages.Utils_H009;
                case STTokenType.ttDirective: return Messages.Utils_H010;
                case STTokenType.ttVarLocDesc: return Messages.Utils_H011;
                case STTokenType.ttILLabel: return Messages.Utils_H012;
                default:
                    return "?";
            }
        }
        */

        public static void ProduceHTMLStream(TokenList tl, bool do_header, out StringBuilder s)
        {
            s = new StringBuilder();
            if (do_header)
                s.Append("<!DOCTYPE HTML PUBLIC \" -//W3C//DTD HTML 4.01 Transitional//EN\">\n\n<html><head>\n" +
"<meta http-equiv=\"content-type\" content=\"text/html; charset=UTF-8\"> \n" +
"<STYLE TYPE=\"text/css\">\n <!-- \nBODY { background: #FFFFFF; text: #000000; font-family: 'Courier New'; }\n" +
".nor {	background-color: #FFFFFF; color: #000000; font-family: 'Courier New'; }\n" +
".ident { background-color: #FFFFFF; color: #000000; font-family: 'Courier New'; }\n" +
".keyw { background-color: #FFFFFF; color: Blue; font-family: 'Courier New'; font: bold;}\n" +
".oper { background-color: #FFFFFF; color: #808000; font-family: 'Courier New'; }\n" +
".delim { background-color: #F0F0F0; color: #008080; font-family: 'Courier New'; }\n" +
".errinf { background-color: DarkRed; color: #C0C0C0; font-family: 'Courier New'; }\n" +
".errpos { background-color: Black; color: Red; font-family: 'Courier New'; }\n" +
".direct { background-color: #F0F0F0; color: #AA0D04; font-family: 'Courier New'; }\n" +
".comment {	background-color: #FFFFFF; color: green; font-family: 'Courier New'; font: italic; }" +
".typconst { background-color: #C0C0C0; color: DarkGreen; font-family: 'Courier New'; } " +
".nums { background-color: #FFFFFF; color: #00AFAF; font-family: 'Courier New'; }" +
".strs { background-color: #FFFF00; color: DarkGreen; font-family: 'Courier New'; }" +
"--> \n </STYLE> \n </head><body bgcolor=\"#FFFFFF\">");
            s.Append("<pre>");
            for (int i = 0; i < tl.Lista.Count; i++)
            {
                switch (tl.Lista[i].Typ)
                {
                    case STTokenType.ttKeyword:
                        s.Append("<span class=\"keyw\">");
                        break;
                    case STTokenType.ttOperator:
                        s.Append("<span class=\"oper\">");
                        break;
                    case STTokenType.ttDelimiter:
                        s.Append("<span class=\"delim\">");
                        break;
                    case STTokenType.ttComment:
                        s.Append("<span class=\"comment\">");
                        break;
                    case STTokenType.ttImmConstant:
                        {
                            if (tl.Lista[i].IsStringConst())
                            {
                                s.Append("<span class=\"strs\">'");
                            }
                            else
                            {
                                if (tl.Lista[i].IsNumConst())
                                    s.Append("<span class=\"nums\">");
                                else
                                    s.Append("<span class=\"typconst\">");
                            }
                        }
                        break;
                    case STTokenType.ttInvalid:
                    case STTokenType.ttUnknown:
                        s.Append("<span class=\"errpos\">");
                        break;
                    case STTokenType.ttDirective:
                        s.Append("<span class=\"direct\">");
                        break;
                    //case STTokenType.ttVarLocDesc:
                    default:
                        s.Append("<span class=\"nor\">");
                        break;
                }
                if (tl.Lista[i].Typ == STTokenType.ttImmConstant && tl.Lista[i].IsStringConst())
                {
                    s.Append(HTMLEncode(STCharEncode(tl.Lista[i].Tekst)));
                    s.Append('\'');
                }
                else
                    s.Append(HTMLEncode(tl.Lista[i].Tekst));
                s.Append("</span>");
            }
            s.Append("</pre>");
            if (do_header)
                s.Append("</body>\n</html>");
        }

        public static string STCharEncode(string p)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            while (i < p.Length)
            {
                switch (p[i])
                {
                    case '$':
                        sb.Append("$$");
                        break;
                    case '\'':
                        sb.Append("$'");
                        break;
                    case '\n':
                        sb.Append("$N");
                        break;
                    case '\r':
                        sb.Append("$R");
                        break;
                    case '\u000B':
                        sb.Append("$P");
                        break;
                    case '\t':
                        sb.Append("$T");
                        break;
                    default:
                        sb.Append(p[i]);
                        break;
                }
                i++;
            }
            return sb.ToString();
        }

        public static string ProduceRTFStream(TokenList tl, bool do_header)
        {
            StringBuilder s = new StringBuilder();
            if (do_header)
                s.Append(@"{\rtf1\ansi\ansicpg1250\deff0\deflang1045{\fonttbl{\f0\fnil\fcharset238{\*\fname Courier New;}Courier New CE;}}
{\colortbl \red0\green0\blue0;\red0\green0\blue255;\red128\green128\blue0;\red0\green128\blue128;\red192\green192\blue192;\red0\green255\blue0;                    
\red0\green128\blue0;\red0\green177\blue177;\red128\green2\blue128;}");
            s.Append(@"\viewkind4\uc1\pard\f0\fs20 ");
            for (int i = 0; i < tl.Lista.Count; i++)
            {
                BasicToken bbt = tl.Lista[i];
                switch (bbt.Typ)
                {
                    case STTokenType.ttKeyword:
                        s.Append(@"\cf1\b ");
                        s.Append(RTFEncode(bbt.Tekst));
                        s.Append(@"\b0\cf0 ");
                        break;
                    case STTokenType.ttOperator:
                        s.Append(@"\cf2 ");
                        s.Append(RTFEncode(bbt.Tekst));
                        s.Append(@"\cf0 ");
                        break;
                    case STTokenType.ttDelimiter:
                        s.Append(@"\cf3 ");
                        s.Append(RTFEncode(bbt.Tekst));
                        s.Append(@"\cf0 ");
                        break;
                    case STTokenType.ttComment:
                        s.Append(@"\cf5\i ");
                        s.Append(RTFEncode(bbt.Tekst));
                        s.Append(@"\i0\cf0 ");
                        break;
                    case STTokenType.ttImmConstant:
                        {
                            if (bbt.IsStringConst())
                            {
                                s.Append(@"\cf8 '");
                                s.Append(RTFEncode(STCharEncode(bbt.Tekst)));
                                s.Append(@"'\cf0 ");
                            }
                            else if (bbt.IsNumConst())
                            {
                                s.Append(@"\cf7 ");
                                s.Append(RTFEncode(bbt.Tekst));
                                s.Append(@"\cf0 ");
                            }
                            else
                            {
                                s.Append(@"\cf6 ");
                                s.Append(RTFEncode(bbt.Tekst));
                                s.Append(@"\cf0 ");
                            }
                            break;
                        }
                    case STTokenType.ttInvalid:
                    case STTokenType.ttUnknown:
                        s.Append(@"\cf4 ");
                        s.Append(RTFEncode(bbt.Tekst));
                        s.Append(@"\cf0 ");
                        break;
                    default:
                        s.Append(@"\cf0 ");
                        s.Append(RTFEncode(bbt.Tekst));
                        s.Append(@"\cf0 ");
                        break;
                }
            }
            if (do_header)
                s.Append("}\n");
            return s.ToString();
        }

        /// <summary>
        /// Funkcja zamieniajπca pojedyncze ampersandy na &amp;amp; oraz znaki &lt;, &gt; na &amp;lt; i &amp;gt;
        /// </summary>
        /// <param name="we">£aÒcuch wejúciowy</param>
        /// <returns>Wynik zamiany</returns>
        public static string HTMLEncode(string we)
        {
            we = we.Replace("&", "&amp;");
            we = we.Replace("<", "&lt;");
            we = we.Replace(">", "&gt;");
            return we;
        }

        public static string HTMLDecode(string we)
        {
            StringBuilder sbu = new StringBuilder();
            int i = 0;
            while (i < we.Length)
            {
                char iter = we[i];
                switch (iter)
                {
                    case '&':
                        {
                            int j = i + 1;
                            while (j < we.Length && we[j] != ';')
                                j++;
                            string scmd = we.Substring(i + 1, j - i - 1);
                            switch (scmd)
                            {
                                case "amp":
                                    sbu.Append('&');
                                    break;
                                case "lt":
                                    sbu.Append('<');
                                    break;
                                case "gt":
                                    sbu.Append('>');
                                    break;
                                case "qt":
                                    sbu.Append('\'');
                                    break;
                                case "dq":
                                    sbu.Append('"');
                                    break;
                                default:
                                    if (scmd.Length != 0 && scmd[0] == '#')
                                    {
                                        try
                                        {
                                            byte rr = Convert.ToByte(scmd.Substring(1), 16);
                                            sbu.Append((char)rr);
                                        }
                                        catch (Exception)
                                        { }
                                    }
                                    else
                                    {
                                        sbu.Append('&');
                                        sbu.Append(scmd);
                                        sbu.Append(';');
                                    }
                                    break;
                            }
                            i = j;
                        }
                        break;
                    case '<':
                        {
                            int j = i + 1;
                            while (j < we.Length && we[j] != '>')
                                j++;
                            string scmd = we.Substring(i + 1, j - i - 1);
                            switch (scmd.Trim().ToLower())
                            {
                                case "br":
                                    sbu.Append('\n');
                                    break;
                                default:
                                    sbu.Append('<');
                                    sbu.Append(scmd);
                                    sbu.Append('>');
                                    break;
                            }
                            i = j;
                        }
                        break;
                    default:
                        sbu.Append(iter);
                        break;
                }
                i++;
            }
            return sbu.ToString();
        }

        public static string RTFEncode(string we)
        {
            we = we.Replace("\\", "\\\\");
            we = we.Replace("{", "\\{");
            we = we.Replace("}", "\\}");
            we = we.Replace("\r\n", "\\par \n");
            return we;
        }
        /*
        public static void PrintTokenListToStdOut(TokenList tl, bool Opt_SkipWs)
        {
            for (int i = 0; i < tl.Lista.Count; i++)
            {
                if (tl.Lista[i].Typ == STTokenType.ttWhiteSpace && Opt_SkipWs)
                    continue;
                System.Console.BackgroundColor = ConsoleColor.Black;
                System.Console.Out.Write(Messages.Utils_H013);
                ConsoleColor prv = System.Console.ForegroundColor;
                System.Console.BackgroundColor = ConsoleColor.DarkGreen;
                switch (tl.Lista[i].Typ)
                {
                    case STTokenType.ttKeyword:
                        System.Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case STTokenType.ttOperator:
                        System.Console.ForegroundColor = ConsoleColor.Black;
                        break;
                    case STTokenType.ttDelimiter:
                        System.Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                }
                System.Console.Out.Write(tl.Lista[i].Tekst);
                System.Console.ForegroundColor = prv;
                System.Console.BackgroundColor = ConsoleColor.Black;
                System.Console.Write(Messages.Utils_H014, tl.Lista[i].LiniaKodu);
                if (tl.Lista[i].Typ == STTokenType.ttInvalid)
                {
                    System.Console.BackgroundColor = ConsoleColor.DarkRed;
                    System.Console.Write(STTokenTypeToString(tl.Lista[i].Typ));
                    System.Console.BackgroundColor = ConsoleColor.Black;
                    System.Console.Out.Write(Messages.Utils_H015);
                    System.Console.BackgroundColor = ConsoleColor.Red;
                    System.Console.Out.Write(tl.Lista[i].Pozycja);
                }
                else
                {
                    System.Console.BackgroundColor = ConsoleColor.Blue;
                    System.Console.Write(STTokenTypeToString(tl.Lista[i].Typ));
                }
                System.Console.BackgroundColor = ConsoleColor.Black;
                System.Console.Out.WriteLine();
            }
        }
        */
        /*    public static void PrintIdentsToStdOut(List<STIdentificator> lst)
            {
                foreach (STIdentificator a in lst)
                {
                    string s = a.DumpData();
                    System.Console.Out.WriteLine(s);
                    if (s.Length > 80)
                        System.Console.Out.WriteLine();
                }
            }*/
        /*
        public static void PrintTokenListToStream(System.IO.FileStream fs, TokenList tl, bool Opt_SkipWs)
        {
            StringBuilder sb = new StringBuilder();
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fs, Encoding.UTF8, 256);
            sw.AutoFlush = true;
            for (int i = 0; i < tl.Lista.Count; i++)
            {
                BasicToken nowTok = tl.Lista[i];
                if (nowTok.Typ == STTokenType.ttWhiteSpace && Opt_SkipWs)
                    continue;
                sb.Append(Messages.Utils_H013);
                sb.Append(nowTok.Tekst);
                sb.Append(Messages.Utils_H016);
                sb.Append(STTokenTypeToString(nowTok.Typ));
                switch (nowTok.Typ)
                {
                    case STTokenType.ttInvalid:
                        {
                            sb.Append(Messages.Utils_H017);
                            sb.Append(nowTok.Pozycja);
                        }
                        break;
                    case STTokenType.ttImmConstant:
                        {
                            sb.Append(Messages.Utils_H018);
                            sb.Append(nowTok.BuildInTypeName);
                        }
                        break;
                    default:
                        sb.Append(';');
                        break;
                }
                sw.WriteLine(sb.ToString());
                sb.Remove(0, sb.Length);
            }
        }
        */
        public static bool String_IsNullOrWhiteSpace(string s)
        {
#if USE_NET_4_OR_ABOVE
            return String.IsNullOrWhiteSpace(s);
#else
            if (s == null)
                return true;
            for (int i = 0; i < s.Length; i++)
            {
                if (!Char.IsWhiteSpace(s[i]))
                    return false;
            }
            return true;
#endif
        }

        /// <summary>
        /// Converts type category to string with sensitive current language 
        /// </summary>
        /// <param name="t">Type tp convert</param>
        /// <returns>Language dependent string</returns>
        
        /*
        public static string STTypeCatToString(STTypeCat t)
        {
            switch (t)
            {
                case STTypeCat.tcAliasType:
                    return Messages.Utils_H019;
                case STTypeCat.tcArrayType:
                    return Messages.Utils_H020;
                case STTypeCat.tcBuildInType:
                    return Messages.Utils_H021;
                case STTypeCat.tcEnumType:
                    return Messages.Utils_H022;
                case STTypeCat.tcStringType:
                    return Messages.Utils_H023;
                case STTypeCat.tcStructType:
                    return Messages.Utils_H024;
                case STTypeCat.tcUserFunction:
                    return Messages.Utils_H025;
                case STTypeCat.tcVariable:
                    return Messages.Utils_H026;
                case STTypeCat.tcFunctionBlock:
                    return Messages.Utils_H027;
                case STTypeCat.tcSysProc:
                    return Messages.Utils_H028;
                case STTypeCat.tcBuildInFunction:
                    return Messages.Utils_H029;
                case STTypeCat.tcProgram:
                    return Messages.Utils_H030;
                case STTypeCat.tcTask:
                    return Messages.Utils_H031;
                case STTypeCat.tcPackage:
                    return Messages.Utils_H032;
                case STTypeCat.tcNamespace:
                    return Messages.Utils_H033;
                default:
                    return Messages.Utils_H034;
            }
        }
        */

        /*public static string CategoryTypeToString(CategoryType cat)
        {
            switch (cat)
            {
                case CategoryType.ctARRAYType:
                    return "ARRAY";
                case CategoryType.ctINTType:
                    return "INTEGER";
                case CategoryType.ctREALType:
                    return "REAL";
                case CategoryType.ctSTRINGType:
                    return "STRING";
                case CategoryType.ctSTRUCTType:
                    return "STRUCT";
                case CategoryType.ctUINTType:
                    return "UNSIGNED";
                case CategoryType.ctFUNBLKType:
                    return "FUNC.BLK";
                case CategoryType.ctUnknownType:
                    return "<unknown>";
                default:
                    return "?unknown?";
            }
        }*/
        public static void PrintErrorsToConsole(LocationList Errors)
        {
            foreach (LocationInfo li in Errors.Items)
            {
                ConsoleColor bb = Console.BackgroundColor;
                ConsoleColor ff = Console.ForegroundColor;
                switch (li.Kind)
                {
                    case CompilerReport.crError:
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case CompilerReport.crHint:
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case CompilerReport.crWarning:
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                }
                //Console.WriteLine(String.Format(Messages.Utils_H035, li.Position, li.Description));
                Console.BackgroundColor = bb;
                Console.ForegroundColor = ff;
            }
        }

        public static void PrintErrorsToConsoleEx(LocationList Errors)
        {
            PrintErrorsToStreamEx(Errors, Console.Out);
        }

        public static void PrintErrorsToStreamEx(LocationList Errors, System.IO.TextWriter wr)
        {
            string tp = String.Empty;
            foreach (LocationInfo li in Errors.Items)
            {
                ConsoleColor bb = Console.BackgroundColor;
                ConsoleColor ff = Console.ForegroundColor;
                switch (li.Kind)
                {
                    case CompilerReport.crError:
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.ForegroundColor = ConsoleColor.White;
                        tp = "<E>";
                        break;
                    case CompilerReport.crHint:
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        tp = "<H>";
                        break;
                    case CompilerReport.crWarning:
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                        tp = "<W>";
                        break;
                    default:
                        tp = String.Empty;
                        break;
                }
                //wr.WriteLine(String.Format(Messages.Utils_H044, tp, li.Position, li.Description));
                Console.BackgroundColor = bb;
                Console.ForegroundColor = ff;
            }
        }

        /*
        public static string GetUniqId(STParser.ParserArguments arg)
        {
            return String.Format("{0:X04}", arg.LFUniq.GetAndInc);
        }   */
        /*
        public static string GetUniqId(string prefix, STParser.ParserArguments arg)
        {
            return String.Format("{0}{1:X04}", prefix, arg.LFUniq.GetAndInc);
        }   */

        private static System.Globalization.NumberFormatInfo fnfiSTUniv;

        public static System.Globalization.NumberFormatInfo getSTDecimalFormatter()
        {
            if (fnfiSTUniv == null)
            {
                fnfiSTUniv = new System.Globalization.NumberFormatInfo();
                fnfiSTUniv.NumberGroupSeparator = "_";
                fnfiSTUniv.NumberDecimalDigits = 7;
            }
            return fnfiSTUniv;
        }

        public static string TimeToString(DateTime dt)
        {
            return String.Format("{0:D2}:{1:D2}:{2:D2}", dt.Hour, dt.Minute, dt.Second);
        }

        internal static readonly DateTime CPDev_BaseDate = new DateTime(2001, 01, 01);

        public static uint ConvertToDATE(int Year, int Month, int Day)
        {
            uint sum;
            DateTime dt = new DateTime(Year, Month, Day);
            Decimal ddd = dt.Ticks;
            ddd -= CPDev_BaseDate.Ticks;
            ddd /= 1E+007m;
            sum = (uint)(ddd);
            return sum;
        }

        public static uint ConvertToDATEANDTIME(int Year, int Month, int Day, int Hour, int Minutes, int Secs)
        {
            uint sum;
            ConvertToDATEANDTIME(out sum, Year, Month, Day, Hour, Minutes, Secs);
            return sum;
        }

        public static bool ConvertToDATEANDTIME(out uint sum, int Year, int Month, int Day, int Hour, int Minutes, int Secs)
        {
            DateTime dt = new DateTime(Year, Month, Day, Hour, Minutes, Secs);
            Decimal ddd = dt.Ticks;
            ddd -= CPDev_BaseDate.Ticks;
            ddd /= 1E+007m;
            if (ddd > UInt32.MaxValue)
            {
                sum = UInt32.MaxValue;
                return false;
            }
            if (ddd < UInt32.MinValue)
            {
                sum = UInt32.MinValue;
                return false;
            }
            sum = (uint)ddd;
            return true;
        }

        public static int ConvertToTIME(int Days, int Hours, int Minutes, int Seconds, int MiliSeconds, out bool Bound)
        {
            long lll = (MiliSeconds + 1000 * (Seconds + 60 * Minutes + 3600 * (Hours + 24 * Days)));
            Bound = false;
            if (lll > Int32.MaxValue)
            {
                lll = Int32.MaxValue;
                Bound = true;
            }
            if (lll < Int32.MinValue)
            {
                lll = Int32.MinValue;
                Bound = true;
            }
            return (int)lll;
        }

        public static void UnConvertDATE(uint value, out int Year, out int Month, out int Day)
        {
            Decimal ddd = CPDev_BaseDate.Ticks;
            ddd += (decimal)value * 1E+007m;
            DateTime dt = new DateTime((long)ddd);
            Year = dt.Year;
            Month = dt.Month;
            Day = dt.Day;
        }

        public static void UnConvertDATEANDTIME(uint value, out int Year, out int Month, out int Day, out int Hour, out int Minutes, out int Secs)
        {
            Decimal ddd = CPDev_BaseDate.Ticks;
            ddd += (decimal)value * 1E+007m;
            DateTime dt = new DateTime((long)ddd);
            Year = dt.Year;
            Month = dt.Month;
            Day = dt.Day;
            Hour = dt.Hour;
            Minutes = dt.Minute;
            Secs = dt.Second;
        }

        public static void UnConvertTIME(int value, out int Days, out int Hours, out int Minutes, out int Seconds, out int MiliSeconds)
        {
            int Multi = 24 * 1000 * 3600;
            Days = (int)(value / Multi);
            value -= (int)(Days * Multi);

            Multi = 1000 * 3600;
            Hours = (int)(value / Multi);
            value -= (int)(Hours * Multi);

            Multi = 1000 * 60;
            Minutes = (int)(value / Multi);
            value -= (int)(Minutes * Multi);

            Multi = 1000;
            Seconds = (int)(value / Multi);
            value -= (int)(Seconds * Multi);

            MiliSeconds = (int)value;
        }

        public static bool IsInStringArray(string tok, string[] tab, StringComparison method)
        {
            bool fnd = false;
            int i = 0;
            while (i < tab.Length && !fnd)
            {
                if (tab[i].Equals(tok, method))
                    fnd = true;
                else
                    i++;
            }
            return fnd;
        }

        public static int GetIndexOfStringArray(string tok, string[] tab, StringComparison method)
        {
            bool fnd = false;
            int i = 0;
            while (i < tab.Length && !fnd)
            {
                if (tab[i].Equals(tok, method))
                    fnd = true;
                else
                    i++;
            }
            if (fnd)
                return i;
            else
                return -1;
        }

        public static char HexChar(int i)
        {
            const string s = "0123456789ABCDEF";
            if (i >= 0x00 && i <= 0x0F)
                return s[i];
            else
                return ' ';
        }
        /// <summary>
        /// Konwersja liczby ca≥kowitej int do liczby szesnastkowej jako tekst (tryb Big-Endian)
        /// </summary>
        /// <param name="val">Liczba przeznaczona do konwersji</param>
        /// <param name="size">Rozmiar w nibblach</param>
        /// <returns>Tekst o d≥ugoúci co namniej size</returns>
        public static string IntToHex(int val, int size)
        {
            StringBuilder sbr = new StringBuilder(Convert.ToString(val, 16));
            while (sbr.Length < size)
                sbr.Insert(0, '0');
            return sbr.ToString();
        }
        /// <summary>
        /// Konwersja liczby ca≥kowitej int do liczby szesnastkowej jako tekst
        /// </summary>
        /// <param name="val">Liczba przeznaczona do konwersji</param>
        /// <param name="size">Wymagany rozmiar w nibblach</param>
        /// <param name="LEorder">Gdy <code>true</code> to wykonuje konwersjÍ w trybie LittleEndian</param>
        /// <returns>Tekst o d≥ugoúci co najmniej size</returns>
        public static string IntToHexEx(int val, int size, bool LEorder)
        {
            if (LEorder)
            {
                StringBuilder sbr = new StringBuilder();
                while (val != 0)
                {
                    sbr.AppendFormat("{0:X02}", val & 0xFF);
                    val = (int)((uint)(val) >> 8);
                }
                while (sbr.Length < size)
                    sbr.Append('0');
                return sbr.ToString();
            }
            else
                return IntToHex(val, size);
        }

        /// <summary>
        /// WyodrÍbnia pierwszπ czÍúÊ nazwy (ucina ostatni fragment)
        /// </summary>
        /// <param name="ItemName"></param>
        /// <returns></returns>
        public static string GetPrefixForName(string ItemName)
        {
            string[] pkgs = ItemName.Split('.');
            string fReturn = "";
            if (pkgs.Length > 0)
            {
                fReturn = pkgs[0];
                for (int i = 1; i < pkgs.Length - 1; i++)
                {
                    fReturn = String.Format("{0}.{1}", fReturn, pkgs[i]);
                }
            }
            return fReturn;
        }
        /*
        public static System.Xml.XmlElement ProduceXmlErrorList(LocationList ErrList, System.Xml.XmlDocument Doc, System.Xml.XmlElement Parent)
        {
            System.Xml.XmlElement xel = Doc.CreateElement("ERROR_INFO");
            foreach (LocationInfo li in ErrList.Items)
            {
                System.Xml.XmlElement xm = Doc.CreateElement("ITEM");
                xel.AppendChild(xm);
                System.Xml.XmlAttribute xa = Doc.CreateAttribute("pos");
                xa.Value = String.Format("{0}", li.Position);
                xm.Attributes.Append(xa);
                xa = Doc.CreateAttribute("pos_hex");
                xa.Value = String.Format("{0:X}", li.Position);
                xm.Attributes.Append(xa);
                xa = Doc.CreateAttribute("type");
                switch (li.Kind)
                {
                    case CompilerReport.crError:
                        xa.Value = "Error";
                        break;
                    case CompilerReport.crHint:
                        xa.Value = "Hint";
                        break;
                    case CompilerReport.crWarning:
                        xa.Value = "Warning";
                        break;
                    default:
                        xa.Value = "Unknown";
                        break;
                }
                xm.Attributes.Append(xa);
                System.Xml.XmlCDataSection cdatSec = CPD_Objs.CreateCDataSection(Doc, xel, li.Description);
                xel.AppendChild(cdatSec);
            }
            if (Parent == null)
                Doc.AppendChild(xel);
            else
                Parent.AppendChild(xel);
            return xel;
        }
        */
        public static int CalcMaxLen(List<string> list)
        {
            int ret = 0;
            foreach (string ss in list)
            {
                if (ss == null)
                    continue;
                if (ss.Length > ret)
                    ret = ss.Length;
            }
            return ret;
        }

        public static bool SplitTokensToDeclImpl(TokenList Code, int tokenizeFlags, out TokenList Decl, out TokenList Impl)
        {
            bool STCO_ALLOW_SEMICOLON_ON_END = ((tokenizeFlags & STTokenizer.toIgnoreSemicolonAtEnd) != 0);
            Decl = new TokenList(Code);
            Impl = new TokenList(Code);
            bool ret = Code.AdjustToZeroItem();

            while (ret && (Code.it.Typ == STTokenType.ttDirective))
                ret = Code.NextImportant();
            if (ret)
            {
                int splitPos = -1;
                List<string> StopKeys = new List<string>();
                if (Code.it.Typ == STTokenType.ttKeyword)
                {
                    switch (Code.it.Tekst.ToUpper())
                    {
                        case "PROGRAM":
                            ret = Code.SeekTo(STTokenType.ttIdentifier);
                            if (ret && Code.NextImportant())
                            {
                                if (STCO_ALLOW_SEMICOLON_ON_END)
                                {
                                    if (Code.it.Tekst.Equals(";"))
                                        ret = Code.NextImportant();
                                }
                                splitPos = Code.nit;
                            }
                            if (ret && Code.it.Typ != STTokenType.ttKeyword)
                                ret = Code.SeekTo(STTokenType.ttKeyword);

                            while (ret && (Code.it.Tekst.Equals("VAR") || Code.it.Tekst.Equals("VAR_EXTERNAL")) || Code.it.Tekst.Equals("VAR_GLOBAL"))
                            {
                                ret = Code.SeekToKeywords("END_VAR");
                                if (ret)
                                {

                                    ret = Code.NextTo();
                                    if (STCO_ALLOW_SEMICOLON_ON_END)
                                    {
                                        if (ret && Code.it.Tekst.Equals(";"))
                                            ret = Code.NextTo();
                                    }
                                    splitPos = Code.nit;
                                    while (ret && (Code.it.Typ == STTokenType.ttComment || Code.it.Typ == STTokenType.ttWhiteSpace))
                                        ret = Code.NextTo();
                                }
                                if (ret && Code.it.Typ != STTokenType.ttKeyword)
                                    ret = Code.SeekTo(STTokenType.ttKeyword);
                            }
                            break;
                        case "FUNCTION_BLOCK":
                            ret = Code.SeekTo(STTokenType.ttIdentifier);
                            if (ret && Code.NextImportant())
                            {
                                if (STCO_ALLOW_SEMICOLON_ON_END)
                                {
                                    if (Code.it.Tekst.Equals(";"))
                                        ret = Code.NextImportant();
                                }
                                splitPos = Code.nit;
                            }
                            if (ret && Code.it.Typ != STTokenType.ttKeyword)
                                ret = Code.SeekTo(STTokenType.ttKeyword);

                            while (ret && (Code.it.Tekst.Equals("VAR") || Code.it.Tekst.Equals("VAR_EXTERNAL") || Code.it.Tekst.Equals("VAR_INPUT") || Code.it.Tekst.Equals("VAR_OUTPUT") || Code.it.Tekst.Equals("VAR_IN_OUT")))
                            {
                                ret = Code.SeekToKeywords("END_VAR");
                                if (ret)
                                {

                                    ret = Code.NextTo();
                                    if (STCO_ALLOW_SEMICOLON_ON_END)
                                    {
                                        if (ret && Code.it.Tekst.Equals(";"))
                                            ret = Code.NextTo();
                                    }
                                    splitPos = Code.nit;
                                    while (ret && (Code.it.Typ == STTokenType.ttComment || Code.it.Typ == STTokenType.ttWhiteSpace))
                                        ret = Code.NextTo();
                                }
                                if (ret && Code.it.Typ != STTokenType.ttKeyword)
                                    ret = Code.SeekTo(STTokenType.ttKeyword);
                            }
                            break;
                        case "FUNCTION":
                            {
                                STTokenType stoped;
                                ret = Code.SeekToEx(out stoped, STTokenType.ttIdentifier, STTokenType.ttDelimiter);
                                if (ret && stoped == STTokenType.ttIdentifier) //Nazwa funkcji
                                    ret = Code.SeekToEx(out stoped, STTokenType.ttDelimiter, STTokenType.ttKeyword, STTokenType.ttIdentifier);
                                if (ret)
                                {
                                    if (stoped == STTokenType.ttDelimiter)
                                    {
                                        if (Code.it.Tekst.Equals(":"))
                                            ret = Code.SeekToEx(out stoped, STTokenType.ttKeyword, STTokenType.ttIdentifier);
                                        if (ret)
                                            splitPos = Code.nit;

                                    }
                                    if (ret && stoped == STTokenType.ttIdentifier) //typ funkcji
                                    {
                                        ret = Code.NextImportant();
                                        splitPos = Code.nit;
                                    }
                                    if (ret && stoped == STTokenType.ttKeyword) //typ funkcji
                                    {
                                        splitPos = Code.nit;
                                        if (STCharConsts.IsBasicBuildType(Code.it.Tekst))
                                            ret = Code.NextImportant();
                                        if (ret)
                                            splitPos = Code.nit;
                                    }
                                }
                                if (STCO_ALLOW_SEMICOLON_ON_END)
                                {
                                    if (ret && Code.it.Tekst.Equals(";"))
                                        ret = Code.NextImportant();
                                }
                                splitPos = Code.nit;

                                if (ret && Code.it.Typ != STTokenType.ttKeyword)
                                    ret = Code.SeekTo(STTokenType.ttKeyword);

                                while (ret && (Code.it.Tekst.Equals("VAR") || Code.it.Tekst.Equals("VAR_INPUT")))
                                {
                                    ret = Code.SeekToKeywords("END_VAR");
                                    if (ret)
                                    {

                                        ret = Code.NextTo();
                                        if (STCO_ALLOW_SEMICOLON_ON_END)
                                        {
                                            if (ret && Code.it.Tekst.Equals(";"))
                                                ret = Code.NextTo();
                                        }
                                        splitPos = Code.nit;
                                        while (ret && (Code.it.Typ == STTokenType.ttComment || Code.it.Typ == STTokenType.ttWhiteSpace))
                                            ret = Code.NextTo();
                                    }
                                    if (ret && Code.it.Typ != STTokenType.ttKeyword)
                                        ret = Code.SeekTo(STTokenType.ttKeyword);
                                }
                            }
                            break;

                    }
                }
                if (splitPos >= 0)
                {
                    Code.GetTokensRange(Decl, 0, splitPos);
                    Code.GetTokensRange(Impl, splitPos, Code.Lista.Count);
                }
                else
                    Impl.Lista = Code.Lista;
            }
            return ret;
        }

        /// <summary>
        /// Sprawdza czy zadany typ tokenu wystÍpuje w tablicy toeknÛw
        /// </summary>
        /// <param name="tokArr">Tablica typÛw tokenÛw do przeszukania</param>
        /// <param name="scantok">Poszukiwany token</param>
        /// <returns><code>true</code> gdy typ token zosta≥ znaleziony w tablicy</returns>
        public static bool IsInTTArray(STTokenType[] tokArr, STTokenType scantok)
        {
            foreach (STTokenType iter in tokArr)
                if (scantok == iter)
                    return true;
            return false;
        }

        public static string ReplaceVarName(string PNazwa, string RNazwa)
        {
            string[] PNames = PNazwa.Split('.');
            string[] RNames = RNazwa.Split('.');
            int i = PNames.Length - 1;
            int j = RNames.Length - 1;
            while (i >= 0 && j >= 0)
            {
                PNames[i] = RNames[j];
                i--;
                j--;
            }
            return String.Join(".", PNames);
        }

        /// <summary>
        /// Kopiuje zawartoúÊ drugiej listy do pierwszej. Przed kopiowaniem wykonywane jest czyszczenie instancji 
        /// pierwszej listy
        /// </summary>
        /// <param name="dst">Lista docelowa</param>
        /// <param name="src">Lista ürÛd≥owa</param>
        public static void AssignLists(System.Collections.IList dst, System.Collections.IList src)
        {
            dst.Clear();
            foreach (object iii in src)
                dst.Add(iii);
        }

        public static string StrMergeNotNull(params string[] IdentName)
        {
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < IdentName.Length; i++)
            {
                string s = IdentName[i];
                if (!String.IsNullOrEmpty(s))
                    ret.Append(s);
            }
            return ret.ToString();
        }


        public static string LIntToHexEx(long Value, int BitsNum, bool LEorder)
        {
            long StartAnd = 0xFFL;
            int ShiftBit = 56;
            StringBuilder sb = new StringBuilder(BitsNum);
            while (ShiftBit >= 0)
            {
                if (LEorder)
                    sb.Insert(0, String.Format("{0:X02}", (byte)((Value & (StartAnd << ShiftBit)) >> ShiftBit)));
                else
                    sb.AppendFormat("{0:X02}", (byte)((Value & (StartAnd << ShiftBit)) >> ShiftBit));
                ShiftBit -= 8;
            }
            if (sb.Length != BitsNum)
            {
                if (sb.Length > BitsNum)
                {
                    if (LEorder)
                        sb.Remove(BitsNum + 1, sb.Length - BitsNum);
                    else
                        sb.Remove(0, sb.Length - BitsNum);
                }
                else
                {
                    if (LEorder)
                        sb.Append('0', BitsNum - sb.Length);
                    else
                        sb.Insert(0, "0", BitsNum - sb.Length);
                }
            }

            return sb.ToString();
        }

        public static string PrintCRCHex(long Value)
        {
#if INTERNAL_CRC_TYPE_MODE
            return String.Format("{0:X16}", Value);
#else
            int[] eigthPrimes = new int[8] { 68687, 96431, 73859, 96989, 59651, 74149, 96337, 95789 };
            int result = 0x1234ABCD;
            byte[] bv = BitConverter.GetBytes(Value);
            for(int i = 0; i < 8; i++)
            {
                byte v = bv[i];
                result = unchecked(result ^ ((v * eigthPrimes[i]) << (7 * i)));
            }
            return String.Format("{0:X08}", result);
#endif
        }

        public static string IncreaseItemNumber(string IdentNazwa)
        {
            char[] zn = IdentNazwa.ToCharArray();
            int iter = zn.Length - 1;
            int apnd_num = 0;
            int multiplicator = 1;
            while (iter >= 0 && iter < zn.Length && Char.IsDigit(zn[iter]))
            {
                apnd_num += (((int)zn[iter]) - 0x30) * multiplicator;
                multiplicator *= 10;
                iter--;
            }
            apnd_num++;
            iter++;
            return String.Format("{0}{1}", IdentNazwa.Substring(0, iter), apnd_num);
        }

        public static bool IsHexChar(char HexLet)
        {
            if (Char.IsDigit(HexLet))
                return true;
            if (HexLet >= 'A' && HexLet <= 'F')
                return true;
            if (HexLet >= 'a' && HexLet <= 'f')
                return true;
            return false;
        }

        public static char DCharToChar(char HiNibble, char LoNibble)
        {
            byte rr = 0;
            if (Char.IsDigit(HiNibble))
                rr |= (byte)((Convert.ToByte(HiNibble) - 0x30) << 4);
            else if (Char.IsUpper(HiNibble))
                rr |= (byte)((Convert.ToByte(HiNibble) - 0x37) << 4);
            else if (Char.IsLower(HiNibble))
                rr |= (byte)((Convert.ToByte(HiNibble) - 0x57) << 4);
            if (Char.IsDigit(LoNibble))
                rr |= (byte)((Convert.ToByte(LoNibble) - 0x30));
            else if (Char.IsUpper(LoNibble))
                rr |= (byte)((Convert.ToByte(LoNibble) - 0x37));
            else if (Char.IsLower(LoNibble))
                rr |= (byte)((Convert.ToByte(LoNibble) - 0x57));
            return (char)rr;
        }

        public static string[] ASCII_Name = {
            "<NUL>", "<SOH>", "<STX>", "<ETX>", "<EOT>", "<ENQ>", "<ACK>", "<BEL>", "<BS>", "<HT>", "<LF>", "<VT>", "<HH>", "<CR>",
            "<SO>", "<SI>", "<DLE>", "<DC1>", "<DC2>", "<DC3>", "<DC4>", "<NAK>", "<SYN>", "<ETB>", "<CAN>", "<EM>", "<SUB>", "<ESC>",
            "<FS>", "<GS>", "<RS>", "<US>", "<SPC>" };

        /// <summary>
        /// Nazywa znak kodem ASCII
        /// </summary>
        /// <param name="a">Znak do nazwania</param>
        /// <returns>£aÒcuch tekstowy reprezentujπcy ten znak.</returns>
        public static string NameAsciiChar(char a)
        {
            short aa = (short)a;
            if (aa >= 0 && a <= 0x20)
                return ASCII_Name[aa];
            else if (aa == 127)
                return "<DEL>";
            else
                return Convert.ToString(a);
        }

        public static string RemoveUnderscore(string s)
        {
            int l;
            l = s.IndexOf('_');
            while (l >= 0)
            {
                s = s.Remove(l, 1);
                l = s.IndexOf('_');
            }
            return s;
        }

        public static bool IsValidIdentificatorName(string IdentName)
        {
            return IsValidIdentificatorNameEx(IdentName, false, false, false);
        }

        /// <summary>
        /// Checks for simple identificator name. Skips checking of first character.
        /// </summary>
        /// <param name="IdentName">Identificator name</param>
        /// <param name="AllowQuestionChar">Permit of using '?' and '.' </param>
        /// <returns></returns>
        public static bool IsValidIdentificatorName(string IdentName, bool AllowQuestionChar, bool AllowDotChar)
        {
            if (IdentName == null || IdentName.Length == 0)
                return false;
            char[] lst = IdentName.ToCharArray();
            if (lst[0] == '_' || STCharConsts.LNumbers.IndexOf(lst[0]) == -1)
            {
                for (int i = 1; i < lst.Length; i++)
                {
                    char ccccc = lst[i];
                    if (!STCharConsts.IsLetter(ccccc))
                    {
                        if (AllowQuestionChar && ccccc == '?')
                            continue;
                        if (AllowDotChar && ccccc == '.')
                            continue;
                        return false;
                    }
                }
                if (STCharConsts.IsBasicBuildType(IdentName) ||
                    STCharConsts.IsKeyWord(IdentName) ||
                    STUtils.IsInStringArray(IdentName, STCharConsts.KeywordsAsOperators, StringComparison.CurrentCultureIgnoreCase))
                    return false;

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Checks for simple identificator name. Checks all character
        /// </summary>
        /// <param name="IdentName">Identificator name</param>
        /// <param name="AllowQuestionChar">Permit of using '?'</param>
        /// <param name="AllowDotChar">Permit of using '.'</param>
        /// <param name="AllowDollarChar">Permit of using '$'</param>
        /// <returns>Correctness status</returns>
        public static bool IsValidIdentificatorNameEx(string IdentName, bool AllowQuestionChar, bool AllowDotChar, bool AllowDollarChar)
        {
            if (String.IsNullOrEmpty(IdentName))
                return false;
            char[] lst = IdentName.ToCharArray();
            //if (lst[0] == '_' || STCharConsts.LNumbers.IndexOf(lst[0]) == -1)
            //{
            for (int i = 0; i < lst.Length; i++)
            {
                char ccccc = lst[i];
                if (ccccc == '_')
                    continue;
                if (i == 0 && STCharConsts.IsNumber(ccccc))
                    return false;
                if (!STCharConsts.IsLetter(ccccc))
                {
                    if (AllowQuestionChar && ccccc == '?')
                        continue;
                    if (AllowDotChar && ccccc == '.')
                        continue;
                    if (AllowDollarChar && ccccc == '$')
                        continue;
                    return false;
                }
            }
            if (STCharConsts.IsBasicBuildType(IdentName) ||
                STCharConsts.IsKeyWord(IdentName) ||
                STUtils.IsInStringArray(IdentName, STCharConsts.KeywordsAsOperators, StringComparison.CurrentCultureIgnoreCase))
                return false;

            return true;
            //}
            //else
            //  return false;
        }

        public static int GetOperPriority(string Oper, out int ArgCnt)
        {
            switch (Oper)
            {
                case "@":
                    ArgCnt = 1;
                    return 13;
                case ".":
                    ArgCnt = 2;
                    return 12;
                case "%":
                    //ArgCnt = 1;
                    //return 11;
                    throw new Exception("error");
                case "**":
                    ArgCnt = 2;
                    return 10;
                case "NOT":
                    ArgCnt = 1;
                    return 8;
                case "*":
                case "/":
                case "MOD":
                    ArgCnt = 2;
                    return 7;
                case "+":
                case "-":
                    ArgCnt = 2;
                    return 6;
                case "<":
                case ">":
                case "<=":
                case ">=":
                    ArgCnt = 2;
                    return 5;
                case "=":
                case "<>":
                    ArgCnt = 2;
                    return 4;
                case "&":
                case "AND":
                    ArgCnt = 2;
                    return 3;
                case "XOR":
                    ArgCnt = 2;
                    return 2;
                case "OR":
                    ArgCnt = 2;
                    return 1;

                default:
                    ArgCnt = 0;
                    return Int32.MinValue;
            }
        }

        private static int fSeq = 0;
        public static string NameVarOp(string OpName)
        {
            switch (OpName)
            {
                case "**":
                    return String.Format("?LPWR?{0:X04}", fSeq++);
                case "NOT":
                    return String.Format("?LNOT?{0:X04}", fSeq++);
                case "*":
                    return String.Format("?LMUL?{0:X04}", fSeq++);
                case "/":
                    return String.Format("?LDIV?{0:X04}", fSeq++);
                case "MOD":
                    return String.Format("?LMOD?{0:X04}", fSeq++);
                case "+":
                    return String.Format("?LADD?{0:X04}", fSeq++);
                case "-":
                    return String.Format("?LSUB?{0:X04}", fSeq++);
                case "<":
                    return String.Format("?LLT?{0:X04}", fSeq++);
                case ">":
                    return String.Format("?LGT?{0:X04}", fSeq++);
                case "<=":
                    return String.Format("?LLE?{0:X04}", fSeq++);
                case ">=":
                    return String.Format("?LGE?{0:X04}", fSeq++);
                case "=":
                    return String.Format("?LEQ?{0:X04}", fSeq++);
                case "<>":
                    return String.Format("?LNEQ?{0:X04}", fSeq++);
                case "&":
                case "AND":
                    return String.Format("?LAND?{0:X04}", fSeq++);
                case "XOR":
                    return String.Format("?LXOR?{0:X04}", fSeq++);
                case "OR":
                    return String.Format("?LOR?{0:X04}", fSeq++);
                case "[]":
                    return String.Format("?LAR?{0:X04}", fSeq++);
                case "(":
                    return String.Format("?LBRK?{0:X04}", fSeq++);
                default:
                    if (STCharConsts.IsValidIdentificatorName(OpName))
                        return String.Format("?{0}{1:X04}", OpName, fSeq++);
                    else
                        return String.Format("?LNOTID?{0:X04}", fSeq++);
            }
        }

        public static int GetOperPriority(int StartIndex, TokenList tt, out int lArg)
        {
            BasicToken curr = tt.Lista[StartIndex];
            if (curr.Tekst.Equals("-") || curr.Tekst.Equals("+"))
            {
                if (StartIndex == 0 || tt.Lista[StartIndex - 1].Typ == STTokenType.ttOperator)
                {
                    lArg = 1;
                    return 9;
                }
            }
            return GetOperPriority(curr.Tekst, out lArg);
        }

        /*
        public static bool IsNumType(CategoryType q)
        {
            return q == CategoryType.ctINTType || q == CategoryType.ctREALType || q == CategoryType.ctUINTType;
        }

        public static bool IsIntType(CategoryType q)
        {
            return q == CategoryType.ctINTType || q == CategoryType.ctUINTType;
        }

        public static string GetValidOper(string OperatorName, out bool TextOper)
        {
            string selector = OperatorName.ToUpper(System.Globalization.CultureInfo.CurrentCulture);
            switch (selector)
            {
                case "MOD":
                    TextOper = true;
                    return selector;
                case "DIV":
                    TextOper = true;
                    return "/";
                case "MUL":
                    TextOper = true;
                    return "*";
                case "ADD":
                    TextOper = true;
                    return "+";
                case "SUB":
                    TextOper = true;
                    return "-";
                case "AND":
                    TextOper = true;
                    return "&";
                case "EQ":
                    TextOper = true;
                    return "=";
                case "NE":
                    TextOper = true;
                    return "<>";
                case "LE":
                    TextOper = true;
                    return "<=";
                case "LT":
                    TextOper = true;
                    return "<";
                case "GE":
                    TextOper = true;
                    return ">=";
                case "GT":
                    TextOper = true;
                    return ">";
                default:
                    TextOper = OperatorName.Length > 0;
                    if (TextOper)
                    {
                        String ss = STCharConsts.LIdents + ".";
                        foreach (char cc in OperatorName.ToCharArray())
                        {
                            TextOper &= ss.IndexOf(cc) != -1;
                        }
                    }
                    return selector;
            }
        }
        */
        /*
        public static string OpToFun(BasicToken B, int Arguments)
        {
            if (B.Typ == STTokenType.ttOperator)
            {
                switch (B.Tekst)
                {
                    case "+":
                        if (Arguments == 1)
                            return null;
                        else
                            return STParser.MakeNamespaceIdent(STParser.fDefNamespace, "ADD");
                    case "-":
                        if (Arguments == 1)
                            return STParser.MakeNamespaceIdent(STParser.fDefNamespace, "NEG");
                        else
                            return STParser.MakeNamespaceIdent(STParser.fDefNamespace, "SUB");
                    case "*":
                        return STParser.MakeNamespaceIdent(STParser.fDefNamespace, "MUL");
                    case "/":
                        return STParser.MakeNamespaceIdent(STParser.fDefNamespace, "DIV");
                    case "**":
                        return STParser.MakeNamespaceIdent(STParser.fDefNamespace, "EXPT");
                    case "&":
                        return STParser.MakeNamespaceIdent(STParser.fDefNamespace, "AND");
                    case "<":
                        return STParser.MakeNamespaceIdent(STParser.fDefNamespace, "LT");
                    case ">":
                        return STParser.MakeNamespaceIdent(STParser.fDefNamespace, "GT");
                    case "<=":
                        return STParser.MakeNamespaceIdent(STParser.fDefNamespace, "LE");
                    case ">=":
                        return STParser.MakeNamespaceIdent(STParser.fDefNamespace, "GE");
                    case "=":
                        return STParser.MakeNamespaceIdent(STParser.fDefNamespace, "EQ");
                    case "<>":
                        return STParser.MakeNamespaceIdent(STParser.fDefNamespace, "NE");
                }
            }
            return B.Tekst;
        }
        */
        public static bool SingleByteRep(string ArrayStream, out string hexval, out int RepCount)
        {
            hexval = null;
            RepCount = -1;
            int i = 1;
            char glob = '\0';
            while (i < ArrayStream.Length)
            {
                char cc = STUtils.DCharToChar(ArrayStream[i - 1], ArrayStream[i]);
                if (i == 1)
                {
                    glob = cc;
                    RepCount = 1;
                }
                else
                {
                    if (glob != cc)
                        return false;
                    else
                        RepCount++;
                }
                i += 2;
            }
            if (ArrayStream.Length > 1)
                hexval = ArrayStream.Substring(0, 2);
            else
                return false;
            return true;
        }

        public static int ConvertToPackedDATE(int Year, int Month, int Day)
        {
            DateTime dt = new DateTime(Year, Month, Day);
            int r;
            /*
            +----+----+----+----+
            | XXXXXXXXXXXXXXXXX |
            +----+----+----+----+ 
            | XXXXXXXXXXXXXXXXX |
            +----+----+----+----+
            Zapisana data: poniedzia≥ek 4 luty 2008r.
            */
            /*            r = ((Day / 10) << 28) | ((Day % 10) << 24);
                        r |= ((Month & 0x0F) << 16);*/
            r = (Year << 16);
            r |= (Month << 8);
            /*            int dv = 0;
                        switch (dt.DayOfWeek)
                        {
                            case DayOfWeek.Monday:
                                dv = 1;
                                break;
                            case DayOfWeek.Tuesday:
                                dv = 2;
                                break;
                            case DayOfWeek.Wednesday:
                                dv = 3;
                                break;
                            case DayOfWeek.Thursday:
                                dv = 4;
                                break;
                            case DayOfWeek.Friday:
                                dv = 5;
                                break;
                            case DayOfWeek.Saturday:
                                dv = 6;
                                break;
                            case DayOfWeek.Sunday:
                                dv = 7;
                                break;
                            default:
                                dv = 0;
                                break;
                        }
                        r |= (dv << 20);*/
            r |= Day;
            /*r |= ((Year % 10) << 8);
            r |= (((Year / 10) % 10) << 12);
            r |= (((Year / 100) % 10));
            r |= (((Year / 1000) % 10) << 4);*/
            return r;
        }

        public static bool CompareVMCode(string FuncCode, string ReqCode)
        {
            char[] fc = FuncCode.ToCharArray();
            char[] rc = ReqCode.ToCharArray();
            int rng = Math.Min(fc.Length, rc.Length);
            int i = 0;
            bool eq = true;
            while (eq && i < rng)
            {
                if (fc[i] == '*' || rc[i] == '*')
                    eq = true;
                else
                    eq = fc[i] == rc[i];

                i++;
            }
            if (i == 0)
                eq = false;
            return eq;
        }

        public static int ConvertToPackedTIME_OF_DAY(int hr, int min, double secs)
        {
            int r;
            /*
            +----+----+----+----+
            | XXXXXXXXXXXXXXXXXX|
            +----+----+----+----+
            | XXXXXXXXXXXXXXXXXX|
            +----+----+----+----+
            Zapisana godzina: 12:34:56.78
            */
            int ss = (int)Math.Truncate(secs);
            int hs = (int)Math.Truncate((secs - ss) * 100);
            r = (hs);
            r |= (ss << 8);
            r |= (min << 16);
            r |= (hr << 24);
            /*            r = ((hs / 10) << 28) | ((hs % 10) << 24);
                        r |= ((ss % 10) << 16);
                        r |= ((ss / 10) << 20);
                        r |= ((min % 10) << 8);
                        r |= ((min / 10) << 12);
                        r |= (hr % 10);
                        r |= ((hr / 10) << 4);*/
            return r;
        }

        public static int TryParseInteger(string Number)
        {
            if (Number.StartsWith("0x"))
                return Convert.ToInt32(Number.Substring(2), 16);
            if (Number.StartsWith("$"))
                return Convert.ToInt32(Number.Substring(1), 16);
            return Convert.ToInt32(Number);
        }

        /// <summary>
        /// Function detects style of number input, and returns its decimal value. The hex value must be prefixed with '0x' or '$' character. Otherwise its is considered to be decimal
        /// </summary>
        /// <param name="number">Non null input string</param>
        /// <param name="val">Result of conversion</param>
        /// <returns>Status of converting</returns>
        public static bool ParseUserInteger(string number, out int val)
        {
            if (number.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                return Int32.TryParse(number.Substring(2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out val);
            if (number.StartsWith("$"))
                return Int32.TryParse(number.Substring(1), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out val);
            return Int32.TryParse(number, out val);
        }

        public static bool AllCharsInAllowSet(string sQuery, string sSet)
        {
            bool ret = true;
            if (String.IsNullOrEmpty(sQuery))
                return ret;
            int iter = 1;
            while (ret && iter < sQuery.Length)
            {
                ret = sSet.IndexOf(sQuery[iter]) != -1;
                iter++;
            }
            return ret;
        }

        public static string DateTimeToString(DateTime dt)
        {
            return String.Format("{0:D04}-{1:D02}-{2:D02}-{3:D02}:{4:D02}:{5:D02}.{6:D03}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
        }
        /*
        public static string LangEnumToString(SupportedLanguage sl)
        {
            switch (sl)
            {
                case SupportedLanguage.slCPD_VMASM:
                    return "VMASM";
                case SupportedLanguage.slDefault:
                    return Messages.Utils_H037;
                case SupportedLanguage.slIEC_FBD:
                    return "FBD";
                case SupportedLanguage.slIEC_IL:
                    return "IL";
                case SupportedLanguage.slIEC_LD:
                    return "LD";
                case SupportedLanguage.slIEC_SFC:
                    return "SFC";
                case SupportedLanguage.slIEC_ST:
                    return "ST";
                case SupportedLanguage.slUnknown:
                    return Messages.Utils_H038;
                default:
                    return String.Format(Messages.Utils_H039, sl.ToString());
            }
        }
        */

        /*
        public static STIdentifier LocateMemberName(List<STIdentifier> MemberList, string MemberName)
        {
            foreach (STIdentifier iter in MemberList)
            {
                if (iter.Nazwa.Equals(MemberName, StringComparison.OrdinalIgnoreCase))
                    return iter;
            }
            return null;
        } */
        /*
        public static string TypeListToCommaString(List<STAbstractType> undef)
        {
            StringBuilder bs = new StringBuilder();
            int i = 0;
            while (i < undef.Count)
            {
                bs.AppendFormat("\"{0}\"", undef[i].PNazwa);
                i++;
                if (i < undef.Count)
                    bs.Append(", ");
            }
            return bs.ToString();
        } */

        /*
        public static string TypeListToCommaString(List<STIdentifier> undef)
        {
            StringBuilder bs = new StringBuilder();
            int i = 0;
            while (i < undef.Count)
            {
                bs.AppendFormat("\"{0}\"", undef[i].PNazwa);
                i++;
                if (i < undef.Count)
                    bs.Append(", ");
            }
            return bs.ToString();
        } */


        /// <summary>
        /// Similar to STTypeCatToString function, but always produces constant informations.
        /// Useful for generating XML files
        /// </summary>
        /// <param name="sTTypeCat"></param>
        /// <returns></returns>
        /*
        public static string acTypeCatToString(STTypeCat t)
        {
            switch (t)
            {
                case STTypeCat.tcAliasType:
                    return "alias";
                case STTypeCat.tcArrayType:
                    return "array";
                case STTypeCat.tcBuildInType:
                    return "build-in type";
                case STTypeCat.tcEnumType:
                    return "enumerated type";
                case STTypeCat.tcStringType:
                    return "string type";
                case STTypeCat.tcStructType:
                    return "struct type";
                case STTypeCat.tcUserFunction:
                    return "user function";
                case STTypeCat.tcVariable:
                    return "variable";
                case STTypeCat.tcFunctionBlock:
                    return "function block";
                case STTypeCat.tcSysProc:
                    return "system proc";
                case STTypeCat.tcBuildInFunction:
                    return "build-in function";
                case STTypeCat.tcProgram:
                    return "program";
                case STTypeCat.tcTask:
                    return "task";
                case STTypeCat.tcPackage:
                    return "library";
                case STTypeCat.tcNamespace:
                    return "namespace";
                default:
                    return "unknown";
            }
        } */

        /// <summary>
        /// Converts table of strings to Pascal CommaText
        /// </summary>
        /// <param name="StrIn">Table of strings</param>
        /// <returns>Whole string</returns>
        public static string TabToCommaText(string[] StrIn)
        {
            char[] quoted_str_chars = { ' ', ',', '"' };
            StringBuilder sbld = new StringBuilder();
            for (int j = 0; j < StrIn.Length; j++)
            {
                string s = StrIn[j];
                if (s == null)
                    s = String.Empty;
                bool Quoted = s.IndexOfAny(quoted_str_chars) >= 0;
                char[] ch = s.ToCharArray();
                int i = 0;
                if (j != 0)
                    sbld.Append(',');
                if (Quoted)
                {
                    sbld.Append('"');
                    while (i < ch.Length)
                    {
                        if (ch[i] == '"')
                            sbld.Append('"', 2);
                        else
                            sbld.Append(ch[i]);
                        i++;
                    }
                    sbld.Append('"');
                }
                else
                    sbld.Append(ch);
            }
            return sbld.ToString();
        }

        /// <summary>
        /// Converts Pascal CommaText to table of strings
        /// </summary>
        /// <param name="StrIn">CommaText line</param>
        /// <returns>Table of strings</returns>        
        public static string[] CommaTextToTab(string StrIn)
        {
            List<string> ret = new List<string>();
            StringBuilder current = new StringBuilder();
            char[] ch = StrIn.ToCharArray();
            int i = 0;
            int state = 0;
            while (i < ch.Length)
            {
                switch (state)
                {
                    case 0:
                        // find string type
                        if (ch[i] == '"')
                            state = 2; //quoted str
                        else
                            state = 1; //std str.
                        current.Remove(0, current.Length);
                        break;
                    case 1:
                        {
                            int StartPos = i;
                            while (i < ch.Length && ch[i] != ',')
                                i++;
                            current.Append(ch, StartPos, i - StartPos);
                            ret.Add(current.ToString());
                            if (i < ch.Length && ch[i] == ',') //seek ,
                                i++;
                            state = 0;
                        }
                        break;
                    case 2:
                        i++; // seek "
                        do
                        {
                            while (i < ch.Length && ch[i] != '"')
                            {
                                current.Append(ch[i]);
                                i++;
                            }
                            if (i < ch.Length && ch[i] == '"')
                            {
                                if (i + 1 < ch.Length && ch[i + 1] == '"')
                                {
                                    current.Append('"');
                                    i += 2;
                                }
                                else
                                {
                                    if (i + 1 < ch.Length && ch[i + 1] == ',') //seek ",
                                        i++;
                                    i++;
                                    break;
                                }
                            }
                        }
                        while (i < ch.Length);
                        ret.Add(current.ToString());
                        state = 0;
                        break;
                }
            }
            return ret.ToArray();
        }

        /// <summary>
        /// Checks for keeping value in correct bounds
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="LowerBound">The value of lower bound</param>
        /// <param name="UpperBound">The value of upper bound</param>
        /// <returns><c>true</c> - if overflow exists.</returns>
        public static bool CheckOverflowInRange(int value, int LowerBound, int UpperBound)
        {
            if (value < LowerBound)
                return true;
            if (value > UpperBound)
                return true;
            return false;
        }

        /// <summary>
        /// Checks for keeping value in correct bounds
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="LowerBound">The value of lower bound</param>
        /// <param name="UpperBound">The value of upper bound</param>
        /// <returns><c>true</c> - if overflow exists.</returns>
        public static bool CheckOverflowInRange(long value, long LowerBound, long UpperBound)
        {
            if (value < LowerBound)
                return true;
            if (value > UpperBound)
                return true;
            return false;
        }

        /// <summary>
        /// Checks for keeping value in correct bounds
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="LowerBound">The value of lower bound</param>
        /// <param name="UpperBound">The value of upper bound</param>
        /// <returns><c>true</c> - if overflow exists.</returns>
        public static bool CheckOverflowInRange(ulong value, ulong LowerBound, ulong UpperBound)
        {
            if (value < LowerBound)
                return true;
            if (value > UpperBound)
                return true;
            return false;
        }

        /// <summary>
        /// Converts Int32 value to string that represents version number
        /// </summary>
        /// <param name="var">value to convert</param>
        /// <returns>version string</returns>
        public static string IntValToVerStrig(int var)
        {
            return String.Format("{0}.{1}.{2}.{3}", (var >> 24) & 0xFF, (var >> 16) & 0xFF, (var >> 8) & 0xFF, var & 0xFF);
        }
        /*
        public static CompilerReportMode CompilerReportToCompilerReportMode(CompilerReport a)
        {
            switch (a)
            {
                case CompilerReport.crError:
                    return CompilerReportMode.crmError;
                case CompilerReport.crHint:
                    return CompilerReportMode.crmHint;
                case CompilerReport.crWarning:
                    return CompilerReportMode.crmWarning;
            }
            return 0;
        } */

        /*
        public static bool GetGroupInfo(System.Xml.XmlElement xnode, STIdentifier ff)
        {
            bool rr = true;
            System.Xml.XmlNodeList nd = xnode.SelectNodes("./group/item");
            foreach (System.Xml.XmlNode ni in nd)
            {
                System.Xml.XmlElement el = ni as System.Xml.XmlElement;
                if (el != null)
                {
                    if (ff.Group == null)
                        ff.Group = new List<GroupMembership>();
                    GroupMembership lc = new GroupMembership();
                    rr &= Int32.TryParse(el.GetAttribute("value"), out lc.GroupID);
                    rr &= Int32.TryParse(el.GetAttribute("mask"), out lc.MaskID);
                    ff.Group.Add(lc);
                }
            }
            return rr;
        } */
        /*
        public static string PtrCompilerModeToString(int vmode)
        {
            switch (vmode)
            {
                case (int)CPDev.STComp05.STParser.ParserArguments.PointerWidth.pwNotSet:
                    return Messages.Utils_H045;

                case (int)CPDev.STComp05.STParser.ParserArguments.PointerWidth.pw16bit:
                    return Messages.Utils_H046;

                case (int)CPDev.STComp05.STParser.ParserArguments.PointerWidth.pw32bit:
                    return Messages.Utils_H047;

                default:
                    return Messages.Utils_H048;
            }
        }
        */
        public static void PrintErrorsToXML(System.Xml.XmlDocument doc, System.Xml.XmlElement node, LocationList Errors)
        {
            string mmode = String.Empty;
            foreach (LocationInfo li in Errors.Items)
            {
                switch (li.Kind)
                {
                    case CompilerReport.crError:
                        mmode = "ERROR";
                        break;
                    case CompilerReport.crHint:
                        mmode = "HINT";
                        break;
                    case CompilerReport.crWarning:
                        mmode = "WARNING";
                        break;
                    default:
                        mmode = "UNKNOWN";
                        break;
                }
                System.Xml.XmlElement erinfo = doc.CreateElement(mmode);
                System.Xml.XmlAttribute linepos = doc.CreateAttribute("pos");
                linepos.Value = li.Position.ToString();
                erinfo.Attributes.Append(linepos);
                erinfo.InnerText = li.Description;
                node.AppendChild(erinfo);
            }
        }

        /// <summary>
        /// Returns address equal to 8-byte align value
        /// </summary>
        /// <param name="addr">input address</param>
        /// <returns>the equal or bigger value dividable by 8</returns>
        public static int AdjustAlignTo8(int addr)
        {
            if ((addr & 0x7) != 0)
                addr = (addr & 0x7FFFFFF8) + 8;
            return addr;
        }
        /*
        public static bool TryParseTime(string txt, LocationList Errors, int ExprLine, out long milisecs)
        {
            string tab;
            milisecs = 0;
            int rev_sig = 1;
            int hpos = txt.IndexOf('#');
            if (hpos == -1)
                tab = txt;
            else
                tab = txt.Substring(hpos + 1);
            char[] ch = tab.ToCharArray();
            long now_num = 0;
            for (int i = 0; i < ch.Length; i++)
            {
                if (ch[i] >= '0' && ch[i] <= '9')
                {
                    now_num *= 10;
                    now_num += (int)ch[i] - 0x30;
                }
                else if (ch[i] == 'm')
                {
                    if (i + 1 < ch.Length)
                    {
                        if (ch[i + 1] == 's')
                        {
                            //milisekundy
                            milisecs += rev_sig * now_num;
                            now_num = 0;
                            i++;
                        }
                    }
                    //minuty
                    milisecs += now_num * rev_sig * 60000;
                    now_num = 0;
                }
                else if (ch[i] == 's')
                {
                    //sekundy
                    milisecs += now_num * rev_sig * 1000;
                    now_num = 0;
                }
                else if (ch[i] == 'h')
                {
                    milisecs += now_num * rev_sig * 3600000;
                    now_num = 0;
                }
                else if (ch[i] == 'd')
                {
                    milisecs += now_num * rev_sig * 86400000;
                    now_num = 0;
                }
                else if (ch[i] == '-')
                {
                    rev_sig = -rev_sig;
                }
                else if (ch[i] == '.')
                {
                    return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E25F, ch[i]));
                }
                else
                    Errors.ReportWarning(ExprLine, String.Format(Messages.Parser_W03E, ch[i]));
            }
            if (now_num != 0)
                milisecs += rev_sig * now_num;
            return true;
        }
        */
        /*
        public static bool TryParseDate(string txt, LocationList Errors, int ExprLine, out int date)
        {
            string tab;
            date = 0;
            int hpos = txt.IndexOf('#');
            if (hpos == -1)
                tab = txt;
            else
                tab = txt.Substring(hpos + 1);


            string[] ch = tab.Split('-');
            int yr = -1, mn = -1, da = -1;
            bool dateOK = false;
            foreach (string iter in ch)
            {
                if (!String.IsNullOrEmpty(iter))
                {
                    if (yr == -1)
                    {
                        if (!Int32.TryParse(iter, out yr))
                            return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1AC, iter, txt));
                        if (yr < 0 || yr > 9999)
                            return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1AD, iter, txt));
                    }
                    else if (mn == -1)
                    {
                        if (!Int32.TryParse(iter, out mn))
                            return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1AE, iter, txt));
                        if (mn < 1 || mn > 12)
                            return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1AF, iter, txt));
                    }
                    else if (da == -1)
                    {
                        if (!Int32.TryParse(iter, out da))
                            return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1B0, iter, txt));
                        if (mn < 1 || mn > 31)
                            return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1B1, iter, txt));
                        dateOK = true;
                    }
                    else
                    {
                        int db;
                        if (Int32.TryParse(iter, out db) && db != 0)
                            Errors.ReportWarning(ExprLine, String.Format(Messages.Parser_W043, db, txt));
                    }
                }
            }
            if (dateOK)
            {
                try
                {
                    DateTime chk = new DateTime(yr, mn, da);
                }
                catch (ArgumentException e)
                {
                    return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1B2, txt, e.Message));
                }
            }
            else
            {
                return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1B3, txt));
            }

            date = STUtils.ConvertToPackedDATE(yr, mn, da);
            return true;
        }
        */
        /*
        public static bool TryParseTime_Of_Day(string txt, LocationList Errors, int ExprLine, out int timeOfDay)
        {
            timeOfDay = 0;
            string tab;
            int hpos = txt.IndexOf('#');
            if (hpos == -1)
                tab = txt;
            else
                tab = txt.Substring(hpos + 1);

            string[] ch = tab.Split('-', ':');
            int hr = -1, min = -1;
            double secs = -1.0;
            bool dateOK = false;
            foreach (string iter in ch)
            {
                if (!String.IsNullOrEmpty(iter))
                {
                    if (hr == -1)
                    {
                        if (!Int32.TryParse(iter, out hr))
                            return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1B4, iter, txt, Messages.Parser_U00F));
                        if (hr < 0 || hr > 23)
                            return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1B5, iter, txt, Messages.Parser_U00F));
                    }
                    else if (min == -1)
                    {
                        if (!Int32.TryParse(iter, out min))
                            return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1B4, iter, txt, Messages.Parser_U010));
                        if (min < 0 || min > 59)
                            return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1B5, iter, txt, Messages.Parser_U010));
                    }
                    else if (secs == -1.0)
                    {
                        if (!Double.TryParse(iter, System.Globalization.NumberStyles.Float, STUtils.getSTDecimalFormatter(), out secs))
                            return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1B4, iter, txt, Messages.Parser_U011));
                        if (secs < 0 || secs > 59.99999)
                            return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1B5, iter, txt, Messages.Parser_U011));
                        dateOK = true;
                    }
                    else
                    {
                        int db;
                        if (Int32.TryParse(iter, out db) && db != 0)
                            Errors.ReportWarning(ExprLine, String.Format(Messages.Parser_W045, db, txt));
                        else
                            return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1B6, iter, txt));
                    }
                }
            }
            if (!dateOK)
            {
                return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1B7, txt));
            }

            timeOfDay = STUtils.ConvertToPackedTIME_OF_DAY(hr, min, secs);
            return true;
        }
        */
        /*
        public static bool TryParseDate_And_Time(string txt, LocationList Errors, int ExprLine, out long dateAndTime)
        {
            dateAndTime = 0;
            string tab;

            int hpos = txt.IndexOf('#');
            if (hpos == -1)
                tab = txt;
            else
                tab = txt.Substring(hpos + 1);

            string[] ch = tab.Split('-', ':');
            int yr = -1, mn = -1, da = -1, hr = -1, min = -1;
            double secs = -1.0;
            string incorrectNumber = Messages.Parser_E1B8;
            string outsideRangeNumber = Messages.Parser_E1B9;
            bool dateOK = false;
            foreach (string iter in ch)
            {
                if (!String.IsNullOrEmpty(iter))
                {
                    if (yr == -1)
                    {
                        if (!Int32.TryParse(iter, out yr))
                            return Errors.ReportError(ExprLine, String.Format(incorrectNumber, iter, txt, Messages.Parser_U012));
                        if (yr < 1)
                            return Errors.ReportError(ExprLine, String.Format(outsideRangeNumber, iter, txt, Messages.Parser_U012));
                    }
                    else if (mn == -1)
                    {
                        if (!Int32.TryParse(iter, out mn))
                            return Errors.ReportError(ExprLine, String.Format(incorrectNumber, iter, txt, Messages.Parser_U013));
                        if (mn < 1 || mn > 12)
                            return Errors.ReportError(ExprLine, String.Format(outsideRangeNumber, iter, txt, Messages.Parser_U013));
                    }
                    else if (da == -1)
                    {
                        if (!Int32.TryParse(iter, out da))
                            return Errors.ReportError(ExprLine, String.Format(incorrectNumber, iter, txt, Messages.Parser_U014));
                        if (mn < 1 || mn > 31)
                            return Errors.ReportError(ExprLine, String.Format(outsideRangeNumber, iter, txt, Messages.Parser_U014));
                        dateOK = true;
                    }
                    else if (hr == -1)
                    {
                        if (!Int32.TryParse(iter, out hr))
                            return Errors.ReportError(ExprLine, String.Format(incorrectNumber, iter, txt, Messages.Parser_U00F));
                        if (hr < 0 || hr > 23)
                            return Errors.ReportError(ExprLine, String.Format(outsideRangeNumber, iter, txt, Messages.Parser_U00F));
                        dateOK = false;
                    }
                    else if (min == -1)
                    {
                        if (!Int32.TryParse(iter, out min))
                            return Errors.ReportError(ExprLine, String.Format(incorrectNumber, iter, txt, Messages.Parser_U010));
                        if (min < 0 || min > 59)
                            return Errors.ReportError(ExprLine, String.Format(outsideRangeNumber, iter, txt, Messages.Parser_U010));
                    }
                    else if (secs == -1.0)
                    {
                        if (!Double.TryParse(iter, System.Globalization.NumberStyles.Float, STUtils.getSTDecimalFormatter(), out secs))
                            //return Errors.ReportError(ExprLine, String.Format(incorrectNumber, iter, txt, Messages.Parser_U011));
                            throw new Exception("Error");
                        if (secs < 0 || secs > 59.99999)
                            //return Errors.ReportError(ExprLine, String.Format(outsideRangeNumber, iter, txt, Messages.Parser_U011));
                            throw new Exception("Error");
                        dateOK = true;
                    }
                    else
                    {
                        int db;
                        if (Int32.TryParse(iter, out db) && db != 0)
                            Errors.ReportWarning(ExprLine, String.Format(Messages.Parser_W047, db, txt));
                        else
                            return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1BA, iter, txt));
                    }
                }
            }
            if (dateOK)
            {
                if (hr == -1)
                    hr = 0;
                if (min == -1)
                    min = 0;
                if (secs == -1.0)
                    secs = 0.0;
                try
                {
                    DateTime chk = new DateTime(yr, mn, da, hr, min, 0);
                    chk = chk.AddSeconds(secs);
                }
                catch (ArgumentException e)
                {
                    return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1BB, txt, e.Message));
                }
            }
            else
            {
                return Errors.ReportError(ExprLine, String.Format(Messages.Parser_E1BC, txt));
            }

            int r = STUtils.ConvertToPackedDATE(yr, mn, da);
            int rr = STUtils.ConvertToPackedTIME_OF_DAY(hr, min, secs);
            dateAndTime = ((long)r << 32);
            dateAndTime |= (uint)rr;
            return true;
        }
        */
        public static string Colaesce(params string[] p)
        {
            for(int i = 0; i < p.Length; i++)
            {
                if (!String_IsNullOrWhiteSpace(p[i]))
                    return p[i];
            }
            return null;
        }
    }

    public sealed class LocationInfo : System.EventArgs, IDisposable
    {
        /// <summary>
        /// Pozycja w treúci pliku
        /// </summary>
        public int Position;
        /// <summary>
        /// Opis po≥oøenia
        /// </summary>
        public string Description;
        /// <summary>
        /// Rodzaj informacji
        /// </summary>
        public CompilerReport Kind;
        /// <summary>
        /// Kod b≥Ídu/ostrzeøenia/informacji
        /// </summary>
        public int InfoCode;
        /// <summary>
        /// Czy po≥oøenie jest znaczπce
        /// </summary>
        public bool IsValidPosition;

        /// <summary>
        /// Pusty konstruktor
        /// </summary>
        public LocationInfo()
        {
            Position = 0;
            Description = String.Empty;
            Kind = CompilerReport.crHint;
            IsValidPosition = true;
        }

        /// <summary>
        /// Konstruktor inicjujπcy pola
        /// </summary>
        /// <param name="Pos">Pozycja w pliku</param>
        /// <param name="Desc">Opis miejsca</param>
        /// <param name="Kind">Rodzaj miejsca</param>
        public LocationInfo(int Pos, string Desc, CompilerReport Kind)
        {
            Position = Pos;
            Description = Desc;
            this.Kind = Kind;
        }

        public LocationInfo(int Pos, int Code, string Desc, CompilerReport Kind)
            : this(Pos, Desc, Kind)
        {
            InfoCode = Code;
        }

        public void Dispose()
        {
            Description = null;
        }
    }

    [Serializable]
    public class MaximumErrorsReachedException : Exception
    {
        public MaximumErrorsReachedException() { }
        public MaximumErrorsReachedException(string message) : base(message) { }
        public MaximumErrorsReachedException(string message, Exception inner) : base(message, inner) { }
        protected MaximumErrorsReachedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class MaximumWarningsReachedException : Exception
    {
        public MaximumWarningsReachedException() { }
        public MaximumWarningsReachedException(string message) : base(message) { }
        public MaximumWarningsReachedException(string message, Exception inner) : base(message, inner) { }
        protected MaximumWarningsReachedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class MaximumHintsReachedException : Exception
    {
        public MaximumHintsReachedException() { }
        public MaximumHintsReachedException(string message) : base(message) { }
        public MaximumHintsReachedException(string message, Exception inner) : base(message, inner) { }
        protected MaximumHintsReachedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    public sealed class LocationList : IDisposable
    {
        /// <summary>
        /// Lista lokalizacji informacji
        /// </summary>
        public List<LocationInfo> Items;
        public Stack<bool> ErrorFlag;
        /// <summary>
        /// Maximum acceptable number of errors. If the CurrentNumberOfErrors value beyonds this value then MaximumErrorsReachedException is thrown.
        /// Read from config string 'CPDev.STComp05.MaximumNumberOfErrors' default -1.
        /// </summary>
        private uint MaximumNumberOfErrors;
        /// <summary>
        /// Current number of errors.
        /// </summary>
        private uint CurrentNumberOfErrors;
        /// <summary>
        /// Maximum acceptable number of warnings. If the CurrentNumberOfWarnings value beyonds this value then MaximumWarningsReachedException is thrown.
        /// Read from config string 'CPDev.STComp05.MaximumNumberOfWarnings' default -1.
        /// </summary>
        private uint MaximumNumberOfWarnings;
        /// <summary>
        /// Current number of warnings.
        /// </summary>
        private uint CurrentNumberOfWarnings;
        /// <summary>
        /// Maximum acceptable number of hints. If the CurrentNumberOfHints value beyonds this value then MaximumHintsReachedException is thrown.
        /// Read from config string 'CPDev.STComp05.MaximumNumberOfHints' default -1.
        /// </summary>
        private uint MaximumNumberOfHints;
        /// <summary>
        /// Current number of hints.
        /// </summary>
        private uint CurrentNumberOfHints;

        public event EventHandler<LocationInfo> ItemsAppeared;

        public LocationList()
        {
            Items = new List<LocationInfo>();
            ErrorFlag = new Stack<bool>();
            ErrorFlag.Push(false);
            MaximumNumberOfErrors = 1;
            MaximumNumberOfWarnings = 1;
            MaximumNumberOfHints = 1;
            CurrentNumberOfErrors = 0;
            CurrentNumberOfWarnings = 0;
            CurrentNumberOfHints = 0;
        }

        public LocationList(int maximumNumberOfErrors, int maximumNumberOfWarnings, int maximumNumberOfHints)
            : this()
        {
            this.MaximumNumberOfErrors = (uint)maximumNumberOfErrors;
            this.MaximumNumberOfWarnings = (uint)maximumNumberOfWarnings;
            this.MaximumNumberOfHints = (uint)maximumNumberOfHints;
        }

        private void incAndCheckNumberErrorsBeyond()
        {
            CurrentNumberOfErrors++;
            if (CurrentNumberOfErrors > MaximumNumberOfErrors)
                throw new MaximumErrorsReachedException();
        }
        private void incAndCheckNumberWarningsBeyond()
        {
            CurrentNumberOfWarnings++;
            if (CurrentNumberOfWarnings > MaximumNumberOfWarnings)
                throw new MaximumWarningsReachedException();
        }
        private void incAndCheckNumberHintsBeyond()
        {
            CurrentNumberOfHints++;
            if (CurrentNumberOfHints > MaximumNumberOfHints)
                throw new MaximumHintsReachedException();
        }

        public void UnsetErrorsNumberLimit()
        {
            this.MaximumNumberOfErrors = UInt32.MaxValue;
        }

        public void UnsetWarningsNumberLimit()
        {
            this.MaximumNumberOfWarnings = UInt32.MaxValue;
        }
        public void UnsetHintsNumberLimit()
        {
            this.MaximumNumberOfHints = UInt32.MaxValue;
        }

        /// <summary>
        /// Czy na liúcie znajdujπ siÍ b≥Ídy
        /// </summary>
        public bool HasErrors()
        {
            foreach (LocationInfo li in Items)
            {
                if (li.Kind == CompilerReport.crError)
                    return true;
            }
            return false;
        }

        public bool HasErrorsWithInfoCode()
        {
            foreach (LocationInfo li in Items)
            {
                if (li.Kind == CompilerReport.crError && li.InfoCode != 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Dodaje b≥πd do listy
        /// </summary>
        /// <param name="Loc">Lokalizacja b≥Ídu</param>
        /// <param name="Desc">Opis</param>
        /// <returns>Zawsze <b>false</b></returns>
        public bool ReportError(int Loc, string Desc)
        {
            if (ErrorFlag.Count > 0)
                ErrorFlag.Pop();
            ErrorFlag.Push(true);
            LocationInfo info = new LocationInfo(Loc, Desc, CompilerReport.crError);
            Items.Add(info);
            if (this.ItemsAppeared != null)
                this.ItemsAppeared(this, info);
            incAndCheckNumberErrorsBeyond();
            return false;
        }

        /// <summary>
        /// Dodaje ostrzeøenie
        /// </summary>
        /// <param name="Loc">Lokalizacja</param>
        /// <param name="Desc">Opis</param>
        public void ReportWarning(int Loc, string Desc)
        {
            LocationInfo info = new LocationInfo(Loc, Desc, CompilerReport.crWarning);
            Items.Add(info);
            if (this.ItemsAppeared != null)
                this.ItemsAppeared(this, info);
            incAndCheckNumberWarningsBeyond();
        }

        /// <summary>
        /// Raportuje informacjÍ
        /// </summary>
        /// <param name="Loc">Lokalizacja</param>
        /// <param name="Desc">Opis</param>
        public void ReportHint(int Loc, string Desc)
        {
            LocationInfo info = new LocationInfo(Loc, Desc, CompilerReport.crHint);
            Items.Add(info);
            if (this.ItemsAppeared != null)
                this.ItemsAppeared(this, info);
            incAndCheckNumberHintsBeyond();
        }

        /// <summary>
        /// Czyúci wszystkie informacje
        /// </summary>
        public void Clear()
        {
            Items.Clear();
            ErrorFlag.Clear();
            ErrorFlag.Push(false);
            CurrentNumberOfErrors = 0;
            CurrentNumberOfWarnings = 0;
            CurrentNumberOfHints = 0;
        }
        /*
        public bool ReportStandardError(int loc, STStandardErrors se, params string[] values)
        {
            switch (se)
            {
                case STStandardErrors.seUnexpEOF:
                    return ReportError(loc, Messages.Parser_E001);

                case STStandardErrors.seDuplicateIdentifier:
                    return ReportError(loc, String.Format(Messages.Parser_E002, values));

                case STStandardErrors.seRangeError:
                    return ReportError(loc, Messages.Parser_E003);

                case STStandardErrors.seInvalidCharacter:
                    return ReportError(loc, Messages.Parser_E004);

                case STStandardErrors.seUnmatchedBrackets:
                    return ReportError(loc, Messages.Parser_E005);

                case STStandardErrors.seTypeNotFound:
                    return ReportError(loc, String.Format(Messages.Parser_E006, values));

                case STStandardErrors.seIdentifierNotRegistered:
                    return ReportError(loc, String.Format(Messages.Parser_E007, values));

                case STStandardErrors.seAmbigousName:
                    return ReportError(loc, String.Format(Messages.Parser_E008, values));

                //               case STStandardErrors.seLongInitialiator:
                                  //  return ReportError(loc, String.Format(Messages.Parser_E009, values)); 

                case STStandardErrors.seIdentifierExpected:
                    return ReportError(loc, Messages.Parser_E00A);

                case STStandardErrors.seLocalVariableNotFound:
                    return ReportError(loc, String.Format(Messages.Parser_E00B, values));

                case STStandardErrors.seIdentifierNotFound:
                    return ReportError(loc, String.Format(Messages.Parser_E112, values));

                case STStandardErrors.seTypeNameExpected:
                    return ReportError(loc, Messages.Parser_E285);

                default:
                    return ReportError(loc, String.Format(Messages.Parser_E00C, se));

            }
        }
        */
        public bool ReportError(string Desc)
        {
            ErrorFlag.Pop();
            ErrorFlag.Push(true);
            LocationInfo ll = new LocationInfo(0, Desc, CompilerReport.crError);
            ll.IsValidPosition = false;
            Items.Add(ll);
            if (this.ItemsAppeared != null)
                this.ItemsAppeared(this, ll);
            incAndCheckNumberErrorsBeyond();
            return false;
        }

        public void ReportWarning(string Desc)
        {
            LocationInfo info = new LocationInfo(0, Desc, CompilerReport.crWarning);
            Items.Add(info);
            if (this.ItemsAppeared != null)
                this.ItemsAppeared(this, info);
            incAndCheckNumberWarningsBeyond();
        }

        public void PushErrorFlag()
        {
            /*            bool Val = ErrorFlag.Pop();
                        ErrorFlag.Push(Val);*/
            ErrorFlag.Push(false);
        }

        public bool PopErrorFlag()
        {
            return ErrorFlag.Pop();
        }

        /// <summary>
        /// UtwÛrz komunikat o b≥Ídzie nie wynikajπcym z kodu ürÛd≥owego
        /// </summary>
        /// <param name="messageErrorCodes">Kod b≥Ídu</param>
        /// <param name="Komunikat">TreúÊ b≥Ídu</param>
        /// <returns>Always <code>false</code>.</returns>
        /*
        public bool ReportErrorNUC(STParser.MessageEWHCodes messageErrorCodes, string Komunikat)
        {
            LocationInfo llf = new LocationInfo();
            llf.Description = Komunikat;
            llf.InfoCode = (int)messageErrorCodes;
            llf.IsValidPosition = false;
            llf.Kind = CompilerReport.crError;
            Items.Add(llf);
            if (this.ItemsAppeared != null)
                this.ItemsAppeared(this, llf);
            incAndCheckNumberErrorsBeyond();
            return false;
        } */
        /// <summary>
        /// UtwÛrz komunikat o b≥Ídzie nie wynikajπcym z kodu ürÛd≥owego
        /// </summary>
        /// <param name="messageErrorCode">Kod b≥Ídu</param>
        /// <param name="Komunikat">TreúÊ b≥Ídu</param>
        /*
        public void ReportWarningNUC(STParser.MessageEWHCodes messageErrorCode, string Komunikat)
        {
            LocationInfo llf = new LocationInfo();
            llf.Description = Komunikat;
            llf.InfoCode = (int)messageErrorCode;
            llf.IsValidPosition = false;
            llf.Kind = CompilerReport.crWarning;
            Items.Add(llf);
            if (this.ItemsAppeared != null)
                this.ItemsAppeared(this, llf);
            incAndCheckNumberWarningsBeyond();
        } */

        public void AppendItems(LocationList ErrorSource)
        {
            AppendItems(ErrorSource, false);
        }

        public void AppendItems(LocationList ErrorSource, bool IgnoreLimits)
        {
            if (ErrorSource == null || ErrorSource.Items.Count == 0)
                return;
            Items.AddRange(ErrorSource.Items);
            if (this.ItemsAppeared != null)
            {
                foreach (LocationInfo info in ErrorSource.Items)
                    this.ItemsAppeared(this, info);
            }
            bool b = ErrorFlag.Pop();
            CurrentNumberOfErrors += ErrorSource.CurrentNumberOfErrors - 1;
            ErrorFlag.Push(b || ErrorSource.HasErrors());
            if (!IgnoreLimits)
                incAndCheckNumberErrorsBeyond();
            CurrentNumberOfWarnings += ErrorSource.CurrentNumberOfWarnings - 1;
            if (!IgnoreLimits)
                incAndCheckNumberWarningsBeyond();
            CurrentNumberOfHints += ErrorSource.CurrentNumberOfHints - 1;
            if (!IgnoreLimits)
                incAndCheckNumberHintsBeyond();
        }

        public void ItemsAppearedClear()
        {
            if (this.ItemsAppeared != null)
            {
                foreach (EventHandler<LocationInfo> i in this.ItemsAppeared.GetInvocationList())
                {
                    this.ItemsAppeared -= i;
                }
            }
        }

        public bool HasItemsAppeared
        {
            get
            {
                if (this.ItemsAppeared == null)
                    return false;
                else
                    return this.ItemsAppeared.GetInvocationList().Length > 0;
            }
        }

        public void ReportItemsAgain()
        {
            if (this.ItemsAppeared != null)
            {
                foreach (LocationInfo info in this.Items)
                    this.ItemsAppeared(this, info);
            }
        }

#region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /* protected virtual <--- to avoid warning */ void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Items != null)
                    {
                        Items = null;
                    }
                    if (ErrorFlag != null)
                    {
                        ErrorFlag.Clear();
                        ErrorFlag = null;
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~LocationList()
        {
            Dispose();
        }
#endregion
    }

    public enum STStandardErrors
    {
        /// <summary>
        /// Tekst o braku ciπg≥oúci pliku
        /// </summary>
        seUnexpEOF,
        /// <summary>
        /// Tekst o podwÛjnym wystÍpowaniu argumentu (0 arg - nazwa)
        /// </summary>
        seDuplicateIdentifier,
        /// <summary>
        /// Tekst o przekroczonym zakresie w obliczaniu sta≥ej
        /// </summary>
        seRangeError,
        /// <summary>
        /// Tekst informujπcy o napotkaniu nierozpoznanych lub niew≥aúciwych znakÛw w strumieniu wejúciowym
        /// </summary>
        seInvalidCharacter,
        /// <summary>
        /// Tekst informujπcy o znalezieniu nie dopasowanych nawiasÛw ( oraz [
        /// </summary>
        seUnmatchedBrackets,
        /// <summary>
        /// Typ {0} nie znaleziony
        /// </summary>
        seTypeNotFound,
        /// <summary>
        /// Identifier "{0}" not registered
        /// </summary>
        seIdentifierNotRegistered,
        /// <summary>
        /// Ambigous short name "{0}"
        /// </summary>
        seAmbigousName,
        /// <summary>
        /// Initializator for variable "{0}" has more than 255 bytes. Not supported now.
        /// </summary>
        seLongInitialiator,
        /// <summary>
        /// Identifier expected
        /// </summary>
        seIdentifierExpected,
        /// <summary>
        /// Local identifier "{0}" not found.
        /// </summary>
        seLocalVariableNotFound,
        /// <summary>
        /// Identifier "{0}" not found. (,,Messages.Parser_E112'')
        /// </summary>
        seIdentifierNotFound,
        /// <summary>
        /// Type name expected (,,Messages.Parser_E285'')
        /// </summary>
        seTypeNameExpected,
    }

    public enum CompilerReport
    {
        /// <summary>
        /// B≥πd podczas kompilacji
        /// </summary>
        crError,
        /// <summary>
        /// Ostrzeøenie podczas kompilacji
        /// </summary>
        crWarning,
        /// <summary>
        /// Podpowiedü podczas kompilacji
        /// </summary>
        crHint,
    }

    //public delegate void OnRemoveGarbageCollector(STIdentifier del);

}
