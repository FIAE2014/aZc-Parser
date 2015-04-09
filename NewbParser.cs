using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace NoobParser.ObjectModel
{
    public class NewbParser                                                  //Logik
    {
        private Dictionary<string, IOperator> operators;                    //Dictionary für Operatoren
        public NewbParser()                                                  //Liste der Operatoren  
        {
            this.operators = new Dictionary<string, IOperator>();           
            this.AddOperator(new Operators.Add());
            this.AddOperator(new Operators.Sub());
            this.AddOperator(new Operators.Div());
            this.AddOperator(new Operators.Mul());
            this.AddOperator(new Operators.Mod());
            this.AddOperator(new Operators.Equal());
            this.AddOperator(new Operators.UnEqual());
            this.AddOperator(new Operators.BiggerAs());
            this.AddOperator(new Operators.BiggerEqualAs());
            this.AddOperator(new Operators.SmallerEqualAs());
            this.AddOperator(new Operators.SmallerAs());
        }                                               
        public IOperator[] Operators                                        //Schnittstelle für Operatoren
        {
            get
            {
                return this.operators.Values.ToArray();
            }
        }
        public void AddOperator(IOperator op)                               //Hinzufügen der Operatoren
        {
            this.operators.Add(op.Sign, op);
        }
        public List<Token> SplitToToken(string input)                       //In Tokens zerlegen
        {
            List<Token> list = new List<Token>();                   //Neu Liste, Typens Token wird erstellt
            string leftPart = string.Empty;
            while (input.Length > 0)
            {
                var token = FindToken(input);
                if (token != null)
                {
                    if (leftPart.Length > 0)
                    {
                        var t = new Token(leftPart, getTokenType(leftPart));
                        leftPart = string.Empty;
                        list.Add(t);
                    }
                    list.Add(token);
                    input = input.Substring(token.Value.Length);
                }
                else
                {
                    leftPart += input[0];
                    input = input.Substring(1);
                }
            }
            if (leftPart.Length > 0)
            {
                var t = new Token(leftPart, getTokenType(leftPart));
                leftPart = string.Empty;
                list.Add(t);
            }
            return list;
        }
        public Token FindToken(String input)                                //Tokens dem Type zuordnen und zuschneiden
        {
            var value = "(";                //Prüfen auf BracketOpen
            if (input.StartsWith(value))
            {
                return new Token(value, TokenType.BracketOpen);
            }
            value = ")";                    //Prüfen auf BracketClose
            if (input.StartsWith(value))
            {
                return new Token(value, TokenType.BracketClose);
            }
            if (input.StartsWith("\""))    //Auf Strings prüfen
            {
                var i = input.IndexOf("\"", 1);
                if (i == -1)
                {
                    throw new Exceptions.ParserException("String nicht geschlossen.");
                }
                value = input.Substring(0, i + 1);
                return new Token(value, TokenType.String);
            }
            foreach (var op in this.operators.Keys.OrderByDescending(p => p.Length)) // Sortieren(Descending) der Länge(lange Operatoren wie >=, werden zuerst angezeigt)
            {
                if (input.StartsWith(op))
                {
                    return new Token(op, TokenType.Operator);
                }
            }
            return null;
        }
        public int findBrackets(List<Token> tokenList, string character)    //Suche nach Klammern
        {
            int count = 0;
            for (int index = tokenList.Count - 1; index >= 0; index--)
            {
                Token token = tokenList[index];
                if (token.Type == TokenType.BracketOpen)
                {
                    count++;
                }
                else if (token.Type == TokenType.BracketClose)
                {
                    count--;
                }
                if (token.Value == character && count == 0)
                {
                    return index;
                }
            }
            return -1;
        }       
        private void checkBrackets(List<Token> tokenList)                   //Auf Volldtändigkeit der Klammer prüfen
        {
            int counter = 0;
            foreach (Token item in tokenList)
            {
                if (item.Type == TokenType.BracketOpen)
                {
                    counter++;
                }
                else if (item.Type == TokenType.BracketClose)
                {
                    counter--;
                }
            }
            if (counter != 0)
            {
                throw new Exceptions.InvalidBracketException("Please close Bracket");
            }
        }
        public IResult getResult(string input)                              //Berechnung
        {
            if (string.IsNullOrEmpty(input))
            {
                input = "0";
            }
            var tokenList = new NewbParser().SplitToToken(input);
            checkBrackets(tokenList);
            if (input[0] == '-' || input[0] == '+')
            {
                input = '0' + input;
            }
            foreach (var item in this.operators)
            {
                for (int i = tokenList.Count - 1; i >= 0; i--)
                {
                    Token token = tokenList[i];
                    if (token.Value == item.Key)
                    {
                        var op = this.operators[token.Value];
                        var index = findBrackets(tokenList, op.Sign);
                        if (index == i)
                        {
                            StringBuilder leftPart = new StringBuilder();
                            for (int i2 = 0; i2 < i; i2++)
                            {
                                var token2 = tokenList[i2];
                                leftPart.Append(token2.Value);
                            }
                            StringBuilder rigthPart = new StringBuilder();
                            for (int i2 = i + 1; i2 < tokenList.Count; i2++)
                            {
                                var token2 = tokenList[i2];
                                rigthPart.Append(token2.Value);
                            }

                            var result = new Operation(getResult(leftPart.ToString()), op, getResult(rigthPart.ToString()));
                            return result;
                        }
                    }
                }
            }
            if (tokenList[0].Type == TokenType.BracketOpen)
            {
                if (tokenList[tokenList.Count - 1].Type == TokenType.BracketClose)
                {
                    return (getResult(input.Substring(1, input.Length - 2)));
                }
                else
                {
                    throw new Exceptions.ParserException("Please use Brackets like...: (x or x+y...n)");
                }
            }
            try
            {
                decimal.Parse(input);
                return new Constant(input);
            }
            catch (FormatException)
            {
                return new Constant(input);P
            }
        }
        private TokenType getTokenType(string value)                        //Try Parse
        {
            decimal tmp;
            if (decimal.TryParse(value, out tmp))
            {
                return TokenType.Number;
            }
            else
            {
                return TokenType.None;
            }
        }
    }
}