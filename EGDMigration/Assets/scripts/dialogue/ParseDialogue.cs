using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParseDialogue {
    public enum Items
    {
        UNKNOWN,
        STATE_LABEL,
        JUMPTO_LABEL,
        IF_BRANCH,
        ELSE_BRANCH,
        EXIT,
        ASSIGNMENT,
        INCREMENT,
        DECREMENT,
        COMPARISON,
        CONDITION,
        PROMPT,
        DIALOGUE,
        CHOICE_LABEL
    }

    public class DialogueDatabase
    {
        private Node _current;
        private Dictionary<string, Node> _database;
        private Database _variables;

        public DialogueDatabase(Database d2)
        {
            _current = null;
            _database = new Dictionary<string, Node>();
            _variables = d2;
        }
        public void AddData(Dictionary<string, Node> d)
        {
            foreach (KeyValuePair<string, Node> kvp in d)
            {
                _database.Add(kvp.Key, kvp.Value);
            }
        }

        private string ParseString(string src)
        {
            int index = 0;
            while (index < src.Length && (index = src.IndexOf('%', index)) != -1)
            {
                int index2 = src.IndexOf('%', index + 1);
                int index3 = src.IndexOf('%', index2 + 1);

                string A = src.Substring(index + 1, index2 - index - 1);
                string B = src.Substring(index2 + 1, index3 - index2 - 1);

                if (A == BasicTokens.VARIABLE.ToString())
                {
                    string value = _variables.getEntry(B);
                    src = src.Substring(0, index) + value + src.Substring(index3 + 1);
                    index3 -= 3 + A.Length + B.Length - value.Length;
                }
            }
            return src;
        }

        public DialogueNode start(string state)
        {
            if (!_database.TryGetValue(state, out _current))
                return null;

            return interpret();
        }
        public DialogueNode next()
        {
            if ( _current == null )
                return null;
            _current = _current.next;
            return interpret();
        }
        public DialogueNode selectionMade(int select)
        {
            Node iter = _current.firstChild;
            if ( iter == null )
            {
                _current = _current.next;
            }
            else
            {
                for (int i = 0; i < select; i++)
                    iter = iter.next;
                _current = iter.firstChild;
            }
            return interpret();
        }

        private bool resolveBoolean(Node n, bool incoming)
        {
            if (n == null)
            {
                return incoming;
            }
            else if ( n.type == Items.CONDITION )
            {
                // LHS Type, LHS Value, Operator, RHS Type, RHS Value
                string lhs = n.data[1];
                string rhs = n.data[4];
                string lhs_type = n.data[0];
                string rhs_type = n.data[3];
                string op = n.data[2];
                bool ret = false;

                // incomparable types
                if ( lhs_type != rhs_type &&
                     lhs_type != BasicTokens.VARIABLE.ToString() &&
                     lhs_type != BasicTokens.VARIABLE.ToString() )
                {
                    ret =  (op == "!=");
                }
                // LHS is a variable
                else if (lhs_type == BasicTokens.VARIABLE.ToString())
                {
                    string data = _variables.getEntry(lhs);
                    if ( rhs_type == BasicTokens.BOOLEAN.ToString())
                    {
                        bool r = (rhs=="true")?true:false;
                        ret =  (r && data == "true" || !r && data == "false");
                    }
                    else if (rhs_type == BasicTokens.INTEGER.ToString())
                    {
                        int r = int.Parse(rhs);
                        int l;
                        ret =  (int.TryParse(data, out l) && r == l);
                    }
                    else if (rhs_type == BasicTokens.STRING.ToString())
                    {
                        ret =  (rhs == data);
                    }
                }
                // RHS is a variable
                else if (n.data[3] == BasicTokens.VARIABLE.ToString())
                {
                    string data = _variables.getEntry(rhs);
                    if (lhs_type == BasicTokens.BOOLEAN.ToString())
                    {
                        bool r = (lhs == "true") ? true : false;
                        ret =  (r && data == "true" || !r && data == "false");
                    }
                    else if (lhs_type == BasicTokens.INTEGER.ToString())
                    {
                        int r = int.Parse(lhs);
                        int l;
                        ret =  (int.TryParse(data, out l) && r == l);
                    }
                    else if (lhs_type == BasicTokens.STRING.ToString())
                    {
                        ret =  (lhs == data);
                    }
                }
                // comparable types
                else
                {
                    if (lhs_type == BasicTokens.BOOLEAN.ToString())
                    {
                        bool r = (lhs == "true") ? true : false;
                        bool l = (rhs == "true") ? true : false;
                        ret =  (r == l);
                    }
                    else if (lhs_type == BasicTokens.INTEGER.ToString())
                    {
                        int r = int.Parse(lhs);
                        int l = int.Parse(rhs);
                        ret =  (r == l);
                    }
                    else if (lhs_type == BasicTokens.STRING.ToString())
                    {
                        ret =  (lhs == rhs);
                    }
                }
                return resolveBoolean(n.firstChild, ret);
            }
            else if ( n.type == Items.COMPARISON )
            {
                if ( n.data[0] == "and" )
                {
                    return incoming && resolveBoolean(n.firstChild, incoming);
                }
                else
                {
                    return incoming || resolveBoolean(n.firstChild, incoming);
                }
            }
            else
            {
                throw new System.Exception("Invalid boolean expression");
            }
        }
        private DialogueNode interpret()
        {
            int temp0, temp1;
            string rhs, lhs;
            for (int x=0;x<100;x++)
                switch (_current.type)
                {
                    case Items.STATE_LABEL:
                        _current = _current.next;
                        break;
                    case Items.JUMPTO_LABEL:
                        if (_current.data[0] == "exit")
                        {
                            return null;
                        }
                        Node output;
                        if (!_database.TryGetValue(_current.data[0], out output))
                        {
                            Debug.LogError("Could not find state label " + _current.data[0]);
                            _current = null;
                            return null;
                        }
                        _current = output;
                        break;
                    case Items.ASSIGNMENT:
                        if (_current.data[2] == BasicTokens.COMMAND.ToString())
                        {
                            return new DialogueNode(_variables, _current);
                        }
                        else
                        {
                            _variables.setEntry(ParseString(_current.data[0]), ParseString(_current.data[1]));
                            _current = _current.next;
                        }
                        break;
                    case Items.IF_BRANCH:
                        Node n = _current.firstChild;
                        bool value = resolveBoolean(n, true);
                        if ( value )
                            _current = n.next;
                        else
                            _current = _current.next;
                        break;
                    case Items.ELSE_BRANCH:
                        _current = _current.firstChild;
                        break;
                    case Items.INCREMENT:
                        lhs = _current.data[0];
                        rhs = _current.data[1];
                        if (_current.data[1] == BasicTokens.VARIABLE.ToString())
                        {
                            temp0 = _variables.getInteger(lhs);
                            temp1 = _variables.getInteger(rhs);
                            temp0 += temp1;
                            _variables.setEntry(lhs, temp0.ToString());
                        }
                        else
                        {
                            temp0 = _variables.getInteger(lhs);
                            temp1 = int.Parse(rhs);
                            temp0 += temp1;
                            _variables.setEntry(lhs, temp0.ToString());
                        }
                        _current = _current.next;
                        break;
                    case Items.DECREMENT:
                        lhs = _current.data[0];
                        rhs = _current.data[1];
                        if (_current.data[1] == BasicTokens.VARIABLE.ToString())
                        {
                            temp0 = _variables.getInteger(lhs);
                            temp1 = _variables.getInteger(rhs);
                            temp0 -= temp1;
                            _variables.setEntry(lhs, temp0.ToString());
                        }
                        else
                        {
                            temp0 = _variables.getInteger(lhs);
                            temp1 = int.Parse(rhs);
                            temp0 -= temp1;
                            _variables.setEntry(lhs, temp0.ToString());
                        }
                        _current = _current.next;
                        break;
                    case Items.DIALOGUE:
                        Node child = _current.firstChild;
                        if (child != null)
                        {
                            List<Node> data = new List<Node>();
                            while (child != null)
                            {
                                if (child.type == Items.CHOICE_LABEL)
                                    data.Add(child);
                                child = child.next;
                            }
                            return new DialogueNode(_variables, _current, data.ToArray());
                        }
                        else
                        {
                            return new DialogueNode(_variables, _current);
                        }
                    case Items.EXIT:
                        return null;
                    default:
                        string res = "Didn't process: " + _current.type.ToString() + "(";
                        foreach( string s in _current.data )
                        {
                            res += " " + s;
                        }
                        Debug.Log(res + "   )");
                        _current = _current.next;
                        break;
                }
            return null;
        }
    }
    public class DialogueNode
    {
        private Database _db;
        private string _src;
        private string _dialogue;
        private string _speaker;
        private string[] _choices;
        private string[] _prompts;
        private string[] _results;
        private string[] _components;
        private bool _write_to_results;
        private int _current_prompt;

        private string GenerateString(string src)
        {
            int result = 0;
            int index = 0;
            while (index < src.Length && (index = src.IndexOf('%', index)) != -1)
            {
                int index2 = src.IndexOf('%', index + 1);
                int index3 = src.IndexOf('%', index2 + 1);
                src = src.Substring(0, index) + _results[result] + src.Substring(index3 + 1);
                result++;
            }
            return src;
        }
        private string ParseString(Database db, string src)
        {
            int index = 0;
            while(index < src.Length && (index = src.IndexOf('%', index)) != -1)
            {
                int index2 = src.IndexOf('%', index  + 1);
                int index3 = src.IndexOf('%', index2 + 1);

                string A = src.Substring(index  + 1, index2 - index  - 1);
                string B = src.Substring(index2 + 1, index3 - index2 - 1);

                if ( A == BasicTokens.VARIABLE.ToString() )
                {
                    string value = db.getEntry(B);
                    src = src.Substring(0, index) + value + src.Substring(index3 + 1);
                    index3 -= 3 + A.Length + B.Length - value.Length;
                }
                else if ( A == BasicTokens.COMMAND.ToString() )
                {
                    if ( _prompts != null )
                    {
                        string[] oldPrompts = _prompts;
                        string[] oldResults = _results;

                        _prompts = new string[oldPrompts.Length];
                        _results = new string[oldResults.Length];

                        for (int i = 0; i < _prompts.Length; i++)
                        {
                            _prompts[i] = oldPrompts[i];
                        }
                    }
                    else
                    {
                        _prompts = new string[1];
                        _results = new string[1];
                    }
                    _prompts[_prompts.Length - 1] = B;
                }
                index = index3 + 1;
            }
            _src = src;
            return src;
        }

        public DialogueNode(Database db, Node a)
        {
            _db = db;
            _current_prompt = 0;
            _src = a.data[1];
            if ( a.type != Items.DIALOGUE )
            {
                _components = null;
                _speaker = null;
                _dialogue = "";
                _choices = null;
                if ( a.type == Items.ASSIGNMENT && a.data[2] == BasicTokens.COMMAND.ToString() )
                {
                    _write_to_results = false;
                    _prompts = new string[] { a.data[1] };
                    _results = new string[] { a.data[0] };
                }
            }
            else
            {
                _prompts = null;
                _results = null;
                _speaker = ParseString(db, a.data[0]);
                _dialogue = ParseString(db, a.data[1]);
                _write_to_results = (_prompts!=null);
                _choices = null;
            }
        }
        public DialogueNode(Database db, Node a, Node[] b)
        {
            _speaker = ParseString(db, a.data[0]);
            _dialogue = ParseString(db, a.data[1]);
            _choices = new string[b.Length];
            for ( int i=0;i<b.Length;i++ )
            {
                _choices[i] = ParseString(db, b[i].data[0]);
            }
            _prompts = null;
            _results = null;
        }

        public bool Submit(string value)
        {
            if (_current_prompt >= _prompts.Length)
                return true;

            if (_write_to_results)
            {
                _results[_current_prompt] = value;
                _current_prompt++;
                if (_current_prompt >= _results.Length)
                {
                    _dialogue = GenerateString(_src);
                    _write_to_results = false;
                }
                return true;
            }
            else
            {
                _db.setEntry(_results[_current_prompt], value);
                _current_prompt++;
                return false;
            }
        }

        public string speaker { get { return _speaker; } }
        public string text {
            get {
                if (_prompts!=null && _current_prompt < _prompts.Length)
                {
                    return _prompts[_current_prompt];
                }
                else
                {
                    return _dialogue;
                }
            }
        }

        public int choices { get { return (_choices == null) ? 0 : _choices.Length; } }
        public string choice(int n)
        {
            if (_choices == null || _choices.Length <= n)
                return null;
            return _choices[n];
        }

        public int prompts { get { return (_prompts == null) ? 0 : _prompts.Length; } }
        public string prompt(int n)
        {
            if (_prompts == null || _prompts.Length <= n)
                return null;
            return _prompts[n];
        }

        public int results { get { return (_results == null) ? 0 : _results.Length; } }
        public string result(int n)
        {
            if (_results == null || _results.Length <= n)
                return null;
            return _results[n];
        }
    }

    private enum BasicTokens {
        NOT_ASSIGNED,
        NEW_LINE,
        INDENT,
        VARIABLE,
        STRING,
        INTEGER,
        BOOLEAN,
        COMMAND,
        SPECIAL,
        OPEN_PAREN,
        CLOSE_PAREN
    }
    private enum State
    {
        COMMENT,
        STRING,
        COMMAND,
        ESCAPE_CHAR,
        SPECIAL,
        DEFAULT
    }
    private class Tokenized
    {
        public BasicTokens type;
        public string value;
        public int line;

        public Tokenized(BasicTokens t, string val)
        {
            type = t;
            value = val;
            line = lineNumber;
        }

        public override string ToString()
        {
            return type.ToString() + " \"" + value + "\""; ;
        }
    }
    private class StateStack {
        private Stack<State> states = new Stack<State>();

        public State value
        {
            get
            {
                if (states.Count == 0)
                    return State.DEFAULT;
                else
                    return states.Peek();
            }
            set
            {
                states.Clear();
                states.Push(value);
                //Debug.Log(value.ToString());
            }
        }

        public void push(State s)
        {
            states.Push(s);
            //Debug.Log(value.ToString());
        }
        public void pop()
        {
            states.Pop();
            //Debug.Log(value.ToString());
        }
    }

    private class TokenIterator
    {
        private int _index;
        private Tokenized[] _tokens;
        private static Tokenized _default = new Tokenized(BasicTokens.NOT_ASSIGNED, "");

        public TokenIterator(Tokenized[] toks)
        {
            _tokens = toks;
            _index = 0;
        }

        public bool good()
        {
            return _index < _tokens.Length;
        }

        public Tokenized get()
        {
            if (_index >= _tokens.Length)
                return _default;
            Tokenized tok = _tokens[_index];
            _index++;
            return tok;
        }
        public Tokenized get(BasicTokens expected)
        {
            if (_index >= _tokens.Length)
                return _default;
            Tokenized tok = _tokens[_index];
            if (tok.type != expected)
                throw new System.Exception("Unexpected token " + tok.value + " at line " + tok.line);
            _index++;
            return tok;
        }
        public Tokenized get(BasicTokens[] expected)
        {
            if (_index >= _tokens.Length)
                return _default;
            Tokenized tok = _tokens[_index];
            bool has = false;
            foreach (BasicTokens t in expected)
                if (t == tok.type)
                {
                    has = true;
                    break;
                }
            if (!has)
                throw new System.Exception("Unexpected token " + tok.value + " at line " + tok.line);
            _index++;
            return tok;
        }
    }

    private class ItemChain
    {
        private Node root = null;
        private Node current = null;
        private Stack<Node> stack = new Stack<Node>();

        public Node first()
        {
            return root;
        }
        public Node top()
        {
            return stack.Peek();
        }
        public Node currentScope()
        {
            return (stack.Count < 2) ? null : stack.ToArray()[1];
        }

        public void add(Node item)
        {
            if (stack.Count == 0)
            {
                root = item;
                stack.Push(item);
            }
            else if (current == null)
            {
                stack.Peek().firstChild = item;
                stack.Push(item);
            }
            else
            {
                stack.Pop();
                stack.Push(item);
                current.next = item;
            }
            current = item;

        }
        public void push()
        {
            current = null;
        }
        public void pop()
        {
            stack.Pop();
            current = (stack.Count == 0)?null:stack.Peek();
        }
    }
    public class Node
    {
        public Items type;
        public string[] data;

        public Node firstChild;
        public Node next;

        public Node(Items t, string[] d)
        {
            type = t;
            data = d;
        }
        public string display()
        {
            string debug = type.ToString();
            if (data != null)
                foreach (string s in data)
                    debug += " " + s;
            else
                debug += " No Data";
            return debug;
        }
    }

    static int lineNumber;
    static BasicTokens LastToken = BasicTokens.NEW_LINE;
    static StateStack state;
    static List<Tokenized> tokenList;
    static List<Node> itemList;
    static Dictionary<string, Node> stateLabels;

    static private bool IsSpecialChar(char c)
    {
        return (c == '#' ||
                c == '%' ||
                c == '&' ||
                c == '|' ||
                c == ':' ||
                c == '+' ||
                c == '-' ||
                c == '=' ||
                c == '>' ||
                c == '<' ||
                c == '.' ||
                c == ',' ||
                c == '[' ||
                c == ']' ||
                c == '(' ||
                c == ')' ||
                c == '"' ||
                c == '”' ||
                c == '“' ||
                c == '\r' ||
                c == '\n' ||
                c == '\t' ||
                c == ' ');
    }
    static private bool IsOperatorChar(char c)
    {
        return (c == '=' ||
                c == '>' ||
                c == '<' ||
                c == '&' ||
                c == '|' ||
                c == '.' ||
                c == ',');
    }
    static private bool IsComparisonOperator(string t)
    {
        return (t == "==" ||
                t == ">=" ||
                t == "<=" ||
                t == ">" ||
                t == "<" ||
                t == "!=");
    }

    static private string MakeToken(BasicTokens t, string buf)
    {
        Tokenized nt = new Tokenized(t, buf);
        if (tokenList.Count != 0)
            LastToken = tokenList[tokenList.Count - 1].type;
        tokenList.Add(nt);
        return "";
    }

    static private string HandleComment(char c, string buffer)
    {
        if (c == '\n')
            state.value = State.DEFAULT;

        return buffer;
    }
    static private string HandleEscapeChar(char c, string buffer)
    {
        if (c == '\\')
            buffer = buffer + '\\';
        else if (c == '"')
            buffer = buffer + '"';
        else
            throw new System.Exception("Invalid escape character \\" + c + " at line " + lineNumber.ToString());
        state.pop();

        return buffer;
    }
    static private string HandleSpecial(char c, string buffer)
    {
        if (IsOperatorChar(c))
            buffer = buffer + c;
        else
        {
            if (buffer.Length != 0)
                MakeToken(BasicTokens.SPECIAL, buffer);
            state.pop();
            buffer = HandleDefault(c, "");
        }
        return buffer;
    }
    static private string HandleCommand(char c, string buffer)
    {
        if (c == ']')
        {
            buffer = MakeToken(BasicTokens.COMMAND, buffer);
            state.pop();
        }
        else if (c == '\\')
            state.push(State.ESCAPE_CHAR);
        else if (c == '\n' || c == '\r')
            throw new System.Exception("Invalid command string at line " + lineNumber.ToString());
        else
            buffer = buffer + c;

        return buffer;
    }
    static private string HandleString(char c, string buffer)
    {
        if (c == '"' || c == '”')
        {
            buffer = MakeToken(BasicTokens.STRING, buffer);
            state.pop();
        }
        else if (c == '\\')
            state.push(State.ESCAPE_CHAR);
        else if (c == '\n' || c == '\r')
            throw new System.Exception("Invalid string at line " + lineNumber.ToString());
        else
            buffer = buffer + c;

        return buffer;
    }
    static private string HandleDefault(char c, string buffer)
    {
        if (c == '\t')
        {
            BasicTokens type = tokenList[tokenList.Count - 1].type;
            if (type == BasicTokens.NEW_LINE || type == BasicTokens.INDENT) {
                buffer = MakeToken(BasicTokens.INDENT, buffer);
            }
        }
        else if (IsSpecialChar(c))
        {
            if (buffer.Length > 0)
            {
                int attempt;
                if ( buffer == "true" || buffer == "false" )
                    buffer = MakeToken(BasicTokens.BOOLEAN, buffer);
                else if ( int.TryParse(buffer, out attempt) )
                    buffer = MakeToken(BasicTokens.INTEGER, buffer);
                else
                    buffer = MakeToken(BasicTokens.VARIABLE, buffer);
            }
                

            if (c == '#')
                state.value = State.COMMENT;
            else if (c == '"' || c == '“')
                state.push(State.STRING);
            else if (c == '[')
                state.push(State.COMMAND);
            else if (c == ')')
                MakeToken(BasicTokens.OPEN_PAREN, "");
            else if (c == '(')
                MakeToken(BasicTokens.CLOSE_PAREN, "");
            else if (c == ']' || c == '”')
                throw new System.Exception("Invalid " + c + " at line " + lineNumber.ToString());
            else if (c != ' ' && c != '\n' && c != '\r')
            {
                buffer = c.ToString();
                state.push(State.SPECIAL);
            }
        }
        else
            buffer = buffer + c;

        return buffer;
    }

    static private Node ParseBoolean(Tokenized lhs, ref TokenIterator iter, Tokenized terminate)
    {
        if (lhs.type == terminate.type && lhs.value == terminate.value)
        {
            return null;
        }
        else if (lhs.type == BasicTokens.VARIABLE || lhs.type == BasicTokens.BOOLEAN || lhs.type == BasicTokens.INTEGER || lhs.type == BasicTokens.STRING)
        {
            Tokenized compare = iter.get(BasicTokens.SPECIAL);
            if (IsComparisonOperator(compare.value))
            {
                Tokenized rhs = iter.get(new BasicTokens[] { BasicTokens.BOOLEAN, BasicTokens.INTEGER, BasicTokens.STRING, BasicTokens.VARIABLE });
                Node condition = new Node(Items.CONDITION, new string[] { lhs.type.ToString(), lhs.value, compare.ToString(), rhs.type.ToString(), rhs.value });
                Node andor = ParseBoolean(iter.get(), ref iter, terminate);
                if (andor != null && andor.type != Items.COMPARISON)
                    throw new System.Exception("Invalid boolean statement");
                condition.firstChild = andor;
                return condition;
            }
            else
            {
                throw new System.Exception("Found invalid token \"" + compare.value + "\" in boolean expression on line " + compare.line);
            }
        }
        else if (lhs.type == BasicTokens.SPECIAL)
        {
               Node compare, next;
            if (lhs.value == "&&")
            {
                compare = new Node(Items.COMPARISON, new string[] { "and" });
            }
            else if (lhs.value == "||")
            {
                compare = new Node(Items.COMPARISON, new string[] { "or" });
            }
            else
            {
                throw new System.Exception("Found invalid token " + lhs.value + " in boolean expression on line " + lhs.line);
            }
            compare = new Node(Items.COMPARISON, new string[] { "and" });
            next = ParseBoolean(iter.get(), ref iter, terminate);
            if (next == null || next.type != Items.CONDITION)
                throw new System.Exception("Invalid boolean expression");
            compare.firstChild = next;
            return compare;
        }
        else if (lhs.type != BasicTokens.OPEN_PAREN && lhs.type != BasicTokens.CLOSE_PAREN && lhs.type != BasicTokens.COMMAND)
        {
            return ParseBoolean(iter.get(), ref iter, terminate);
        }
        else
            throw new System.Exception("Invalid boolean statement");
    }
    static private string ParseString(Tokenized token, ref TokenIterator iter)
    {
        bool found = true;
        Tokenized extra = null;
        Tokenized ending = null;
        try
        {
            extra = iter.get(BasicTokens.SPECIAL);
        }
        catch
        {
            found = false;
        }
        if (found )
        {
            if (extra.value != "%")
                return token.value;

            string[] elems = token.value.Split(new char[] { '%' });
            if ( elems.Length == 0 )
                throw new System.Exception("Invalid characters " + extra.value + " after string at line " + token.line.ToString());

            string finish = elems[0];
            int index = 1;
            while (ending == null || ending.type != BasicTokens.NEW_LINE)
            {
                if ( index >= elems.Length )
                    throw new System.Exception("Incorrect number of arguments after string at line " + token.line.ToString());

                extra = iter.get(new BasicTokens[] { BasicTokens.STRING, BasicTokens.VARIABLE, BasicTokens.INTEGER, BasicTokens.BOOLEAN, BasicTokens.COMMAND });
                string data = (extra.type == BasicTokens.STRING) ? ParseString(extra, ref iter) : extra.value;

                finish += "%"+ extra.type.ToString() + "%" + data + "%";
                finish += elems[index];
                index++;

                try
                {
                    ending = iter.get(new BasicTokens[] { BasicTokens.SPECIAL });
                }
                catch
                {
                    break;
                }
                if (ending.type == BasicTokens.SPECIAL && ending.value != ",")
                    break;
            }

            return finish;
        }
        else
        {
            return token.value;
        }
            
    }
    static private Node TokenizeSpecial(Tokenized token, ref TokenIterator iter)
    {
        if (token.value == ".")
        {
            Tokenized next = iter.get(BasicTokens.STRING);

            Node i;
            if (next.value.Length == 0)
                throw new System.Exception("Invalid state label \""+ next.value+ "\" on line " + next.line + ", cannot have empty state label");
            else if (next.value.Contains("%"))
                throw new System.Exception("Invalid state label \"" + next.value + "\" on line " + next.line + ", cannot use variable insertion when defining a state label");
            else if (stateLabels.TryGetValue(next.value, out i))
                throw new System.Exception("Invalid state label \"" + next.value + "\" on line " + next.line + ", the state label is a duplicate");
            iter.get(BasicTokens.NEW_LINE);

            i = new Node(Items.STATE_LABEL, new string[1] { next.value });
            stateLabels.Add(next.value, i);
            return i;
        }
        else if ( token.value == "=>" )
        {
            Tokenized next = iter.get(new BasicTokens[] { BasicTokens.STRING, BasicTokens.VARIABLE });
            if (next.type == BasicTokens.VARIABLE && next.value != "exit")
                throw new System.Exception("Invalid target \"" + next.value + "\" for jump");
            iter.get(BasicTokens.NEW_LINE);
            Node i = new Node(Items.JUMPTO_LABEL, new string[] { next.value });
            return i;
        }
        else
        {
            throw new System.Exception("Invalid characters " + token.value + " at line " + token.line.ToString());
        }
    }
    static private Node TokenizeVariable(Tokenized token, ref TokenIterator iter)
    {
        if (token.value == "exit" )
        {
            return new Node(Items.EXIT, null);
        }
        else if (token.value == "if")
        {
            Node ifstmt = new Node(Items.IF_BRANCH, null);
            return ifstmt;
        }
        else if (token.value == "else")
        {
            Tokenized next = iter.get(BasicTokens.SPECIAL);
            if (next.value != ":")
                throw new System.Exception("Invalid else statement");
            return new Node(Items.ELSE_BRANCH, null);
        }
        else if (token.value == "endif" )
        {
            return null;
        }
        else
        {
            Tokenized next = iter.get(new BasicTokens[] { BasicTokens.SPECIAL, BasicTokens.VARIABLE });
            if ( next.value == "=" )
            {
                Tokenized rhs = iter.get(new BasicTokens[] { BasicTokens.COMMAND, BasicTokens.STRING, BasicTokens.BOOLEAN, BasicTokens.INTEGER, BasicTokens.VARIABLE });
                string rhs_str;
                if ( rhs.type == BasicTokens.STRING)
                {
                    rhs_str = ParseString(rhs, ref iter);
                }
                else
                {
                    rhs_str = rhs.value;
                }
                Node i = new Node(Items.ASSIGNMENT, new string[] { token.value, rhs_str, rhs.type.ToString() });
                return i;
            }
            else if ( next.value == "+" )
            {
                Tokenized rhs = iter.get(new BasicTokens[] { BasicTokens.INTEGER, BasicTokens.VARIABLE });
                Node i = new Node(Items.INCREMENT, new string[] { token.value, rhs.value, rhs.type.ToString() });
                return i;
            }
            else if (next.value == "-")
            {
                Tokenized rhs = iter.get(new BasicTokens[] { BasicTokens.INTEGER, BasicTokens.VARIABLE });
                Node i = new Node(Items.DECREMENT, new string[] { token.value, rhs.value, rhs.type.ToString() });
                return i;
            }
            else
            {
                throw new System.Exception("Invalid characters " + next.value + " at line " + token.line.ToString());
            }
        }
    }
    static private Node TokenizeString(Tokenized token, ref TokenIterator iter)
    {
        string val = ParseString(token, ref iter);
        Tokenized next = iter.get(new BasicTokens[] { BasicTokens.STRING, BasicTokens.NEW_LINE });
        if ( next.type == BasicTokens.STRING )
        {
            string val2 = ParseString(next, ref iter);
            iter.get(BasicTokens.NEW_LINE);
            return new Node(Items.DIALOGUE, new string[] { val, val2 });
        }
        else
        {
            return new Node(Items.CHOICE_LABEL, new string[] { val });
        }
    }

    static public void Parse(TextAsset dialogue, ref DialogueDatabase db)
    {
        state = new StateStack();
        lineNumber = 1;
        tokenList = new List<Tokenized>();

        MakeToken(BasicTokens.NEW_LINE, "");

        string buffer = "";

        foreach (char c in dialogue.text)
        {
            //Debug.Log(buffer);

            if (state.value == State.COMMENT)
                buffer = HandleComment(c, buffer);

            else if (state.value == State.SPECIAL)
                buffer = HandleSpecial(c, buffer);

            else if (state.value == State.ESCAPE_CHAR)
                buffer = HandleEscapeChar(c, buffer);

            else if (state.value == State.COMMAND)
                buffer = HandleCommand(c, buffer);

            else if (state.value == State.STRING)
                buffer = HandleString(c, buffer);

            else if (state.value == State.DEFAULT)
                buffer = HandleDefault(c, buffer);

            if (c == '\n')
            {
                MakeToken(BasicTokens.NEW_LINE, "");
                lineNumber++;
            }
        }

        stateLabels = new Dictionary<string, Node>();

        int ifelse_depth = 0;
        int indent_last_count = 0;
        int indent_count = 0;
        bool defining_choice = false;
        Tokenized[] tokens = tokenList.ToArray();
        TokenIterator iterator = new TokenIterator(tokens);
        ItemChain items = new ItemChain();

        while ( iterator.good() )
        {
            Tokenized token = iterator.get();
            Node i;
            switch (token.type)
            {
                case BasicTokens.NOT_ASSIGNED:
                    throw new System.Exception("Unexpected file termination");
                case BasicTokens.NEW_LINE:
                    indent_last_count = indent_count;
                    indent_count = 0;
                    break;
                case BasicTokens.INDENT:
                    ++indent_count;
                    break;
                case BasicTokens.SPECIAL:
                    i = TokenizeSpecial(token, ref iterator);
                    if ( i.type == Items.STATE_LABEL && defining_choice )
                    {
                        defining_choice = false;
                        items.pop();
                        items.pop();
                    }
                    items.add(i);
                    break;
                case BasicTokens.VARIABLE:
                    i = TokenizeVariable(token, ref iterator);
                    if (i == null)
                    {
                        if (ifelse_depth == 0)
                            throw new System.Exception("Invalid endif statement");
                        for (int j = 0; j < ifelse_depth; j++)
                        {
                            items.pop();
                        }
                        ifelse_depth = 0;
                    }
                    else if (i.type == Items.IF_BRANCH)
                    {
                        Node condition = ParseBoolean(iterator.get(), ref iterator, new Tokenized(BasicTokens.SPECIAL, ":"));
                        items.add(i);
                        items.push();
                        items.add(condition);
                        ifelse_depth++;
                    }
                    else if (i.type == Items.ELSE_BRANCH)
                    {
                        items.pop();
                        items.add(i);
                        items.push();
                    }
                    else if (i.type == Items.EXIT)
                    {
                        if (items.top().type != Items.JUMPTO_LABEL)
                            throw new System.Exception("Invalid use of the 'exit' keyword");
                        items.add(i);
                    }
                    else
                    {
                        items.add(i);
                    }
                    break;
                case BasicTokens.STRING:
                    i = TokenizeString(token, ref iterator);
                    indent_last_count = indent_count;
                    indent_count = 0;
                    if (i.type == Items.CHOICE_LABEL)
                    {
                        if (defining_choice)
                        {
                            items.pop();
                            items.add(i);
                            items.push();
                        }
                        else if (items.top().type == Items.DIALOGUE)
                        {
                            defining_choice = true;
                            items.push();
                            items.add(i);
                            items.push();
                        }
                        else
                        {
                            throw new System.Exception("Cannot define choice label \"" + i.data[0] + "\" here");
                        }
                    }
                    else if (i.type == Items.DIALOGUE)
                    {
                        if (defining_choice)
                        {
                            defining_choice = false;
                            items.pop();
                            items.pop();
                        }
                        items.add(i);
                    }
                    break;
            }
        }

        Node rt = items.first();

        db.AddData(stateLabels);
    }
}
