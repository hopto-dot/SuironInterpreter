using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static SuironInterpreter.Stmt;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SuironInterpreter
{
    class Parser
    {
        private readonly List<Token> tokens;
        private int current = 0; // next token (not character) to be parsed

        private class ParseError : Exception { }

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }
        public List<Stmt> parse()
        {

            List<Stmt> statements = new List<Stmt>();
            while (!isAtEnd())
            {
                statements.Add(declaration());
            }

            return statements;
        }

        private Stmt declaration()
        {
            try
            {
                
                if (match(TokenType.VAR)) return varDeclaration();

                return statement();
            }
            catch (ParseError error)
            {
                synchronize();
                return null;
            }
        }

        private Stmt varDeclaration()
        {
            Token name = consume(TokenType.IDENTIFIER, "Expected a variable name.");

            Expr initializer = null;
            if (peek().Type == TokenType.STRING)
            {
                error(peek(), "Expected '=' after variable name.");
                return null;
            }
            
            if (match(TokenType.EQUAL))
            {
                initializer = expression();
            }

            string errorString = initializer == null ? $"Expected initialiser after declaration of variable '{name.Lexeme}' but got '{peek().Lexeme}'." : "Expected a ';' after variable declaration.";

            consume(TokenType.SEMICOLON, errorString);
            return new Stmt.Var(name, initializer);
        }

        #region Program BNF
        //program   := statement* EOF;

        //statement := exprStmt
        //          | printStmt ;

        //exprStmt  := expression ";" ;
        //printStmt := "print" expression ";" ;
        #endregion

        private Stmt statement()
        {
            if (match(TokenType.PRINT))
            {
                return printStatement();
            }
            return expressionStatement();
        }

        private Stmt printStatement()
        {
            Expr value = expression();
            consume(TokenType.SEMICOLON, "Expected ';' at the end of the line.");
            return new Stmt.Print(value);
        }
        private Stmt expressionStatement()
        {
            Expr expr = expression();
            consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }


        #region Expression BNF
        // extended BNF grammar representation:

        // expression := equality ;
        // equality   := comparison ( ( "!=" | "==" ) comparison )* ;
        // comparison := term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
        // term       := factor ( ( "-" | "+" ) factor )* ;
        // factor     := unary ( ( "/" | "*" ) unary )* ;
        // unary      := ( "!" | "-" ) unary
        //               | primary ;
        // primary    := NUMBER | STRING | "true" | "false" | "nil"
        //               | "(" expression ")" ;
        #endregion
        private Expr expression()
        {
            return equality();
        }
        private Expr equality()
        {
            Expr expr = comparison();

            while (match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token @operator = previous();
                Expr right = comparison();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }
        private Expr comparison()
        {
            Expr expr = term();

            while (match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token @operator = previous();
                Expr right = term();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }
        private Expr term()
        {
            Expr expr = factor();

            while (match(TokenType.MINUS, TokenType.PLUS, TokenType.AMPERSAND))
            {
                Token @operator = previous();
                Expr right = factor();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }
        private Expr factor()
        {
            Expr expr = unary();

            while (match(TokenType.SLASH, TokenType.STAR))
            {
                Token @operator = previous();
                Expr right = unary();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }
        private Expr unary()
        {
            if (match(TokenType.BANG, TokenType.MINUS))
            {
                Token @operator = previous();
                Expr right = unary();
                return new Expr.Unary(@operator, right);
            }

            return primary();
        }
        private Expr primary()
        {
            if (match(TokenType.FALSE)) return new Expr.Literal(false);
            if (match(TokenType.TRUE)) return new Expr.Literal(true);
            if (match(TokenType.NIL)) return new Expr.Literal(null);

            if (match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(previous().Literal);
            }

            if (match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(previous());
            }

            if (match(TokenType.LEFT_PAREN))
            {
                Expr expr = expression();
                consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            // throw error(peek(), "Expect expression.");

            //throw error(
            //    peek(),
            //    $"Expected expression, but found '{peek().Lexeme}'. " +
            //    "An expression should start with 'true', 'false', 'nil', a number, a string, or '('."
            //    );

            // Look at the previous token to provide context
            Token prev = current > 0 ? tokens[current - 1] : null;
            Token curr = peek();

            string message;
            if (prev != null)
            {
                switch (prev.Type)
                {
                    case TokenType.PLUS:
                    case TokenType.MINUS:
                    case TokenType.STAR:
                    case TokenType.SLASH:
                        message = $"Expected expression after operator '{prev.Lexeme}'";
                        break;
                    case TokenType.LEFT_PAREN:
                        message = "Expected expression after '('";
                        break;
                    default:
                        message = $"Expected expression before '{curr.Lexeme}'";
                        break;
                }
            }
            else
            {
                message = $"Expected expression at start of line but found '{curr.Lexeme}'";
            }

            throw error(peek(), message);
        }

        private Token consume(TokenType type, string message)
        {
            if (check(type)) return advance();

                throw error(peek(), message);
            return null;
        }

        private ParseError error(Token token, string message)
        {
            Program.error(token.Line, message);
            return new ParseError();
        }

        private void synchronize()
        {
            advance();

            while (!isAtEnd())
            {
                if (previous().Type == TokenType.SEMICOLON) return;

                switch (peek().Type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                advance();
            }
        }
        private bool match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (check(type))
                {
                    advance();
                    return true;
                }
            }

            return false;
        }
        private bool check(TokenType type)
        {
            if (isAtEnd())
            {
                return false;
            }
            else
            {
                return peek().Type == type;
            }
        }
        private Token advance()
        {
            if (!isAtEnd())
            {
                current++;
            }
            return previous();
        }
        private bool isAtEnd()
        {
            return peek().Type == TokenType.EOF;
        }
        private Token peek()
        {
            return tokens[current];
        }
        private Token previous()
        {
            return tokens[current - 1];
        }
    }

}
